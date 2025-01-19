# React Reouter

### Package.json
Install React router DOM
```bash
npm install react-router-dom
```

### main.jsx
Provider
```jsx
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import Router from './Router.jsx'

createRoot(document.getElementById('root')).render(
    <StrictMode>
        <BrowserRouter>
            <Router />
        </BrowserRouter>
    </StrictMode>,
)
```

### Router.jsx
Replace for router
```jsx
import { Route, Routes } from 'react-router-dom';
import App from './App';
import Browse from './pages/browse';
import Eligibility from './pages/eligibility';
import Program from './pages/program';
import Devices from './pages/devices';

const Router = () => {
    return (
        <Routes>
            <Route path="" element={<App />}>
                {/* Define the 'id' as a route parameter */}
                <Route path="browse/:id" element={<Browse />} />
                <Route path="devices" element={<Devices />} />
                <Route path="eligibility" element={<Eligibility />} />
                <Route path="program" element={<Program />} />
            </Route>
        </Routes>
    );
};

export default Router;
```

### Reading :id from <Browse/>
using useParams()
```jsx
import { useParams } from 'react-router-dom';

const Browse = () => {
    const { id } = useParams();

    return (
        <div>
            <p>Received ID: {id}</p>
        </div>
    );
};

export default Browse;
```

### Outlet Usage
Using Outlet's
```jsx
import Footer from "../precomps/footer";
import { Outlet } from 'react-router-dom';

const Content = () => {
    return (
        <div className="page-wrapper">
            <Outlet />
            <Footer/>
        </div>
    );
}

export default Content;
```

### Using Links
For Routing
```jsx
import { Link } from 'react-router-dom';

function Menu() {

    return (
        <div className="navbar-nav mainmenu">
            <ul>
                <li>
                    <Link to="/home">Home</Link>
                </li>
                <li>
                    <Link to="/browse">Browse</Link>
                </li>
            </ul>
        </div>
    );
}

export default Menu;
```

## CDN Redirects
Using error.html
```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Fallback</title>
</head>
<body>
    <script type="text/javascript">
        // Redirect to the main page with the original URL as a query parameter
        var originalUrl = encodeURIComponent(window.location.pathname + window.location.search);
        window.location = "/?referrer=" + originalUrl;
    </script>
</body>
</html>
```


### Capyuring CDN Redirects & Programic Navigation
Using Navigatior
```jsx
import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

// Function to get the 'referrer' parameter from the URL
const getReferrerParam = () => {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get('referrer');
};

// Function to clean up the URL by removing query parameters
const cleanUpUrl = () => {
    window.history.replaceState(null, '', window.location.pathname);
};

// Function to navigate to the decoded referrer URL after a delay
const navigateToReferrer = (navigate, referrerParam) => {
    setTimeout(() => {
        navigate(decodeURIComponent(referrerParam));
    }, 2000);
};

function App() {
    const navigate = useNavigate();

    useEffect(() => {
        const referrerParam = getReferrerParam(); // Get referrer parameter from URL
        if (referrerParam) {
            cleanUpUrl(); // Remove query parameters from the URL
            navigateToReferrer(navigate, referrerParam); // Navigate to the referrer URL
        }
    }, [navigate]);

    return (
        <>
            <h1>Programic Navigator Usage Example</h1>
        </>
    );
}

export default App;

```
