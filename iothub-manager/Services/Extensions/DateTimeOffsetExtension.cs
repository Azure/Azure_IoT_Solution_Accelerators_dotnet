using System;
using System.Text.RegularExpressions;
using System.Xml;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Extensions
{
    public static class DateTimeOffsetExtension
    {
        public static DateTimeOffset Parse(string text, DateTimeOffset @default)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return @default;
            }

            var reg = new Regex("^NOW((?<operator>[+-])(?<offset>.*))?$");
            var match = reg.Match(text);
            if (!match.Success)
            {
                // Not in form `NOW(+|-)offset`, try to parse as common date time offset string
                DateTimeOffset result;
                return DateTimeOffset.TryParse(text, out result) ? result : @default;
            }

            var now = DateTimeOffset.UtcNow;

            var @operator = match.Groups["operator"].Value;
            if (string.IsNullOrWhiteSpace(@operator))
            {
                // No operator and offset part. Return now directly
                return now;
            }

            // Try parse offset in ISO8601
            TimeSpan offset;
            try
            {
                offset = XmlConvert.ToTimeSpan(match.Groups["offset"].Value);
            }
            catch (Exception)
            {
                return @default;
            }

            return @operator == "+" ? now + offset : now - offset;
        }
    }
}
