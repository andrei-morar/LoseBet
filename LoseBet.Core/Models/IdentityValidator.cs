public static class IdentityValidator
{
    public static bool ValidateCNP(string cnp)
    {
        if (cnp.Length != 13 || !cnp.All(char.IsDigit)) return false;

        // Extragem data nașterii din CNP pentru verificare vârstă
        // Format CNP: S AA LL ZZ ...
        int s = int.Parse(cnp[0].ToString());
        int aa = int.Parse(cnp.Substring(1, 2));
        int ll = int.Parse(cnp.Substring(3, 2));
        int zz = int.Parse(cnp.Substring(5, 2));

        int anComplet = (s == 1 || s == 2) ? 1900 + aa : 2000 + aa;

        try
        {
            DateTime dataNasterii = new DateTime(anComplet, ll, zz);
            int varsta = DateTime.Now.Year - dataNasterii.Year;
            if (dataNasterii > DateTime.Now.AddYears(-varsta)) varsta--;

            return varsta >= 18; // Validăm doar dacă are 18+ ani
        }
        catch
        {
            return false;
        }
    }
}