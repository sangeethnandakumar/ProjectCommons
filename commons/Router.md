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
import { Route, Routes, Navigate } from 'react-router-dom';

const Router = () => {

    return (
        <Routes>
            <Route path="" element={<App />}>

                <Route path="new" element={<NewRegistrationPage />}>
                    <Route path="basic" element={<SearchPage />} />
                    <Route path="education" element={<SearchPage />} />
                    <Route path="employment" element={<SearchPage />} />
                    <Route path="family" element={<SearchPage />} />
                    <Route path="profession" element={<SearchPage />} />
                    <Route path="prime" element={<SearchPage />} />
                </Route>

                <Route path="home" element={<HomePage />}>
                </Route>

                <Route path="search" element={<SearchPage />}>
                    <Route path="new" element={<SearchPage />}/>
                    {/*<Route path="matching" element={<SearchPage />}/>*/}
                    {/*<Route path="otherstates" element={<SearchPage />}/>*/}
                    {/*<Route path="education" element={<SearchPage />}/>*/}
                    {/*<Route path="profession" element={<SearchPage />}/>*/}
                    {/*<Route path="prime" element={<SearchPage />}/>*/}
                </Route>

                <Route path="shortlist" element={<ShortListPage />}>
                    <Route path=":userid" element={<ProfilePage />}>
                        <Route index element={<Navigate to="details" />} />
                        <Route path="details" element={<PersonnelDetails />} />
                        <Route path="contact" element={<PartnerPreference />} />
                        <Route path="chat" element={<ChatWindow />} />
                        <Route path="settings" element={<SettingsSubPage />} />
                    </Route>
                </Route>
                <Route path="interests" element={<InterestPage />}>
                    <Route path=":userid" element={<ProfilePage />}>
                        <Route index element={<Navigate to="details" />} />
                        <Route path="details" element={<PersonnelDetails />} />
                        <Route path="contact" element={<PartnerPreference />} />
                        <Route path="chat" element={<ChatWindow />} />
                        <Route path="settings" element={<SettingsSubPage />} />
                    </Route>
                </Route>
                <Route path="chats" element={<ChatPage />}>
                    <Route path=":userid" element={<ChatPage />}/>
                </Route>
                <Route path="profiles/:userid" element={<ProfilePage />}>
                    <Route path="details" element={<PersonnelDetails />} />
                    <Route path="contact" element={<PartnerPreference />} />
                    <Route path="chat" element={<ChatWindow />} />
                    <Route path="settings" element={<SettingsSubPage />} />
                </Route>
            </Route>
            <Route path="login" element={<LoginPage />} />
            <Route path="*" element={<h1>Not Found</h1>} />
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
