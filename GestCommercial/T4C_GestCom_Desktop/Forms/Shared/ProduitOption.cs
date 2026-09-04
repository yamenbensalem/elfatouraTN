namespace T4C_GestCom_Desktop.Forms.Shared;

/// <summary>
/// Lightweight row for a Produit ComboBox DataSource. A named type instead of an anonymous
/// `new { p.CodeProduit, p.DesignationProduit }` deliberately — WinForms' DisplayMember/ValueMember
/// binding resolves "CodeProduit"/"DesignationProduit" by reflection at runtime, and an anonymous
/// type's compiler-generated properties get renamed by obfuscation tools (breaking that lookup)
/// where a named, explicitly-excludable type does not.
/// </summary>
public sealed record ProduitOption(string CodeProduit, string DesignationProduit);
