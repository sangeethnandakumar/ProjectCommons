# RPACK C# Equalant

```csharp
using System;
using System.Text;

/// <summary>
/// RPAK (Restricted Public Access Key) Static Class
/// For generating and validating time-based access keys
/// </summary>
public static class RPAK
{
    /// <summary>
    /// Validation result containing key details
    /// </summary>
    public class ValidationResult
    {
        public bool Valid { get; set; }
        public int Validity { get; set; }
        public long KeyEpoch { get; set; }
        public long CurrentEpoch { get; set; }
        public long TimeDifference { get; set; }
        public string Payload { get; set; }
        public bool Expired { get; set; }
        public bool NotYetValid { get; set; }
        public string Error { get; set; }
    }

    // Private helper methods
    private static long GetCurrentEpoch()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    private static string GetTodaysDate()
    {
        DateTime now = DateTime.Now;
        return $"{now.Day:D2}{now.Month:D2}{now.Year}";
    }

    private static string FormatValidity(int validityInSeconds)
    {
        return validityInSeconds.ToString("D2");
    }

    private static string EncodeToBase64(string plainText)
    {
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }

    private static string DecodeFromBase64(string base64EncodedData)
    {
        byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        return Encoding.UTF8.GetString(base64EncodedBytes);
    }

    /// <summary>
    /// Generates a new RPAK key
    /// </summary>
    /// <param name="validityInSeconds">Validity duration in seconds</param>
    /// <param name="payload">Payload string to embed</param>
    /// <returns>Generated RPAK key</returns>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid</exception>
    public static string Generate(int validityInSeconds, string payload)
    {
        if (validityInSeconds < 1)
        {
            throw new ArgumentException("Validity must be a positive number", nameof(validityInSeconds));
        }

        if (string.IsNullOrEmpty(payload))
        {
            throw new ArgumentException("Payload must be a non-empty string", nameof(payload));
        }

        // Step 1: Get current epoch
        long currentEpoch = GetCurrentEpoch();

        // Step 2: Get today's date (DDMMYYYY format)
        string todaysDate = GetTodaysDate();

        // Step 3: Add epoch and date together
        long combined = currentEpoch + long.Parse(todaysDate);

        // Step 4: Format validity with leading zero if needed
        string formattedValidity = FormatValidity(validityInSeconds);

        // Step 5: Prefix with validity
        string prefixedValue = $"{formattedValidity}{combined}";

        // Step 6: Combine with payload using pipe delimiter
        string dataString = $"{prefixedValue}|{payload}";

        // Step 7: Convert to Base64
        string base64Encoded = EncodeToBase64(dataString);

        // Step 8: Append RPAK_ prefix
        string rpakKey = $"RPAK_{base64Encoded}";

        return rpakKey;
    }

    /// <summary>
    /// Validates an RPAK key
    /// </summary>
    /// <param name="rpakKey">The RPAK key to validate</param>
    /// <returns>ValidationResult containing validation details</returns>
    public static ValidationResult Validate(string rpakKey)
    {
        try
        {
            // Check if key starts with RPAK_
            if (string.IsNullOrEmpty(rpakKey) || !rpakKey.StartsWith("RPAK_"))
            {
                return new ValidationResult
                {
                    Valid = false,
                    Error = "Invalid RPAK format: Missing RPAK_ prefix"
                };
            }

            // Remove RPAK_ prefix
            string base64Part = rpakKey.Substring(5);

            // Decode from Base64
            string decoded = DecodeFromBase64(base64Part);

            // Split by pipe to get prefixed value and payload
            string[] parts = decoded.Split(new[] { '|' }, 2);
            if (parts.Length < 2)
            {
                return new ValidationResult
                {
                    Valid = false,
                    Error = "Invalid RPAK format: Missing pipe delimiter"
                };
            }

            string prefixedValue = parts[0];
            string payload = parts[1];

            // Extract validity (first 2 characters)
            if (prefixedValue.Length < 2)
            {
                return new ValidationResult
                {
                    Valid = false,
                    Error = "Invalid RPAK format: Prefixed value too short"
                };
            }

            if (!int.TryParse(prefixedValue.Substring(0, 2), out int validityInSeconds))
            {
                return new ValidationResult
                {
                    Valid = false,
                    Error = "Invalid RPAK format: Cannot parse validity"
                };
            }

            // Extract combined epoch+date value
            string combinedString = prefixedValue.Substring(2);
            if (!long.TryParse(combinedString, out long combinedValue))
            {
                return new ValidationResult
                {
                    Valid = false,
                    Error = "Invalid RPAK format: Cannot parse combined value"
                };
            }

            // Calculate the original epoch from the key
            string todaysDate = GetTodaysDate();
            long keyEpoch = combinedValue - long.Parse(todaysDate);

            // Get current epoch
            long currentEpoch = GetCurrentEpoch();

            // Calculate time difference
            long timeDifference = currentEpoch - keyEpoch;

            // Check if key is still valid (within validity window)
            bool isValid = timeDifference >= 0 && timeDifference <= validityInSeconds;

            return new ValidationResult
            {
                Valid = isValid,
                Validity = validityInSeconds,
                KeyEpoch = keyEpoch,
                CurrentEpoch = currentEpoch,
                TimeDifference = timeDifference,
                Payload = payload,
                Expired = timeDifference > validityInSeconds,
                NotYetValid = timeDifference < 0
            };
        }
        catch (Exception ex)
        {
            return new ValidationResult
            {
                Valid = false,
                Error = $"Validation error: {ex.Message}"
            };
        }
    }
}

// Example usage:
/*
// Generate a key valid for 3 seconds
string key = RPAK.Generate(3, "Hello how are you");
Console.WriteLine(key);

// Validate the key
RPAK.ValidationResult result = RPAK.Validate(key);
Console.WriteLine($"Valid: {result.Valid}");
Console.WriteLine($"Time Difference: {result.TimeDifference} seconds");
Console.WriteLine($"Payload: {result.Payload}");
Console.WriteLine($"Expired: {result.Expired}");
*/
```
