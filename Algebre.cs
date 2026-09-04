using System;
using System.Collections.Generic;
using System.Globalization;

namespace ChronoComp
{
    /// <summary>
    /// Algèbre / résolution numérique : matrices et équations.
    ///
    /// Adaptation : la version WLangage manipulait des matrices nommées globalement
    /// (MatCrée/MatEcrit/MatLit/...), avec des paramètres "nom de matrice" en chaîne — un
    /// mécanisme équivalent existe déjà côté bibliothèque WL (WL.Numeriques.MatXxx), mais
    /// ChronoComp ne référence pas ce projet ; les fonctions ci-dessous travaillent donc
    /// directement sur des double[,] / double[] C#, ce qui revient au même sans la
    /// plomberie de nommage (si tu préfères t'appuyer sur WL.Numeriques à la place,
    /// dis-le-moi, j'ajouterai la référence de projet et je réécrirai ces fonctions
    /// par-dessus MatCrée/MatEcrit/MatLit).
    /// MDichotomie prenait une chaîne de formule évaluée avec ÉvalueExpression (fonction
    /// WLangage non portée) ; elle prend ici directement un Func&lt;double, double&gt;, plus
    /// idiomatique en C#.
    /// </summary>
    public static class Algebre
    {
        // ********************************************************************************
        /// <summary>
        /// Transforme une matrice en une matrice carrée ; les cases manquantes sont
        /// complétées par valeurManquante.
        /// </summary>
        /// <param name="matrice">La matrice de départ.</param>
        /// <param name="valeurManquante">La valeur à mettre dans les cases manquantes.</param>
        /// <returns>Une nouvelle matrice carrée (les tableaux C# étant de taille fixe, le
        /// résultat est une nouvelle matrice plutôt qu'une modification en place).</returns>
        public static double[,] MatriceCarrée(double[,] matrice, double valeurManquante)
        {
            int nbLignes = matrice.GetLength(0);
            int nbColonnes = matrice.GetLength(1);
            int taille = Math.Max(nbLignes, nbColonnes);

            var résultat = new double[taille, taille];
            for (int i = 0; i < taille; i++)
                for (int j = 0; j < taille; j++)
                    résultat[i, j] = (i < nbLignes && j < nbColonnes) ? matrice[i, j] : valeurManquante;

            return résultat;
        }
        // ********************************************************************************
        /// <summary>
        /// Transforme une matrice en une chaîne de L lignes, chaque ligne étant une chaîne
        /// de C éléments séparés par séparateurColonne.
        /// </summary>
        /// <param name="matrice">La matrice à convertir.</param>
        /// <param name="séparateurColonne">Le séparateur entre colonnes (tabulation par défaut).</param>
        /// <returns>La matrice sous forme de texte, une ligne par ligne de matrice.</returns>
        public static string MatriceVersChaine(double[,] matrice, char séparateurColonne = '\t')
        {
            int nbLignes = matrice.GetLength(0);
            int nbColonnes = matrice.GetLength(1);
            if (nbLignes == 0 || nbColonnes == 0)
                return "";

            var lignes = new List<string>(nbLignes);
            for (int i = 0; i < nbLignes; i++)
            {
                var valeurs = new string[nbColonnes];
                for (int j = 0; j < nbColonnes; j++)
                    valeurs[j] = matrice[i, j].ToString(CultureInfo.InvariantCulture);
                lignes.Add(string.Join(séparateurColonne, valeurs));
            }
            return string.Join('\n', lignes);
        }
        // ********************************************************************************
        /// <summary>
        /// Regroupe un tableau de tableaux (les lignes) en une matrice rectangulaire.
        /// </summary>
        /// <param name="tableau">Le tableau de lignes à assembler (toutes les lignes
        /// doivent avoir la même longueur).</param>
        /// <returns>La matrice correspondante, ou null si tableau est vide ou si ses lignes
        /// n'ont pas toutes la même longueur.</returns>
        public static double[,]? TableauVersMatrice(double[][] tableau)
        {
            int nbLignes = tableau.Length;
            if (nbLignes == 0)
                return null;
            int nbColonnes = tableau[0].Length;
            if (nbColonnes == 0)
                return null;

            var matrice = new double[nbLignes, nbColonnes];
            for (int i = 0; i < nbLignes; i++)
            {
                if (tableau[i].Length != nbColonnes)
                    return null;
                for (int j = 0; j < nbColonnes; j++)
                    matrice[i, j] = tableau[i][j];
            }
            return matrice;
        }
        // ********************************************************************************
        /// <summary>
        /// Résout par dichotomie fonction(x) = 0 entre limiteBasse et limiteHaute.
        /// </summary>
        /// <param name="fonction">La fonction dont on cherche le zéro.</param>
        /// <param name="limiteBasse">Borne basse de la recherche.</param>
        /// <param name="limiteHaute">Borne haute de la recherche.</param>
        /// <param name="approximation">Précision demandée (1E-7 par défaut).</param>
        /// <param name="maxItérations">Nombre maximal d'itérations (100 par défaut).</param>
        /// <returns>La solution trouvée, ou 0 si aucune racine n'est garantie sur l'intervalle
        /// (fonction(limiteBasse) et fonction(limiteHaute) de même signe).</returns>
        public static double MDichotomie(
            Func<double, double> fonction, double limiteBasse, double limiteHaute,
            double approximation = 1e-7, int maxItérations = 100)
        {
            double fa = fonction(limiteBasse);
            double fb = fonction(limiteHaute);

            if (fa == 0) return limiteBasse;
            if (fb == 0) return limiteHaute;
            if (fa * fb > 0) return 0; // pas de racine garantie

            double résultat = (limiteBasse + limiteHaute) / 2;
            for (int i = 0; i < maxItérations; i++)
            {
                résultat = (limiteBasse + limiteHaute) / 2;
                double fm = fonction(résultat);
                if (Math.Abs(fm) <= approximation || (limiteHaute - limiteBasse) / 2 <= approximation)
                    return résultat;

                if (fa * fm < 0)
                {
                    limiteHaute = résultat;
                    fb = fm;
                }
                else
                {
                    limiteBasse = résultat;
                    fa = fm;
                }
            }
            return (limiteHaute + limiteBasse) / 2;
        }
        // ********************************************************************************
        /// <summary>
        /// Résout A x = b par élimination de Gauss avec pivot partiel.
        /// </summary>
        /// <param name="a">La matrice A (n x n).</param>
        /// <param name="b">Le vecteur b (n éléments).</param>
        /// <param name="x">En sortie, la solution x (n éléments), ou null en cas d'échec.</param>
        /// <param name="codeErreur">En sortie : 0 = OK, 1 = dimensions incompatibles,
        /// 2 = matrice singulière.</param>
        /// <returns>Vrai si le système a été résolu.</returns>
        public static bool MSolveAXB(double[,] a, double[] b, out double[]? x, out int codeErreur)
        {
            codeErreur = 1;
            x = null;

            int n = a.GetLength(0);
            if (n == 0 || a.GetLength(1) != n || b.Length != n)
                return false;

            double[,] matriceA = (double[,])a.Clone();
            double[] vecteurB = (double[])b.Clone();
            const double epsilon = 1e-12;
            codeErreur = 0;

            for (int k = 0; k < n; k++)
            {
                int ligneDuPivot = k;
                double maxAbs = Math.Abs(matriceA[k, k]);
                for (int i = k + 1; i < n; i++)
                {
                    double v = Math.Abs(matriceA[i, k]);
                    if (v > maxAbs) { maxAbs = v; ligneDuPivot = i; }
                }

                if (maxAbs <= epsilon)
                {
                    codeErreur = 2;
                    return false;
                }

                if (ligneDuPivot != k)
                {
                    for (int j = 0; j < n; j++)
                        (matriceA[k, j], matriceA[ligneDuPivot, j]) = (matriceA[ligneDuPivot, j], matriceA[k, j]);
                    (vecteurB[k], vecteurB[ligneDuPivot]) = (vecteurB[ligneDuPivot], vecteurB[k]);
                }

                double pivot = matriceA[k, k];
                for (int i = k + 1; i < n; i++)
                {
                    double facteur = matriceA[i, k] / pivot;
                    for (int j = k; j < n; j++)
                        matriceA[i, j] -= facteur * matriceA[k, j];
                    vecteurB[i] -= facteur * vecteurB[k];
                }
            }

            var résultat = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                double somme = vecteurB[i];
                for (int j = i + 1; j < n; j++)
                    somme -= matriceA[i, j] * résultat[j];

                double diagonale = matriceA[i, i];
                if (Math.Abs(diagonale) <= epsilon)
                {
                    codeErreur = 2;
                    return false;
                }
                résultat[i] = somme / diagonale;
            }

            x = résultat;
            return true;
        }
    }
}
