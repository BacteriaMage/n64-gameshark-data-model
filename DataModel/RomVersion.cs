using System.Globalization;
using System.Text.RegularExpressions;

namespace BacteriaMage.N64.GameShark;

public class RomVersion
{
    public readonly string Raw;
    public readonly double Number;
    public readonly DateTime BuildTimestamp;

    private readonly string? _nameClarifier;

    public string DisplayName => ToString();

    // v1.08: "11:58 Nov 24 97" -> 19971124
    // v1.09: "17:40 Jan 5 98"  -> 19980105
    // v2.00: "08:06 Mar 5 98"  -> 19980305
    // v2.00: "10:05 Apr 6 98"  -> 19980406
    // v2.10: "13:57 Aug 25 98" -> 19980825
    // v2.21: "12:47 Dec 18 98" -> 19981218
    // v3.00: "15:05 Apr 1 99"  -> 19990401
    // v3.10: "16:50 Jun 9 99"  -> 19990609
    // v3.20: "18:45 Jun 22 99" -> 19990622
    // v3.21: "14:26 Jan 4"     -> 20000104
    // v3.30: "09:54 Mar 27"    -> 20000327
    // v3.30: "15:56 Apr 4"     -> 20000404

    public RomVersion(string rawStr)
    {
        Raw = rawStr;
        string cleanStr = rawStr.Trim();
        switch (cleanStr)
        {
            case "11:58 Nov 24 97":
                Number = 1.08;
                BuildTimestamp = new DateTime(1997, 11, 24);
                break;
            case "17:40 Jan 5 98":
                Number = 1.09;
                BuildTimestamp = new DateTime(1998, 01, 05);
                break;
            case "08:06 Mar 5 98":
                Number = 2.00;
                BuildTimestamp = new DateTime(1998, 03, 05);
                _nameClarifier = "March";
                break;
            case "10:05 Apr 6 98":
                Number = 2.00;
                BuildTimestamp = new DateTime(1998, 04, 06);
                _nameClarifier = "April";
                break;
            case "13:57 Aug 25 98":
                Number = 2.10;
                BuildTimestamp = new DateTime(1998, 08, 25);
                break;
            // TODO: Find a v2.20 ROM and add its build timestamp here
            // case "??:?? ??? ?? 98":
            //     Number = 2.20;
            //     BuildTimestamp = new DateTime(1998, ??, ??);
            //     break;
            case "12:47 Dec 18 98":
                Number = 2.21;
                BuildTimestamp = new DateTime(1998, 12, 18);
                break;
            case "15:05 Apr 1 99":
                Number = 3.00;
                BuildTimestamp = new DateTime(1999, 04, 01);
                break;
            case "16:50 Jun 9 99":
                Number = 3.10;
                BuildTimestamp = new DateTime(1999, 06, 09);
                break;
            case "18:45 Jun 22 99":
                Number = 3.20;
                BuildTimestamp = new DateTime(1999, 06, 22);
                break;
            case "14:26 Jan 4":
                Number = 3.21;
                BuildTimestamp = new DateTime(2000, 01, 04);
                break;
            case "09:54 Mar 27":
                Number = 3.30;
                BuildTimestamp = new DateTime(2000, 03, 27);
                _nameClarifier = "March";
                break;
            case "15:56 Apr 4":
                Number = 3.30;
                BuildTimestamp = new DateTime(2000, 04, 04);
                _nameClarifier = "April";
                break;
            default:
                var match = Regex.Match(cleanStr, @"(?<HH>\d\d):(?<mm>\d\d) (?<MMM>\w\w\w) (?<dd>\d\d?)(?: (?<yy>\d\d)?)?");
                if (!match.Success)
                {
                    Console.Error.WriteLine($"ERROR: Invalid GS ROM build timestamp: '{cleanStr}'(len = {cleanStr.Length}). Expected HH:mm MMM dd [yy].");
                    break;
                }
                var HH = match.Groups["HH"].Value;
                var mm = match.Groups["mm"].Value;
                var MMM = match.Groups["MMM"].Value;
                var dd = match.Groups["dd"].Value;
                // v3.21 and v3.0 builds omit the year from the timestamp.
                var yy = match.Groups["yy"].Success ? match.Groups["yy"].Value : "97";
                cleanStr = $"{HH}:{mm} {MMM} {dd} 19{yy}";
                if (!Is(cleanStr, "HH:mm MMM d yyyy", out BuildTimestamp))
                {
                    Console.Error.WriteLine($"ERROR: Invalid GS ROM build timestamp: '{cleanStr}' (len = {cleanStr.Length}). Expected HH:mm MMM dd yyyy.");
                    break;
                }
                break;
        }
    }

    private static bool Is(string rawDateTime, string dateTimeFormat, out DateTime datetime)
    {
        return DateTime.TryParseExact(rawDateTime, dateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out datetime);
    }

    public override string ToString()
    {
        return $"v{Number:F2}" +
               (string.IsNullOrEmpty(_nameClarifier) ? "" : $" ({_nameClarifier}),") +
               $" built on {BuildTimestamp:yyyy-MM-dd}";
    }
}
