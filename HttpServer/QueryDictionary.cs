using System.Globalization;
using System.Text;

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
            bool.TryParse(value, out bool result);
            return result;
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

        public override string ToString()
        {
            if (Count == 0) return "";

            var builder = new StringBuilder("?", 128);

            int i = 1;
            foreach(var kvp in this)
            {
                builder.Append($"{kvp.Key}={kvp.Value}");
                if (i != Count)
                {
                    builder.Append('&');
                }
            }
            return Uri.EscapeDataString(builder.ToString());
        }
    }
}