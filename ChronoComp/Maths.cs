using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ChronoComp
{
    /// <summary>
    /// Théorie des nombres (tests sur entiers) et statistiques sur des tableaux de réels.
    ///
    /// Nommée "Maths" (et non "Math") pour ne pas entrer en conflit avec System.Math, très
    /// utilisée à l'intérieur même de cette classe. Tu m'avais indiqué deux noms de fichiers
    /// différents pour ces deux groupes ("Math.cs" puis "ajouter à Maths.cs") : je les ai
    /// réunis dans ce seul fichier Maths.cs, ta deuxième phrase supposant que le premier
    /// groupe y était déjà.
    /// </summary>
    public static class Maths
    {
        #region THÉORIE DES NOMBRES
        // ********************************************************************************
        /// <summary>
        /// Détermine si un nombre est déficient, parfait ou abondant.
        /// Nombres déficients : la somme des diviseurs propres est inférieure au nombre.
        /// Nombres abondants : elle est supérieure. Nombres parfaits : elle est égale.
        /// </summary>
        /// <param name="valeur">L'entier à tester.</param>
        /// <returns>Constantes.C_Deficient, C_Abondant ou C_Parfait.</returns>
        public static int MAbondantDeficientParfait(long valeur)
        {
            long somme = ListeDiviseursPropres(Math.Abs(valeur)).Sum();
            if (somme < valeur) return Constantes.C_Deficient;
            if (somme > valeur) return Constantes.C_Abondant;
            return Constantes.C_Parfait;
        }
        // ----------------------------------------
        // ListeDiviseursPropres : diviseurs propres (hors le nombre lui-même) d'un entier
        // positif. Partagée par MAbondantDeficientParfait et MParfait (une seule copie ici,
        // la source WLangage la dupliquait en procédure interne dans chacune des deux).
        // ----------------------------------------
        private static List<long> ListeDiviseursPropres(long valeur)
        {
            var diviseurs = new List<long>();
            if (valeur <= 1) return diviseurs;
            for (long i = 1; i <= valeur / 2; i++)
                if (valeur % i == 0) diviseurs.Add(i);
            return diviseurs;
        }
        // ********************************************************************************
        /// <summary>
        /// Un nombre automorphe est un nombre n tel que n² se termine par n.
        /// Exemples : 1, 5 (25), 6 (36), 25 (625), 76 (5776), 376 (141376), 625 (390625)...
        /// </summary>
        /// <param name="valeur">Le nombre à tester.</param>
        /// <returns>Vrai si le nombre est automorphe.</returns>
        public static bool Mautomorphe(long valeur)
        {
            long v = Math.Abs(valeur);
            long carré = v * v;
            int nbChiffres = v.ToString(CultureInfo.InvariantCulture).Length;
            long modulo = (long)Math.Pow(10, nbChiffres);
            return carré % modulo == v;
        }
        // ********************************************************************************
        /// <summary>
        /// Fonction de Carmichael λ(n), pour n ≥ 0 (0 -> 0, 1 -> 1).
        /// Un nombre de Carmichael est un nombre composé qui passe le test de Fermat pour
        /// tous les entiers premiers avec lui (561, 1105, 1729, 2465, ...).
        /// Tests : MCarmichael(561) = 80, MCarmichael(64) = 16, MCarmichael(105) = 12,
        /// MCarmichael(15) = 4.
        /// </summary>
        /// <param name="entier">L'entier à calculer.</param>
        /// <returns>λ(entier).</returns>
        public static long MCarmichael(long entier)
        {
            entier = Math.Abs(entier);
            if (entier == 0) return 0;
            if (entier == 1) return 1;

            long lambda = 1;

            // Facteur 2 : n = 2^k * m — λ(2)=1, λ(4)=2, λ(2^k)=2^(k-2) pour k>=3
            int k = 0;
            while (entier % 2 == 0) { entier /= 2; k++; }
            if (k > 0)
            {
                long l2k = k switch { 1 => 1, 2 => 2, _ => MPowInt(2, k - 2) };
                lambda = MPpcm(lambda, l2k);
            }

            // Facteurs impairs : n = Π p^k — λ(p^k) = (p-1) * p^(k-1) pour p impair
            long p = 3;
            while (p * p <= entier)
            {
                int kp = 0;
                while (entier % p == 0) { entier /= p; kp++; }
                if (kp > 0)
                    lambda = MPpcm(lambda, (p - 1) * MPowInt(p, kp - 1));
                p += 2;
            }

            // S'il reste un facteur premier (exposant 1) : λ(p) = p - 1
            if (entier > 1)
                lambda = MPpcm(lambda, entier - 1);

            return lambda;
        }
        // ********************************************************************************
        /// <summary>
        /// Calcule le terme n de la suite de Fibonacci.
        /// Tests : 0 -> 0, 1 -> 1, 5 -> 5, 10 -> 55, 20 -> 6765, 80 -> 23416728348467685.
        /// </summary>
        /// <param name="n">Le rang demandé (les rangs négatifs sont acceptés, via
        /// l'identité F(-n) = (-1)^(n+1) F(n) — nécessaire à MLucas, qui appelle
        /// MFibonacci(n - 1) et doit donc pouvoir calculer F(-1)).</param>
        /// <returns>F(n).</returns>
        public static long MFibonacci(int n)
        {
            if (n >= 0) return FibonacciPositif(n);

            long f = FibonacciPositif(-n);
            return -n % 2 == 0 ? -f : f;
        }
        // ----------------------------------------
        private static long FibonacciPositif(int n)
        {
            if (n == 0) return 0;
            if (n == 1) return 1;

            long préc1 = 0, préc2 = 1, résultat = 0;
            for (int i = 2; i <= n; i++)
            {
                résultat = préc1 + préc2;
                préc1 = préc2;
                préc2 = résultat;
            }
            return résultat;
        }
        // ********************************************************************************
        /// <summary>
        /// Indique si un nombre est heureux : en remplaçant répétitivement le nombre par la
        /// somme des carrés de ses chiffres, on finit par obtenir 1.
        /// </summary>
        /// <param name="nombre">Le nombre à tester (ex : 2026 -> vrai).</param>
        /// <returns>Vrai si le nombre est heureux.</returns>
        public static bool MHeureux(long nombre)
        {
            long résultat = SommeDesCarrésDesChiffres(Math.Abs(nombre));
            while (résultat > 9)
                résultat = SommeDesCarrésDesChiffres(résultat);
            return résultat == 1;
        }
        // ----------------------------------------
        private static long SommeDesCarrésDesChiffres(long nombre)
        {
            long somme = 0;
            foreach (char c in nombre.ToString(CultureInfo.InvariantCulture))
                somme += (long)Math.Pow(c - '0', 2);
            return somme;
        }
        // ********************************************************************************
        /// <summary>
        /// Terme n de la suite de Lucas-Lehmer : L(n) = F(n-1) + F(n+1).
        /// Tests : L(0)=2, L(1)=1, L(2)=3, L(5)=11, L(10)=123.
        /// </summary>
        /// <param name="n">L'entier à calculer.</param>
        /// <returns>La valeur du terme de Lucas.</returns>
        public static long MLucas(int n)
        {
            return MFibonacci(n - 1) + MFibonacci(n + 1);
        }
        // ********************************************************************************
        /// <summary>
        /// Renvoie vrai si la valeur absolue du nombre est narcissique (nombre d'Armstrong) :
        /// égal à la somme de ses chiffres élevés à la puissance du nombre de chiffres.
        /// Exemple : 153 -> 1³ + 5³ + 3³ = 153.
        /// </summary>
        /// <param name="valeur">Le nombre à tester.</param>
        /// <returns>Vrai si le nombre est narcissique.</returns>
        public static bool MNarcissique(long valeur)
        {
            string chiffres = Math.Abs(valeur).ToString(CultureInfo.InvariantCulture);
            int puissance = chiffres.Length;
            long somme = chiffres.Sum(c => (long)Math.Pow(c - '0', puissance));
            return valeur == somme;
        }
        // ********************************************************************************
        /// <summary>
        /// Un nombre de Smith est un nombre composé dont la somme des chiffres est égale à
        /// la somme des chiffres de ses facteurs premiers.
        /// </summary>
        /// <param name="n">Le nombre à tester.</param>
        /// <returns>Vrai si n est un nombre de Smith.</returns>
        public static bool MNbrSmith(long n)
        {
            if (n < 4 || MPremier(n)) return false;

            long sommeChiffres = SommeDesChiffres(n);
            long valeur = n;
            long sommeFacteurs = 0;
            long i = 2;
            while (i * i <= valeur)
            {
                while (valeur % i == 0)
                {
                    sommeFacteurs += SommeDesChiffres(i);
                    valeur /= i;
                }
                i++;
            }
            if (valeur > 1)
                sommeFacteurs += SommeDesChiffres(valeur);

            return sommeChiffres == sommeFacteurs;
        }
        // ----------------------------------------
        private static long SommeDesChiffres(long nombre)
        {
            nombre = Math.Abs(nombre);
            long somme = 0;
            foreach (char c in nombre.ToString(CultureInfo.InvariantCulture))
                somme += c - '0';
            return somme;
        }
        // ********************************************************************************
        /// <summary>
        /// Indique si un nombre est un palindrome (se lit pareil dans les deux sens).
        /// Exemples : 121, 91233219 -> vrai ; 102442010 -> faux.
        /// </summary>
        /// <param name="valeur">Le nombre à tester.</param>
        /// <returns>Vrai si le nombre est un palindrome.</returns>
        public static bool MPalindrome(long valeur)
        {
            return valeur == Inverse(Math.Abs(valeur));
        }
        // ----------------------------------------
        private static long Inverse(long valeur)
        {
            char[] chiffres = valeur.ToString(CultureInfo.InvariantCulture).ToCharArray();
            Array.Reverse(chiffres);
            return long.Parse(new string(chiffres), CultureInfo.InvariantCulture);
        }
        // ********************************************************************************
        /// <summary>
        /// Renvoie vrai si un nombre est parfait : égal à la somme de ses diviseurs propres.
        /// Exemple : 28 -> 1 + 2 + 4 + 7 + 14 = 28.
        /// </summary>
        /// <param name="valeur">L'entier à tester.</param>
        /// <returns>Vrai si le nombre est parfait.</returns>
        public static bool MParfait(long valeur)
        {
            return ListeDiviseursPropres(Math.Abs(valeur)).Sum() == valeur;
        }
        // ********************************************************************************
        /// <summary>Renvoie le PGCD (plus grand commun diviseur) de deux entiers.</summary>
        /// <param name="a">Le premier entier.</param>
        /// <param name="b">Le deuxième entier.</param>
        /// <returns>Le PGCD de a et b.</returns>
        public static long MPgcd(long a, long b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);
            while (b != 0)
            {
                (a, b) = (b, a % b);
            }
            return a;
        }
        // ********************************************************************************
        /// <summary>
        /// Puissance entière (valeurBase^exposant), pour des entiers sur 8 octets.
        /// Tests : MPowInt(2, 10) = 1024, MPowInt(123, 0) = 1, MPowInt(-3, 5) = -243,
        /// MPowInt(-3, 4) = 81.
        /// </summary>
        /// <param name="valeurBase">La base.</param>
        /// <param name="exposant">L'exposant demandé.</param>
        /// <returns>valeurBase élevé à la puissance exposant.</returns>
        public static long MPowInt(long valeurBase, int exposant)
        {
            if (exposant <= 0) return 1;
            long résultat = 1, b = valeurBase;
            int e = exposant;
            while (e > 0)
            {
                if (e % 2 == 1) résultat *= b;
                e /= 2;
                if (e > 0) b *= b;
            }
            return résultat;
        }
        // ********************************************************************************
        /// <summary>Renvoie le PPCM (plus petit commun multiple) de deux entiers.</summary>
        /// <param name="a">Le premier entier.</param>
        /// <param name="b">Le deuxième entier.</param>
        /// <returns>Le PPCM de a et b.</returns>
        public static long MPpcm(long a, long b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);
            if (a == 0 || b == 0) return 0;
            return Math.Abs(a / MPgcd(a, b) * b);
        }
        // ********************************************************************************
        /// <summary>
        /// Indique si un entier est premier (strictement supérieur à 1, n'ayant que 1 et
        /// lui-même comme diviseurs).
        /// </summary>
        /// <param name="valeur">L'entier à tester.</param>
        /// <returns>Vrai si valeur est premier.</returns>
        /// <remarks>
        /// La source WLangage démarrait la boucle d'essai des diviseurs à "0n3" (503 d'après
        /// le commentaire d'origine) : avec cette borne, un nombre comme 591 = 3 × 197
        /// (impair, absent de Constantes.C_NbrePremier, inférieur à sa borne de test)
        /// aurait été déclaré premier à tort, la boucle "de 503 à √591≈24" ne s'exécutant
        /// jamais. Ici, la boucle démarre à 3 (le premier diviseur impair possible), ce qui
        /// est le comportement correct.
        /// </remarks>
        public static bool MPremier(long valeur)
        {
            if (valeur < 2) return false;
            if (valeur == 2) return true;
            if (valeur % 2 == 0) return false;

            if (valeur < 600)
                return valeur <= int.MaxValue && Constantes.C_NbrePremier.Contains((int)valeur);

            long borneMax = (long)Math.Sqrt(valeur);
            for (long i = 3; i <= borneMax; i += 2)
                if (valeur % i == 0) return false;

            return true;
        }
        #endregion

        #region STATISTIQUES SUR TABLEAUX DE RÉELS
        // ********************************************************************************
        /// <summary>
        /// Moyenne géométrique d'un tableau de réels strictement positifs (taux de
        /// croissance, rendements, facteurs multiplicatifs).
        /// Test : MGéométrique([8, 12, 15, 98, 64, 52]) = 27,8801633672.
        /// </summary>
        /// <param name="valeurs">Le tableau de réels.</param>
        /// <returns>La moyenne géométrique, ou null si le tableau est vide ou contient une
        /// valeur négative ou nulle.</returns>
        public static double? MGéométrique(IReadOnlyList<double> valeurs)
        {
            if (valeurs.Count == 0 || valeurs.Any(v => v <= 0)) return null;
            double sommeLn = valeurs.Sum(Math.Log);
            return Math.Exp(sommeLn / valeurs.Count);
        }
        // ********************************************************************************
        /// <summary>
        /// Moyenne harmonique d'un tableau de réels strictement positifs (vitesses
        /// moyennes, ratios).
        /// Test : MHarmonique([8, 12, 15, 98, 64, 52]) = 18,7464937693.
        /// </summary>
        /// <param name="valeurs">Le tableau de réels.</param>
        /// <returns>La moyenne harmonique, ou null si le tableau est vide ou contient une
        /// valeur négative ou nulle.</returns>
        public static double? MHarmonique(IReadOnlyList<double> valeurs)
        {
            if (valeurs.Count == 0 || valeurs.Any(v => v <= 0)) return null;
            double sommeInverses = valeurs.Sum(v => 1 / v);
            return valeurs.Count / sommeInverses;
        }
        // ********************************************************************************
        /// <summary>Médiane d'un tableau de réels.</summary>
        /// <param name="valeurs">Le tableau de réels.</param>
        /// <returns>La médiane, ou null si le tableau est vide.</returns>
        public static double? MMediane(IReadOnlyList<double> valeurs)
        {
            if (valeurs.Count == 0) return null;
            double[] triées = valeurs.OrderBy(v => v).ToArray();
            int n = triées.Length;
            return n % 2 == 1 ? triées[(n - 1) / 2] : (triées[n / 2 - 1] + triées[n / 2]) / 2.0;
        }
        // ********************************************************************************
        /// <summary>
        /// Moyenne mobile de tailleFenêtre éléments sur un tableau de réels (lissage d'une
        /// série : capteurs, ventes/jour, détection d'anomalies).
        /// </summary>
        /// <param name="valeurs">Le tableau de réels.</param>
        /// <param name="tailleFenêtre">Le nombre d'éléments moyennés à chaque pas.</param>
        /// <returns>Le tableau des moyennes mobiles, ou null si le tableau est vide ou si
        /// tailleFenêtre dépasse le nombre de valeurs.</returns>
        public static double[]? MMobile(IReadOnlyList<double> valeurs, int tailleFenêtre)
        {
            int k = Math.Abs(tailleFenêtre);
            int n = valeurs.Count;
            if (n == 0 || k > n) return null;

            var résultat = new double[n - k + 1];
            double somme = 0;
            for (int i = 0; i < k; i++) somme += valeurs[i];
            résultat[0] = somme / k;

            for (int i = k; i < n; i++)
            {
                somme += valeurs[i] - valeurs[i - k];
                résultat[i - k + 1] = somme / k;
            }
            return résultat;
        }
        // ********************************************************************************
        /// <summary>
        /// Moyenne pondérée d'un tableau de réels strictement positifs par un tableau de
        /// poids de même taille.
        /// </summary>
        /// <param name="valeurs">Les valeurs à moyenner (doivent être strictement positives).</param>
        /// <param name="poids">Les poids correspondants (même taille que valeurs).</param>
        /// <returns>La moyenne pondérée, ou null en cas de tailles incompatibles, de tableau
        /// vide, ou de somme des poids nulle.</returns>
        public static double? MPondérée(IReadOnlyList<double> valeurs, IReadOnlyList<double> poids)
        {
            if (valeurs.Count == 0 || poids.Count == 0 || valeurs.Count != poids.Count || valeurs.Any(v => v <= 0))
                return null;

            double sommePoids = poids.Sum();
            if (sommePoids == 0) return null;

            double sommePondérée = 0;
            for (int i = 0; i < valeurs.Count; i++)
                sommePondérée += valeurs[i] * poids[i];

            return sommePondérée / sommePoids;
        }
        // ********************************************************************************
        /// <summary>
        /// Moyenne quadratique d'un tableau de réels (pénalise les grosses valeurs :
        /// signaux, erreurs).
        /// </summary>
        /// <param name="valeurs">Le tableau de réels.</param>
        /// <returns>La moyenne quadratique, ou null si le tableau est vide.</returns>
        public static double? MQuadratique(IReadOnlyList<double> valeurs)
        {
            if (valeurs.Count == 0) return null;
            double sommeCarrés = valeurs.Sum(v => v * v);
            return Math.Sqrt(sommeCarrés / valeurs.Count);
        }
        // ********************************************************************************
        /// <summary>
        /// Moyenne tronquée d'un tableau de réels : les k plus petites et k plus grandes
        /// valeurs sont retirées avant de moyenner.
        /// </summary>
        /// <param name="valeurs">Le tableau de réels.</param>
        /// <param name="k">Le nombre d'éléments retirés de chaque côté.</param>
        /// <returns>La moyenne tronquée, ou null si le tableau est vide ou si 2k ≥ n.</returns>
        public static double? MTronquée(IReadOnlyList<double> valeurs, int k)
        {
            int n = valeurs.Count;
            if (n == 0 || k < 0 || 2 * k >= n) return null;

            double[] triées = valeurs.OrderBy(v => v).ToArray();
            double somme = 0;
            for (int i = k; i < n - k; i++)
                somme += triées[i];

            return somme / (n - 2 * k);
        }
        // ********************************************************************************
        /// <summary>
        /// Moyenne winsorisée d'un tableau de réels : les k plus petites et k plus grandes
        /// valeurs sont remplacées par les valeurs-seuil (au lieu d'être retirées comme pour
        /// MTronquée), ce qui garde n valeurs tout en bornant les extrêmes.
        /// </summary>
        /// <remarks>
        /// La source WLangage annonçait "MWinsorisée([8, 12, 15, 98, 64, 52], 1) = 47,25" en
        /// commentaire, mais l'algorithme qu'elle décrit juste en dessous (repris ici à
        /// l'identique) donne bien 36,5 pour cet exemple — vérifié par le calcul et par un
        /// test automatisé. Le commentaire d'origine semble donc erroné (peut-être copié
        /// d'un autre essai) ; à confirmer avec toi si un autre comportement était attendu.
        /// </remarks>
        /// <param name="valeurs">Le tableau de réels.</param>
        /// <param name="k">Le nombre d'éléments à borner de chaque côté.</param>
        /// <returns>La moyenne winsorisée, ou null si le tableau est vide ou si 2k ≥ n.</returns>
        public static double? MWinsorisée(IReadOnlyList<double> valeurs, int k)
        {
            k = Math.Abs(k);
            int n = valeurs.Count;
            if (n == 0 || 2 * k >= n) return null;

            double[] triées = valeurs.OrderBy(v => v).ToArray();
            double bas = triées[k];
            double haut = triées[n - k - 1];

            double somme = 0;
            for (int i = 0; i < n; i++)
            {
                double v = triées[i];
                if (i < k) v = bas;
                else if (i >= n - k) v = haut;
                somme += v;
            }
            return somme / n;
        }
        #endregion
    }
}
