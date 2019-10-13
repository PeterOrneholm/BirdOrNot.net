using System.Globalization;

namespace Orneholm.BirdOrNot.Web
{
    public static class DoubleExtensions
    {
        public static string ToCssPercentageString(this double value)
        {
            return $"{value.ToString(CultureInfo.GetCultureInfo("en-US"))}%";
        }
    }
}