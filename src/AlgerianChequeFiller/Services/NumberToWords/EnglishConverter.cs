using System.Text;

namespace AlgerianChequeFiller.Services.NumberToWords;

/// <summary>
/// English number-to-words converter for Algerian Dinars.
/// </summary>
public class EnglishConverter : INumberToWordsConverter
{
    public bool IsRtl => false;

    private static readonly string[] Units =
        { "", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };

    private static readonly string[] Teens =
        { "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };

    private static readonly string[] Tens =
        { "", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

    public string Convert(decimal amount)
    {
        var (dinars, centimes) = SplitAmount(amount);
        var result = new StringBuilder();

        if (dinars == 0 && centimes == 0)
            return "zero Algerian dinars";

        if (dinars > 0)
        {
            result.Append(ConvertNumber(dinars));
            result.Append(dinars == 1 ? " Algerian dinar" : " Algerian dinars");
        }

        if (centimes > 0)
        {
            if (dinars > 0) result.Append(" and ");
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
        if (n == 0) return "zero";
        if (n < 0) return "minus " + ConvertNumber(-n);

        var result = new StringBuilder();

        // Billions
        if (n >= 1_000_000_000)
        {
            long billions = n / 1_000_000_000;
            result.Append(ConvertNumber(billions) + " billion");
            n %= 1_000_000_000;
            if (n > 0) result.Append(" ");
        }

        // Millions
        if (n >= 1_000_000)
        {
            long millions = n / 1_000_000;
            result.Append(ConvertNumber(millions) + " million");
            n %= 1_000_000;
            if (n > 0) result.Append(" ");
        }

        // Thousands
        if (n >= 1000)
        {
            long thousands = n / 1000;
            result.Append(ConvertNumber(thousands) + " thousand");
            n %= 1000;
            if (n > 0) result.Append(" ");
        }

        // Hundreds
        if (n >= 100)
        {
            long hundreds = n / 100;
            result.Append(Units[hundreds] + " hundred");
            n %= 100;
            if (n > 0) result.Append(" ");
        }

        // Tens and units
        if (n >= 10 && n < 20)
        {
            result.Append(Teens[n - 10]);
        }
        else if (n >= 20)
        {
            int tensDigit = (int)(n / 10);
            int unitsDigit = (int)(n % 10);
            result.Append(Tens[tensDigit]);
            if (unitsDigit > 0)
            {
                result.Append("-");
                result.Append(Units[unitsDigit]);
            }
        }
        else if (n > 0)
        {
            result.Append(Units[n]);
        }

        return result.ToString();
    }
}
