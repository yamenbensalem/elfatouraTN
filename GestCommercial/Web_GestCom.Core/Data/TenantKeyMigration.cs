using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Web_GestCom.Data.Models;

namespace Web_GestCom.Data;

/// <summary>
/// Amène une base existante (clé primaire = code seul, global à la base) au schéma "clé par
/// entreprise" décrit par <see cref="AppDbContext"/> : clé primaire (company_id, code) sur les 10
/// tables métier, colonne company_id obligatoire sur leurs 9 tables filles, et tous les liens sur
/// deux colonnes. Voir AppDbContext.ConfigureTenantKeys pour le pourquoi.
///
/// Tout est dérivé du modèle EF (noms de tables/colonnes, clés, liens, index) plutôt que d'une
/// liste SQL écrite à la main : le schéma obtenu est celui qu'EnsureCreated produit sur une base
/// neuve. S'exécute dans une seule transaction — au moindre contrôle en échec, rien n'est modifié.
/// Idempotent : ne fait rien sur une base déjà à ce schéma (base neuve ou déjà migrée).
/// </summary>
public static class TenantKeyMigration
{
    private sealed record Table(IEntityType Type, string Name, string CompanyColumn, bool IsParent);

    /// <returns>true si la base a été migrée, false si elle était déjà au bon schéma.</returns>
    public static bool Apply(AppDbContext db, ILogger logger)
    {
        if (!db.Database.IsSqlServer()) return false;

        var tables = AppDbContext.TenantKeyedTypes
            .Select(clr => db.Model.FindEntityType(clr)!)
            .Select(t => new Table(
                t,
                t.GetTableName()!,
                Column(t, t.FindProperty(nameof(ITenantOwned.CompanyId))!),
                t.FindPrimaryKey()!.Properties.Count == 2))
            .ToList();

        if (tables.All(t => IsUpToDate(db, t))) return false;

        logger.LogWarning("Migration des clés par entreprise (company_id + code) : démarrage.");
        var lignesAvant = tables.ToDictionary(t => t.Name, t => Count(db, $"SELECT COUNT(*) AS Value FROM [{t.Name}]"));

        using var tx = db.Database.BeginTransaction();

        // 1. Les lignes métier doivent toutes appartenir à une entreprise (Program.cs rattache les
        //    lignes historiques à la première entreprise juste avant d'appeler cette migration).
        foreach (var t in tables.Where(t => t.IsParent))
        {
            var orphelines = Count(db, $"SELECT COUNT(*) AS Value FROM [{t.Name}] WHERE [{t.CompanyColumn}] IS NULL");
            if (orphelines > 0)
                throw new InvalidOperationException(
                    $"Migration des clés par entreprise annulée : {orphelines} ligne(s) de [{t.Name}] sans entreprise.");
        }

        // 2. Supprimer tous les liens de ces tables, et tous ceux qui pointent vers elles.
        var noms = string.Join(", ", tables.Select(t => $"N'{t.Name}'"));
        var liensExistants = db.Database.SqlQueryRaw<string>($"""
            SELECT OBJECT_NAME(fk.parent_object_id) + N'|' + fk.name AS Value
            FROM sys.foreign_keys fk
            WHERE OBJECT_NAME(fk.parent_object_id) IN ({noms}) OR OBJECT_NAME(fk.referenced_object_id) IN ({noms})
            """).ToList();
        var tablesTouchees = new HashSet<string>(tables.Select(t => t.Name), StringComparer.OrdinalIgnoreCase);
        foreach (var lien in liensExistants)
        {
            var (table, nom) = (lien.Split('|')[0], lien.Split('|')[1]);
            tablesTouchees.Add(table);
            Execute(db, $"ALTER TABLE [{table}] DROP CONSTRAINT [{nom}]");
        }

        // 3. Supprimer les index secondaires de ces tables : ceux posés sur company_id empêchent de
        //    rendre la colonne obligatoire, et les anciens index à une colonne (un par lien) ne
        //    correspondent plus à aucun lien. Ceux du modèle sont recréés à l'étape 8.
        foreach (var t in tables)
        {
            var index = db.Database.SqlQueryRaw<string>($"""
                SELECT i.name AS Value
                FROM sys.indexes i
                WHERE i.object_id = OBJECT_ID(N'{t.Name}') AND i.name IS NOT NULL
                  AND i.is_primary_key = 0 AND i.is_unique_constraint = 0
                """).ToList();
            foreach (var nom in index)
                Execute(db, $"DROP INDEX [{nom}] ON [{t.Name}]");
        }

        // 4. Tables filles : ajouter company_id et le recopier depuis le document parent (dont le
        //    code est encore unique à ce stade — sa clé d'origine n'est supprimée qu'à l'étape 6).
        foreach (var t in tables.Where(t => !t.IsParent))
        {
            if (!ColumnExists(db, t.Name, t.CompanyColumn))
                Execute(db, $"ALTER TABLE [{t.Name}] ADD [{t.CompanyColumn}] INT NULL");

            var versParent = t.Type.GetForeignKeys().First(fk => fk.PrincipalToDependent is not null
                && AppDbContext.TenantKeyedTypes.Contains(fk.PrincipalEntityType.ClrType));
            var parent = tables.Single(p => p.Type == versParent.PrincipalEntityType);
            var codeFille = Column(t.Type, versParent.Properties[1]);
            var codeParent = Column(parent.Type, versParent.PrincipalKey.Properties[1]);

            Execute(db, $"""
                UPDATE f SET f.[{t.CompanyColumn}] = p.[{parent.CompanyColumn}]
                FROM [{t.Name}] f JOIN [{parent.Name}] p ON f.[{codeFille}] = p.[{codeParent}]
                WHERE f.[{t.CompanyColumn}] IS NULL
                """);

            var orphelines = Count(db, $"SELECT COUNT(*) AS Value FROM [{t.Name}] WHERE [{t.CompanyColumn}] IS NULL");
            if (orphelines > 0)
                throw new InvalidOperationException(
                    $"Migration des clés par entreprise annulée : {orphelines} ligne(s) de [{t.Name}] sans document parent.");
        }

        // 5. company_id devient obligatoire partout.
        foreach (var t in tables)
            Execute(db, $"ALTER TABLE [{t.Name}] ALTER COLUMN [{t.CompanyColumn}] INT NOT NULL");

        // 6. Nouvelle clé primaire (company_id, code) sur les tables métier.
        foreach (var t in tables.Where(t => t.IsParent))
        {
            var ancienneCle = db.Database.SqlQueryRaw<string>(
                $"SELECT name AS Value FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID(N'{t.Name}') AND type = 'PK'").ToList();
            foreach (var nom in ancienneCle)
                Execute(db, $"ALTER TABLE [{t.Name}] DROP CONSTRAINT [{nom}]");

            var cle = t.Type.FindPrimaryKey()!;
            Execute(db, $"ALTER TABLE [{t.Name}] ADD CONSTRAINT [{cle.GetName()}] PRIMARY KEY ({Columns(t.Type, cle.Properties)})");
        }

        // 7. Recréer, d'après le modèle, tous les liens des tables touchées (WITH CHECK : les
        //    données existantes sont vérifiées, une incohérence annule toute la migration).
        var liens = db.Model.GetEntityTypes()
            .Where(e => e.GetTableName() is { } nom && tablesTouchees.Contains(nom))
            .SelectMany(e => e.GetForeignKeys())
            .ToList();
        foreach (var fk in liens)
        {
            var dependant = fk.DeclaringEntityType;
            var principal = fk.PrincipalEntityType;
            var action = fk.DeleteBehavior switch
            {
                DeleteBehavior.Cascade => " ON DELETE CASCADE",
                DeleteBehavior.SetNull => " ON DELETE SET NULL",
                _ => ""
            };
            Execute(db, $"""
                ALTER TABLE [{dependant.GetTableName()}] WITH CHECK ADD CONSTRAINT [{fk.GetConstraintName()}]
                FOREIGN KEY ({Columns(dependant, fk.Properties)})
                REFERENCES [{principal.GetTableName()}] ({Columns(principal, fk.PrincipalKey.Properties)}){action}
                """);
        }

        // 8. Index du modèle manquants (ceux supprimés à l'étape 3, et ceux des nouveaux liens).
        foreach (var t in tables)
        {
            foreach (var index in t.Type.GetIndexes())
            {
                var nom = index.GetDatabaseName()!;
                if (Count(db, $"SELECT COUNT(*) AS Value FROM sys.indexes WHERE object_id = OBJECT_ID(N'{t.Name}') AND name = N'{nom}'") > 0)
                    continue;
                Execute(db, $"CREATE {(index.IsUnique ? "UNIQUE " : "")}INDEX [{nom}] ON [{t.Name}] ({Columns(t.Type, index.Properties)})");
            }
        }

        // 9. Aucune ligne ne doit avoir été perdue.
        foreach (var t in tables)
        {
            var apres = Count(db, $"SELECT COUNT(*) AS Value FROM [{t.Name}]");
            if (apres != lignesAvant[t.Name])
                throw new InvalidOperationException(
                    $"Migration des clés par entreprise annulée : [{t.Name}] est passée de {lignesAvant[t.Name]} à {apres} ligne(s).");
        }

        tx.Commit();
        logger.LogWarning(
            "Migration des clés par entreprise terminée : {Tables} tables, {Liens} liens recréés, {Lignes} lignes conservées.",
            tables.Count, liens.Count, lignesAvant.Values.Sum());
        return true;
    }

    private static bool IsUpToDate(AppDbContext db, Table t)
    {
        var companyObligatoire = Count(db, $"""
            SELECT COUNT(*) AS Value FROM sys.columns
            WHERE object_id = OBJECT_ID(N'{t.Name}') AND name = N'{t.CompanyColumn}' AND is_nullable = 0
            """) == 1;
        if (!companyObligatoire) return false;
        if (!t.IsParent) return true;

        return Count(db, $"""
            SELECT COUNT(*) AS Value
            FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
            WHERE i.object_id = OBJECT_ID(N'{t.Name}') AND i.is_primary_key = 1
            """) == 2;
    }

    private static bool ColumnExists(AppDbContext db, string table, string column)
        => Count(db, $"SELECT COUNT(*) AS Value FROM sys.columns WHERE object_id = OBJECT_ID(N'{table}') AND name = N'{column}'") > 0;

    private static string Column(IEntityType type, IProperty property)
        => property.GetColumnName(StoreObjectIdentifier.Table(type.GetTableName()!, type.GetSchema()))!;

    private static string Columns(IEntityType type, IEnumerable<IProperty> properties)
        => string.Join(", ", properties.Select(p => $"[{Column(type, p)}]"));

    // Les noms injectés dans le SQL viennent tous du modèle EF ou du catalogue système, jamais d'une saisie.
#pragma warning disable EF1002, EF1003
    private static int Count(AppDbContext db, string sql) => db.Database.SqlQueryRaw<int>(sql).AsEnumerable().Single();

    private static void Execute(AppDbContext db, string sql) => db.Database.ExecuteSqlRaw(sql.Replace("{", "{{").Replace("}", "}}"));
#pragma warning restore EF1002, EF1003
}
