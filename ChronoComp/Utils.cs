using System;
using System.Diagnostics;

namespace ChronoComp
{
    /// <summary>
    /// Fonctions diverses / utilitaires n'ayant pas trouvé leur place dans les autres
    /// classes du composant.
    /// </summary>
    public static class Utils
    {
        // ********************************************************************************
        /// <summary>
        /// Renvoie "OUI" ou "NON" en fonction du booléen passé en paramètre.
        /// </summary>
        /// <param name="valeur">Le booléen à convertir.</param>
        /// <returns>"OUI" si valeur est vrai, "NON" sinon.</returns>
        public static string OUI_NON(bool valeur)
        {
            return valeur ? "OUI" : "NON";
        }
        // ********************************************************************************
        /// <summary>
        /// Indique si le code s'exécute avec un débogueur attaché.
        /// Adaptation de la fonction WLangage EnModeTest() (qui indique qu'on est dans
        /// l'environnement de test de WINDEV) : il n'existe pas d'équivalent .NET direct,
        /// on se rapproche donc du même besoin ("suis-je en train de déboguer ?") avec
        /// Debugger.IsAttached.
        /// </summary>
        /// <returns>Vrai si un débogueur est attaché au processus courant.</returns>
        public static bool EnModeTest()
        {
            return Debugger.IsAttached;
        }
        // ********************************************************************************
        /// <summary>
        /// Trace le message uniquement si EnModeTest() est vrai.
        /// Adaptation : la version WLangage appelait Trace(), qui repose ici sur le
        /// TraceViewer de la bibliothèque WL (WL.DiversUtils.Trace, non référencée depuis
        /// ChronoComp). On écrit donc dans la fenêtre de sortie du débogueur
        /// (Debug.WriteLine) ; si tu préfères réutiliser le TraceViewer de WL, il suffira
        /// d'ajouter une référence de projet vers WL et de remplacer l'appel ci-dessous.
        /// </summary>
        /// <param name="message">Le message à tracer.</param>
        public static void TraceModeTest(string message)
        {
            if (EnModeTest())
                Debug.WriteLine(message);
        }
    }
}
