using Microsoft.Extensions.Logging;
using Web_T4C_GestCom.Data.Models;

namespace Web_T4C_GestCom.Data;

public static class MockDataSeeder
{
    private const double TimbreFiscal = 0.6;

    public static void Seed(AppDbContext db, ILogger logger)
    {
        var rng = new Random(20260331);

        EnsureEntreprise(db);

        var deviseCodes = db.Devises.Select(d => d.CodeDevise).ToList();
        var tvaByCode = db.TvasProduit.ToDictionary(t => t.CodeTvaProduit, t => t.TauxTvaProduit);
        var modePayementCodes = db.ModesPayement.Select(m => m.CodeModePayement).ToList();
        var uniteCodes = db.UnitesProduit.Select(u => u.CodeUniteProduit).ToList();
        var categorieCodes = db.CategoriesProduit.Select(c => c.CodeCategorieProduit).ToList();
        var fabriquantCodes = db.FabriquantsProduit.Select(f => f.CodeFabriquantProduit).ToList();

        if (deviseCodes.Count == 0 || tvaByCode.Count == 0 || modePayementCodes.Count == 0)
        {
            logger.LogWarning("MockDataSeeder: reference tables are empty (devise/tva/modepayement).");
            return;
        }

        if (uniteCodes.Count == 0)
            uniteCodes.Add(1);
        if (categorieCodes.Count == 0)
            categorieCodes.Add(1);
        if (fabriquantCodes.Count == 0)
            fabriquantCodes.Add(1);

        var fournisseurs = BuildFournisseurs(rng, deviseCodes);
        var clients = BuildClients(rng, deviseCodes);
        var produits = BuildProduits(rng, fournisseurs, deviseCodes, tvaByCode, uniteCodes, categorieCodes, fabriquantCodes);

        var existingFournisseurCodes = db.Fournisseurs.Select(f => f.CodeFournisseur).ToHashSet();
        var existingClientCodes = db.Clients.Select(c => c.CodeClient).ToHashSet();
        var existingProduitCodes = db.Produits.Select(p => p.CodeProduit).ToHashSet();

        var newFournisseurs = fournisseurs.Where(f => !existingFournisseurCodes.Contains(f.CodeFournisseur)).ToList();
        var newClients = clients.Where(c => !existingClientCodes.Contains(c.CodeClient)).ToList();
        var newProduits = produits.Where(p => !existingProduitCodes.Contains(p.CodeProduit)).ToList();

        if (newFournisseurs.Count > 0)
            db.Fournisseurs.AddRange(newFournisseurs);
        if (newClients.Count > 0)
            db.Clients.AddRange(newClients);
        if (newProduits.Count > 0)
            db.Produits.AddRange(newProduits);

        if (newFournisseurs.Count > 0 || newClients.Count > 0 || newProduits.Count > 0)
            db.SaveChanges();

        var mockClients = db.Clients.Where(c => c.CodeClient.StartsWith("CLM")).ToList();
        var mockFournisseurs = db.Fournisseurs.Where(f => f.CodeFournisseur.StartsWith("FRM")).ToList();
        var mockProduits = db.Produits.Where(p => p.CodeProduit.StartsWith("PRDM")).ToList();

        if (mockClients.Count == 0 || mockFournisseurs.Count == 0 || mockProduits.Count == 0)
        {
            logger.LogWarning("MockDataSeeder: unable to continue, mock master data set is incomplete.");
            return;
        }

        var docsAlreadySeeded =
            db.DevisClient.Any(d => d.NumeroDevis.StartsWith("MDV")) ||
            db.CommandesVente.Any(c => c.NumeroCommandeVente.StartsWith("MCV")) ||
            db.BonsLivraison.Any(b => b.NumeroBonLivraison.StartsWith("MBL")) ||
            db.FacturesClient.Any(f => f.NumeroFactureClient.StartsWith("MFC")) ||
            db.CommandesAchat.Any(c => c.NumeroCommandeAchat.StartsWith("MCA")) ||
            db.BonsReception.Any(b => b.NumeroBonReception.StartsWith("MBR")) ||
            db.FacturesFournisseur.Any(f => f.NumeroFactureFournisseur.StartsWith("MFF"));

        if (docsAlreadySeeded)
        {
            logger.LogInformation(
                "MockDataSeeder: mock documents already present. Added {ClientsAdded} clients, {FournisseursAdded} fournisseurs, {ProduitsAdded} produits.",
                newClients.Count,
                newFournisseurs.Count,
                newProduits.Count);
            return;
        }

        var salesCounters = SeedSalesDocuments(db, rng, mockClients, mockProduits, modePayementCodes, tvaByCode);
        var purchaseCounters = SeedPurchaseDocuments(db, rng, mockFournisseurs, mockProduits, modePayementCodes, tvaByCode);

        db.SaveChanges();

        logger.LogInformation(
            "MockDataSeeder: added {ClientsAdded} clients, {FournisseursAdded} fournisseurs, {ProduitsAdded} produits; created {Devis} devis, {CommandesVente} commandes vente, {BonsLivraison} bons livraison, {FacturesClient} factures client, {CommandesAchat} commandes achat, {BonsReception} bons reception, {FacturesFournisseur} factures fournisseur.",
            newClients.Count,
            newFournisseurs.Count,
            newProduits.Count,
            salesCounters.Devis,
            salesCounters.CommandesVente,
            salesCounters.BonsLivraison,
            salesCounters.FacturesClient,
            purchaseCounters.CommandesAchat,
            purchaseCounters.BonsReception,
            purchaseCounters.FacturesFournisseur);
    }

    private static void EnsureEntreprise(AppDbContext db)
    {
        if (db.Entreprises.Any())
            return;

        db.Entreprises.Add(new Entreprise
        {
            CodeEntreprise = "ENT01",
            NomEntreprise = "T4C Demo Company",
            MatriculeFiscale = "MF123456",
            Adresse = "Rue des Entrepreneurs",
            CodePostal = "1002",
            Ville = "Tunis",
            Pays = "Tunisie",
            Tel = "+21670000000",
            Fax = "+21671000000",
            Email = "contact@t4c-demo.tn",
            Site = "https://t4c-demo.tn",
            PathLogo = "./logoApp.png",
            Rib = "TN5900100000000000000000",
            Note = "Entreprise de demonstration generee automatiquement"
        });

        db.SaveChanges();
    }

    private static List<Client> BuildClients(Random rng, IReadOnlyList<int> deviseCodes)
    {
        var names = new[]
        {
            "Sahara Distribution",
            "Atlas Retail",
            "Maghreb Services",
            "El Amal Trading",
            "Nova Market",
            "Carthage Solutions",
            "Tunitech Industries",
            "Rades Logistics",
            "Horizon Commerce",
            "MediPlus Store",
            "Sigma Equipements",
            "Delta Office"
        };

        var clients = new List<Client>(names.Length);
        for (int i = 0; i < names.Length; i++)
        {
            var morale = i % 2 == 0;
            clients.Add(new Client
            {
                CodeClient = $"CLM{i + 1:0000}",
                NomClient = names[i],
                MatriculeFiscale = $"CL{i + 1000:0000}",
                TypePersonne = morale ? "Morale" : "Physique",
                TypeEntreprise = morale ? "SARL" : null,
                Rib = $"TN59{rng.NextInt64(1_000_000_000_000_000, 9_999_999_999_999_999)}",
                Adresse = $"{10 + i} Avenue Principale",
                CodePostal = $"10{i:00}",
                Ville = i % 3 == 0 ? "Tunis" : i % 3 == 1 ? "Sfax" : "Sousse",
                Pays = "Tunisie",
                Tel = $"+2167{rng.Next(1000000, 9999999)}",
                TelMobile = $"+2169{rng.Next(1000000, 9999999)}",
                Fax = $"+2167{rng.Next(1000000, 9999999)}",
                Email = $"contact{i + 1}@client-demo.tn",
                Site = $"https://client{i + 1}.demo.tn",
                EtatClient = "Actif",
                Etranger = i % 5 == 0 ? "OUI" : "NON",
                Exonore = i % 6 == 0 ? "OUI" : "NON",
                MaxCredit = 5000 + (i * 1500),
                CodeDevise = Pick(rng, deviseCodes),
                Responsable = $"Responsable {i + 1}",
                Note = "Client mock"
            });
        }

        return clients;
    }

    private static List<Fournisseur> BuildFournisseurs(Random rng, IReadOnlyList<int> deviseCodes)
    {
        var names = new[]
        {
            "Nord Fournitures",
            "Global Import",
            "Prime Industrie",
            "Technica Supply",
            "Mercure Wholesale",
            "Azur Materiel",
            "Orion Components",
            "Medina Distribution"
        };

        var fournisseurs = new List<Fournisseur>(names.Length);
        for (int i = 0; i < names.Length; i++)
        {
            fournisseurs.Add(new Fournisseur
            {
                CodeFournisseur = $"FRM{i + 1:0000}",
                NomFournisseur = names[i],
                MatriculeFiscale = $"FR{i + 2000:0000}",
                Adresse = $"{20 + i} Zone Industrielle",
                CodePostal = $"20{i:00}",
                Ville = i % 2 == 0 ? "Ben Arous" : "Ariana",
                Pays = "Tunisie",
                Tel = $"+2167{rng.Next(1000000, 9999999)}",
                TelMobile = $"+2162{rng.Next(1000000, 9999999)}",
                Fax = $"+2167{rng.Next(1000000, 9999999)}",
                Email = $"sales{i + 1}@fournisseur-demo.tn",
                Rib = $"TN59{rng.NextInt64(1_000_000_000_000_000, 9_999_999_999_999_999)}",
                EtatFournisseur = "Actif",
                Note = "Fournisseur mock",
                CodeDevise = Pick(rng, deviseCodes)
            });
        }

        return fournisseurs;
    }

    private static List<Produit> BuildProduits(
        Random rng,
        IReadOnlyList<Fournisseur> fournisseurs,
        IReadOnlyList<int> deviseCodes,
        IReadOnlyDictionary<int, double> tvaByCode,
        IReadOnlyList<int> uniteCodes,
        IReadOnlyList<int> categorieCodes,
        IReadOnlyList<int> fabriquantCodes)
    {
        var labels = new[]
        {
            "Papier A4 80g", "Stylo Bleu", "Classeur Bureau", "Cartouche Encre", "Ecran 24 pouces",
            "Clavier USB", "Souris Optique", "Ramette Premium", "Cahier Spirale", "Support Ecran",
            "Chaise Bureau", "Bureau Compact", "Routeur Wifi", "Switch 8 ports", "Cordon HDMI",
            "Disque SSD 512", "Disque Externe 1To", "Imprimante Laser", "Scanner A4", "Webcam HD",
            "Casque Audio", "Microphone USB", "Onduleur 1200VA", "Projecteur", "Tablette 10 pouces",
            "Adaptateur USB-C", "Cable Reseau Cat6", "Batterie Backup", "Boite Archive", "Etiquettes Adhesives"
        };

        var tvaCodes = tvaByCode.Keys.OrderBy(k => k).ToList();
        var produits = new List<Produit>(labels.Length);

        for (int i = 0; i < labels.Length; i++)
        {
            var codeTva = i % 5 == 0 ? 2 : Pick(rng, tvaCodes);
            var tva = tvaByCode.TryGetValue(codeTva, out var taux) ? taux : 19;

            var prixAchatHT = R3(rng.Next(15, 220) + rng.NextDouble());
            var marge = rng.Next(15, 45);
            var prixVenteHT = R3(prixAchatHT * (1 + marge / 100.0));
            var prixAchatTTC = R3(prixAchatHT * (1 + tva / 100.0));
            var prixVenteTTC = R3(prixVenteHT * (1 + tva / 100.0));
            var remise = rng.Next(0, 6);
            var remiseMax = Math.Max(remise + 5, rng.Next(8, 22));

            produits.Add(new Produit
            {
                CodeProduit = $"PRDM{i + 1:0000}",
                DesignationProduit = labels[i],
                PrixUnitaire = prixVenteHT,
                CodeDevise = Pick(rng, deviseCodes),
                Quantite = rng.Next(120, 420),
                CodeFournisseur = Pick(rng, fournisseurs).CodeFournisseur,
                CodeUniteProduit = Pick(rng, uniteCodes),
                PrixAchatTTC = prixAchatTTC,
                TauxMarge = marge,
                PrixVenteHT = prixVenteHT,
                Remise = remise,
                CodeTvaProduit = codeTva,
                Fodec = i % 4 == 0 ? 1 : i % 9 == 0 ? 2 : 0,
                PrixVenteTTC = prixVenteTTC,
                CodeCategorieProduit = Pick(rng, categorieCodes),
                CodeFabriquantProduit = Pick(rng, fabriquantCodes),
                StockMinimal = rng.Next(8, 30),
                RemiseMaximale = remiseMax,
                Rayon = $"R{1 + (i % 6)}",
                Etage = (1 + (i % 3)).ToString()
            });
        }

        return produits;
    }

    private static (int Devis, int CommandesVente, int BonsLivraison, int FacturesClient) SeedSalesDocuments(
        AppDbContext db,
        Random rng,
        IReadOnlyList<Client> clients,
        IReadOnlyList<Produit> produits,
        IReadOnlyList<int> modePayementCodes,
        IReadOnlyDictionary<int, double> tvaByCode)
    {
        var commandes = new List<CommandeVente>();

        for (int i = 1; i <= 12; i++)
        {
            var date = RandomDate(rng, 120, 30);
            var client = Pick(rng, clients);
            var prepared = PrepareSalesLines(rng, produits, tvaByCode, 2, 4, includeFodec: false);
            var remiseDoc = rng.Next(0, 7);

            var devis = new DevisClient
            {
                NumeroDevis = MakeNumero("MDV", date, 900 + i),
                DateDevis = date,
                CodeClient = client.CodeClient,
                Remise = remiseDoc,
                Timbre = TimbreFiscal,
                EtatDevis = Pick(rng, new[] { "Ouvert", "Confirmé", "Annulé" }),
                Note = "Devis mock"
            };

            var ht = prepared.Sum(l => l.MontantHT);
            var tva = prepared.Sum(l => l.MontantHT * l.Tva / 100.0);
            var remiseAmount = ht * remiseDoc / 100.0;

            devis.MontantHT = R3(ht);
            devis.MontantTVA = R3(tva);
            devis.MontantTTC = R3(ht - remiseAmount + tva);
            devis.Lignes = prepared.Select(l => new LigneDevisClient
            {
                NumeroDevis = devis.NumeroDevis,
                CodeProduit = l.CodeProduit,
                Quantite = l.Quantite,
                PrixUnitaire = l.PrixUnitaire,
                Remise = l.Remise,
                Tva = l.Tva,
                MontantHT = l.MontantHT
            }).ToList();

            db.DevisClient.Add(devis);
        }

        for (int i = 1; i <= 10; i++)
        {
            var date = RandomDate(rng, 110, 20);
            var client = Pick(rng, clients);
            var prepared = PrepareSalesLines(rng, produits, tvaByCode, 2, 5, includeFodec: false);
            var remiseDoc = rng.Next(0, 7);

            var commande = new CommandeVente
            {
                NumeroCommandeVente = MakeNumero("MCV", date, 900 + i),
                DateCommandeVente = date,
                CodeClient = client.CodeClient,
                Remise = remiseDoc,
                EtatCommandeVente = Pick(rng, new[] { "Ouvert", "Confirmé" }),
                EtatLivraison = Pick(rng, new[] { "Non Livré", "Partiellement Livré", "Livré" }),
                Note = "Commande vente mock"
            };

            var ht = prepared.Sum(l => l.MontantHT);
            var tva = prepared.Sum(l => l.MontantHT * l.Tva / 100.0);
            var remiseAmount = ht * remiseDoc / 100.0;

            commande.MontantHT = R3(ht);
            commande.MontantTVA = R3(tva);
            commande.MontantTTC = R3(ht - remiseAmount + tva);
            commande.Lignes = prepared.Select(l => new LigneCommandeVente
            {
                NumeroCommandeVente = commande.NumeroCommandeVente,
                CodeProduit = l.CodeProduit,
                Quantite = l.Quantite,
                PrixUnitaire = l.PrixUnitaire,
                Remise = l.Remise,
                Tva = l.Tva,
                MontantHT = l.MontantHT
            }).ToList();

            commandes.Add(commande);
            db.CommandesVente.Add(commande);
        }

        for (int i = 1; i <= 8; i++)
        {
            var date = RandomDate(rng, 90, 10);
            var client = Pick(rng, clients);
            var prepared = PrepareSalesLines(rng, produits, tvaByCode, 2, 4, includeFodec: false);
            var remiseDoc = rng.Next(0, 6);
            var commandeLiee = commandes.Where(c => c.CodeClient == client.CodeClient).OrderBy(_ => rng.Next()).FirstOrDefault();

            var bon = new BonLivraison
            {
                NumeroBonLivraison = MakeNumero("MBL", date, 900 + i),
                DateBonLivraison = date,
                CodeClient = client.CodeClient,
                NumeroCommandeVente = commandeLiee?.NumeroCommandeVente,
                Remise = remiseDoc,
                EtatBonLivraison = Pick(rng, new[] { "Ouvert", "Livré" }),
                EtatFacture = Pick(rng, new[] { "Non Facturé", "Facturé" }),
                Note = "Bon de livraison mock"
            };

            var ht = prepared.Sum(l => l.MontantHT);
            var tva = prepared.Sum(l => l.MontantHT * l.Tva / 100.0);
            var remiseAmount = ht * remiseDoc / 100.0;

            bon.MontantHT = R3(ht);
            bon.MontantTVA = R3(tva);
            bon.MontantTTC = R3(ht - remiseAmount + tva);
            bon.Lignes = prepared.Select(l => new LigneBonLivraison
            {
                NumeroBonLivraison = bon.NumeroBonLivraison,
                CodeProduit = l.CodeProduit,
                Quantite = l.Quantite,
                PrixUnitaire = l.PrixUnitaire,
                Remise = l.Remise,
                Tva = l.Tva,
                MontantHT = l.MontantHT
            }).ToList();

            foreach (var line in prepared)
                line.Produit.Quantite = Math.Max(0, R3(line.Produit.Quantite - line.Quantite));

            db.BonsLivraison.Add(bon);
        }

        for (int i = 1; i <= 10; i++)
        {
            var date = RandomDate(rng, 80, 2);
            var client = Pick(rng, clients);
            var prepared = PrepareSalesLines(rng, produits, tvaByCode, 2, 5, includeFodec: true);
            var remiseDoc = rng.Next(0, 6);

            var facture = new FactureClient
            {
                NumeroFactureClient = MakeNumero("MFC", date, 900 + i),
                DateFactureClient = date,
                CodeClient = client.CodeClient,
                Remise = remiseDoc,
                Timbre = TimbreFiscal,
                EtatFacture = Pick(rng, new[] { "Facture Ouverte", "Facture Livrée" }),
                Note = "Facture client mock",
                IsAvoir = false
            };

            var ht = prepared.Sum(l => l.MontantHT);
            var tva = prepared.Sum(l => l.MontantHT * l.Tva / 100.0);
            var fodec = prepared.Sum(l => l.MontantHT * l.Fodec / 100.0);
            var remiseAmount = ht * remiseDoc / 100.0;

            facture.MontantHT = R3(ht);
            facture.MontantTVA = R3(tva);
            facture.Fodec = R3(fodec);
            facture.MontantTTC = R3(ht - remiseAmount + tva + fodec);
            facture.Lignes = prepared.Select(l => new LigneFactureClient
            {
                NumeroFactureClient = facture.NumeroFactureClient,
                CodeProduit = l.CodeProduit,
                Quantite = l.Quantite,
                PrixUnitaire = l.PrixUnitaire,
                Remise = l.Remise,
                Tva = l.Tva,
                Fodec = l.Fodec,
                MontantHT = l.MontantHT
            }).ToList();

            foreach (var line in prepared)
                line.Produit.Quantite = Math.Max(0, R3(line.Produit.Quantite - line.Quantite));

            var totalDue = facture.MontantTTC + facture.Timbre;
            var etatReglement = PickReglementEtat(rng);
            facture.EtatReglement = etatReglement;
            facture.Reglements = BuildReglementsClient(rng, facture, totalDue, etatReglement, modePayementCodes);

            db.FacturesClient.Add(facture);
        }

        return (12, 10, 8, 10);
    }

    private static (int CommandesAchat, int BonsReception, int FacturesFournisseur) SeedPurchaseDocuments(
        AppDbContext db,
        Random rng,
        IReadOnlyList<Fournisseur> fournisseurs,
        IReadOnlyList<Produit> produits,
        IReadOnlyList<int> modePayementCodes,
        IReadOnlyDictionary<int, double> tvaByCode)
    {
        var commandes = new List<CommandeAchat>();

        for (int i = 1; i <= 8; i++)
        {
            var date = RandomDate(rng, 120, 25);
            var fournisseur = Pick(rng, fournisseurs);
            var prepared = PreparePurchaseLines(rng, produits, tvaByCode, 2, 4);

            var commande = new CommandeAchat
            {
                NumeroCommandeAchat = MakeNumero("MCA", date, 900 + i),
                DateCommandeAchat = date,
                CodeFournisseur = fournisseur.CodeFournisseur,
                EtatCommandeAchat = Pick(rng, new[] { "Ouvert", "Confirmé" }),
                EtatReception = Pick(rng, new[] { "Non Reçu", "Partiellement Reçu", "Reçu" }),
                Note = "Commande achat mock"
            };

            var ht = prepared.Sum(l => l.MontantHT);
            var tva = prepared.Sum(l => l.MontantHT * l.Tva / 100.0);

            commande.MontantHT = R3(ht);
            commande.MontantTVA = R3(tva);
            commande.MontantTTC = R3(ht + tva);
            commande.Lignes = prepared.Select(l => new LigneCommandeAchat
            {
                NumeroCommandeAchat = commande.NumeroCommandeAchat,
                CodeProduit = l.CodeProduit,
                Quantite = l.Quantite,
                PrixUnitaire = l.PrixUnitaire,
                Tva = l.Tva,
                MontantHT = l.MontantHT
            }).ToList();

            commandes.Add(commande);
            db.CommandesAchat.Add(commande);
        }

        for (int i = 1; i <= 7; i++)
        {
            var date = RandomDate(rng, 90, 12);
            var fournisseur = Pick(rng, fournisseurs);
            var prepared = PreparePurchaseLines(rng, produits, tvaByCode, 2, 4);
            var commandeLiee = commandes.Where(c => c.CodeFournisseur == fournisseur.CodeFournisseur).OrderBy(_ => rng.Next()).FirstOrDefault();

            var bon = new BonReception
            {
                NumeroBonReception = MakeNumero("MBR", date, 900 + i),
                DateBonReception = date,
                CodeFournisseur = fournisseur.CodeFournisseur,
                NumeroCommandeAchat = commandeLiee?.NumeroCommandeAchat,
                EtatBonReception = Pick(rng, new[] { "Ouvert", "Confirmé" }),
                EtatFacture = Pick(rng, new[] { "Non Facturé", "Facturé" }),
                Note = "Bon de reception mock"
            };

            var ht = prepared.Sum(l => l.MontantHT);
            var tva = prepared.Sum(l => l.MontantHT * l.Tva / 100.0);

            bon.MontantHT = R3(ht);
            bon.MontantTVA = R3(tva);
            bon.MontantTTC = R3(ht + tva);
            bon.Lignes = prepared.Select(l => new LigneBonReception
            {
                NumeroBonReception = bon.NumeroBonReception,
                CodeProduit = l.CodeProduit,
                Quantite = l.Quantite,
                PrixUnitaire = l.PrixUnitaire,
                Tva = l.Tva,
                MontantHT = l.MontantHT
            }).ToList();

            foreach (var line in prepared)
                line.Produit.Quantite = R3(line.Produit.Quantite + line.Quantite);

            db.BonsReception.Add(bon);
        }

        for (int i = 1; i <= 8; i++)
        {
            var date = RandomDate(rng, 70, 1);
            var fournisseur = Pick(rng, fournisseurs);
            var prepared = PreparePurchaseLines(rng, produits, tvaByCode, 2, 5);

            var facture = new FactureFournisseur
            {
                NumeroFactureFournisseur = MakeNumero("MFF", date, 900 + i),
                DateFactureFournisseur = date,
                CodeFournisseur = fournisseur.CodeFournisseur,
                Timbre = TimbreFiscal,
                EtatFacture = "Facture Ouverte",
                Note = "Facture fournisseur mock"
            };

            var ht = prepared.Sum(l => l.MontantHT);
            var tva = prepared.Sum(l => l.MontantHT * l.Tva / 100.0);

            facture.MontantHT = R3(ht);
            facture.MontantTVA = R3(tva);
            facture.MontantTTC = R3(ht + tva);
            facture.Lignes = prepared.Select(l => new LigneFactureFournisseur
            {
                NumeroFactureFournisseur = facture.NumeroFactureFournisseur,
                CodeProduit = l.CodeProduit,
                Quantite = l.Quantite,
                PrixUnitaire = l.PrixUnitaire,
                Tva = l.Tva,
                MontantHT = l.MontantHT
            }).ToList();

            foreach (var line in prepared)
                line.Produit.Quantite = R3(line.Produit.Quantite + line.Quantite);

            var totalDue = facture.MontantTTC + facture.Timbre;
            var etatReglement = PickReglementEtat(rng);
            facture.EtatReglement = etatReglement;
            facture.Reglements = BuildReglementsFournisseur(rng, facture, totalDue, etatReglement, modePayementCodes);

            db.FacturesFournisseur.Add(facture);
        }

        return (8, 7, 8);
    }

    private static List<PreparedLine> PrepareSalesLines(
        Random rng,
        IReadOnlyList<Produit> produits,
        IReadOnlyDictionary<int, double> tvaByCode,
        int minLines,
        int maxLines,
        bool includeFodec)
    {
        var selected = PickDistinctProduits(rng, produits, minLines, maxLines);
        var lines = new List<PreparedLine>(selected.Count);

        foreach (var produit in selected)
        {
            var qty = rng.Next(1, 8);
            var remiseLigne = rng.Next(0, (int)Math.Max(2, Math.Min(8, produit.RemiseMaximale + 1)));
            var tva = tvaByCode.TryGetValue(produit.CodeTvaProduit, out var taux) ? taux : 19;
            var lineHT = R3(qty * produit.PrixVenteHT * (1 - remiseLigne / 100.0));

            lines.Add(new PreparedLine
            {
                Produit = produit,
                CodeProduit = produit.CodeProduit,
                Quantite = qty,
                PrixUnitaire = produit.PrixVenteHT,
                Remise = remiseLigne,
                Tva = tva,
                Fodec = includeFodec ? produit.Fodec : 0,
                MontantHT = lineHT
            });
        }

        return lines;
    }

    private static List<PreparedLine> PreparePurchaseLines(
        Random rng,
        IReadOnlyList<Produit> produits,
        IReadOnlyDictionary<int, double> tvaByCode,
        int minLines,
        int maxLines)
    {
        var selected = PickDistinctProduits(rng, produits, minLines, maxLines);
        var lines = new List<PreparedLine>(selected.Count);

        foreach (var produit in selected)
        {
            var qty = rng.Next(2, 10);
            var tva = tvaByCode.TryGetValue(produit.CodeTvaProduit, out var taux) ? taux : 19;
            var prixHt = R3(produit.PrixAchatTTC / (1 + tva / 100.0));
            var lineHT = R3(qty * prixHt);

            lines.Add(new PreparedLine
            {
                Produit = produit,
                CodeProduit = produit.CodeProduit,
                Quantite = qty,
                PrixUnitaire = prixHt,
                Remise = 0,
                Tva = tva,
                Fodec = 0,
                MontantHT = lineHT
            });
        }

        return lines;
    }

    private static List<ReglementFactureClient> BuildReglementsClient(
        Random rng,
        FactureClient facture,
        double totalDue,
        string etatReglement,
        IReadOnlyList<int> modePayementCodes)
    {
        if (etatReglement == "Non Réglé")
            return [];

        var reglements = new List<ReglementFactureClient>();

        if (etatReglement == "Réglé")
        {
            if (rng.NextDouble() < 0.5)
            {
                var first = R3(totalDue * 0.6);
                var second = R3(totalDue - first);
                reglements.Add(NewReglementClient(rng, facture, first, modePayementCodes, "REG-C-1"));
                reglements.Add(NewReglementClient(rng, facture, second, modePayementCodes, "REG-C-2"));
            }
            else
            {
                reglements.Add(NewReglementClient(rng, facture, R3(totalDue), modePayementCodes, "REG-C-FULL"));
            }

            return reglements;
        }

        var partial = R3(totalDue * (0.35 + rng.NextDouble() * 0.35));
        reglements.Add(NewReglementClient(rng, facture, partial, modePayementCodes, "REG-C-PART"));
        return reglements;
    }

    private static List<ReglementFactureFournisseur> BuildReglementsFournisseur(
        Random rng,
        FactureFournisseur facture,
        double totalDue,
        string etatReglement,
        IReadOnlyList<int> modePayementCodes)
    {
        if (etatReglement == "Non Réglé")
            return [];

        var reglements = new List<ReglementFactureFournisseur>();

        if (etatReglement == "Réglé")
        {
            if (rng.NextDouble() < 0.5)
            {
                var first = R3(totalDue * 0.55);
                var second = R3(totalDue - first);
                reglements.Add(NewReglementFournisseur(rng, facture, first, modePayementCodes, "REG-F-1"));
                reglements.Add(NewReglementFournisseur(rng, facture, second, modePayementCodes, "REG-F-2"));
            }
            else
            {
                reglements.Add(NewReglementFournisseur(rng, facture, R3(totalDue), modePayementCodes, "REG-F-FULL"));
            }

            return reglements;
        }

        var partial = R3(totalDue * (0.30 + rng.NextDouble() * 0.40));
        reglements.Add(NewReglementFournisseur(rng, facture, partial, modePayementCodes, "REG-F-PART"));
        return reglements;
    }

    private static ReglementFactureClient NewReglementClient(
        Random rng,
        FactureClient facture,
        double montant,
        IReadOnlyList<int> modePayementCodes,
        string reference)
    {
        return new ReglementFactureClient
        {
            NumeroFactureClient = facture.NumeroFactureClient,
            DateReglement = facture.DateFactureClient.AddDays(rng.Next(1, 20)),
            Montant = montant,
            CodeModePayement = Pick(rng, modePayementCodes),
            Reference = reference,
            Note = "Reglement mock"
        };
    }

    private static ReglementFactureFournisseur NewReglementFournisseur(
        Random rng,
        FactureFournisseur facture,
        double montant,
        IReadOnlyList<int> modePayementCodes,
        string reference)
    {
        return new ReglementFactureFournisseur
        {
            NumeroFactureFournisseur = facture.NumeroFactureFournisseur,
            DateReglement = facture.DateFactureFournisseur.AddDays(rng.Next(1, 20)),
            Montant = montant,
            CodeModePayement = Pick(rng, modePayementCodes),
            Reference = reference
        };
    }

    private static string PickReglementEtat(Random rng)
    {
        var roll = rng.NextDouble();
        if (roll < 0.35)
            return "Non Réglé";
        if (roll < 0.75)
            return "Partiellement Réglé";
        return "Réglé";
    }

    private static List<Produit> PickDistinctProduits(Random rng, IReadOnlyList<Produit> produits, int min, int max)
    {
        var count = rng.Next(min, max + 1);
        return produits
            .OrderBy(_ => rng.Next())
            .Take(count)
            .ToList();
    }

    private static DateTime RandomDate(Random rng, int maxDaysBack, int minDaysBack)
    {
        var days = rng.Next(minDaysBack, maxDaysBack + 1);
        return DateTime.Today.AddDays(-days);
    }

    private static string MakeNumero(string prefix, DateTime date, int sequence)
        => $"{prefix}{date:yyyyMM}{sequence:000}";

    private static double R3(double value)
        => Math.Round(value, 3, MidpointRounding.AwayFromZero);

    private static T Pick<T>(Random rng, IReadOnlyList<T> values)
        => values[rng.Next(values.Count)];

    private sealed class PreparedLine
    {
        public required Produit Produit { get; init; }
        public required string CodeProduit { get; init; }
        public double Quantite { get; init; }
        public double PrixUnitaire { get; init; }
        public double Remise { get; init; }
        public double Tva { get; init; }
        public double Fodec { get; init; }
        public double MontantHT { get; init; }
    }
}
