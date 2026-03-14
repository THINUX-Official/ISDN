using System;

namespace ISDN.Helpers
{
    public static class CityNicHelper
    {
        public static string CombineCityAndNic(string city, string nic)
        {
            if (string.IsNullOrWhiteSpace(city)) city = string.Empty;
            if (string.IsNullOrWhiteSpace(nic)) nic = string.Empty;
            return $"{city.Trim()}|{nic.Trim()}";
        }

        public static string GetCity(string combinedValue)
        {
            if (string.IsNullOrWhiteSpace(combinedValue)) return string.Empty;
            
            var parts = combinedValue.Split('|', StringSplitOptions.None);
            return parts.Length > 0 ? parts[0] : combinedValue;
        }

        public static string GetNic(string combinedValue)
        {
            if (string.IsNullOrWhiteSpace(combinedValue)) return string.Empty;
            
            var parts = combinedValue.Split('|', StringSplitOptions.None);
            return parts.Length > 1 ? parts[1] : string.Empty;
        }
    }
}
