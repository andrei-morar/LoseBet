using System;
using System.Linq;

namespace LoseBet.Core.Models // Asigură-te că folosești namespace-ul corect
{
    public static class IdentityValidator
    {
        // Acum cerem și dataNasteriiDeclarata ca parametru
        public static bool ValidateCNP(string cnp, DateTime dataNasteriiDeclarata)
        {
            if (string.IsNullOrWhiteSpace(cnp) || cnp.Length != 13 || !cnp.All(char.IsDigit))
                return false;

            // Extragem componentele datei din CNP
            int s = int.Parse(cnp[0].ToString());
            int aa = int.Parse(cnp.Substring(1, 2));
            int ll = int.Parse(cnp.Substring(3, 2));
            int zz = int.Parse(cnp.Substring(5, 2));

            // Stabilim anul complet (1,2 = 1900+ ; 5,6 = 2000+)
            int anComplet = (s == 1 || s == 2) ? 1900 + aa : 2000 + aa;

            try
            {
                // Construim data de naștere reală ascunsă în CNP
                DateTime dataNasteriiCnp = new DateTime(anComplet, ll, zz);

                // --- NOU: 1. Verificăm dacă data declarată coincide cu cea din CNP ---
                // Folosim .Date pentru a ignora orele/minutele care ar putea veni din WPF
                if (dataNasteriiCnp.Date != dataNasteriiDeclarata.Date)
                {
                    return false; // Nu se potrivesc!
                }

                // --- 2. Verificăm dacă are 18 ani împliniți ---
                int varsta = DateTime.Now.Year - dataNasteriiCnp.Year;

                // Dacă nu și-a serbat încă ziua anul acesta, scădem un an
                if (DateTime.Now < dataNasteriiCnp.AddYears(varsta))
                {
                    varsta--;
                }

                return varsta >= 18;
            }
            catch
            {
                // Pică aici dacă CNP-ul conține o dată imposibilă (ex: 30 Februarie)
                return false;
            }
        }
    }
}