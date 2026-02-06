using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestCom.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodeEntreprise = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Actif = table.Column<bool>(type: "bit", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DerniereConnexion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Devise",
                columns: table => new
                {
                    code_devise = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    symbole = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    code_iso = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    taux_change = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    devise_principale = table.Column<bool>(type: "bit", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devise", x => x.code_devise);
                });

            migrationBuilder.CreateTable(
                name: "DouaneProduit",
                columns: table => new
                {
                    code_douane = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    designation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    numero_hs = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DouaneProduit", x => x.code_douane);
                });

            migrationBuilder.CreateTable(
                name: "FabriquantProduit",
                columns: table => new
                {
                    code_fabriquant = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    designation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    pays = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    site_web = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabriquantProduit", x => x.code_fabriquant);
                });

            migrationBuilder.CreateTable(
                name: "Licence",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code_entreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    cle_licence = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    date_debut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_fin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    type_licence = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    nombre_utilisateurs = table.Column<int>(type: "int", nullable: false),
                    actif = table.Column<bool>(type: "bit", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Licence", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ModePayement",
                columns: table => new
                {
                    code_mode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    designation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    actif = table.Column<bool>(type: "bit", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModePayement", x => x.code_mode);
                });

            migrationBuilder.CreateTable(
                name: "ParametresDecimales",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre_decimales_quantite = table.Column<int>(type: "int", nullable: false),
                    nombre_decimales_prix = table.Column<int>(type: "int", nullable: false),
                    nombre_decimales_montant = table.Column<int>(type: "int", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParametresDecimales", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "PathLogo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    chemin_logo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    chemin_rapport = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PathLogo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "PaysProduit",
                columns: table => new
                {
                    code_pays = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    designation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    code_iso = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaysProduit", x => x.code_pays);
                });

            migrationBuilder.CreateTable(
                name: "RetenuSource",
                columns: table => new
                {
                    code_retenu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    designation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    taux = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetenuSource", x => x.code_retenu);
                });

            migrationBuilder.CreateTable(
                name: "TvaProduit",
                columns: table => new
                {
                    code_tva = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    designation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    taux = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    par_defaut = table.Column<bool>(type: "bit", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TvaProduit", x => x.code_tva);
                });

            migrationBuilder.CreateTable(
                name: "UniteProduit",
                columns: table => new
                {
                    code_unite = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    designation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    symbole = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniteProduit", x => x.code_unite);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expires = table.Column<DateTime>(type: "datetime2", nullable: false),
                    revoked = table.Column<DateTime>(type: "datetime2", nullable: true),
                    replaced_by_token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    reason_revoked = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Entreprise",
                columns: table => new
                {
                    codeentreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    matriculeFiscale_entreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    raisonSociale_entreprise = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    nomCommercial_entreprise = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    typePersonne_entreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    comptebancaire_entreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    registrecommerce_entreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    capitalSocial_entreprise = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    description_entreprise = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    adresse_entreprise = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    codepostal_entreprise = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ville_entreprise = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    pays_entreprise = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    telfixe1_entreprise = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    telfixe2_entreprise = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    telmobile_entreprise = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    fax_entreprise = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    email_entreprise = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    site_entreprise = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    matricule_entreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    codeTVA_entreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    codecatego_entreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    numetab_entreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    assujittieTVA_entreprise = table.Column<bool>(type: "bit", nullable: false),
                    assujittieFodec_entreprise = table.Column<bool>(type: "bit", nullable: false),
                    exonore_entreprise = table.Column<bool>(type: "bit", nullable: false),
                    codedouane_entreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    code_devise = table.Column<int>(type: "int", nullable: false),
                    logo_entreprise = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RIB = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomBanque = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entreprise", x => x.codeentreprise);
                    table.ForeignKey(
                        name: "FK_Entreprise_Devise_code_devise",
                        column: x => x.code_devise,
                        principalTable: "Devise",
                        principalColumn: "code_devise",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategorieProduit",
                columns: table => new
                {
                    code_categorie = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code_entreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    designation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    categorie_parent_id = table.Column<int>(type: "int", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategorieProduit", x => x.code_categorie);
                    table.ForeignKey(
                        name: "FK_CategorieProduit_CategorieProduit_categorie_parent_id",
                        column: x => x.categorie_parent_id,
                        principalTable: "CategorieProduit",
                        principalColumn: "code_categorie",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CategorieProduit_Entreprise_code_entreprise",
                        column: x => x.code_entreprise,
                        principalTable: "Entreprise",
                        principalColumn: "codeentreprise",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Client",
                columns: table => new
                {
                    code_client = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    codeentreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    matriculefiscale_client = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    nom_client = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    typepersonne_client = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    typeentreprise_client = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    rib_client = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    adresse_client = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    codepostal_client = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ville_client = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    pays_client = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    tel_client = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    telmobile_client = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    fax_client = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    email_client = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    site_client = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    etat_client = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    nbtransactions_client = table.Column<int>(type: "int", nullable: false),
                    note_client = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    etranger_client = table.Column<bool>(type: "bit", nullable: false),
                    exonore_client = table.Column<bool>(type: "bit", nullable: false),
                    maxcredit_client = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    CreditMaximum = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoumisRAS = table.Column<bool>(type: "bit", nullable: false),
                    code_devise = table.Column<int>(type: "int", nullable: false),
                    responsable_client = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client", x => x.code_client);
                    table.ForeignKey(
                        name: "FK_Client_Devise_code_devise",
                        column: x => x.code_devise,
                        principalTable: "Devise",
                        principalColumn: "code_devise",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Client_Entreprise_codeentreprise",
                        column: x => x.codeentreprise,
                        principalTable: "Entreprise",
                        principalColumn: "codeentreprise",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Fournisseur",
                columns: table => new
                {
                    CodeFournisseur = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeEntreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatriculeFiscale = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TypePersonne = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TypeEntreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RIB = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Adresse = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CodePostal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ville = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Pays = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Tel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TelMobile = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SiteWeb = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Etat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NombreTransactions = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Etranger = table.Column<bool>(type: "bit", nullable: false),
                    SoumisRAS = table.Column<bool>(type: "bit", nullable: false),
                    CodeDevise = table.Column<int>(type: "int", nullable: false),
                    Responsable = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fournisseur", x => x.CodeFournisseur);
                    table.ForeignKey(
                        name: "FK_Fournisseur_Devise_CodeDevise",
                        column: x => x.CodeDevise,
                        principalTable: "Devise",
                        principalColumn: "code_devise",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fournisseur_Entreprise_CodeEntreprise",
                        column: x => x.CodeEntreprise,
                        principalTable: "Entreprise",
                        principalColumn: "codeentreprise",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MagasinProduit",
                columns: table => new
                {
                    code_magasin = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code_entreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    designation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    adresse = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ville = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    responsable = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    principal = table.Column<bool>(type: "bit", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MagasinProduit", x => x.code_magasin);
                    table.ForeignKey(
                        name: "FK_MagasinProduit_Entreprise_code_entreprise",
                        column: x => x.code_entreprise,
                        principalTable: "Entreprise",
                        principalColumn: "codeentreprise",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DevisClient",
                columns: table => new
                {
                    NumeroDevis = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeEntreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeClient = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateDevis = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEcheance = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateValidite = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remise = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TauxRemise = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TauxRemiseGlobale = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantRemise = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Timbre = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CodeDevise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TauxChange = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NumeroCommande = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MontantHT = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTVA = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTTC = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevisClient", x => x.NumeroDevis);
                    table.ForeignKey(
                        name: "FK_DevisClient_Client_CodeClient",
                        column: x => x.CodeClient,
                        principalTable: "Client",
                        principalColumn: "code_client",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DevisClient_Entreprise_CodeEntreprise",
                        column: x => x.CodeEntreprise,
                        principalTable: "Entreprise",
                        principalColumn: "codeentreprise",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FactureClient",
                columns: table => new
                {
                    numero_factureclient = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    codeentreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    codeclient_factureclient = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    date_factureclient = table.Column<DateTime>(type: "datetime2", nullable: false),
                    dateecheance_factureclient = table.Column<DateTime>(type: "datetime2", nullable: true),
                    remise_factureclient = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    TauxRemise = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TauxRemiseGlobale = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    montantHT_factureclient = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    montantTVA_factureclient = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    timbre_factureclient = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    montantTTC_factureclient = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    apayer_factureclient = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    montantApresRAS_factureclient = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    montantRestant_factureclient = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    MontantRegle = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontantFodec = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontantRemise = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TauxRAS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontantRAS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetAPayer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    origine_factureclient = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    statut_factureclient = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    modepayement_factureclient = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CodeModePaiement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodeDevise = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TauxChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NumeroBonLivraison = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumeroCommande = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    avoir_factureclient = table.Column<bool>(type: "bit", nullable: false),
                    notes_factureclient = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModePayementNavigationCodeMode = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    DeviseNavigationCodeDevise = table.Column<int>(type: "int", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactureClient", x => x.numero_factureclient);
                    table.ForeignKey(
                        name: "FK_FactureClient_Client_codeclient_factureclient",
                        column: x => x.codeclient_factureclient,
                        principalTable: "Client",
                        principalColumn: "code_client",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FactureClient_Devise_DeviseNavigationCodeDevise",
                        column: x => x.DeviseNavigationCodeDevise,
                        principalTable: "Devise",
                        principalColumn: "code_devise");
                    table.ForeignKey(
                        name: "FK_FactureClient_Entreprise_codeentreprise",
                        column: x => x.codeentreprise,
                        principalTable: "Entreprise",
                        principalColumn: "codeentreprise",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FactureClient_ModePayement_ModePayementNavigationCodeMode",
                        column: x => x.ModePayementNavigationCodeMode,
                        principalTable: "ModePayement",
                        principalColumn: "code_mode");
                });

            migrationBuilder.CreateTable(
                name: "CommandeAchat",
                columns: table => new
                {
                    NumeroCommande = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeEntreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeFournisseur = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateCommande = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateLivraison = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateLivraisonPrevue = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NumeroDemandePrix = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NumeroBonReception = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remise = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TauxRemise = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CodeDevise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TauxChange = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MontantHT = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTVA = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTTC = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandeAchat", x => x.NumeroCommande);
                    table.ForeignKey(
                        name: "FK_CommandeAchat_Entreprise_CodeEntreprise",
                        column: x => x.CodeEntreprise,
                        principalTable: "Entreprise",
                        principalColumn: "codeentreprise",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommandeAchat_Fournisseur_CodeFournisseur",
                        column: x => x.CodeFournisseur,
                        principalTable: "Fournisseur",
                        principalColumn: "CodeFournisseur",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DemandePrix",
                columns: table => new
                {
                    NumeroDemande = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeEntreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeFournisseur = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateDemande = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEcheance = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Statut = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandePrix", x => x.NumeroDemande);
                    table.ForeignKey(
                        name: "FK_DemandePrix_Entreprise_CodeEntreprise",
                        column: x => x.CodeEntreprise,
                        principalTable: "Entreprise",
                        principalColumn: "codeentreprise",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DemandePrix_Fournisseur_CodeFournisseur",
                        column: x => x.CodeFournisseur,
                        principalTable: "Fournisseur",
                        principalColumn: "CodeFournisseur",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FactureFournisseur",
                columns: table => new
                {
                    NumeroFacture = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeEntreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeFournisseur = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateFacture = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEcheance = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remise = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TauxRemiseGlobale = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantRemise = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    NumeroBonReception = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Observation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MontantHT = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTVA = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Timbre = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTTC = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    APayer = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantRegle = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantRestant = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantFodec = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TauxRAS = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantRAS = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    NetAPayer = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModePayement = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NumeroFactureFournisseur = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModePayementNavigationCodeMode = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactureFournisseur", x => x.NumeroFacture);
                    table.ForeignKey(
                        name: "FK_FactureFournisseur_Entreprise_CodeEntreprise",
                        column: x => x.CodeEntreprise,
                        principalTable: "Entreprise",
                        principalColumn: "codeentreprise",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FactureFournisseur_Fournisseur_CodeFournisseur",
                        column: x => x.CodeFournisseur,
                        principalTable: "Fournisseur",
                        principalColumn: "CodeFournisseur",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FactureFournisseur_ModePayement_ModePayementNavigationCodeMode",
                        column: x => x.ModePayementNavigationCodeMode,
                        principalTable: "ModePayement",
                        principalColumn: "code_mode");
                });

            migrationBuilder.CreateTable(
                name: "Produit",
                columns: table => new
                {
                    code_produit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    codeentreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    designation_produit = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CodeBarre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    prixunitaire_produit = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    code_devise = table.Column<int>(type: "int", nullable: false),
                    prixachatTTC_produit = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    tauxmarge_produit = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    prixventeHT_produit = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    remise_produit = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    prixventeTTC_produit = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    fodec_produit = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    TauxTVA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TauxFODEC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CodeUnite = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodeTVA = table.Column<int>(type: "int", nullable: false),
                    quantite_produit = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    stockminimal_produit = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    remisemaximale_produit = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    rayon_produit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    etage_produit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    code_fournisseur = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    code_uniteproduit = table.Column<int>(type: "int", nullable: true),
                    code_tvaproduit = table.Column<int>(type: "int", nullable: true),
                    code_categorieproduit = table.Column<int>(type: "int", nullable: true),
                    CodeCategorie = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    code_magasinproduit = table.Column<int>(type: "int", nullable: true),
                    CodeMagasin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    code_fabriquantproduit = table.Column<int>(type: "int", nullable: true),
                    code_paysproduit = table.Column<int>(type: "int", nullable: true),
                    code_douaneproduit = table.Column<int>(type: "int", nullable: true),
                    DeviseCodeDevise = table.Column<int>(type: "int", nullable: true),
                    FabriquantProduitCodeFabriquant = table.Column<int>(type: "int", nullable: true),
                    PaysProduitCodePays = table.Column<int>(type: "int", nullable: true),
                    DouaneProduitCodeDouane = table.Column<int>(type: "int", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produit", x => x.code_produit);
                    table.ForeignKey(
                        name: "FK_Produit_CategorieProduit_code_categorieproduit",
                        column: x => x.code_categorieproduit,
                        principalTable: "CategorieProduit",
                        principalColumn: "code_categorie",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Produit_Devise_DeviseCodeDevise",
                        column: x => x.DeviseCodeDevise,
                        principalTable: "Devise",
                        principalColumn: "code_devise");
                    table.ForeignKey(
                        name: "FK_Produit_DouaneProduit_DouaneProduitCodeDouane",
                        column: x => x.DouaneProduitCodeDouane,
                        principalTable: "DouaneProduit",
                        principalColumn: "code_douane");
                    table.ForeignKey(
                        name: "FK_Produit_Entreprise_codeentreprise",
                        column: x => x.codeentreprise,
                        principalTable: "Entreprise",
                        principalColumn: "codeentreprise",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Produit_FabriquantProduit_FabriquantProduitCodeFabriquant",
                        column: x => x.FabriquantProduitCodeFabriquant,
                        principalTable: "FabriquantProduit",
                        principalColumn: "code_fabriquant");
                    table.ForeignKey(
                        name: "FK_Produit_Fournisseur_code_fournisseur",
                        column: x => x.code_fournisseur,
                        principalTable: "Fournisseur",
                        principalColumn: "CodeFournisseur",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Produit_MagasinProduit_code_magasinproduit",
                        column: x => x.code_magasinproduit,
                        principalTable: "MagasinProduit",
                        principalColumn: "code_magasin",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Produit_PaysProduit_PaysProduitCodePays",
                        column: x => x.PaysProduitCodePays,
                        principalTable: "PaysProduit",
                        principalColumn: "code_pays");
                    table.ForeignKey(
                        name: "FK_Produit_TvaProduit_code_tvaproduit",
                        column: x => x.code_tvaproduit,
                        principalTable: "TvaProduit",
                        principalColumn: "code_tva",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Produit_UniteProduit_code_uniteproduit",
                        column: x => x.code_uniteproduit,
                        principalTable: "UniteProduit",
                        principalColumn: "code_unite",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CommandeVente",
                columns: table => new
                {
                    NumeroCommande = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeEntreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeClient = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateCommande = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateLivraison = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateLivraisonPrevue = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdresseLivraison = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Remise = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TauxRemise = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TauxRemiseGlobale = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantRemise = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CodeDevise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TauxChange = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Observation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MontantHT = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTVA = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTTC = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NumeroDevis = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NumeroBonLivraison = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandeVente", x => x.NumeroCommande);
                    table.ForeignKey(
                        name: "FK_CommandeVente_Client_CodeClient",
                        column: x => x.CodeClient,
                        principalTable: "Client",
                        principalColumn: "code_client",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommandeVente_DevisClient_NumeroDevis",
                        column: x => x.NumeroDevis,
                        principalTable: "DevisClient",
                        principalColumn: "NumeroDevis",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CommandeVente_Entreprise_CodeEntreprise",
                        column: x => x.CodeEntreprise,
                        principalTable: "Entreprise",
                        principalColumn: "codeentreprise",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReglementFacture",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code_entreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    numero_facture = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    code_client = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    date_reglement = table.Column<DateTime>(type: "datetime2", nullable: false),
                    montant = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    mode_payement = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    numero_transaction = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ClientCodeClient = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglementFacture", x => x.id);
                    table.ForeignKey(
                        name: "FK_ReglementFacture_Client_ClientCodeClient",
                        column: x => x.ClientCodeClient,
                        principalTable: "Client",
                        principalColumn: "code_client");
                    table.ForeignKey(
                        name: "FK_ReglementFacture_Client_code_client",
                        column: x => x.code_client,
                        principalTable: "Client",
                        principalColumn: "code_client",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReglementFacture_Entreprise_code_entreprise",
                        column: x => x.code_entreprise,
                        principalTable: "Entreprise",
                        principalColumn: "codeentreprise",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReglementFacture_FactureClient_numero_facture",
                        column: x => x.numero_facture,
                        principalTable: "FactureClient",
                        principalColumn: "numero_factureclient",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BonReception",
                columns: table => new
                {
                    NumeroBon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeEntreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeFournisseur = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateReception = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MontantHT = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTVA = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTTC = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NumeroCommande = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NumeroFacture = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonReception", x => x.NumeroBon);
                    table.ForeignKey(
                        name: "FK_BonReception_CommandeAchat_NumeroCommande",
                        column: x => x.NumeroCommande,
                        principalTable: "CommandeAchat",
                        principalColumn: "NumeroCommande",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BonReception_Entreprise_CodeEntreprise",
                        column: x => x.CodeEntreprise,
                        principalTable: "Entreprise",
                        principalColumn: "codeentreprise",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BonReception_Fournisseur_CodeFournisseur",
                        column: x => x.CodeFournisseur,
                        principalTable: "Fournisseur",
                        principalColumn: "CodeFournisseur",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReglementFournisseur",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodeEntreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumeroFacture = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeFournisseur = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateReglement = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Montant = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ModePayement = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumeroTransaction = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglementFournisseur", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReglementFournisseur_Entreprise_CodeEntreprise",
                        column: x => x.CodeEntreprise,
                        principalTable: "Entreprise",
                        principalColumn: "codeentreprise",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReglementFournisseur_FactureFournisseur_NumeroFacture",
                        column: x => x.NumeroFacture,
                        principalTable: "FactureFournisseur",
                        principalColumn: "NumeroFacture",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReglementFournisseur_Fournisseur_CodeFournisseur",
                        column: x => x.CodeFournisseur,
                        principalTable: "Fournisseur",
                        principalColumn: "CodeFournisseur",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LigneCommandeAchat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroLigne = table.Column<int>(type: "int", nullable: false),
                    NumeroCommande = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeProduit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Quantite = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    QuantiteRecue = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    PrixUnitaireHT = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Remise = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantHT = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TauxTVA = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTVA = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTTC = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ProduitCodeProduit = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LigneCommandeAchat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LigneCommandeAchat_CommandeAchat_NumeroCommande",
                        column: x => x.NumeroCommande,
                        principalTable: "CommandeAchat",
                        principalColumn: "NumeroCommande",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LigneCommandeAchat_Produit_CodeProduit",
                        column: x => x.CodeProduit,
                        principalTable: "Produit",
                        principalColumn: "code_produit",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LigneCommandeAchat_Produit_ProduitCodeProduit",
                        column: x => x.ProduitCodeProduit,
                        principalTable: "Produit",
                        principalColumn: "code_produit");
                });

            migrationBuilder.CreateTable(
                name: "LigneDemandePrix",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroDemande = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeProduit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Quantite = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    PrixPropose = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LigneDemandePrix", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LigneDemandePrix_DemandePrix_NumeroDemande",
                        column: x => x.NumeroDemande,
                        principalTable: "DemandePrix",
                        principalColumn: "NumeroDemande",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LigneDemandePrix_Produit_CodeProduit",
                        column: x => x.CodeProduit,
                        principalTable: "Produit",
                        principalColumn: "code_produit",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LigneDevisClient",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroLigne = table.Column<int>(type: "int", nullable: false),
                    NumeroDevis = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeProduit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Quantite = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    PrixUnitaireHT = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Remise = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TauxRemise = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantRemise = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantHT = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TauxTVA = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTVA = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TauxFodec = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantFodec = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTTC = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ProduitCodeProduit = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LigneDevisClient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LigneDevisClient_DevisClient_NumeroDevis",
                        column: x => x.NumeroDevis,
                        principalTable: "DevisClient",
                        principalColumn: "NumeroDevis",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LigneDevisClient_Produit_CodeProduit",
                        column: x => x.CodeProduit,
                        principalTable: "Produit",
                        principalColumn: "code_produit",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LigneDevisClient_Produit_ProduitCodeProduit",
                        column: x => x.ProduitCodeProduit,
                        principalTable: "Produit",
                        principalColumn: "code_produit");
                });

            migrationBuilder.CreateTable(
                name: "LigneFactureClient",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroLigne = table.Column<int>(type: "int", nullable: false),
                    numero_factureclient = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    code_produit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    designation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    quantite = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    prixunitaire = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    PrixUnitaireHT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    remise = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    TauxRemise = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontantRemise = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TauxFodec = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontantFodec = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    montantHT = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    tauxTVA = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    montantTVA = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    montantTTC = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LigneFactureClient", x => x.id);
                    table.ForeignKey(
                        name: "FK_LigneFactureClient_FactureClient_numero_factureclient",
                        column: x => x.numero_factureclient,
                        principalTable: "FactureClient",
                        principalColumn: "numero_factureclient",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LigneFactureClient_Produit_code_produit",
                        column: x => x.code_produit,
                        principalTable: "Produit",
                        principalColumn: "code_produit",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LigneFactureFournisseur",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroFacture = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeProduit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Quantite = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    PrixUnitaireHT = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Remise = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TauxRemise = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TauxFodec = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantFodec = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantHT = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TauxTVA = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTVA = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTTC = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ProduitCodeProduit = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LigneFactureFournisseur", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LigneFactureFournisseur_FactureFournisseur_NumeroFacture",
                        column: x => x.NumeroFacture,
                        principalTable: "FactureFournisseur",
                        principalColumn: "NumeroFacture",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LigneFactureFournisseur_Produit_CodeProduit",
                        column: x => x.CodeProduit,
                        principalTable: "Produit",
                        principalColumn: "code_produit",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LigneFactureFournisseur_Produit_ProduitCodeProduit",
                        column: x => x.ProduitCodeProduit,
                        principalTable: "Produit",
                        principalColumn: "code_produit");
                });

            migrationBuilder.CreateTable(
                name: "BonsLivraison",
                columns: table => new
                {
                    NumeroBon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeEntreprise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeClient = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateLivraison = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MontantHT = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTVA = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTTC = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Observations = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AdresseLivraison = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NumeroCommande = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NumeroFacture = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Facture = table.Column<bool>(type: "bit", nullable: false),
                    EntrepriseCodeEntreprise = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonsLivraison", x => x.NumeroBon);
                    table.ForeignKey(
                        name: "FK_BonsLivraison_Client_CodeClient",
                        column: x => x.CodeClient,
                        principalTable: "Client",
                        principalColumn: "code_client",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BonsLivraison_CommandeVente_NumeroCommande",
                        column: x => x.NumeroCommande,
                        principalTable: "CommandeVente",
                        principalColumn: "NumeroCommande",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BonsLivraison_Entreprise_EntrepriseCodeEntreprise",
                        column: x => x.EntrepriseCodeEntreprise,
                        principalTable: "Entreprise",
                        principalColumn: "codeentreprise");
                });

            migrationBuilder.CreateTable(
                name: "LigneCommandeVente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroLigne = table.Column<int>(type: "int", nullable: false),
                    NumeroCommande = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeProduit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Quantite = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    QuantiteLivree = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    PrixUnitaireHT = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Remise = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TauxRemise = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantRemise = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TauxFodec = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantFodec = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantHT = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TauxTVA = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTVA = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTTC = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ProduitCodeProduit = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LigneCommandeVente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LigneCommandeVente_CommandeVente_NumeroCommande",
                        column: x => x.NumeroCommande,
                        principalTable: "CommandeVente",
                        principalColumn: "NumeroCommande",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LigneCommandeVente_Produit_CodeProduit",
                        column: x => x.CodeProduit,
                        principalTable: "Produit",
                        principalColumn: "code_produit",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LigneCommandeVente_Produit_ProduitCodeProduit",
                        column: x => x.ProduitCodeProduit,
                        principalTable: "Produit",
                        principalColumn: "code_produit");
                });

            migrationBuilder.CreateTable(
                name: "LigneBonReception",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroLigne = table.Column<int>(type: "int", nullable: false),
                    NumeroBon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeProduit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Quantite = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Remise = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantHT = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TauxTVA = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTVA = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTTC = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ProduitCodeProduit = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LigneBonReception", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LigneBonReception_BonReception_NumeroBon",
                        column: x => x.NumeroBon,
                        principalTable: "BonReception",
                        principalColumn: "NumeroBon",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LigneBonReception_Produit_CodeProduit",
                        column: x => x.CodeProduit,
                        principalTable: "Produit",
                        principalColumn: "code_produit",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LigneBonReception_Produit_ProduitCodeProduit",
                        column: x => x.ProduitCodeProduit,
                        principalTable: "Produit",
                        principalColumn: "code_produit");
                });

            migrationBuilder.CreateTable(
                name: "BonsLivraison_Factures",
                columns: table => new
                {
                    NumeroBon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumeroFacture = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonsLivraison_Factures", x => new { x.NumeroBon, x.NumeroFacture });
                    table.ForeignKey(
                        name: "FK_BonsLivraison_Factures_BonsLivraison_NumeroBon",
                        column: x => x.NumeroBon,
                        principalTable: "BonsLivraison",
                        principalColumn: "NumeroBon",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BonsLivraison_Factures_FactureClient_NumeroFacture",
                        column: x => x.NumeroFacture,
                        principalTable: "FactureClient",
                        principalColumn: "numero_factureclient",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LignesBonLivraison",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroLigne = table.Column<int>(type: "int", nullable: false),
                    NumeroBon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumeroBonLivraison = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeProduit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Quantite = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    PrixUnitaireHT = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Remise = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantHT = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TauxTVA = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTVA = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantTTC = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TauxRemise = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TauxFodec = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MontantFodec = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ProduitCodeProduit = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilisateurCreation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilisateurModification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LignesBonLivraison", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LignesBonLivraison_BonsLivraison_NumeroBon",
                        column: x => x.NumeroBon,
                        principalTable: "BonsLivraison",
                        principalColumn: "NumeroBon",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LignesBonLivraison_Produit_CodeProduit",
                        column: x => x.CodeProduit,
                        principalTable: "Produit",
                        principalColumn: "code_produit",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LignesBonLivraison_Produit_ProduitCodeProduit",
                        column: x => x.ProduitCodeProduit,
                        principalTable: "Produit",
                        principalColumn: "code_produit");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BonReception_CodeEntreprise",
                table: "BonReception",
                column: "CodeEntreprise");

            migrationBuilder.CreateIndex(
                name: "IX_BonReception_CodeFournisseur",
                table: "BonReception",
                column: "CodeFournisseur");

            migrationBuilder.CreateIndex(
                name: "IX_BonReception_DateReception",
                table: "BonReception",
                column: "DateReception");

            migrationBuilder.CreateIndex(
                name: "IX_BonReception_NumeroCommande",
                table: "BonReception",
                column: "NumeroCommande");

            migrationBuilder.CreateIndex(
                name: "IX_BonsLivraison_CodeClient",
                table: "BonsLivraison",
                column: "CodeClient");

            migrationBuilder.CreateIndex(
                name: "IX_BonsLivraison_CodeEntreprise",
                table: "BonsLivraison",
                column: "CodeEntreprise");

            migrationBuilder.CreateIndex(
                name: "IX_BonsLivraison_DateLivraison",
                table: "BonsLivraison",
                column: "DateLivraison");

            migrationBuilder.CreateIndex(
                name: "IX_BonsLivraison_EntrepriseCodeEntreprise",
                table: "BonsLivraison",
                column: "EntrepriseCodeEntreprise");

            migrationBuilder.CreateIndex(
                name: "IX_BonsLivraison_NumeroCommande",
                table: "BonsLivraison",
                column: "NumeroCommande");

            migrationBuilder.CreateIndex(
                name: "IX_BonsLivraison_Factures_NumeroFacture",
                table: "BonsLivraison_Factures",
                column: "NumeroFacture");

            migrationBuilder.CreateIndex(
                name: "IX_CategorieProduit_categorie_parent_id",
                table: "CategorieProduit",
                column: "categorie_parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_CategorieProduit_code_entreprise",
                table: "CategorieProduit",
                column: "code_entreprise");

            migrationBuilder.CreateIndex(
                name: "IX_Client_code_devise",
                table: "Client",
                column: "code_devise");

            migrationBuilder.CreateIndex(
                name: "IX_Client_codeentreprise",
                table: "Client",
                column: "codeentreprise");

            migrationBuilder.CreateIndex(
                name: "IX_CommandeAchat_CodeEntreprise",
                table: "CommandeAchat",
                column: "CodeEntreprise");

            migrationBuilder.CreateIndex(
                name: "IX_CommandeAchat_CodeFournisseur",
                table: "CommandeAchat",
                column: "CodeFournisseur");

            migrationBuilder.CreateIndex(
                name: "IX_CommandeAchat_DateCommande",
                table: "CommandeAchat",
                column: "DateCommande");

            migrationBuilder.CreateIndex(
                name: "IX_CommandeVente_CodeClient",
                table: "CommandeVente",
                column: "CodeClient");

            migrationBuilder.CreateIndex(
                name: "IX_CommandeVente_CodeEntreprise",
                table: "CommandeVente",
                column: "CodeEntreprise");

            migrationBuilder.CreateIndex(
                name: "IX_CommandeVente_DateCommande",
                table: "CommandeVente",
                column: "DateCommande");

            migrationBuilder.CreateIndex(
                name: "IX_CommandeVente_NumeroDevis",
                table: "CommandeVente",
                column: "NumeroDevis");

            migrationBuilder.CreateIndex(
                name: "IX_DemandePrix_CodeEntreprise",
                table: "DemandePrix",
                column: "CodeEntreprise");

            migrationBuilder.CreateIndex(
                name: "IX_DemandePrix_CodeFournisseur",
                table: "DemandePrix",
                column: "CodeFournisseur");

            migrationBuilder.CreateIndex(
                name: "IX_DemandePrix_DateDemande",
                table: "DemandePrix",
                column: "DateDemande");

            migrationBuilder.CreateIndex(
                name: "IX_DevisClient_CodeClient",
                table: "DevisClient",
                column: "CodeClient");

            migrationBuilder.CreateIndex(
                name: "IX_DevisClient_CodeEntreprise",
                table: "DevisClient",
                column: "CodeEntreprise");

            migrationBuilder.CreateIndex(
                name: "IX_DevisClient_DateDevis",
                table: "DevisClient",
                column: "DateDevis");

            migrationBuilder.CreateIndex(
                name: "IX_Devise_code_iso",
                table: "Devise",
                column: "code_iso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entreprise_code_devise",
                table: "Entreprise",
                column: "code_devise");

            migrationBuilder.CreateIndex(
                name: "IX_FactureClient_codeclient_factureclient",
                table: "FactureClient",
                column: "codeclient_factureclient");

            migrationBuilder.CreateIndex(
                name: "IX_FactureClient_codeentreprise",
                table: "FactureClient",
                column: "codeentreprise");

            migrationBuilder.CreateIndex(
                name: "IX_FactureClient_DeviseNavigationCodeDevise",
                table: "FactureClient",
                column: "DeviseNavigationCodeDevise");

            migrationBuilder.CreateIndex(
                name: "IX_FactureClient_ModePayementNavigationCodeMode",
                table: "FactureClient",
                column: "ModePayementNavigationCodeMode");

            migrationBuilder.CreateIndex(
                name: "IX_FactureFournisseur_CodeEntreprise",
                table: "FactureFournisseur",
                column: "CodeEntreprise");

            migrationBuilder.CreateIndex(
                name: "IX_FactureFournisseur_CodeFournisseur",
                table: "FactureFournisseur",
                column: "CodeFournisseur");

            migrationBuilder.CreateIndex(
                name: "IX_FactureFournisseur_DateFacture",
                table: "FactureFournisseur",
                column: "DateFacture");

            migrationBuilder.CreateIndex(
                name: "IX_FactureFournisseur_ModePayementNavigationCodeMode",
                table: "FactureFournisseur",
                column: "ModePayementNavigationCodeMode");

            migrationBuilder.CreateIndex(
                name: "IX_Fournisseur_CodeDevise",
                table: "Fournisseur",
                column: "CodeDevise");

            migrationBuilder.CreateIndex(
                name: "IX_Fournisseur_CodeEntreprise",
                table: "Fournisseur",
                column: "CodeEntreprise");

            migrationBuilder.CreateIndex(
                name: "IX_Fournisseur_MatriculeFiscale",
                table: "Fournisseur",
                column: "MatriculeFiscale");

            migrationBuilder.CreateIndex(
                name: "IX_Licence_cle_licence",
                table: "Licence",
                column: "cle_licence",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Licence_code_entreprise",
                table: "Licence",
                column: "code_entreprise");

            migrationBuilder.CreateIndex(
                name: "IX_LigneBonReception_CodeProduit",
                table: "LigneBonReception",
                column: "CodeProduit");

            migrationBuilder.CreateIndex(
                name: "IX_LigneBonReception_NumeroBon",
                table: "LigneBonReception",
                column: "NumeroBon");

            migrationBuilder.CreateIndex(
                name: "IX_LigneBonReception_ProduitCodeProduit",
                table: "LigneBonReception",
                column: "ProduitCodeProduit");

            migrationBuilder.CreateIndex(
                name: "IX_LigneCommandeAchat_CodeProduit",
                table: "LigneCommandeAchat",
                column: "CodeProduit");

            migrationBuilder.CreateIndex(
                name: "IX_LigneCommandeAchat_NumeroCommande",
                table: "LigneCommandeAchat",
                column: "NumeroCommande");

            migrationBuilder.CreateIndex(
                name: "IX_LigneCommandeAchat_ProduitCodeProduit",
                table: "LigneCommandeAchat",
                column: "ProduitCodeProduit");

            migrationBuilder.CreateIndex(
                name: "IX_LigneCommandeVente_CodeProduit",
                table: "LigneCommandeVente",
                column: "CodeProduit");

            migrationBuilder.CreateIndex(
                name: "IX_LigneCommandeVente_NumeroCommande",
                table: "LigneCommandeVente",
                column: "NumeroCommande");

            migrationBuilder.CreateIndex(
                name: "IX_LigneCommandeVente_ProduitCodeProduit",
                table: "LigneCommandeVente",
                column: "ProduitCodeProduit");

            migrationBuilder.CreateIndex(
                name: "IX_LigneDemandePrix_CodeProduit",
                table: "LigneDemandePrix",
                column: "CodeProduit");

            migrationBuilder.CreateIndex(
                name: "IX_LigneDemandePrix_NumeroDemande",
                table: "LigneDemandePrix",
                column: "NumeroDemande");

            migrationBuilder.CreateIndex(
                name: "IX_LigneDevisClient_CodeProduit",
                table: "LigneDevisClient",
                column: "CodeProduit");

            migrationBuilder.CreateIndex(
                name: "IX_LigneDevisClient_NumeroDevis",
                table: "LigneDevisClient",
                column: "NumeroDevis");

            migrationBuilder.CreateIndex(
                name: "IX_LigneDevisClient_ProduitCodeProduit",
                table: "LigneDevisClient",
                column: "ProduitCodeProduit");

            migrationBuilder.CreateIndex(
                name: "IX_LigneFactureClient_code_produit",
                table: "LigneFactureClient",
                column: "code_produit");

            migrationBuilder.CreateIndex(
                name: "IX_LigneFactureClient_numero_factureclient",
                table: "LigneFactureClient",
                column: "numero_factureclient");

            migrationBuilder.CreateIndex(
                name: "IX_LigneFactureFournisseur_CodeProduit",
                table: "LigneFactureFournisseur",
                column: "CodeProduit");

            migrationBuilder.CreateIndex(
                name: "IX_LigneFactureFournisseur_NumeroFacture",
                table: "LigneFactureFournisseur",
                column: "NumeroFacture");

            migrationBuilder.CreateIndex(
                name: "IX_LigneFactureFournisseur_ProduitCodeProduit",
                table: "LigneFactureFournisseur",
                column: "ProduitCodeProduit");

            migrationBuilder.CreateIndex(
                name: "IX_LignesBonLivraison_CodeProduit",
                table: "LignesBonLivraison",
                column: "CodeProduit");

            migrationBuilder.CreateIndex(
                name: "IX_LignesBonLivraison_NumeroBon",
                table: "LignesBonLivraison",
                column: "NumeroBon");

            migrationBuilder.CreateIndex(
                name: "IX_LignesBonLivraison_ProduitCodeProduit",
                table: "LignesBonLivraison",
                column: "ProduitCodeProduit");

            migrationBuilder.CreateIndex(
                name: "IX_MagasinProduit_code_entreprise",
                table: "MagasinProduit",
                column: "code_entreprise");

            migrationBuilder.CreateIndex(
                name: "IX_Produit_code_categorieproduit",
                table: "Produit",
                column: "code_categorieproduit");

            migrationBuilder.CreateIndex(
                name: "IX_Produit_code_fournisseur",
                table: "Produit",
                column: "code_fournisseur");

            migrationBuilder.CreateIndex(
                name: "IX_Produit_code_magasinproduit",
                table: "Produit",
                column: "code_magasinproduit");

            migrationBuilder.CreateIndex(
                name: "IX_Produit_code_tvaproduit",
                table: "Produit",
                column: "code_tvaproduit");

            migrationBuilder.CreateIndex(
                name: "IX_Produit_code_uniteproduit",
                table: "Produit",
                column: "code_uniteproduit");

            migrationBuilder.CreateIndex(
                name: "IX_Produit_codeentreprise",
                table: "Produit",
                column: "codeentreprise");

            migrationBuilder.CreateIndex(
                name: "IX_Produit_DeviseCodeDevise",
                table: "Produit",
                column: "DeviseCodeDevise");

            migrationBuilder.CreateIndex(
                name: "IX_Produit_DouaneProduitCodeDouane",
                table: "Produit",
                column: "DouaneProduitCodeDouane");

            migrationBuilder.CreateIndex(
                name: "IX_Produit_FabriquantProduitCodeFabriquant",
                table: "Produit",
                column: "FabriquantProduitCodeFabriquant");

            migrationBuilder.CreateIndex(
                name: "IX_Produit_PaysProduitCodePays",
                table: "Produit",
                column: "PaysProduitCodePays");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_token",
                table: "RefreshTokens",
                column: "token");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_user_id",
                table: "RefreshTokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_ReglementFacture_ClientCodeClient",
                table: "ReglementFacture",
                column: "ClientCodeClient");

            migrationBuilder.CreateIndex(
                name: "IX_ReglementFacture_code_client",
                table: "ReglementFacture",
                column: "code_client");

            migrationBuilder.CreateIndex(
                name: "IX_ReglementFacture_code_entreprise",
                table: "ReglementFacture",
                column: "code_entreprise");

            migrationBuilder.CreateIndex(
                name: "IX_ReglementFacture_date_reglement",
                table: "ReglementFacture",
                column: "date_reglement");

            migrationBuilder.CreateIndex(
                name: "IX_ReglementFacture_numero_facture",
                table: "ReglementFacture",
                column: "numero_facture");

            migrationBuilder.CreateIndex(
                name: "IX_ReglementFournisseur_CodeEntreprise",
                table: "ReglementFournisseur",
                column: "CodeEntreprise");

            migrationBuilder.CreateIndex(
                name: "IX_ReglementFournisseur_CodeFournisseur",
                table: "ReglementFournisseur",
                column: "CodeFournisseur");

            migrationBuilder.CreateIndex(
                name: "IX_ReglementFournisseur_DateReglement",
                table: "ReglementFournisseur",
                column: "DateReglement");

            migrationBuilder.CreateIndex(
                name: "IX_ReglementFournisseur_NumeroFacture",
                table: "ReglementFournisseur",
                column: "NumeroFacture");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BonsLivraison_Factures");

            migrationBuilder.DropTable(
                name: "Licence");

            migrationBuilder.DropTable(
                name: "LigneBonReception");

            migrationBuilder.DropTable(
                name: "LigneCommandeAchat");

            migrationBuilder.DropTable(
                name: "LigneCommandeVente");

            migrationBuilder.DropTable(
                name: "LigneDemandePrix");

            migrationBuilder.DropTable(
                name: "LigneDevisClient");

            migrationBuilder.DropTable(
                name: "LigneFactureClient");

            migrationBuilder.DropTable(
                name: "LigneFactureFournisseur");

            migrationBuilder.DropTable(
                name: "LignesBonLivraison");

            migrationBuilder.DropTable(
                name: "ParametresDecimales");

            migrationBuilder.DropTable(
                name: "PathLogo");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "ReglementFacture");

            migrationBuilder.DropTable(
                name: "ReglementFournisseur");

            migrationBuilder.DropTable(
                name: "RetenuSource");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "BonReception");

            migrationBuilder.DropTable(
                name: "DemandePrix");

            migrationBuilder.DropTable(
                name: "BonsLivraison");

            migrationBuilder.DropTable(
                name: "Produit");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "FactureClient");

            migrationBuilder.DropTable(
                name: "FactureFournisseur");

            migrationBuilder.DropTable(
                name: "CommandeAchat");

            migrationBuilder.DropTable(
                name: "CommandeVente");

            migrationBuilder.DropTable(
                name: "CategorieProduit");

            migrationBuilder.DropTable(
                name: "DouaneProduit");

            migrationBuilder.DropTable(
                name: "FabriquantProduit");

            migrationBuilder.DropTable(
                name: "MagasinProduit");

            migrationBuilder.DropTable(
                name: "PaysProduit");

            migrationBuilder.DropTable(
                name: "TvaProduit");

            migrationBuilder.DropTable(
                name: "UniteProduit");

            migrationBuilder.DropTable(
                name: "ModePayement");

            migrationBuilder.DropTable(
                name: "Fournisseur");

            migrationBuilder.DropTable(
                name: "DevisClient");

            migrationBuilder.DropTable(
                name: "Client");

            migrationBuilder.DropTable(
                name: "Entreprise");

            migrationBuilder.DropTable(
                name: "Devise");
        }
    }
}
