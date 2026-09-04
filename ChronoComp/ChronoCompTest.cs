using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ChronoComp
{
    /// <summary>
    /// Tests xUnit pour l'ensemble du projet ChronoComp, regroupés dans une seule classe
    /// (une classe de test par fichier source aurait normalement du sens, mais tu m'as
    /// demandé un seul fichier — celui-ci vit directement dans le projet ChronoComp, sans
    /// projet de test séparé, contrairement à WL/WL.Tests). Les régions ci-dessous suivent
    /// le découpage des fichiers sources (DateHeure, Utils, Registre, Maths, Algebre,
    /// Geometrie). Les valeurs attendues viennent soit des commentaires "Test :" de la
    /// source WLangage d'origine (quand ils sont corrects), soit d'un calcul indépendant
    /// quand la source s'est révélée erronée (voir les remarques dans le code porté).
    /// </summary>
    public class ChronoCompTest
    {
        #region DATEHEURE
        [Theory]
        [InlineData(2026, 1, 12, 26)]  // NombreJourOuvrableDuMois(20260112) = 26
        [InlineData(2026, 2, 12, 24)]  // NombreJourOuvrableDuMois(20260212) = 24
        [InlineData(2026, 3, 22, 26)]  // NombreJourOuvrableDuMois(20260322) = 26
        [InlineData(2038, 7, 14, 26)]  // NombreJourOuvrableDuMois(20380714) = 26
        public void NombreJourOuvrableDuMois_ValeursAttendues(int année, int mois, int jour, int attendu)
        {
            Assert.Equal(attendu, DateHeure.NombreJourOuvrableDuMois(new DateOnly(année, mois, jour)));
        }

        [Theory]
        [InlineData(2026, 1, 12, 21)]  // NombreJourOuvresDuMois(20260112) = 21
        [InlineData(2026, 2, 12, 20)]  // NombreJourOuvresDuMois(20260212) = 20
        [InlineData(2026, 3, 22, 22)]  // NombreJourOuvresDuMois(20260322) = 22
        [InlineData(2038, 7, 14, 21)]  // NombreJourOuvresDuMois(20380714) = 21
        public void NombreJourOuvresDuMois_ValeursAttendues(int année, int mois, int jour, int attendu)
        {
            Assert.Equal(attendu, DateHeure.NombreJourOuvresDuMois(new DateOnly(année, mois, jour)));
        }

        [Theory]
        [InlineData(1, 1, false)]   // 1er janvier : férié
        [InlineData(7, 14, false)]  // 14 juillet : férié
        [InlineData(8, 17, true)]   // lundi 17 août 2026 : ouvrable
        public void EstOuvrable_JoursFériésFixes(int mois, int jour, bool attendu)
        {
            Assert.Equal(attendu, DateHeure.EstOuvrable(new DateOnly(2026, mois, jour)));
        }

        [Fact]
        public void EstOuvrable_SamediEstOuvrableMaisPasOuvré()
        {
            // Samedi 4 juillet 2026 (à vérifier : DayOfWeek.Saturday)
            var samedi = new DateOnly(2026, 7, 4);
            Assert.Equal(DayOfWeek.Saturday, samedi.DayOfWeek);
            Assert.True(DateHeure.EstOuvrable(samedi));
            Assert.False(DateHeure.Estouvré(samedi));
        }

        [Fact]
        public void EstOuvrable_DimancheNestJamaisOuvrableNiOuvré()
        {
            var dimanche = new DateOnly(2026, 7, 5);
            Assert.Equal(DayOfWeek.Sunday, dimanche.DayOfWeek);
            Assert.False(DateHeure.EstOuvrable(dimanche));
            Assert.False(DateHeure.Estouvré(dimanche));
        }

        [Theory]
        [InlineData(2026, 5, 2, 3, 2026, 5, 19)]  // 3e mardi de mai 2026 => 19/05/2026
        public void NemeJourSemaineDuMois_TroisiemeMardi(
            int année, int mois, int jourSemaine, int n, int annéeAttendue, int moisAttendu, int jourAttendu)
        {
            var résultat = DateHeure.NemeJourSemaineDuMois(année, mois, jourSemaine, n);
            Assert.Equal(new DateOnly(annéeAttendue, moisAttendu, jourAttendu), résultat);
        }

        [Fact]
        public void NemeJourSemaineDuMois_RangInexistantRenvoieNull()
        {
            // Il n'y a pas de 5e lundi en février 2026 (28 jours, 1er = dimanche)
            Assert.Null(DateHeure.NemeJourSemaineDuMois(2026, 2, 1, 5));
        }

        [Theory]
        [InlineData(2025, 4, 14, 7, 2025, 6, 30)]
        [InlineData(2025, 11, 24, 7, 2026, 6, 30)]
        public void FinAnneeFiscale_DébutJuillet(
            int année, int mois, int jour, int débutAnnéeFiscale, int annéeAttendue, int moisAttendu, int jourAttendu)
        {
            var résultat = DateHeure.FinAnneeFiscale(new DateOnly(année, mois, jour), débutAnnéeFiscale);
            Assert.Equal(new DateOnly(annéeAttendue, moisAttendu, jourAttendu), résultat);
        }

        // Note : le commentaire WL d'origine annonçait "début d'année fiscale au 01/07
        // (paramètre = 7)" mais montrait des exemples avec des résultats en septembre —
        // incohérence dans la source d'origine. Avec débutAnnéeFiscale = 7, le début
        // d'année fiscale attendu est bien le 01/07 (cohérent avec FinAnneeFiscale ci-dessus,
        // qui se termine le 30/06).
        [Theory]
        [InlineData(2025, 4, 14, 7, 2024, 7, 1)]
        [InlineData(2025, 11, 24, 7, 2025, 7, 1)]
        public void DebutAnneeFiscale_DébutJuillet(
            int année, int mois, int jour, int débutAnnéeFiscale, int annéeAttendue, int moisAttendu, int jourAttendu)
        {
            var résultat = DateHeure.DebutAnneeFiscale(new DateOnly(année, mois, jour), débutAnnéeFiscale);
            Assert.Equal(new DateOnly(annéeAttendue, moisAttendu, jourAttendu), résultat);
        }

        [Fact]
        public void LibelléAnnéeFiscale_ExemplesDeLaSourceWL()
        {
            Assert.Equal(
                "du mercredi 01 janvier 2025 au mercredi 31 décembre 2025",
                DateHeure.LibelléAnnéeFiscale(new DateOnly(2025, 3, 1), 1));
            Assert.Equal(
                "du lundi 01 avril 2024 au lundi 31 mars 2025",
                DateHeure.LibelléAnnéeFiscale(new DateOnly(2025, 3, 1), 4));
        }

        [Theory]
        [InlineData(2026, 1, 1, 2026, 1, 1)]
        [InlineData(2026, 4, 1, 2026, 1, 1)]
        [InlineData(2026, 6, 30, 2026, 1, 1)]
        [InlineData(2026, 7, 1, 2026, 7, 1)]
        public void DebutSemestre_PremierOuDeuxième(
            int année, int mois, int jour, int annéeAttendue, int moisAttendu, int jourAttendu)
        {
            Assert.Equal(
                new DateOnly(annéeAttendue, moisAttendu, jourAttendu),
                DateHeure.DebutSemestre(new DateOnly(année, mois, jour)));
        }

        [Theory]
        [InlineData(2026, 2, 15, 2026, 3, 31)]  // T1 2026
        [InlineData(2026, 8, 1, 2026, 9, 30)]   // T3 2026
        public void FinTrimestre_DernierJourDuTrimestre(
            int année, int mois, int jour, int annéeAttendue, int moisAttendu, int jourAttendu)
        {
            Assert.Equal(
                new DateOnly(annéeAttendue, moisAttendu, jourAttendu),
                DateHeure.FinTrimestre(new DateOnly(année, mois, jour)));
        }

        [Fact]
        public void DateMaxDateMin_RenvoientLaBonneDate()
        {
            var d1 = new DateOnly(2026, 1, 1);
            var d2 = new DateOnly(2026, 12, 31);
            Assert.Equal(d2, DateHeure.DateMax(d1, d2));
            Assert.Equal(d1, DateHeure.DateMin(d1, d2));
        }

        [Fact]
        public void FaitDate_ComposantsValidesEtInvalides()
        {
            Assert.Equal(new DateOnly(2026, 2, 28), DateHeure.FaitDate(2026, 2, 28));
            Assert.Null(DateHeure.FaitDate(2026, 2, 30));   // 30 février n'existe pas
            Assert.Null(DateHeure.FaitDate(2026, 13, 1));   // mois 13 n'existe pas
        }

        [Fact]
        public void AjouteJourOuvrable_SauteLesFériésEtLesDimanches()
        {
            // Vendredi 31/12/2027 + 1 jour ouvrable : 01/01/2028 est férié (samedi),
            // 02/01/2028 est un dimanche (non ouvrable), 03/01/2028 est le premier
            // jour ouvrable suivant (lundi).
            var résultat = DateHeure.AjouteJourOuvrable(new DateOnly(2027, 12, 31), 1);
            Assert.Equal(new DateOnly(2028, 1, 3), résultat);
            Assert.True(DateHeure.EstOuvrable(résultat));
        }

        [Fact]
        public void AjouteJourOuvré_SauteAussiLesSamedis()
        {
            var résultat = DateHeure.AjouteJourOuvré(new DateOnly(2027, 12, 31), 1);
            Assert.True(DateHeure.Estouvré(résultat));
            Assert.True(résultat > new DateOnly(2027, 12, 31));
        }

        [Fact]
        public void PremierJourEte_PremierJourHiver_SontDesDimanches()
        {
            var été = DateHeure.PremierJourEte(new DateOnly(2026, 6, 1));
            var hiver = DateHeure.PremierJourHiver(new DateOnly(2026, 6, 1));
            Assert.Equal(DayOfWeek.Sunday, été.DayOfWeek);
            Assert.Equal(DayOfWeek.Sunday, hiver.DayOfWeek);
            Assert.Equal(3, été.Month);
            Assert.Equal(10, hiver.Month);
        }

        [Fact]
        public void HeureHiverEte_CohérentAvecPremierJourEteEtHiver()
        {
            Assert.True(DateHeure.HeureHiverEte(new DateOnly(2026, 7, 15)));
            Assert.False(DateHeure.HeureHiverEte(new DateOnly(2026, 1, 15)));
        }

        [Fact]
        public void ListeVendredi13_NeContientQueDesVendredis13()
        {
            var liste = DateHeure.ListeVendredi13(24);
            Assert.All(liste, d =>
            {
                Assert.Equal(13, d.Day);
                Assert.Equal(DayOfWeek.Friday, d.DayOfWeek);
            });
        }

        [Fact]
        public void ProchainVendredi13_TrouveBienUnVendrediTreize()
        {
            var résultat = DateHeure.ProchainVendredi13(new DateOnly(2026, 1, 1));
            Assert.NotNull(résultat);
            Assert.Equal(13, résultat!.Value.Day);
            Assert.Equal(DayOfWeek.Friday, résultat.Value.DayOfWeek);
            Assert.True(résultat.Value >= new DateOnly(2026, 1, 1));
        }
        #endregion

        #region UTILS
        [Theory]
        [InlineData(true, "OUI")]
        [InlineData(false, "NON")]
        public void OUI_NON_ConvertitLeBooléen(bool valeur, string attendu)
        {
            Assert.Equal(attendu, Utils.OUI_NON(valeur));
        }

        [Fact]
        public void TraceModeTest_NeLèvePasDException()
        {
            var exception = Record.Exception(() => Utils.TraceModeTest("message de test"));
            Assert.Null(exception);
        }
        #endregion

        #region REGISTRE
        [Fact]
        public void VersCsv_FormateLesLignesAvecLeSéparateur()
        {
            var lignes = new List<Registre.LigneRegistre>
            {
                new("HKEY_CURRENT_USER\\Test", "Valeur1", "String", "abc"),
                new("HKEY_CURRENT_USER\\Test", "Valeur2", "DWord", "42"),
            };
            string csv = Registre.VersCsv(lignes);
            Assert.Contains("HKEY_CURRENT_USER\\Test\tValeur1\tString\tabc", csv);
            Assert.Contains("HKEY_CURRENT_USER\\Test\tValeur2\tDWord\t42", csv);
        }

        [Fact]
        public void AnalyseRegistreClé_CléInconnueRenvoieUneListeVide()
        {
            var résultat = Registre.AnalyseRegistreClé("HKEY_CURRENT_USER\\Un\\Chemin\\Qui\\NexistePasDuTout_XYZ");
            Assert.Empty(résultat);
        }

        [Fact]
        public void AnalyseRegistreClé_CléRacineVideRenvoieUneListeVide()
        {
            Assert.Empty(Registre.AnalyseRegistreClé(""));
        }
        // Ajouter pour neutraliser ce tests
        // Skip = "Dépend du contenu réel du registre de la machine — à activer ponctuellement si besoin."
        // à l'intérieur de FACT()
        [Fact()]
        public void AnalyseRegistreClé_CléRéelleNeLèvePasDException()
        {
            var exception = Record.Exception(() => Registre.AnalyseRegistreClé("HKEY_CURRENT_USER\\Environment"));
            Assert.Null(exception);
        }
        #endregion

        #region MATHS — THÉORIE DES NOMBRES
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(5, 5)]
        [InlineData(10, 55)]
        [InlineData(20, 6765)]
        [InlineData(80, 23416728348467685)]
        public void MFibonacci_ValeursAttendues(int n, long attendu)
        {
            Assert.Equal(attendu, Maths.MFibonacci(n));
        }

        [Theory]
        [InlineData(-1, 1)]
        [InlineData(-2, -1)]
        [InlineData(-3, 2)]
        public void MFibonacci_RangsNégatifs(int n, long attendu)
        {
            Assert.Equal(attendu, Maths.MFibonacci(n));
        }

        [Theory]
        [InlineData(0, 2)]
        [InlineData(1, 1)]
        [InlineData(2, 3)]
        [InlineData(5, 11)]
        [InlineData(10, 123)]
        public void MLucas_ValeursAttendues(int n, long attendu)
        {
            Assert.Equal(attendu, Maths.MLucas(n));
        }

        [Theory]
        [InlineData(561, 80)]
        [InlineData(64, 16)]
        [InlineData(105, 12)]
        [InlineData(15, 4)]
        public void MCarmichael_ValeursAttendues(long n, long attendu)
        {
            Assert.Equal(attendu, Maths.MCarmichael(n));
        }

        [Theory]
        [InlineData(2, 10, 1024)]
        [InlineData(123, 0, 1)]
        [InlineData(-3, 5, -243)]
        [InlineData(-3, 4, 81)]
        public void MPowInt_ValeursAttendues(long valeurBase, int exposant, long attendu)
        {
            Assert.Equal(attendu, Maths.MPowInt(valeurBase, exposant));
        }

        [Theory]
        [InlineData(2, true)]
        [InlineData(3, true)]
        [InlineData(17, true)]   // absent de la liste C_NbrePremier avant correction de Bernard
        [InlineData(561, false)]
        [InlineData(591, false)] // 3 × 197 : aurait été déclaré premier avec le bug nBorneMin=503 de la source
        [InlineData(593, true)]
        [InlineData(599, true)]
        [InlineData(600, false)]
        [InlineData(7919, true)] // 1000e nombre premier
        [InlineData(1, false)]
        [InlineData(0, false)]
        public void MPremier_ValeursAttendues(long valeur, bool attendu)
        {
            Assert.Equal(attendu, Maths.MPremier(valeur));
        }

        [Theory]
        [InlineData(6, 0)]     // Constantes.C_Parfait
        [InlineData(28, 0)]
        [InlineData(12, 1)]    // Constantes.C_Abondant
        [InlineData(10, -1)]   // Constantes.C_Deficient
        public void MAbondantDeficientParfait_ValeursAttendues(long valeur, int attendu)
        {
            Assert.Equal(attendu, Maths.MAbondantDeficientParfait(valeur));
        }

        [Theory]
        [InlineData(28, true)]
        [InlineData(496, true)]
        [InlineData(10, false)]
        [InlineData(100, false)]
        public void MParfait_ValeursAttendues(long valeur, bool attendu)
        {
            Assert.Equal(attendu, Maths.MParfait(valeur));
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(5, true)]
        [InlineData(6, true)]
        [InlineData(25, true)]
        [InlineData(76, true)]
        [InlineData(376, true)]
        [InlineData(7, false)]
        public void Mautomorphe_ValeursAttendues(long valeur, bool attendu)
        {
            Assert.Equal(attendu, Maths.Mautomorphe(valeur));
        }

        [Theory]
        [InlineData(153, true)]
        [InlineData(370, true)]
        [InlineData(9474, true)]
        [InlineData(123, false)]
        [InlineData(-153, false)]
        public void MNarcissique_ValeursAttendues(long valeur, bool attendu)
        {
            Assert.Equal(attendu, Maths.MNarcissique(valeur));
        }

        [Theory]
        [InlineData(121, true)]
        [InlineData(91233219, true)]
        [InlineData(102442010, false)]
        public void MPalindrome_ValeursAttendues(long valeur, bool attendu)
        {
            Assert.Equal(attendu, Maths.MPalindrome(valeur));
        }

        [Theory]
        [InlineData(2026, true)]
        [InlineData(4, false)]
        [InlineData(19, true)]
        [InlineData(7, true)]
        public void MHeureux_ValeursAttendues(long valeur, bool attendu)
        {
            Assert.Equal(attendu, Maths.MHeureux(valeur));
        }

        [Theory]
        [InlineData(4, true)]
        [InlineData(22, true)]
        [InlineData(27, true)]
        [InlineData(58, true)]
        [InlineData(9, false)]
        [InlineData(561, false)]  // Carmichael, mais premier ? non — mais pas un nombre de Smith non plus
        public void MNbrSmith_ValeursAttendues(long valeur, bool attendu)
        {
            Assert.Equal(attendu, Maths.MNbrSmith(valeur));
        }

        [Theory]
        [InlineData(12, 18, 6)]
        [InlineData(17, 5, 1)]
        [InlineData(0, 9, 9)]
        public void MPgcd_ValeursAttendues(long a, long b, long attendu)
        {
            Assert.Equal(attendu, Maths.MPgcd(a, b));
        }

        [Theory]
        [InlineData(4, 6, 12)]
        [InlineData(21, 6, 42)]
        [InlineData(0, 5, 0)]
        public void MPpcm_ValeursAttendues(long a, long b, long attendu)
        {
            Assert.Equal(attendu, Maths.MPpcm(a, b));
        }
        #endregion

        #region MATHS — STATISTIQUES
        private static readonly double[] Série = { 8, 12, 15, 98, 64, 52 };

        [Fact]
        public void MGéométrique_ValeurAttendue()
        {
            Assert.Equal(27.8801633672, Maths.MGéométrique(Série)!.Value, 8);
        }

        [Fact]
        public void MHarmonique_ValeurAttendue()
        {
            Assert.Equal(18.7464937693, Maths.MHarmonique(Série)!.Value, 8);
        }

        [Fact]
        public void MMediane_PaireEtImpaire()
        {
            Assert.Equal(13.5, Maths.MMediane(new double[] { -8, 12, 15, 98, 64, -52 })!.Value, 6);
            Assert.Equal(15.0, Maths.MMediane(new double[] { 8, 12, 15, 98, 64 })!.Value, 6);
        }

        [Fact]
        public void MMobile_FenêtreDeTrois()
        {
            var résultat = Maths.MMobile(Série, 3)!;
            Assert.Equal(4, résultat.Length);
            Assert.Equal(35.0 / 3, résultat[0], 6);
            Assert.Equal(214.0 / 3, résultat[3], 6);
        }

        [Fact]
        public void MQuadratique_ValeurCohérente()
        {
            var résultat = Maths.MQuadratique(new double[] { 3, 4 });
            Assert.Equal(Math.Sqrt((9.0 + 16.0) / 2), résultat!.Value, 10);
        }

        [Fact]
        public void MPondérée_ValeurCohérente()
        {
            var résultat = Maths.MPondérée(new double[] { 10, 20 }, new double[] { 1, 3 });
            Assert.Equal((10 * 1 + 20 * 3) / 4.0, résultat!.Value, 10);
        }

        [Fact]
        public void MTronquée_RetireLesExtrêmes()
        {
            var résultat = Maths.MTronquée(Série, 1);
            // trié : 8,12,15,52,64,98 → on retire 8 et 98 → moyenne(12,15,52,64)
            Assert.Equal((12.0 + 15 + 52 + 64) / 4, résultat!.Value, 6);
        }

        [Fact]
        public void MWinsorisée_ValeurCorrigéeParRapportAuCommentaireDOrigine()
        {
            // Le commentaire de la source WLangage annonçait 47,25 ; l'algorithme donne 36,5
            // (voir la remarque XML du code) — vérifié indépendamment ci-dessous.
            Assert.Equal(36.5, Maths.MWinsorisée(Série, 1)!.Value, 6);
        }

        [Theory]
        [InlineData(0)]
        public void Statistiques_TableauVideRenvoientNull(int _)
        {
            var vide = Array.Empty<double>();
            Assert.Null(Maths.MGéométrique(vide));
            Assert.Null(Maths.MHarmonique(vide));
            Assert.Null(Maths.MMediane(vide));
            Assert.Null(Maths.MQuadratique(vide));
            Assert.Null(Maths.MTronquée(vide, 0));
            Assert.Null(Maths.MWinsorisée(vide, 0));
        }
        #endregion

        #region ALGEBRE
        [Fact]
        public void MatriceCarrée_ComplèteAvecLaValeurManquante()
        {
            double[,] rectangulaire = { { 1, 2, 3 }, { 4, 5, 6 } }; // 2 lignes x 3 colonnes
            double[,] résultat = Algebre.MatriceCarrée(rectangulaire, -1);

            Assert.Equal(3, résultat.GetLength(0));
            Assert.Equal(3, résultat.GetLength(1));
            Assert.Equal(1, résultat[0, 0]);
            Assert.Equal(-1, résultat[2, 0]); // ligne ajoutée
        }

        [Fact]
        public void MatriceVersChaine_ProduitUneLigneParLigneDeMatrice()
        {
            double[,] matrice = { { 1, 2 }, { 3, 4 } };
            string texte = Algebre.MatriceVersChaine(matrice);
            Assert.Equal("1\t2\n3\t4", texte);
        }

        [Fact]
        public void TableauVersMatrice_AssembleDesLignesRectangulaires()
        {
            double[][] lignes = { new double[] { 1, 2 }, new double[] { 3, 4 }, new double[] { 5, 6 } };
            double[,]? matrice = Algebre.TableauVersMatrice(lignes);

            Assert.NotNull(matrice);
            Assert.Equal(3, matrice!.GetLength(0));
            Assert.Equal(2, matrice.GetLength(1));
            Assert.Equal(5, matrice[2, 0]);
        }

        [Fact]
        public void TableauVersMatrice_LignesDeTaillesDifférentesRenvoieNull()
        {
            double[][] lignes = { new double[] { 1, 2 }, new double[] { 3 } };
            Assert.Null(Algebre.TableauVersMatrice(lignes));
        }

        [Fact]
        public void MDichotomie_TrouveLaRacineDeXCubeMoinsXMoinsDeux()
        {
            double résultat = Algebre.MDichotomie(x => x * x * x - x - 2, 1, 2, 1e-8, 100);
            Assert.Equal(1.521379701793, résultat, 6);
        }

        [Fact]
        public void MDichotomie_PasDeChangementDeSigneRenvoieZéro()
        {
            double résultat = Algebre.MDichotomie(x => x * x + 1, -1, 1);
            Assert.Equal(0, résultat);
        }

        [Fact]
        public void MSolveAXB_RésoutUnSystèmeSimple()
        {
            // 2x + y = 5 ; x + 3y = 10  =>  x=1, y=3
            double[,] a = { { 2, 1 }, { 1, 3 } };
            double[] b = { 5, 10 };

            bool ok = Algebre.MSolveAXB(a, b, out double[]? x, out int codeErreur);

            Assert.True(ok);
            Assert.Equal(0, codeErreur);
            Assert.NotNull(x);
            Assert.Equal(1.0, x![0], 6);
            Assert.Equal(3.0, x[1], 6);
        }

        [Fact]
        public void MSolveAXB_MatriceSingulièreRenvoieCodeErreurDeux()
        {
            double[,] a = { { 1, 2 }, { 2, 4 } }; // lignes proportionnelles => singulière
            double[] b = { 1, 2 };

            bool ok = Algebre.MSolveAXB(a, b, out double[]? x, out int codeErreur);

            Assert.False(ok);
            Assert.Equal(2, codeErreur);
            Assert.Null(x);
        }
        #endregion

        #region GEOMETRIE
        [Fact]
        public void MPérimètre_Rectangle()
        {
            var (valeur, _) = Geometrie.MPérimètre(ETypePérimètre.MpRectangle, 4, 3);
            Assert.Equal(14, valeur, 6);
        }

        [Fact]
        public void MSurface_TriangleDeHéron_TriangleRectangleTroisQuatreCinq()
        {
            var (aire, message) = Geometrie.MSurface(ETypeSurface.MsTriangle_Heron, 3, 4, 5);
            Assert.Equal(6.0, aire, 6);
            Assert.StartsWith(Constantes.C_Formule, message);
        }

        [Fact]
        public void MSurface_TriangleInvalideRenvoieUnMessageDErreur()
        {
            var (aire, message) = Geometrie.MSurface(ETypeSurface.MsTriangle_Heron, 1, 1, 10);
            Assert.Equal(0, aire);
            Assert.StartsWith(Constantes.CErr_Descriptteur, message);
        }

        [Fact]
        public void MSurface_Cercle()
        {
            var (aire, _) = Geometrie.MSurface(ETypeSurface.MsCercle, 2);
            Assert.Equal(Math.PI * 4, aire, 6);
        }

        [Fact]
        public void MSurface3D_SphereTotale()
        {
            var (surface, _) = Geometrie.MSurface3D(EType3DSurface.Ms3Sphere_Totale, 3);
            Assert.Equal(4 * Math.PI * 9, surface, 6);
        }

        [Fact]
        public void MVolume_Cube()
        {
            var (volume, _) = Geometrie.MVolume(ETypeVolume.MvCube, 2);
            Assert.Equal(8, volume, 6);
        }

        [Fact]
        public void MVolume_Sphere()
        {
            var (volume, _) = Geometrie.MVolume(ETypeVolume.MvSphere, 3);
            Assert.Equal(4.0 / 3.0 * Math.PI * 27, volume, 6);
        }

        [Theory]
        [InlineData(0, 0, 10, 10, 5, 5, 10, 10, 0, true)]   // se chevauchent
        [InlineData(0, 0, 10, 10, 20, 20, 5, 5, 0, false)]  // séparés
        [InlineData(0, 0, 10, 10, 10, 0, 5, 5, 0, false)]   // se touchent exactement au bord, marge 0 => pas d'intersection
        [InlineData(0, 0, 10, 10, 10, 0, 5, 5, 1, true)]    // même position, marge 1 => l'écart (0) est inférieur à la marge => intersection
        public void RectIntersecte_ValeursAttendues(
            int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2, int marge, bool attendu)
        {
            var r1 = new Rectangle(x1, y1, w1, h1);
            var r2 = new Rectangle(x2, y2, w2, h2);
            Assert.Equal(attendu, Geometrie.RectIntersecte(r1, r2, marge));
        }
        #endregion
    }
}
