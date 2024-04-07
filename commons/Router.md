# React Reouter

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
Add BrowserRouter
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
