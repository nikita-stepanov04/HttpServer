using System.Globalization;

namespace HttpServerCore
{
    public class QueryDictionary : Dictionary<string, string>
    {
        public string? GetString(string param) =>
            TryGetValue(param, out string? value) ? value : null;

        public int? GetInt(string param)
        {
            TryGetValue(param, out string? value);
            return Convert.ToInt32(value);
        }
        
        public long? GetLong(string param)
        {
            TryGetValue(param, out string? value);
            return Convert.ToInt64(value);
        }

        public bool? GetBool(string param)
        {
            TryGetValue(param, out string? value);
            return Convert.ToBoolean(value);
        }

        public DateTime? GetDateTime(string param)
        {
            TryGetValue(param, out string? value);
            if (value == null) 
                return null;

            string format = "yyyy-MM-ddTHH:mm";
            return DateTime.TryParseExact(value, format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dateTime) ? dateTime : null;
        }
    }
}