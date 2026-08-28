namespace MagMini.Application.Common.Validators;

public static class NipValidator
{
    private static readonly int[] Weights = { 6, 5, 7, 2, 3, 4, 5, 6, 7 };

    public static bool IsValid(string? nip)
    {
        if (string.IsNullOrWhiteSpace(nip)) return false;

        // Usunięcie myślników i spacji
        var cleanNip = nip.Replace("-", "").Replace(" ", "").Trim();

        if (cleanNip.Length != 10 || !cleanNip.All(char.IsDigit))
            return false;

        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            sum += (cleanNip[i] - '0') * Weights[i];
        }

        int controlDigit = sum % 11;
        if (controlDigit == 10) return false; // NIP nie może mieć sumy kontrolnej 10

        return controlDigit == (cleanNip[9] - '0');
    }
}