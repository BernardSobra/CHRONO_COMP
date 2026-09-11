using System;
using System.Drawing;

namespace ChronoComp
{
    /// <summary>Type de figure pour MPérimètre.</summary>
    public enum ETypePérimètre
    {
        MpRectangle, MpCarre, MpCercle, MpTriangle, MpLosange, MpParallelogramme,
        MpTrapeze, MpEllipse_Approx, MpArcCercle, MpSecteurCercle,
    }

    /// <summary>Type de figure pour MSurface.</summary>
    public enum ETypeSurface
    {
        MsRectangle, MsCarre, MsCercle, MsLosange, MsTriangle_BH, MsParallelogramme,
        MsTrapeze, MsEllipse, MsCouronneCirculaire, MsTriangle_Heron, MsSecteurCercle,
        MsPolygoneRegulier_Rayon, MsPolygoneRegulier_Apotheme,
    }

    /// <summary>Type de surface développée pour MSurface3D.</summary>
    public enum EType3DSurface
    {
        Ms3Cube_Totale, Ms3PaveDroit_Totale, Ms3Cylindre_Laterale, Ms3Cylindre_Totale,
        Ms3Cone_Laterale, Ms3Cone_Totale, Ms3Sphere_Totale, Ms3DemiSphere_Totale,
    }

    /// <summary>Type de volume pour MVolume.</summary>
    public enum ETypeVolume
    {
        MvCube, MvPaveDroit, MvPrismeDroit, MvCylindre, MvCone, MvTroncDeCone,
        MvSphere, MvDemiSphere, MvPyramide, MvTroncDePyramide,
        // Conservé tel quel : la source WL d'origine nomme cette valeur "Mav..." et non
        // "Mv..." comme toutes ses voisines — probablement une coquille, mais je l'ai
        // recopiée fidèlement plutôt que de la renommer sans te le signaler.
        MavTetraedreRegulier,
        MvTore,
    }

    /// <summary>
    /// Calculs géométriques : périmètres, surfaces (2D et développées 3D), volumes,
    /// et intersection de rectangles.
    ///
    /// Les quatre énumérations ci-dessus remplacent COL_PROC_MATH.ETypePerimetre /
    /// ETypeSurface / EType3DSurface / ETypeVolume de la source WLangage, qui n'existaient
    /// que dans ton projet d'origine. Les paramètres rLong/rLar/rHaut/rCoté4 ont été
    /// renommés longueur/largeur/hauteur/côté4 pour rester lisibles en C# (le rôle exact de
    /// chacun change selon le cas — voir le commentaire de chaque branche).
    /// </summary>
    public static class Geometrie
    {
        // ********************************************************************************
        /// <summary>
        /// Calcule le périmètre d'une figure.
        /// </summary>
        /// <param name="type">La figure à calculer.</param>
        /// <param name="longueur">Premier paramètre de la figure (rôle variable selon le type).</param>
        /// <param name="largeur">Deuxième paramètre de la figure.</param>
        /// <param name="hauteur">Troisième paramètre de la figure.</param>
        /// <param name="côté4">Quatrième côté, pour les trapèzes.</param>
        /// <returns>Le périmètre, et un message d'erreur (vide si tout s'est bien passé).</returns>
        public static (double Valeur, string Message) MPérimètre(
            ETypePérimètre type, double longueur = 0, double largeur = 0, double hauteur = 0, double côté4 = 0)
        {
            longueur = Math.Abs(longueur);
            largeur = Math.Abs(largeur);
            hauteur = Math.Abs(hauteur);
            côté4 = Math.Abs(côté4);

            return type switch
            {
                ETypePérimètre.MpRectangle => (2 * (longueur + largeur), ""),                 // 2(L + l)
                ETypePérimètre.MpCarre => (4 * longueur, ""),                                  // 4c
                ETypePérimètre.MpCercle => (2 * ValPI * longueur, ""),                       // 2πr
                ETypePérimètre.MpTriangle => (longueur + largeur + hauteur, ""),               // a + b + c
                ETypePérimètre.MpLosange => (4 * longueur, ""),                                // 4c (si côté)
                ETypePérimètre.MpParallelogramme => (2 * (longueur + largeur), ""),            // 2(a + b)
                ETypePérimètre.MpTrapeze => (longueur + largeur + hauteur + côté4, ""),        // somme des 4 côtés
                // Approximation de Ramanujan : π [3(a + b) - √((3a + b)(a + 3b))] — longueur = a, largeur = b
                ETypePérimètre.MpEllipse_Approx => (ValPI * (3 * (longueur + largeur)
                    - Math.Sqrt((3 * longueur + largeur) * (longueur + 3 * largeur))), ""),
                ETypePérimètre.MpArcCercle => (longueur * largeur, ""),                        // r * θ — longueur = rayon, largeur = angle (rad)
                ETypePérimètre.MpSecteurCercle => (2 * longueur + longueur * largeur, ""),     // 2r + arc — largeur = angle (rad)
                _ => (0, "Type de périmètre non pris en charge."),
            };
        }
        // ********************************************************************************
        /// <summary>
        /// Calcule l'aire d'une figure plane.
        /// </summary>
        /// <param name="type">La figure à calculer.</param>
        /// <param name="longueur">Premier paramètre de la figure (rôle variable selon le type).</param>
        /// <param name="largeur">Deuxième paramètre de la figure.</param>
        /// <param name="hauteur">Troisième paramètre de la figure.</param>
        /// <returns>L'aire, et un message décrivant la formule utilisée (ou l'erreur).</returns>
        public static (double Valeur, string Message) MSurface(
            ETypeSurface type, double longueur = 0, double largeur = 0, double hauteur = 0)
        {
            longueur = Math.Abs(longueur);
            largeur = Math.Abs(largeur);
            hauteur = Math.Abs(hauteur);

            switch (type)
            {
                case ETypeSurface.MsRectangle:
                    return (longueur * largeur, Constantes.C_Formule + "Long * Larg");
                case ETypeSurface.MsCarre:
                    return (longueur * longueur, Constantes.C_Formule + "Long * Long");
                case ETypeSurface.MsCercle:
                    return (ValPI * longueur * longueur, Constantes.C_Formule + "π * Long * Long");
                case ETypeSurface.MsLosange:
                    return (longueur * largeur / 2, Constantes.C_Formule + "(Long * Larg) / 2");
                case ETypeSurface.MsTriangle_BH:
                    return (longueur * largeur / 2, Constantes.C_Formule + "(Long * Larg) / 2");
                case ETypeSurface.MsParallelogramme:
                    return (longueur * largeur, Constantes.C_Formule + "Long * Larg");
                case ETypeSurface.MsTrapeze:
                    return ((longueur + largeur) * hauteur / 2, Constantes.C_Formule + "(Long + Larg) * Haut / 2");
                case ETypeSurface.MsEllipse:
                    return (ValPI * longueur * largeur, Constantes.C_Formule + "π * Long * Larg");
                case ETypeSurface.MsCouronneCirculaire:
                    // longueur = grand rayon (R), largeur = petit rayon (r)
                    return (Math.Abs(ValPI * (longueur * longueur - largeur * largeur)),
                        Constantes.C_Formule + "π * (Long² - Larg²)");
                case ETypeSurface.MsTriangle_Heron:
                    // longueur = a, largeur = b, hauteur = c
                    if (longueur + largeur > hauteur && longueur + hauteur > largeur && largeur + hauteur > longueur)
                    {
                        double s = (longueur + largeur + hauteur) / 2;
                        return (Math.Sqrt(s * (s - longueur) * (s - largeur) * (s - hauteur)),
                            Constantes.C_Formule + "√[s * (s-Long) * (s-Larg) * (s-Haut)], s = (Long+Larg+Haut)/2");
                    }
                    return (0, Constantes.CErr_Descriptteur + " L'une des dimensions n'est pas conforme à la description attendue.");
                case ETypeSurface.MsSecteurCercle:
                    // longueur = rayon, largeur = angle (radians)
                    return (largeur / 2 * (longueur * longueur), Constantes.C_Formule + "Long² * Larg / 2 (Larg = angle en radians)");
                case ETypeSurface.MsPolygoneRegulier_Rayon:
                    int n = (int)longueur;
                    if (n >= 3 && largeur > 0)
                        return (n / 2.0 * (largeur * largeur) * Math.Sin(2.0 * ValPI / n),
                            Constantes.C_Formule + "(n / 2) * Larg² * sin(2π / n)");
                    return (0, Constantes.CErr_Descriptteur + " Moins de trois côtés, calcul impossible.");
                case ETypeSurface.MsPolygoneRegulier_Apotheme:
                    // (Périmètre * apothème) / 2
                    return (longueur * largeur / 2, Constantes.C_Formule + "Long * Larg / 2");
                default:
                    return (0, Constantes.CErr_Descriptteur + " Cas de calcul de surface non prévu.");
            }
        }
        // ********************************************************************************
        /// <summary>
        /// Calcule la surface développée d'un volume.
        /// </summary>
        /// <param name="type">Le volume dont on calcule la surface développée.</param>
        /// <param name="longueur">Premier paramètre (rôle variable selon le type).</param>
        /// <param name="largeur">Deuxième paramètre.</param>
        /// <param name="hauteur">Troisième paramètre.</param>
        /// <returns>La surface développée, et un message décrivant la formule utilisée (ou l'erreur).</returns>
        public static (double Valeur, string Message) MSurface3D(
            EType3DSurface type, double longueur = 0, double largeur = 0, double hauteur = 0)
        {
            longueur = Math.Abs(longueur);
            largeur = Math.Abs(largeur);
            hauteur = Math.Abs(hauteur);

            return type switch
            {
                EType3DSurface.Ms3Cube_Totale => (6 * longueur * longueur, Constantes.C_Formule + "6 * Long²"),
                EType3DSurface.Ms3PaveDroit_Totale => (2 * (longueur * largeur + longueur * hauteur + largeur * hauteur),
                    Constantes.C_Formule + "2 * (Long*Larg + Long*Haut + Larg*Haut)"),
                EType3DSurface.Ms3Cylindre_Laterale => (2 * ValPI * longueur * hauteur, Constantes.C_Formule + "2π * Rayon * Haut"),
                EType3DSurface.Ms3Cylindre_Totale => (2 * ValPI * longueur * (hauteur + longueur),
                    Constantes.C_Formule + "2π * Rayon * (Haut + Rayon)"),
                // longueur = rayon, hauteur = génératrice
                EType3DSurface.Ms3Cone_Laterale => (ValPI * longueur * hauteur, Constantes.C_Formule + "π * Rayon * Génératrice"),
                EType3DSurface.Ms3Cone_Totale => (ValPI * longueur * (hauteur + longueur),
                    Constantes.C_Formule + "π * Rayon * (Génératrice + Rayon)"),
                EType3DSurface.Ms3Sphere_Totale => (4 * ValPI * longueur * longueur, Constantes.C_Formule + "4π * Rayon²"),
                EType3DSurface.Ms3DemiSphere_Totale => (3 * ValPI * longueur * longueur, Constantes.C_Formule + "3π * Rayon²"),
                _ => (0, Constantes.CErr_Descriptteur + " Type de volume non pris en charge ou données insuffisantes."),
            };
        }
        // ********************************************************************************
        /// <summary>
        /// Calcule le volume d'un solide.
        /// </summary>
        /// <param name="type">Le solide dont on calcule le volume.</param>
        /// <param name="longueur">Premier paramètre (rôle variable selon le type).</param>
        /// <param name="largeur">Deuxième paramètre.</param>
        /// <param name="hauteur">Troisième paramètre.</param>
        /// <returns>Le volume, et un message décrivant la formule utilisée (ou l'erreur).</returns>
        public static (double Valeur, string Message) MVolume(
            ETypeVolume type, double longueur = 0, double largeur = 0, double hauteur = 0)
        {
            longueur = Math.Abs(longueur);
            largeur = Math.Abs(largeur);
            hauteur = Math.Abs(hauteur);

            switch (type)
            {
                case ETypeVolume.MvCube:
                    return (Math.Pow(longueur, 3), Constantes.C_Formule + "Long³");
                case ETypeVolume.MvPaveDroit:
                    return (longueur * largeur * hauteur, Constantes.C_Formule + "Long * Larg * Haut");
                case ETypeVolume.MvPrismeDroit:
                    // longueur = aire de la base (supposée déjà connue)
                    return (longueur * hauteur, Constantes.C_Formule + "AireBase * Haut");
                case ETypeVolume.MvCylindre:
                    return (ValPI * (longueur * longueur) * hauteur, Constantes.C_Formule + "π * Rayon² * Haut");
                case ETypeVolume.MvCone:
                    return (ValPI * (longueur * longueur) * hauteur / 3, Constantes.C_Formule + "π * Rayon² * Haut / 3");
                case ETypeVolume.MvTroncDeCone:
                    // longueur = grand rayon (R), largeur = petit rayon (r)
                    return (ValPI * hauteur / 3 * (longueur * longueur + longueur * largeur + largeur * largeur),
                        Constantes.C_Formule + "π * Haut / 3 * (R² + Rr + r²)");
                case ETypeVolume.MvSphere:
                    return (4.0 / 3.0 * ValPI * Math.Pow(longueur, 3), Constantes.C_Formule + "(4/3) * π * Rayon³");
                case ETypeVolume.MvDemiSphere:
                    return (2.0 / 3.0 * ValPI * Math.Pow(longueur, 3), Constantes.C_Formule + "(2/3) * π * Rayon³");
                case ETypeVolume.MvPyramide:
                    // longueur = base du triangle, largeur = hauteur du triangle de base, hauteur = hauteur de la pyramide
                    double aireBase = MSurface(ETypeSurface.MsTriangle_BH, longueur, largeur).Valeur;
                    return (aireBase * hauteur / 3, Constantes.C_Formule + "((Base * HauteurBase) / 2) * Haut / 3");
                case ETypeVolume.MvTroncDePyramide:
                    // longueur = A1, largeur = A2, hauteur = h
                    return (hauteur / 3 * (longueur + largeur + Math.Sqrt(longueur * largeur)),
                        Constantes.C_Formule + "(Haut / 3) * (A1 + A2 + √(A1 * A2))");
                case ETypeVolume.MavTetraedreRegulier:
                    return (Math.Pow(longueur, 3) / (6 * Math.Sqrt(2)), Constantes.C_Formule + "Long³ / (6√2)");
                case ETypeVolume.MvTore:
                    // longueur = rayon du tore (R), largeur = rayon du tube (r)
                    return (2 * Math.Pow(ValPI, 2) * longueur * (largeur * largeur), Constantes.C_Formule + "2π² * R * r²");
                default:
                    return (0, Constantes.CErr_Descriptteur + " Type de volume non pris en charge ou données insuffisantes.");
            }
        }
        // ********************************************************************************
        /// <summary>
        /// Indique si deux rectangles ont une zone commune, à la marge près.
        /// </summary>
        /// <param name="r1">Premier rectangle.</param>
        /// <param name="r2">Deuxième rectangle.</param>
        /// <param name="marge">Écart minimal exigé entre les deux rectangles pour ne pas
        /// être considérés en intersection (0 par défaut).</param>
        /// <returns>Vrai si les deux rectangles se chevauchent.</returns>
        public static bool RectIntersecte(Rectangle r1, Rectangle r2, int marge = 0)
        {
            marge = Math.Max(0, marge);
            return !(
                r1.X + r1.Width + marge <= r2.X ||
                r2.X + r2.Width <= r1.X - marge ||
                r1.Y + r1.Height + marge <= r2.Y ||
                r2.Y + r2.Height <= r1.Y - marge);
        }
    }
}
