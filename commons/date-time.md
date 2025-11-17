# DateTimeConverter JS
## UTC -> System
## System -> UTC
```js
const DateTimeConverter = (function() {
  // Private helper to get browser timezone
  function getBrowserTimezone() {
    return Intl.DateTimeFormat().resolvedOptions().timeZone;
  }

  // Public method to get timezone
  function getTimezone() {
    return getBrowserTimezone();
  }

  // Convert UTC to System timezone with optional template
  function utcToSystem(utcDateString, template = 'iso') {
    // Remove 'Z' if present and create Date object
    const cleanStr = utcDateString.replace('Z', '');
    const date = new Date(cleanStr + 'Z'); // Ensure it's treated as UTC
    
    // Get date components
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    const seconds = String(date.getSeconds()).padStart(2, '0');
    const ms = String(date.getMilliseconds()).padStart(3, '0');
    const hours12 = date.getHours() % 12 || 12;
    const ampm = date.getHours() >= 12 ? 'PM' : 'AM';
    
    const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 
                        'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const monthFull = ['January', 'February', 'March', 'April', 'May', 'June',
                       'July', 'August', 'September', 'October', 'November', 'December'];
    const dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
    
    // Template formats
    const formats = {
      'iso': `${year}-${month}-${day}T${hours}:${minutes}:${seconds}.${ms}000`,
      'date': `${year}-${month}-${day}`,
      'time': `${hours}:${minutes}:${seconds}`,
      'time-short': `${hours}:${minutes}`,
      'datetime': `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`,
      'datetime-short': `${year}-${month}-${day} ${hours}:${minutes}`,
      'us': `${month}/${day}/${year}`,
      'us-time': `${month}/${day}/${year} ${hours12}:${minutes} ${ampm}`,
      'us-short': `${month}/${day}/${year} ${hours12}:${minutes}${ampm}`,
      'eu': `${day}/${month}/${year}`,
      'eu-time': `${day}/${month}/${year} ${hours}:${minutes}`,
      'readable': `${monthNames[date.getMonth()]} ${day}, ${year}`,
      'readable-time': `${monthNames[date.getMonth()]} ${day}, ${year} ${hours12}:${minutes} ${ampm}`,
      'full': `${dayNames[date.getDay()]}, ${monthFull[date.getMonth()]} ${day}, ${year} ${hours12}:${minutes} ${ampm}`,
      'timestamp': date.getTime().toString()
    };
    
    return formats[template] || formats['iso'];
  }

  // Convert System timezone to UTC
  function systemToUtc(systemDateStringOrObject) {
    let date;
    
    // If no argument, use current time
    if (systemDateStringOrObject === undefined) {
      date = new Date();
    }
    // If Date object, use it directly
    else if (systemDateStringOrObject instanceof Date) {
      date = systemDateStringOrObject;
    }
    // If string, parse it
    else {
      const cleanStr = systemDateStringOrObject.replace('Z', '');
      date = new Date(cleanStr);
    }
    
    // Format to ISO string in UTC
    const year = date.getUTCFullYear();
    const month = String(date.getUTCMonth() + 1).padStart(2, '0');
    const day = String(date.getUTCDate()).padStart(2, '0');
    const hours = String(date.getUTCHours()).padStart(2, '0');
    const minutes = String(date.getUTCMinutes()).padStart(2, '0');
    const seconds = String(date.getUTCSeconds()).padStart(2, '0');
    const ms = String(date.getUTCMilliseconds()).padStart(3, '0');
    
    return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}.${ms}000Z`;
  }

  // Reveal public API
  return {
    getTimezone,
    utcToSystem,
    systemToUtc
  };
})();

// ============================================================
// USAGE EXAMPLES
// ============================================================

// --- GET BROWSER TIMEZONE ---
// console.log('Browser Timezone:', DateTimeConverter.getTimezone());
// => "America/New_York" (or your browser's timezone)

// --- UTC TO SYSTEM CONVERSIONS ---
// console.log('ISO:', DateTimeConverter.utcToSystem('2025-11-19T05:00:00.000000Z'));
// => "2025-11-19T00:00:00.000000" (EST example, UTC-5)

// console.log('Date Only:', DateTimeConverter.utcToSystem('2025-11-19T05:00:00.000000Z', 'date'));
// => "2025-11-19"

// console.log('Time Only:', DateTimeConverter.utcToSystem('2025-11-19T05:00:00.000000Z', 'time-short'));
// => "00:00"

// console.log('US Format:', DateTimeConverter.utcToSystem('2025-11-19T05:00:00.000000Z', 'us-time'));
// => "11/19/2025 12:00 AM"

// console.log('EU Format:', DateTimeConverter.utcToSystem('2025-11-19T05:00:00.000000Z', 'eu-time'));
// => "19/11/2025 00:00"

// console.log('Readable:', DateTimeConverter.utcToSystem('2025-11-19T05:00:00.000000Z', 'readable-time'));
// => "Nov 19, 2025 12:00 AM"

// console.log('Full:', DateTimeConverter.utcToSystem('2025-11-19T05:00:00.000000Z', 'full'));
// => "Tue, November 19, 2025 12:00 AM"

// --- SYSTEM TO UTC CONVERSIONS ---
// // Convert string to UTC
// console.log('String to UTC:', DateTimeConverter.systemToUtc('2025-11-19T05:00:00.000000'));
// => "2025-11-19T10:00:00.000000Z" (if system is EST, UTC-5)

// // Convert current time to UTC
// console.log('Current time to UTC:', DateTimeConverter.systemToUtc());
// => "2025-11-19T15:30:45.123000Z" (current time in UTC)

// // Convert Date object to UTC
// const myDate = new Date(2025, 10, 19, 5, 0, 0); // Nov 19, 2025 5:00 AM local
// console.log('Date object to UTC:', DateTimeConverter.systemToUtc(myDate));
// => "2025-11-19T10:00:00.000000Z" (if system is EST, UTC-5)
```
