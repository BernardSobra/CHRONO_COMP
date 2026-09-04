using System;
using System.Collections.Generic;
using System.Globalization;

namespace ChronoComp
{
    /// <summary>
    /// Dates, calendrier, jours ouvrés/ouvrables et année fiscale.
    ///
    /// Une "Date" WLangage est ici un System.DateOnly (comme dans la bibliothèque WL — voir
    /// WL.Date). DateOnly ne pouvant pas représenter une date "vide"/invalide comme le type
    /// Date de WLangage, les fonctions qui pouvaient renvoyer "" en cas d'échec renvoient ici
    /// un DateOnly? (null au lieu de vide) ; celles qui construisent toujours une date valide
    /// (à partir d'une DateOnly déjà valide en entrée) renvoient un DateOnly non-nullable.
    ///
    /// EstOuvrable/Estouvré recalculent les jours fériés français au lieu de s'appuyer sur
    /// JourFériéAjoute/JourFériéListe (fonctions WINDEV liées à l'environnement d'exécution,
    /// non portées) : Pâques est calculé par l'algorithme de Gauss (dit "algorithme
    /// anonyme grégorien"), et les jours fériés mobiles (lundi de Pâques, Ascension, lundi
    /// de Pentecôte) en découlent. Le nom Estouvré (casse différente de EstOuvrable) est
    /// repris tel quel de la source WLangage.
    /// </summary>
    public static class DateHeure
    {
        // ----------------------------------------
        private static DateOnly PremierJourDuMois(DateOnly date) => new(date.Year, date.Month, 1);
        private static DateOnly DernierJourDuMois(DateOnly date) => new(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

        // ----------------------------------------
        // Cache des jours fériés par année (calculés une fois par année demandée).
        // ----------------------------------------
        private static readonly Dictionary<int, HashSet<DateOnly>> _cacheFériésOuvrable = new();
        private static readonly Dictionary<int, HashSet<DateOnly>> _cacheFériésOuvré = new();

        private static DateOnly Pâques(int année)
        {
            // Algorithme de Gauss (algorithme "anonyme grégorien", Meeus/Jones/Butcher).
            int a = année % 19;
            int b = année / 100;
            int c = année % 100;
            int d = b / 4;
            int e = b % 4;
            int f = (b + 8) / 25;
            int g = (b - f + 1) / 3;
            int h = (19 * a + b - d - g + 15) % 30;
            int i = c / 4;
            int k = c % 4;
            int l = (32 + 2 * e + 2 * i - h - k) % 7;
            int m = (a + 11 * h + 22 * l) / 451;
            int mois = (h + l - 7 * m + 114) / 31;
            int jour = (h + l - 7 * m + 114) % 31 + 1;
            return new DateOnly(année, mois, jour);
        }

        private static HashSet<DateOnly> JoursFériésOuvrable(int année)
        {
            if (_cacheFériésOuvrable.TryGetValue(année, out var cache))
                return cache;

            DateOnly pâques = Pâques(année);
            var jours = new HashSet<DateOnly>
            {
                new DateOnly(année, 1, 1),
                pâques,
                pâques.AddDays(1),   // lundi de Pâques
                new DateOnly(année, 5, 1),
                new DateOnly(année, 5, 8),
                pâques.AddDays(39),  // jeudi de l'Ascension
                pâques.AddDays(50),  // lundi de Pentecôte
                new DateOnly(année, 7, 14),
                new DateOnly(année, 8, 15),
            };
            _cacheFériésOuvrable[année] = jours;
            return jours;
        }

        private static HashSet<DateOnly> JoursFériésOuvré(int année)
        {
            if (_cacheFériésOuvré.TryGetValue(année, out var cache))
                return cache;

            var jours = new HashSet<DateOnly>(JoursFériésOuvrable(année))
            {
                new DateOnly(année, 11, 1),  // Toussaint
                new DateOnly(année, 11, 11),
                new DateOnly(année, 12, 25), // Noël
            };
            _cacheFériésOuvré[année] = jours;
            return jours;
        }

        // ********************************************************************************
        /// <summary>Ajoute (ou retranche si n est négatif) n jours ouvrables (lun-sam, hors jours fériés) à une date.</summary>
        /// <param name="date">La date de départ, non comptée.</param>
        /// <param name="n">Le nombre de jours ouvrables à ajouter ou retrancher.</param>
        /// <returns>La date correspondante.</returns>
        public static DateOnly AjouteJourOuvrable(DateOnly date, int n)
        {
            DateOnly résultat = date;
            if (n == 0)
            {
                while (!EstOuvrable(résultat))
                    résultat = résultat.AddDays(1);
                return résultat;
            }

            int sens = n < 0 ? -1 : 1;
            n = Math.Abs(n);

            int ajoutés = 0;
            while (ajoutés < n)
            {
                résultat = résultat.AddDays(sens);
                if (EstOuvrable(résultat)) ajoutés++;
            }
            return résultat;
        }
        // ********************************************************************************
        /// <summary>Ajoute (ou retranche si n est négatif) n jours ouvrés (lun-ven, hors jours fériés) à une date.</summary>
        /// <param name="date">La date de départ, non comptée.</param>
        /// <param name="n">Le nombre de jours ouvrés à ajouter ou retrancher.</param>
        /// <returns>La date correspondante.</returns>
        public static DateOnly AjouteJourOuvré(DateOnly date, int n)
        {
            DateOnly résultat = date;
            if (n == 0)
            {
                while (!Estouvré(résultat))
                    résultat = résultat.AddDays(1);
                return résultat;
            }

            int sens = n < 0 ? -1 : 1;
            n = Math.Abs(n);

            int ajoutés = 0;
            while (ajoutés < n)
            {
                résultat = résultat.AddDays(sens);
                if (Estouvré(résultat)) ajoutés++;
            }
            return résultat;
        }
        // ********************************************************************************
        /// <summary>Renvoie la plus grande des deux dates.</summary>
        /// <param name="date1">La première date.</param>
        /// <param name="date2">La deuxième date.</param>
        /// <returns>La plus grande des deux dates.</returns>
        public static DateOnly DateMax(DateOnly date1, DateOnly date2) => date1 > date2 ? date1 : date2;
        // ********************************************************************************
        /// <summary>Renvoie la plus petite des deux dates.</summary>
        /// <param name="date1">La première date.</param>
        /// <param name="date2">La deuxième date.</param>
        /// <returns>La plus petite des deux dates.</returns>
        public static DateOnly DateMin(DateOnly date1, DateOnly date2) => date1 < date2 ? date1 : date2;
        // ********************************************************************************
        /// <summary>Renvoie le 1er jour de l'année fiscale qui contient date.</summary>
        /// <param name="date">La date à étudier.</param>
        /// <param name="débutAnnéeFiscale">Le mois de début de l'année fiscale (1 par défaut).</param>
        /// <returns>Le 1er jour de l'année fiscale.</returns>
        public static DateOnly DebutAnneeFiscale(DateOnly date, int débutAnnéeFiscale = 1)
        {
            débutAnnéeFiscale = Math.Clamp(débutAnnéeFiscale, 1, 12);
            int année = date.Year;
            if (date.Month < débutAnnéeFiscale) année--;
            return new DateOnly(année, débutAnnéeFiscale, 1);
        }
        // ********************************************************************************
        /// <summary>Renvoie le 1er jour du semestre en cours.</summary>
        /// <param name="date">La date du jour considéré.</param>
        /// <returns>Le premier jour du semestre qui contient cette date.</returns>
        public static DateOnly DebutSemestre(DateOnly date)
        {
            int moisDébut = date.Month <= 6 ? 1 : 7;
            return new DateOnly(date.Year, moisDébut, 1);
        }
        // ********************************************************************************
        /// <summary>Renvoie le 1er jour du trimestre civil qui contient date.</summary>
        /// <param name="date">La date de référence.</param>
        /// <returns>Le premier jour du trimestre civil.</returns>
        public static DateOnly DébutTrimestre(DateOnly date)
        {
            int trimestre = (date.Month - 1) / 3 + 1;
            int moisDébut = (trimestre - 1) * 3 + 1;
            return new DateOnly(date.Year, moisDébut, 1);
        }
        // ********************************************************************************
        /// <summary>Indique si la date est un jour ouvrable (lundi-samedi, hors jours fériés).</summary>
        /// <param name="date">La date du jour à tester.</param>
        /// <returns>Vrai si la date est un jour ouvrable.</returns>
        public static bool EstOuvrable(DateOnly date)
        {
            if (JoursFériésOuvrable(date.Year).Contains(date)) return false;
            return date.DayOfWeek != DayOfWeek.Sunday;
        }
        // ********************************************************************************
        /// <summary>Indique si la date est un jour ouvré (lundi-vendredi, hors jours fériés).</summary>
        /// <param name="date">La date à tester.</param>
        /// <returns>Vrai si la date est un jour ouvré.</returns>
        public static bool Estouvré(DateOnly date)
        {
            if (JoursFériésOuvré(date.Year).Contains(date)) return false;
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }
        // ********************************************************************************
        /// <summary>
        /// Construit une date à partir de ses composants.
        /// Adaptation : les valeurs par défaut WLangage (AnnéeEnCours()/MoisEnCours()/JourEnCours())
        /// ne sont pas des constantes de compilation en C# ; passer 0 (ou omettre le
        /// paramètre) prend donc la date du jour pour ce composant.
        /// </summary>
        /// <param name="année">L'année (0 = année en cours).</param>
        /// <param name="mois">Le mois (0 = mois en cours).</param>
        /// <param name="jour">Le jour (0 = jour en cours).</param>
        /// <returns>La date obtenue, ou null si les composants ne forment pas une date valide.</returns>
        public static DateOnly? FaitDate(int année = 0, int mois = 0, int jour = 0)
        {
            DateOnly aujourdHui = DateOnly.FromDateTime(DateTime.Today);
            if (année == 0) année = aujourdHui.Year;
            if (mois == 0) mois = aujourdHui.Month;
            if (jour == 0) jour = aujourdHui.Day;

            if (année < 1 || mois < 1 || mois > 12 || jour < 1 || jour > 31)
                return null;

            try
            {
                return new DateOnly(année, mois, jour);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }
        // ********************************************************************************
        /// <summary>Renvoie le dernier jour de l'année fiscale qui contient date.</summary>
        /// <param name="date">La date à étudier.</param>
        /// <param name="débutAnnéeFiscale">Le mois de début de l'année fiscale (1 par défaut).</param>
        /// <returns>Le dernier jour de l'année fiscale.</returns>
        public static DateOnly FinAnneeFiscale(DateOnly date, int débutAnnéeFiscale = 1)
        {
            DateOnly début = DebutAnneeFiscale(date, débutAnnéeFiscale);
            return début.AddYears(1).AddDays(-1);
        }
        // ********************************************************************************
        /// <summary>Renvoie le dernier jour du semestre en cours.</summary>
        /// <param name="date">La date à étudier.</param>
        /// <returns>Le dernier jour du semestre.</returns>
        public static DateOnly FinSemestre(DateOnly date)
        {
            int moisFin = date.Month <= 6 ? 6 : 12;
            return DernierJourDuMois(new DateOnly(date.Year, moisFin, 1));
        }
        // ********************************************************************************
        /// <summary>Renvoie le dernier jour du trimestre civil qui contient date.</summary>
        /// <param name="date">La date de référence.</param>
        /// <returns>Le dernier jour du trimestre civil.</returns>
        public static DateOnly FinTrimestre(DateOnly date)
        {
            int trimestre = (date.Month - 1) / 3 + 1;
            int moisFin = (trimestre - 1) * 3 + 3;
            return DernierJourDuMois(new DateOnly(date.Year, moisFin, 1));
        }
        // ********************************************************************************
        /// <summary>Indique si la date est en heure d'été (vrai) ou en heure d'hiver (faux), en France métropolitaine.</summary>
        /// <param name="date">La date à tester.</param>
        /// <returns>Faux en hiver, vrai en été.</returns>
        public static bool HeureHiverEte(DateOnly date)
        {
            DateOnly débutÉté = PremierJourEte(date);
            DateOnly débutHiver = PremierJourHiver(date);
            return date >= débutÉté && date < débutHiver;
        }
        // ----------------------------------------
        // FormateDate : formateur minimal ne reconnaissant que les jetons utilisés par
        // défaut (DDDD, DD, MMMM, AAAA) — ce n'est pas un moteur DateVersChaîne complet.
        // ----------------------------------------
        private static readonly CultureInfo _cultureFr = new("fr-FR");

        private static string FormateDate(DateOnly date, string format)
        {
            DateTime dt = date.ToDateTime(TimeOnly.MinValue);
            string résultat = format;
            résultat = résultat.Replace("DDDD", dt.ToString("dddd", _cultureFr));
            résultat = résultat.Replace("DD", date.Day.ToString("00", CultureInfo.InvariantCulture));
            résultat = résultat.Replace("MMMM", dt.ToString("MMMM", _cultureFr));
            résultat = résultat.Replace("AAAA", date.Year.ToString("0000", CultureInfo.InvariantCulture));
            return résultat;
        }
        // ********************************************************************************
        /// <summary>Construit une chaîne "du DATEMIN au DATEMAX" correctement formatée.</summary>
        /// <param name="date1">Date de début ou de fin.</param>
        /// <param name="date2">Date de début ou de fin.</param>
        /// <param name="format">Le formatage demandé (jetons DDDD/DD/MMMM/AAAA uniquement).</param>
        /// <returns>La chaîne de la période.</returns>
        public static string LibellePeriode(DateOnly date1, DateOnly date2, string format = "DDDD DD MMMM AAAA")
        {
            DateOnly min = DateMin(date1, date2);
            DateOnly max = DateMax(date1, date2);
            return $"du {FormateDate(min, format)} au {FormateDate(max, format)}";
        }
        // ********************************************************************************
        /// <summary>Construit une chaîne "du DATEMIN au DATEMAX" pour l'année fiscale qui contient date.</summary>
        /// <param name="date">Date de l'exercice.</param>
        /// <param name="débutAnnéeFiscale">Le mois de début de l'année fiscale (1 par défaut).</param>
        /// <param name="format">Le formatage demandé (jetons DDDD/DD/MMMM/AAAA uniquement).</param>
        /// <returns>La chaîne de l'année fiscale.</returns>
        public static string LibelléAnnéeFiscale(DateOnly date, int débutAnnéeFiscale = 1, string format = "DDDD DD MMMM AAAA")
        {
            DateOnly min = DebutAnneeFiscale(date, débutAnnéeFiscale);
            DateOnly max = FinAnneeFiscale(date, débutAnnéeFiscale);
            return $"du {FormateDate(min, format)} au {FormateDate(max, format)}";
        }
        // ********************************************************************************
        /// <summary>Renvoie les mois, parmi les nbMois prochains à partir du mois en cours, qui contiennent un vendredi 13.</summary>
        /// <param name="nbMois">Le nombre de mois à examiner à partir du mois en cours (12 par défaut).</param>
        /// <returns>La liste des dates de vendredi 13 trouvées.</returns>
        public static List<DateOnly> ListeVendredi13(int nbMois = 12)
        {
            var résultat = new List<DateOnly>();
            if (nbMois < 1) return résultat;

            DateOnly jour = PremierJourDuMois(DateOnly.FromDateTime(DateTime.Today));
            for (int i = 0; i < nbMois; i++)
            {
                // Le 13 tombe un vendredi si et seulement si le 1er du mois tombe un dimanche.
                if (jour.DayOfWeek == DayOfWeek.Sunday)
                    résultat.Add(new DateOnly(jour.Year, jour.Month, 13));
                jour = jour.AddMonths(1);
            }
            return résultat;
        }
        // ********************************************************************************
        /// <summary>
        /// Renvoie la date du n-ième jour de semaine d'un mois donné.
        /// Exemple : NemeJourSemaineDuMois(2026, 5, 2, 3) = le 3e mardi de mai 2026 (19/05/2026).
        /// </summary>
        /// <param name="année">L'année (bornée entre 1800 et 3000).</param>
        /// <param name="mois">Le mois (1-12).</param>
        /// <param name="jourSemaine">Le jour de semaine visé (1=lundi ... 7=dimanche).</param>
        /// <param name="n">Le rang demandé (1 à 5).</param>
        /// <returns>La date correspondante, ou null si ce rang n'existe pas dans le mois.</returns>
        public static DateOnly? NemeJourSemaineDuMois(int année, int mois, int jourSemaine, int n = 1)
        {
            année = Math.Clamp(année, 1800, 3000);
            mois = Math.Clamp(mois, 1, 12);
            jourSemaine = Math.Clamp(jourSemaine, 1, 7);
            n = Math.Clamp(n, 1, 5);

            DateOnly premierDuMois = new(année, mois, 1);
            int jourSemainePremier = premierDuMois.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)premierDuMois.DayOfWeek;

            int décalage = (jourSemaine - jourSemainePremier + 7) % 7;
            DateOnly premierJourVoulu = premierDuMois.AddDays(décalage);
            DateOnly résultat = premierJourVoulu.AddDays((n - 1) * 7);

            return résultat.Month == mois ? résultat : null;
        }
        // ********************************************************************************
        /// <summary>Renvoie le nombre de jours ouvrables du mois de la date passée en paramètre.</summary>
        /// <param name="date">Une date du mois à étudier.</param>
        /// <returns>Le nombre de jours ouvrables du mois.</returns>
        public static int NombreJourOuvrableDuMois(DateOnly date)
        {
            int nbJours = 0;
            int dernierJour = DernierJourDuMois(date).Day;
            for (int jour = 1; jour <= dernierJour; jour++)
                if (EstOuvrable(new DateOnly(date.Year, date.Month, jour))) nbJours++;
            return nbJours;
        }
        // ********************************************************************************
        /// <summary>Renvoie le nombre de jours ouvrés du mois de la date passée en paramètre.</summary>
        /// <param name="date">Une date du mois à étudier.</param>
        /// <returns>Le nombre de jours ouvrés du mois.</returns>
        public static int NombreJourOuvresDuMois(DateOnly date)
        {
            int nbJours = 0;
            int dernierJour = DernierJourDuMois(date).Day;
            for (int jour = 1; jour <= dernierJour; jour++)
                if (Estouvré(new DateOnly(date.Year, date.Month, jour))) nbJours++;
            return nbJours;
        }
        // ********************************************************************************
        /// <summary>Renvoie le 1er jour de l'heure d'été (dernier dimanche de mars).</summary>
        /// <param name="date">Une date de l'année considérée.</param>
        /// <returns>Le premier jour de l'heure d'été.</returns>
        public static DateOnly PremierJourEte(DateOnly date)
        {
            DateOnly dernierJourMars = DernierJourDuMois(new DateOnly(date.Year, 3, 1));
            int décalage = dernierJourMars.DayOfWeek == DayOfWeek.Sunday ? 0 : (int)dernierJourMars.DayOfWeek;
            return dernierJourMars.AddDays(-décalage);
        }
        // ********************************************************************************
        /// <summary>Renvoie le 1er jour de l'heure d'hiver (dernier dimanche d'octobre).</summary>
        /// <param name="date">Une date de l'année considérée.</param>
        /// <returns>Le premier jour de l'heure d'hiver.</returns>
        public static DateOnly PremierJourHiver(DateOnly date)
        {
            DateOnly candidat = PremierJourEte(date).AddDays(210); // toujours un dimanche
            DateOnly suivant = candidat.AddDays(7);
            return suivant.Month == 10 ? suivant : candidat;
        }
        // ********************************************************************************
        /// <summary>Renvoie le prochain vendredi 13 à partir d'une date donnée.</summary>
        /// <param name="date">La date à partir de laquelle chercher.</param>
        /// <returns>La date du prochain vendredi 13, ou null si aucun n'est trouvé dans les
        /// ~100 prochains mois (ne devrait jamais arriver).</returns>
        /// <remarks>
        /// La version WLangage testait d'abord le mois de départ puis bouclait sur 100 mois
        /// supplémentaires, et pouvait renvoyer une date qui n'était pas un vendredi 13 si
        /// aucun n'était trouvé dans cette fenêtre. Ici, une seule boucle de 101 mois fait le
        /// même travail et renvoie null plutôt qu'une date incorrecte en cas d'échec.
        /// </remarks>
        public static DateOnly? ProchainVendredi13(DateOnly date)
        {
            DateOnly candidat = date.Day < 14 ? PremierJourDuMois(date) : PremierJourDuMois(date).AddMonths(1);

            for (int i = 0; i < 101; i++)
            {
                if (candidat.DayOfWeek == DayOfWeek.Sunday)
                    return new DateOnly(candidat.Year, candidat.Month, 13);
                candidat = candidat.AddMonths(1);
            }
            return null;
        }
    }
}
