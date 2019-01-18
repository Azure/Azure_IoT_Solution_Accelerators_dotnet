using System.Text.RegularExpressions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Helpers
{
    public class InputValidator
    {
        private const string INVALID_CHARACTER = @"[^A-Za-z0-9:;.!,_\-* ]";

        // Check illegal characters in input
        public static void Validate(string input)
        {
            if (Regex.IsMatch(input.Trim(), INVALID_CHARACTER))
            {
                throw new InvalidInputException($"Input '{input}' contains invalid characters.");
            }
        }
    }
}
