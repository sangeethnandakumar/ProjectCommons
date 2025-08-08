# Time Helper Js

```js
// timeHelpers.js
const timeHelpers = (function () {

    function sanitizeUTCString(utcString) {
        // Strip excessive microseconds beyond 3 digits
        return utcString.replace(/(\.\d{3})\d+Z$/, '$1Z');
    }

    function formatUTCDateToLocal(utcString) {
        const cleanString = sanitizeUTCString(utcString);
        const date = new Date(cleanString);

        const options = {
            day: 'numeric',
            month: 'short',
            year: 'numeric',
            hour: 'numeric',
            minute: '2-digit',
            hour12: true
        };
        const localDateStr = date.toLocaleString(undefined, options);

        const offsetMinutes = date.getTimezoneOffset();
        const sign = offsetMinutes <= 0 ? '+' : '-';
        const absMinutes = Math.abs(offsetMinutes);
        const hours = String(Math.floor(absMinutes / 60)).padStart(2, '0');
        const minutes = String(absMinutes % 60).padStart(2, '0');
        const gmtOffset = `GMT ${sign}${hours}:${minutes}`;

        return {
            localTime: localDateStr,
            gmtOffset
        };
    }

    function convertLocalDateToUTC(localDateString) {
        try {
            const [year, month, day] = localDateString.split('-').map(Number);
            if (isNaN(year) || isNaN(month) || isNaN(day)) {
                throw new Error('Invalid date format');
            }
            return new Date(Date.UTC(year, month - 1, day)).toISOString();
        } catch (error) {
            console.error('Error converting local date to UTC:', error);
            return new Date().toISOString();
        }
    }

    function formatDateToLocal(utcString) {
        try {
            const result = formatUTCDateToLocal(utcString);
            return `${result.localTime} (${result.gmtOffset})`;
        } catch (error) {
            console.error('Error formatting date:', error);
            return utcString;
        }
    }

    function formatDateRangeToLocal(fromDate, toDate) {
        try {
            const fromResult = formatUTCDateToLocal(fromDate);
            const toResult = formatUTCDateToLocal(toDate);

            const sameOffset = fromResult.gmtOffset === toResult.gmtOffset;
            return sameOffset
                ? `${fromResult.localTime} - ${toResult.localTime} (${fromResult.gmtOffset})`
                : `${fromResult.localTime} (${fromResult.gmtOffset}) - ${toResult.localTime} (${toResult.gmtOffset})`;
        } catch (error) {
            console.error('Error formatting date range:', error);
            return `${fromDate} - ${toDate}`;
        }
    }

    return {
        formatUTCDateToLocal,
        convertLocalDateToUTC,
        formatDateToLocal,
        formatDateRangeToLocal
    };

})();

/*
Sample usage:

const result = timeHelpers.formatUTCDateToLocal("2025-08-08T17:47:22.277655Z");
console.log(result.localTime); // "9 Aug 2025, 5:30 AM"
console.log(result.gmtOffset); // "GMT +05:30"

const utcDate = timeHelpers.convertLocalDateToUTC("2025-08-08");
console.log(utcDate); // "2025-08-08T00:00:00.000Z"

const formattedDate = timeHelpers.formatDateToLocal("2025-08-08T17:47:22.277655Z");
console.log(formattedDate); // "9 Aug 2025, 5:30 AM (GMT +05:30)"

const formattedRange = timeHelpers.formatDateRangeToLocal("2025-08-08T17:47:22.277655Z", "2025-09-07T17:47:22.277655Z");
console.log(formattedRange); // "9 Aug 2025, 5:30 AM - 7 Sep 2025, 5:30 AM (GMT +05:30)"
*/

export default timeHelpers;

```
