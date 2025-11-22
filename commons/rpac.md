# RPAK Standard

```js
/**
 * RPAK (Restricted Public Access Key) Module
 * Revealing Module Pattern for generating and validating time-based access keys
 */
const RPAK = (function() {
  'use strict';

  // Private helper functions
  function getCurrentEpoch() {
    return Math.floor(Date.now() / 1000);
  }

  function getTodaysDate() {
    const now = new Date();
    const day = String(now.getDate()).padStart(2, '0');
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const year = now.getFullYear();
    return `${day}${month}${year}`;
  }

  function formatValidity(validityInSeconds) {
    return String(validityInSeconds).padStart(2, '0');
  }

  function encodeToBase64(str) {
    // Handle both browser and Node.js environments
    if (typeof btoa !== 'undefined') {
      return btoa(str);
    } else if (typeof Buffer !== 'undefined') {
      return Buffer.from(str).toString('base64');
    }
    throw new Error('Base64 encoding not supported in this environment');
  }

  function decodeFromBase64(str) {
    // Handle both browser and Node.js environments
    if (typeof atob !== 'undefined') {
      return atob(str);
    } else if (typeof Buffer !== 'undefined') {
      return Buffer.from(str, 'base64').toString('utf-8');
    }
    throw new Error('Base64 decoding not supported in this environment');
  }

  // Public API
  function generate(validityInSeconds, payload) {
    if (typeof validityInSeconds !== 'number' || validityInSeconds < 1) {
      throw new Error('Validity must be a positive number');
    }
    if (typeof payload !== 'string' || payload.length === 0) {
      throw new Error('Payload must be a non-empty string');
    }

    // Step 1: Get current epoch
    const currentEpoch = getCurrentEpoch();

    // Step 2: Get today's date (DDMMYYYY format)
    const todaysDate = getTodaysDate();

    // Step 3: Add epoch and date together
    const combined = currentEpoch + parseInt(todaysDate, 10);

    // Step 4: Format validity with leading zero if needed
    const formattedValidity = formatValidity(validityInSeconds);

    // Step 5: Prefix with validity
    const prefixedValue = `${formattedValidity}${combined}`;

    // Step 6: Combine with payload using pipe delimiter
    const dataString = `${prefixedValue}|${payload}`;

    // Step 7: Convert to Base64
    const base64Encoded = encodeToBase64(dataString);

    // Step 8: Append RPAK_ prefix
    const rpakKey = `RPAK_${base64Encoded}`;

    return rpakKey;
  }

  function validate(rpakKey) {
    try {
      // Check if key starts with RPAK_
      if (!rpakKey || !rpakKey.startsWith('RPAK_')) {
        return {
          valid: false,
          error: 'Invalid RPAK format: Missing RPAK_ prefix'
        };
      }

      // Remove RPAK_ prefix
      const base64Part = rpakKey.substring(5);

      // Decode from Base64
      const decoded = decodeFromBase64(base64Part);

      // Split by pipe to get prefixed value and payload
      const parts = decoded.split('|');
      if (parts.length < 2) {
        return {
          valid: false,
          error: 'Invalid RPAK format: Missing pipe delimiter'
        };
      }

      const prefixedValue = parts[0];
      const payload = parts.slice(1).join('|'); // Handle payloads with pipes

      // Extract validity (first 2 characters)
      const validityInSeconds = parseInt(prefixedValue.substring(0, 2), 10);
      if (isNaN(validityInSeconds)) {
        return {
          valid: false,
          error: 'Invalid RPAK format: Cannot parse validity'
        };
      }

      // Extract combined epoch+date value
      const combinedValue = parseInt(prefixedValue.substring(2), 10);
      if (isNaN(combinedValue)) {
        return {
          valid: false,
          error: 'Invalid RPAK format: Cannot parse combined value'
        };
      }

      // Calculate the original epoch from the key
      const todaysDate = getTodaysDate();
      const keyEpoch = combinedValue - parseInt(todaysDate, 10);

      // Get current epoch
      const currentEpoch = getCurrentEpoch();

      // Calculate time difference
      const timeDifference = currentEpoch - keyEpoch;

      // Check if key is still valid (within validity window)
      const isValid = timeDifference >= 0 && timeDifference <= validityInSeconds;

      return {
        valid: isValid,
        validity: validityInSeconds,
        keyEpoch: keyEpoch,
        currentEpoch: currentEpoch,
        timeDifference: timeDifference,
        payload: payload,
        expired: timeDifference > validityInSeconds,
        notYetValid: timeDifference < 0
      };

    } catch (error) {
      return {
        valid: false,
        error: `Validation error: ${error.message}`
      };
    }
  }

  // Reveal public API
  return {
    generate: generate,
    validate: validate
  };

})();

// Export for Node.js (if in Node environment)
if (typeof module !== 'undefined' && module.exports) {
  module.exports = RPAK;
}

// Example usage:
// const key = RPAK.generate(3, "Hello how are you");
// console.log(key);
// 
// const result = RPAK.validate(key);
// console.log(result);
```
