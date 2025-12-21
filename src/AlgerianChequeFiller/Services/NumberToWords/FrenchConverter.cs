using System.Text;

namespace AlgerianChequeFiller.Services.NumberToWords;

/// <summary>
/// French number-to-words converter for Algerian Dinars.
/// </summary>
public class FrenchConverter : INumberToWordsConverter
{
    public bool IsRtl => false;

    private static readonly string[] Units =
        { "", "un", "deux", "trois", "quatre", "cinq", "six", "sept", "huit", "neuf" };

    private static readonly string[] Teens =
        { "dix", "onze", "douze", "treize", "quatorze", "quinze", "seize", "dix-sept", "dix-huit", "dix-neuf" };

    private static readonly string[] Tens =
        { "", "dix", "vingt", "trente", "quarante", "cinquante", "soixante", "soixante", "quatre-vingt", "quatre-vingt" };

    public string Convert(decimal amount)
    {
        var (dinars, centimes) = SplitAmount(amount);
        var result = new StringBuilder();

        if (dinars == 0 && centimes == 0)
            return "zéro dinar";

        if (dinars > 0)
        {
            result.Append(ConvertNumber(dinars));
            result.Append(dinars == 1 ? " dinar" : " dinars");
        }

        if (centimes > 0)
        {
            if (dinars > 0) result.Append(" et ");
            result.Append(ConvertNumber(centimes));
            result.Append(centimes == 1 ? " centime" : " centimes");
        }

        return result.ToString();
    }

    private static (long Dinars, int Centimes) SplitAmount(decimal amount)
    {
        var dinars = (long)Math.Floor(amount);
        var centimes = (int)Math.Round((amount - dinars) * 100);
        return (dinars, centimes);
    }

    private string ConvertNumber(long n)
    {
        if (n == 0) return "zéro";
        if (n < 0) return "moins " + ConvertNumber(-n);

        var result = new StringBuilder();

        // Millions
        if (n >= 1_000_000)
        {
            long millions = n / 1_000_000;
            if (millions == 1)
                result.Append("un million");
            else
                result.Append(ConvertNumber(millions) + " millions");
            n %= 1_000_000;
            if (n > 0) result.Append(" ");
        }

        // Thousands
        if (n >= 1000)
        {
            long thousands = n / 1000;
            if (thousands == 1)
                result.Append("mille");
            else
                result.Append(ConvertNumber(thousands) + " mille");
            n %= 1000;
            if (n > 0) result.Append(" ");
        }

        // Hundreds
        if (n >= 100)
        {
            long hundreds = n / 100;
            if (hundreds == 1)
                result.Append("cent");
            else
                result.Append(ConvertNumber(hundreds) + " cent");

            n %= 100;
            // "s" only if exactly multiple of 100
            if (hundreds > 1 && n == 0)
                result.Append("s");
            else if (n > 0)
                result.Append(" ");
        }

        // Tens and units
        if (n >= 10)
        {
            int tensDigit = (int)(n / 10);
            int unitsDigit = (int)(n % 10);

            if (tensDigit == 1) // 10-19
            {
                result.Append(Teens[unitsDigit]);
            }
            else if (tensDigit == 7 || tensDigit == 9) // 70-79, 90-99
            {
                result.Append(Tens[tensDigit]);
                if (unitsDigit == 0)
                    result.Append("-dix");
                else if (unitsDigit == 1)
                    result.Append(tensDigit == 7 ? " et onze" : "-onze");
                else
                    result.Append("-" + Teens[unitsDigit]);
            }
            else // 20-69, 80-89
            {
                result.Append(Tens[tensDigit]);

                if (tensDigit == 8 && unitsDigit == 0)
                    result.Append("s"); // quatre-vingts
                else if (unitsDigit > 0)
                {
                    if (unitsDigit == 1 && tensDigit != 8)
                        result.Append(" et ");
                    else
                        result.Append("-");
                    result.Append(Units[unitsDigit]);
                }
            }
        }
        else if (n > 0)
        {
            result.Append(Units[n]);
        }

        return result.ToString();
    }
}
