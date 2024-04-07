# Tabler Setup
For using Tabler in React, Just setup these CDN's in index.html

```html
<!doctype html>
<html lang="en">
<head>
    <meta charset="UTF-8" />
    <link rel="icon" type="image/svg+xml" href="https://cdn-icons-png.flaticon.com/512/2702/2702172.png" />

    <!-- CSS files -->
    <link href="https://preview.tabler.io/dist/css/tabler.min.css?1695847769" rel="stylesheet" />
    <link href="https://preview.tabler.io/dist/css/tabler-flags.min.css?1695847769" rel="stylesheet" />
    <link href="https://preview.tabler.io/dist/css/tabler-payments.min.css?1695847769" rel="stylesheet" />
    <link href="https://preview.tabler.io/dist/css/tabler-vendors.min.css?1695847769" rel="stylesheet" />
    <link href="https://preview.tabler.io/dist/css/demo.min.css?1695847769" rel="stylesheet" />

    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>InstaRead AI EPub Processor</title>

    <style>
        @import url('https://rsms.me/inter/inter.css');

        :root {
            --tblr-font-sans-serif: 'Inter Var', -apple-system, BlinkMacSystemFont, San Francisco, Segoe UI, Roboto, Helvetica Neue, sans-serif;
        }

        body {
            font-feature-settings: "cv03", "cv04", "cv11";
        }
    </style>
</head>
<body>
    <script src="https://preview.tabler.io/dist/js/demo-theme.min.js?1695847769"></script>
    <div id="root"></div>
    <script type="module" src="/src/main.jsx"></script>
    <script src="https://preview.tabler.io/dist/js/tabler.min.js?1695847769" defer></script>
    <script src="https://preview.tabler.io/dist/js/demo.min.js?1695847769" defer></script>
</body>
</html>
```
