# Free Js

Javascript sniplet for quick tool prototyiping

### Extract Text Between Using RegEx As Array
```js
function extractTextBetween(fullText, start, end) {
    // Function to escape regex special characters
    const escapeRegExp = (text) => {
        return text.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    };

    // Escape the start and end patterns
    const escapedStart = escapeRegExp(start);
    const escapedEnd = escapeRegExp(end);

    // Create the regex pattern
    const regex = new RegExp(`${escapedStart}(.*?)${escapedEnd}`, 'g');

    // Get all matches and convert to array
    const matches = [...fullText.matchAll(regex)];

    // Return array of captured groups or empty array if no matches
    return matches.map(match => match[1]);
}

// Test with multiple matches
let text = "First (match one) then (match two) and (match three)";

let results = extractTextBetween(text, "(", ")");
console.log(results); 
``
