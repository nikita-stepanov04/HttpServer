namespace HttpServerCore
{
    public class HeaderDictionary : Dictionary<string, string>
    {
        public string? Get(string header)
        {
            TryGetValue(header, out string? value);
            return value;
        }

        public long? GetDigit(string header)
        {
            string? value = Get(header);

            if (value == null)
                return null;

            return Convert.ToInt64(value);
        }
    }
}
