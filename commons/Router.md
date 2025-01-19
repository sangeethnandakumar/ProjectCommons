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
                <Route path="browse" element={<Browse />} />
                <Route path="devices" element={<Devices />} />
                <Route path="eligibility" element={<Eligibility />} />
                <Route path="program" element={<Program />} />
            </Route>
        </Routes>
    )
}

export default Router;
```

### Main.jsx
Add BrowserRouter
```jsx
import React from 'react'
import ReactDOM from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import Router from './Router.jsx'

ReactDOM.createRoot(document.getElementById('root')).render(
    <React.StrictMode>
        <BrowserRouter>
            <Router />
        </BrowserRouter>
    </React.StrictMode>,
)
```

### Router.jsx
Add different routes
```jsx
import { Route, Routes } from 'react-router-dom';
import Dashboard from './Dashboard';

function Router() {

    return (
        <Routes>
            <Route path="/home" element={<Dashboard />} />
            <Route path="*" element={<h1>Not Found</h1>} />
        </Routes>
    )
}

export default Router
```
