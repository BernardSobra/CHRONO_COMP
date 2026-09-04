using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace ChronoComp
{
    /// <summary>
    /// Fonctions d'exploration du registre Windows.
    ///
    /// Adaptation : la version WLangage originale (_AnalyseRegistreClé / _LitClé) remplissait
    /// une table globale liée à une fenêtre (tabRegistre) puis copiait le résultat au format
    /// CSV dans le presse-papiers — deux effets de bord propres à une procédure de fenêtre
    /// WINDEV, pas à une fonction de bibliothèque réutilisable. Ici, AnalyseRegistreClé()
    /// renvoie directement la liste des couples (clé, nom de valeur, type, valeur) ; utilise
    /// VersCsv(...) si tu veux reproduire le texte qui partait auparavant au presse-papiers
    /// (à copier toi-même avec Clipboard.SetText si besoin).
    /// La double passe de l'original (liste complète des clés, puis relecture de chaque clé
    /// pour ses valeurs) est également simplifiée en un seul parcours récursif.
    /// </summary>
    public static class Registre
    {
        /// <summary>Une ligne du registre : une clé, un nom de valeur, son type et sa valeur.</summary>
        public sealed record LigneRegistre(string Clé, string NomValeur, string TypeValeur, string? Valeur);

        // ********************************************************************************
        /// <summary>
        /// Parcourt récursivement toutes les sous-clés du registre à partir d'une clé racine
        /// et renvoie la liste des couples (clé, nom de valeur, type, valeur) rencontrés.
        /// </summary>
        /// <param name="cléRacine">Clé de départ (par défaut Constantes.C_BaseRegistreV2026).
        /// Pour une autre version de WINDEV, adapter le numéro de version dans la clé.</param>
        /// <returns>La liste des lignes trouvées (vide si la clé racine n'existe pas).</returns>
        public static List<LigneRegistre> AnalyseRegistreClé(string cléRacine = Constantes.C_BaseRegistreV2026)
        {
            var résultat = new List<LigneRegistre>();
            if (string.IsNullOrEmpty(cléRacine))
                return résultat;

            Parcourt(cléRacine, résultat);
            return résultat;
        }
        // ----------------------------------------
        // Parcourt : ajoute les valeurs de cheminClé à résultat, puis descend
        // récursivement dans chacune de ses sous-clés (triées alphabétiquement).
        // ----------------------------------------
        private static void Parcourt(string cheminClé, List<LigneRegistre> résultat)
        {
            using RegistryKey? clé = OuvreClé(cheminClé);
            if (clé is null)
                return;

            foreach (string nomValeur in clé.GetValueNames().OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
            {
                object? valeur = clé.GetValue(nomValeur);
                RegistryValueKind type = clé.GetValueKind(nomValeur);
                résultat.Add(new LigneRegistre(
                    cheminClé,
                    nomValeur.Length == 0 ? "(défaut)" : nomValeur,
                    type.ToString(),
                    valeur?.ToString()));
            }

            foreach (string sousNom in clé.GetSubKeyNames().OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
                Parcourt(cheminClé + "\\" + sousNom, résultat);
        }
        // ----------------------------------------
        // OuvreClé : ouvre une clé à partir d'un chemin complet "HKEY_XXX\Sous\Chemin".
        // Renvoie null si la ruche est inconnue ou si la clé n'existe pas.
        // ----------------------------------------
        private static RegistryKey? OuvreClé(string chemin)
        {
            string[] segments = chemin.Split('\\', 2);

            RegistryKey? racine = segments[0].ToUpperInvariant() switch
            {
                "HKEY_CURRENT_USER" => Registry.CurrentUser,
                "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
                "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
                "HKEY_USERS" => Registry.Users,
                "HKEY_CURRENT_CONFIG" => Registry.CurrentConfig,
                _ => null,
            };
            if (racine is null)
                return null;

            return segments.Length == 1 ? racine : racine.OpenSubKey(segments[1]);
        }
        // ********************************************************************************
        /// <summary>
        /// Transforme une liste de lignes de registre en une chaîne CSV (une ligne par
        /// LigneRegistre, colonnes séparées par séparateurColonne).
        /// </summary>
        /// <param name="lignes">Les lignes à convertir (typiquement le résultat de
        /// AnalyseRegistreClé).</param>
        /// <param name="séparateurColonne">Le séparateur de colonnes (tabulation par défaut,
        /// comme dans la version WLangage d'origine).</param>
        /// <returns>Le texte CSV correspondant.</returns>
        public static string VersCsv(IEnumerable<LigneRegistre> lignes, char séparateurColonne = '\t')
        {
            var texte = new StringBuilder();
            foreach (LigneRegistre ligne in lignes)
            {
                texte.Append(ligne.Clé).Append(séparateurColonne);
                texte.Append(ligne.NomValeur).Append(séparateurColonne);
                texte.Append(ligne.TypeValeur).Append(séparateurColonne);
                texte.Append(ligne.Valeur);
                texte.Append('\n');
            }
            return texte.ToString();
        }
    }
}
