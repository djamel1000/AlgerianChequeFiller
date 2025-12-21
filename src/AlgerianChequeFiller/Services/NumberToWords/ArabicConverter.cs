using System.Text;

namespace AlgerianChequeFiller.Services.NumberToWords;

/// <summary>
/// Arabic number-to-words converter for Algerian Dinars.
/// </summary>
public class ArabicConverter : INumberToWordsConverter
{
    public bool IsRtl => true;

    private static readonly string[] Units =
        { "", "واحد", "اثنان", "ثلاثة", "أربعة", "خمسة", "ستة", "سبعة", "ثمانية", "تسعة" };

    private static readonly string[] UnitsFeminine =
        { "", "واحدة", "اثنتان", "ثلاث", "أربع", "خمس", "ست", "سبع", "ثمان", "تسع" };

    private static readonly string[] Tens =
        { "", "عشرة", "عشرون", "ثلاثون", "أربعون", "خمسون", "ستون", "سبعون", "ثمانون", "تسعون" };

    private static readonly string[] Teens =
        { "عشرة", "أحد عشر", "اثنا عشر", "ثلاثة عشر", "أربعة عشر", "خمسة عشر", "ستة عشر", "سبعة عشر", "ثمانية عشر", "تسعة عشر" };

    private static readonly string[] Hundreds =
        { "", "مائة", "مائتان", "ثلاثمائة", "أربعمائة", "خمسمائة", "ستمائة", "سبعمائة", "ثمانمائة", "تسعمائة" };

    public string Convert(decimal amount)
    {
        var (dinars, centimes) = SplitAmount(amount);
        var result = new StringBuilder();

        if (dinars == 0 && centimes == 0)
            return "صفر دينار";

        if (dinars > 0)
        {
            result.Append(ConvertNumber(dinars));
            result.Append(" ");
            result.Append(GetDinarWord(dinars));
        }

        if (centimes > 0)
        {
            if (dinars > 0) result.Append(" و");
            result.Append(ConvertNumber(centimes));
            result.Append(" ");
            result.Append(GetCentimeWord(centimes));
        }

        return result.ToString();
    }

    private static (long Dinars, int Centimes) SplitAmount(decimal amount)
    {
        var dinars = (long)Math.Floor(amount);
        var centimes = (int)Math.Round((amount - dinars) * 100);
        return (dinars, centimes);
    }

    private string GetDinarWord(long n) => n switch
    {
        1 => "دينار جزائري",
        2 => "ديناران جزائريان",
        >= 3 and <= 10 => "دنانير جزائرية",
        _ => "دينارًا جزائريًا"
    };

    private string GetCentimeWord(int n) => n switch
    {
        1 => "سنتيم",
        2 => "سنتيمان",
        >= 3 and <= 10 => "سنتيمات",
        _ => "سنتيمًا"
    };

    private string ConvertNumber(long n)
    {
        if (n == 0) return "صفر";
        if (n < 0) return "سالب " + ConvertNumber(-n);

        var result = new StringBuilder();

        // Millions
        if (n >= 1_000_000)
        {
            long millions = n / 1_000_000;
            if (millions == 1)
                result.Append("مليون");
            else if (millions == 2)
                result.Append("مليونان");
            else if (millions >= 3 && millions <= 10)
                result.Append(ConvertNumber(millions) + " ملايين");
            else
                result.Append(ConvertNumber(millions) + " مليون");
            
            n %= 1_000_000;
            if (n > 0) result.Append(" و");
        }

        // Thousands
        if (n >= 1000)
        {
            long thousands = n / 1000;
            if (thousands == 1)
                result.Append("ألف");
            else if (thousands == 2)
                result.Append("ألفان");
            else if (thousands >= 3 && thousands <= 10)
                result.Append(ConvertHundredsAndBelow(thousands) + " آلاف");
            else
                result.Append(ConvertHundredsAndBelow(thousands) + " ألف");
            
            n %= 1000;
            if (n > 0) result.Append(" و");
        }

        // Hundreds and below
        if (n > 0)
        {
            result.Append(ConvertHundredsAndBelow(n));
        }

        return result.ToString();
    }

    private string ConvertHundredsAndBelow(long n)
    {
        var result = new StringBuilder();

        // Hundreds
        if (n >= 100)
        {
            int hundreds = (int)(n / 100);
            result.Append(Hundreds[hundreds]);
            n %= 100;
            if (n > 0) result.Append(" و");
        }

        // Tens and units
        if (n >= 10 && n < 20)
        {
            // Teens
            result.Append(Teens[n - 10]);
        }
        else if (n >= 20)
        {
            int tensDigit = (int)(n / 10);
            int unitsDigit = (int)(n % 10);

            if (unitsDigit > 0)
            {
                result.Append(Units[unitsDigit]);
                result.Append(" و");
            }
            result.Append(Tens[tensDigit]);
        }
        else if (n > 0)
        {
            result.Append(Units[n]);
        }

        return result.ToString();
    }
}
