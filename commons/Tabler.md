# Tabler Setup
For using Tabler in React, Just setup these CDN's in index.html

## Install Tabler via NPM
```cmd
npm install  @tabler/core
```

## Add Public.html HEAD tag
```html
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, viewport-fit=cover" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <title>Freyauth Admin – Identity & Access Management</title>

    <!-- Branding & Theming -->
    <meta name="theme-color" content="#066fd1" />
    <meta name="msapplication-TileColor" content="#066fd1" />
    <meta name="apple-mobile-web-app-status-bar-style" content="black-translucent" />
    <meta name="apple-mobile-web-app-capable" content="yes" />
    <meta name="mobile-web-app-capable" content="yes" />
    <meta name="HandheldFriendly" content="True" />
    <meta name="MobileOptimized" content="320" />

    <!-- Favicons -->
    <link rel="icon" href="./favicon.ico" type="image/x-icon" />
    <link rel="shortcut icon" href="./favicon.ico" type="image/x-icon" />

    <!-- SEO & Social -->
    <meta name="description" content="Freyauth is a modern, self-hosted, open-source Identity and Access Management server built with .NET. Easily manage users, roles, and authentication flows with complete flexibility." />
    <meta name="canonical" content="https://freyauth.com/admin" />

    <!-- Open Graph -->
    <meta property="og:title" content="Freyauth Admin – Identity & Access Management" />
    <meta property="og:description" content="Self-hosted IAM server for modern applications. Configure, control, and customize your authentication system with Freyauth." />
    <meta property="og:image" content="https://freyauth.com/assets/og-image.png" />
    <meta property="og:image:width" content="1280" />
    <meta property="og:image:height" content="640" />
    <meta property="og:site_name" content="Freyauth" />
    <meta property="og:type" content="website" />
    <meta property="og:url" content="https://freyauth.com/admin" />

    <!-- Twitter Card -->
    <meta name="twitter:card" content="summary_large_image" />
    <meta name="twitter:title" content="Freyauth Admin – Identity & Access Management" />
    <meta name="twitter:description" content="Self-hosted, open-source IAM built with .NET. Secure, extensible, and fully customizable." />
    <meta name="twitter:image" content="https://freyauth.com/assets/og-image.png" />
    <meta name="twitter:site" content="@freyauth" />
</head>
```

## Initial App.jsx
```jsx
import './App.css'
import '@tabler/core/dist/css/tabler.min.css'
import '@tabler/core/dist/js/tabler.min.js'

function App() {
    return (
        <>
            <div className="page">
                <aside
                    className="navbar navbar-vertical navbar-expand-sm"
                    data-bs-theme="dark"
                >
                    <div className="container-fluid">
                        <button
                            className="navbar-toggler"
                            type="button"
                            data-bs-toggle="collapse"
                            data-bs-target="#sidebar-menu"
                            aria-controls="sidebar-menu"
                            aria-expanded="false"
                            aria-label="Toggle navigation"
                        >
                            <span className="navbar-toggler-icon" />
                        </button>

                        <h1 className="navbar-brand navbar-brand-autodark">
                            <a href="#">
                                <img
                                    src="https://preview.tabler.io/static/logo-white.svg"
                                    width={110}
                                    height={32}
                                    alt="Tabler"
                                    className="navbar-brand-image"
                                />
                            </a>
                        </h1>
                        <div className="collapse navbar-collapse" id="sidebar-menu">

                            <ul className="navbar-nav pt-lg-3">
                                <li className="nav-item">
                                    <a className="nav-link" href="./">
                                        <span className="nav-link-icon d-md-none d-lg-inline-block">
                                            {/* Download SVG icon from http://tabler.io/icons/icon/home */}
                                            <svg
                                                xmlns="http://www.w3.org/2000/svg"
                                                width={24}
                                                height={24}
                                                viewBox="0 0 24 24"
                                                fill="none"
                                                stroke="currentColor"
                                                strokeWidth={2}
                                                strokeLinecap="round"
                                                strokeLinejoin="round"
                                                className="icon icon-1"
                                            >
                                                <path d="M5 12l-2 0l9 -9l9 9l-2 0" />
                                                <path d="M5 12v7a2 2 0 0 0 2 2h10a2 2 0 0 0 2 -2v-7" />
                                                <path d="M9 21v-6a2 2 0 0 1 2 -2h2a2 2 0 0 1 2 2v6" />
                                            </svg>
                                        </span>
                                        <span className="nav-link-title"> Dashboard </span>
                                    </a>
                                </li>

                                <li className="nav-item dropdown mt-3">
                                    <a
                                        className="nav-link dropdown-toggle"
                                        href="#navbar-base"
                                        data-bs-toggle="dropdown"
                                        data-bs-auto-close="false"
                                        role="button"
                                        aria-expanded="false"
                                    >
                                        <span className="nav-link-icon d-md-none d-lg-inline-block">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="icon icon-tabler icons-tabler-outline icon-tabler-user"><path stroke="none" d="M0 0h24v24H0z" fill="none" /><path d="M8 7a4 4 0 1 0 8 0a4 4 0 0 0 -8 0" /><path d="M6 21v-2a4 4 0 0 1 4 -4h4a4 4 0 0 1 4 4v2" /></svg>
                                        </span>
                                        <span className="nav-link-title"> Users </span>
                                    </a>
                                    <div className="dropdown-menu">
                                        <div className="dropdown-menu-columns">
                                            <div className="dropdown-menu-column">
                                                <a className="dropdown-item" href="./accordion.html">
                                                    All Users
                                                </a>
                                                <a className="dropdown-item" href="./accordion.html">
                                                    New User
                                                </a>
                                                <a className="dropdown-item" href="./alerts.html">
                                                    Deleted Users
                                                </a>
                                                <a className="dropdown-item" href="./accordion.html">
                                                    Login Attempts
                                                </a>
                                            </div>
                                        </div>
                                    </div>
                                </li>
                                <li className="nav-item dropdown">
                                    <a
                                        className="nav-link dropdown-toggle"
                                        href="#navbar-base"
                                        data-bs-toggle="dropdown"
                                        data-bs-auto-close="false"
                                        role="button"
                                        aria-expanded="false"
                                    >
                                        <span className="nav-link-icon d-md-none d-lg-inline-block">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="icon icon-tabler icons-tabler-outline icon-tabler-users"><path stroke="none" d="M0 0h24v24H0z" fill="none" /><path d="M9 7m-4 0a4 4 0 1 0 8 0a4 4 0 1 0 -8 0" /><path d="M3 21v-2a4 4 0 0 1 4 -4h4a4 4 0 0 1 4 4v2" /><path d="M16 3.13a4 4 0 0 1 0 7.75" /><path d="M21 21v-2a4 4 0 0 0 -3 -3.85" /></svg>
                                        </span>
                                        <span className="nav-link-title"> Groups </span>
                                    </a>
                                    <div className="dropdown-menu">
                                        <div className="dropdown-menu-columns">
                                            <div className="dropdown-menu-column">
                                                <a className="dropdown-item" href="./accordion.html">
                                                    All Groups
                                                </a>
                                                <a className="dropdown-item" href="./accordion.html">
                                                    New Group
                                                </a>
                                                <a className="dropdown-item" href="./alerts.html">
                                                    Deleted Groups
                                                </a>
                                                <a className="dropdown-item" href="./accordion.html">
                                                    Login Attempts
                                                </a>
                                            </div>
                                        </div>
                                    </div>
                                </li>
                                <li className="nav-item dropdown">
                                    <a
                                        className="nav-link dropdown-toggle"
                                        href="#navbar-base"
                                        data-bs-toggle="dropdown"
                                        data-bs-auto-close="false"
                                        role="button"
                                        aria-expanded="false"
                                    >
                                        <span className="nav-link-icon d-md-none d-lg-inline-block">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="icon icon-tabler icons-tabler-outline icon-tabler-circles"><path stroke="none" d="M0 0h24v24H0z" fill="none" /><path d="M12 7m-4 0a4 4 0 1 0 8 0a4 4 0 1 0 -8 0" /><path d="M6.5 17m-4 0a4 4 0 1 0 8 0a4 4 0 1 0 -8 0" /><path d="M17.5 17m-4 0a4 4 0 1 0 8 0a4 4 0 1 0 -8 0" /></svg>
                                        </span>
                                        <span className="nav-link-title"> Roles & Permissions </span>
                                    </a>
                                    <div className="dropdown-menu">
                                        <div className="dropdown-menu-columns">
                                            <div className="dropdown-menu-column">
                                                <a className="dropdown-item" href="./accordion.html">
                                                    All Roles
                                                </a>
                                                <a className="dropdown-item" href="./accordion.html">
                                                    New Role
                                                </a>
                                            </div>
                                        </div>
                                    </div>
                                </li>
                                <li className="nav-item dropdown mt-3">
                                    <a
                                        className="nav-link dropdown-toggle"
                                        href="#navbar-base"
                                        data-bs-toggle="dropdown"
                                        data-bs-auto-close="false"
                                        role="button"
                                        aria-expanded="false"
                                    >
                                        <span className="nav-link-icon d-md-none d-lg-inline-block">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="icon icon-tabler icons-tabler-outline icon-tabler-apps"><path stroke="none" d="M0 0h24v24H0z" fill="none" /><path d="M4 4m0 1a1 1 0 0 1 1 -1h4a1 1 0 0 1 1 1v4a1 1 0 0 1 -1 1h-4a1 1 0 0 1 -1 -1z" /><path d="M4 14m0 1a1 1 0 0 1 1 -1h4a1 1 0 0 1 1 1v4a1 1 0 0 1 -1 1h-4a1 1 0 0 1 -1 -1z" /><path d="M14 14m0 1a1 1 0 0 1 1 -1h4a1 1 0 0 1 1 1v4a1 1 0 0 1 -1 1h-4a1 1 0 0 1 -1 -1z" /><path d="M14 7l6 0" /><path d="M17 4l0 6" /></svg>
                                        </span>
                                        <span className="nav-link-title"> App Registrations </span>
                                    </a>
                                    <div className="dropdown-menu">
                                        <div className="dropdown-menu-columns">
                                            <div className="dropdown-menu-column">
                                                <a className="dropdown-item" href="./accordion.html">
                                                    All Apps
                                                </a>
                                                <a className="dropdown-item" href="./accordion.html">
                                                    New Registration
                                                </a>
                                            </div>
                                        </div>
                                    </div>
                                </li>
                                <li className="nav-item dropdown mt-3">
                                    <a
                                        className="nav-link dropdown-toggle"
                                        href="#navbar-base"
                                        data-bs-toggle="dropdown"
                                        data-bs-auto-close="false"
                                        role="button"
                                        aria-expanded="false"
                                    >
                                        <span className="nav-link-icon d-md-none d-lg-inline-block">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="icon icon-tabler icons-tabler-outline icon-tabler-sitemap"><path stroke="none" d="M0 0h24v24H0z" fill="none" /><path d="M3 15m0 2a2 2 0 0 1 2 -2h2a2 2 0 0 1 2 2v2a2 2 0 0 1 -2 2h-2a2 2 0 0 1 -2 -2z" /><path d="M15 15m0 2a2 2 0 0 1 2 -2h2a2 2 0 0 1 2 2v2a2 2 0 0 1 -2 2h-2a2 2 0 0 1 -2 -2z" /><path d="M9 3m0 2a2 2 0 0 1 2 -2h2a2 2 0 0 1 2 2v2a2 2 0 0 1 -2 2h-2a2 2 0 0 1 -2 -2z" /><path d="M6 15v-1a2 2 0 0 1 2 -2h8a2 2 0 0 1 2 2v1" /><path d="M12 9l0 3" /></svg>                                        </span>
                                        <span className="nav-link-title"> User Flows </span>
                                    </a>
                                    <div className="dropdown-menu">
                                        <div className="dropdown-menu-columns">
                                            <div className="dropdown-menu-column">
                                                <a className="dropdown-item" href="./accordion.html">
                                                    All Flows
                                                </a>
                                                <a className="dropdown-item" href="./accordion.html">
                                                    Conditional MFA
                                                </a>
                                            </div>
                                        </div>
                                    </div>
                                </li>

                                <li className="nav-item dropdown">
                                    <a
                                        className="nav-link dropdown-toggle"
                                        href="#navbar-base"
                                        data-bs-toggle="dropdown"
                                        data-bs-auto-close="false"
                                        role="button"
                                        aria-expanded="false"
                                    >
                                        <span className="nav-link-icon d-md-none d-lg-inline-block">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="icon icon-tabler icons-tabler-outline icon-tabler-brand-auth0"><path stroke="none" d="M0 0h24v24H0z" fill="none" /><path d="M12 14.5l-5.5 3.5l2 -6l-4.5 -4h6l2 -5l2 5h6l-4.5 4l2 6z" /><path d="M20.507 8.872l-2.01 -5.872h-12.994l-2.009 5.872c-1.242 3.593 -.135 7.094 3.249 9.407l5.257 3.721l5.257 -3.721c3.385 -2.313 4.49 -5.814 3.25 -9.407z" /></svg>                                        </span>
                                        <span className="nav-link-title"> Policies </span>
                                    </a>
                                    <div className="dropdown-menu">
                                        <div className="dropdown-menu-columns">
                                            <div className="dropdown-menu-column">
                                                <a className="dropdown-item" href="./accordion.html">
                                                    All Flows
                                                </a>
                                                <a className="dropdown-item" href="./accordion.html">
                                                    Conditional MFA
                                                </a>
                                            </div>
                                        </div>
                                    </div>
                                </li>

                                <li className="nav-item dropdown mt-3">
                                    <a
                                        className="nav-link dropdown-toggle"
                                        href="#navbar-base"
                                        data-bs-toggle="dropdown"
                                        data-bs-auto-close="false"
                                        role="button"
                                        aria-expanded="false"
                                    >
                                        <span className="nav-link-icon d-md-none d-lg-inline-block">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="icon icon-tabler icons-tabler-outline icon-tabler-device-cctv"><path stroke="none" d="M0 0h24v24H0z" fill="none" /><path d="M3 3m0 1a1 1 0 0 1 1 -1h16a1 1 0 0 1 1 1v2a1 1 0 0 1 -1 1h-16a1 1 0 0 1 -1 -1z" /><path d="M12 14m-4 0a4 4 0 1 0 8 0a4 4 0 1 0 -8 0" /><path d="M19 7v7a7 7 0 0 1 -14 0v-7" /><path d="M12 14l.01 0" /></svg>                                        </span>
                                        <span className="nav-link-title"> Monitoring </span>
                                    </a>
                                    <div className="dropdown-menu">
                                        <div className="dropdown-menu-columns">
                                            <div className="dropdown-menu-column">
                                                <a className="dropdown-item" href="./accordion.html">
                                                    Audit Logs
                                                </a>
                                                <a className="dropdown-item" href="./accordion.html">
                                                    Deep Investigate
                                                </a>
                                            </div>
                                        </div>
                                    </div>
                                </li>

                            </ul>

                        </div>
                    </div>
                </aside>
                <div className="page-wrapper">
                    <div className="sticky-top">

                        <header className="navbar navbar-expand-md sticky-top d-print-none" data-bs-theme="light">
                            <div className="container-xl">
                                {/* BEGIN NAVBAR TOGGLER */}
                                <button
                                    className="navbar-toggler"
                                    type="button"
                                    data-bs-toggle="collapse"
                                    data-bs-target="#navbar-menu"
                                    aria-controls="navbar-menu"
                                    aria-expanded="false"
                                    aria-label="Toggle navigation"
                                >
                                    <span className="navbar-toggler-icon" />
                                </button>
                                {/* END NAVBAR TOGGLER */}
                                {/* BEGIN NAVBAR LOGO */}
                                <div className="navbar-brand navbar-brand-autodark d-none-navbar-horizontal pe-0 pe-md-3">
                                    <a href=".">
                                        <svg
                                            xmlns="http://www.w3.org/2000/svg"
                                            width={110}
                                            height={32}
                                            viewBox="0 0 232 68"
                                            className="navbar-brand-image"
                                        >
                                            <path
                                                d="M64.6 16.2C63 9.9 58.1 5 51.8 3.4 40 1.5 28 1.5 16.2 3.4 9.9 5 5 9.9 3.4 16.2 1.5 28 1.5 40 3.4 51.8 5 58.1 9.9 63 16.2 64.6c11.8 1.9 23.8 1.9 35.6 0C58.1 63 63 58.1 64.6 51.8c1.9-11.8 1.9-23.8 0-35.6zM33.3 36.3c-2.8 4.4-6.6 8.2-11.1 11-1.5.9-3.3.9-4.8.1s-2.4-2.3-2.5-4c0-1.7.9-3.3 2.4-4.1 2.3-1.4 4.4-3.2 6.1-5.3-1.8-2.1-3.8-3.8-6.1-5.3-2.3-1.3-3-4.2-1.7-6.4s4.3-2.9 6.5-1.6c4.5 2.8 8.2 6.5 11.1 10.9 1 1.4 1 3.3.1 4.7zM49.2 46H37.8c-2.1 0-3.8-1-3.8-3s1.7-3 3.8-3h11.4c2.1 0 3.8 1 3.8 3s-1.7 3-3.8 3z"
                                                fill="#066fd1"
                                                style={{ fill: "var(--tblr-primary, #066fd1)" }}
                                            />
                                            <path
                                                d="M105.8 46.1c.4 0 .9.2 1.2.6s.6 1 .6 1.7c0 .9-.5 1.6-1.4 2.2s-2 .9-3.2.9c-2 0-3.7-.4-5-1.3s-2-2.6-2-5.4V31.6h-2.2c-.8 0-1.4-.3-1.9-.8s-.9-1.1-.9-1.9c0-.7.3-1.4.8-1.8s1.2-.7 1.9-.7h2.2v-3.1c0-.8.3-1.5.8-2.1s1.3-.8 2.1-.8 1.5.3 2 .8.8 1.3.8 2.1v3.1h3.4c.8 0 1.4.3 1.9.8s.8 1.2.8 1.9-.3 1.4-.8 1.8-1.2.7-1.9.7h-3.4v13c0 .7.2 1.2.5 1.5s.8.5 1.4.5c.3 0 .6-.1 1.1-.2.5-.2.8-.3 1.2-.3zm28-20.7c.8 0 1.5.3 2.1.8.5.5.8 1.2.8 2.1v20.3c0 .8-.3 1.5-.8 2.1-.5.6-1.2.8-2.1.8s-1.5-.3-2-.8-.8-1.2-.8-2.1c-.8.9-1.9 1.7-3.2 2.4-1.3.7-2.8 1-4.3 1-2.2 0-4.2-.6-6-1.7-1.8-1.1-3.2-2.7-4.2-4.7s-1.6-4.3-1.6-6.9c0-2.6.5-4.9 1.5-6.9s2.4-3.6 4.2-4.8c1.8-1.1 3.7-1.7 5.9-1.7 1.5 0 3 .3 4.3.8 1.3.6 2.5 1.3 3.4 2.1 0-.8.3-1.5.8-2.1.5-.5 1.2-.7 2-.7zm-9.7 21.3c2.1 0 3.8-.8 5.1-2.3s2-3.4 2-5.7-.7-4.2-2-5.8c-1.3-1.5-3-2.3-5.1-2.3-2 0-3.7.8-5 2.3-1.3 1.5-2 3.5-2 5.8s.6 4.2 1.9 5.7 3 2.3 5.1 2.3zm32.1-21.3c2.2 0 4.2.6 6 1.7 1.8 1.1 3.2 2.7 4.2 4.7s1.6 4.3 1.6 6.9-.5 4.9-1.5 6.9-2.4 3.6-4.2 4.8c-1.8 1.1-3.7 1.7-5.9 1.7-1.5 0-3-.3-4.3-.9s-2.5-1.4-3.4-2.3v.3c0 .8-.3 1.5-.8 2.1-.5.6-1.2.8-2.1.8s-1.5-.3-2.1-.8c-.5-.5-.8-1.2-.8-2.1V18.9c0-.8.3-1.5.8-2.1.5-.6 1.2-.8 2.1-.8s1.5.3 2.1.8c.5.6.8 1.3.8 2.1v10c.8-1 1.8-1.8 3.2-2.5 1.3-.7 2.8-1 4.3-1zm-.7 21.3c2 0 3.7-.8 5-2.3s2-3.5 2-5.8-.6-4.2-1.9-5.7-3-2.3-5.1-2.3-3.8.8-5.1 2.3-2 3.4-2 5.7.7 4.2 2 5.8c1.3 1.6 3 2.3 5.1 2.3zm23.6 1.9c0 .8-.3 1.5-.8 2.1s-1.3.8-2.1.8-1.5-.3-2-.8-.8-1.3-.8-2.1V18.9c0-.8.3-1.5.8-2.1s1.3-.8 2.1-.8 1.5.3 2 .8.8 1.3.8 2.1v29.7zm29.3-10.5c0 .8-.3 1.4-.9 1.9-.6.5-1.2.7-2 .7h-15.8c.4 1.9 1.3 3.4 2.6 4.4 1.4 1.1 2.9 1.6 4.7 1.6 1.3 0 2.3-.1 3.1-.4.7-.2 1.3-.5 1.8-.8.4-.3.7-.5.9-.6.6-.3 1.1-.4 1.6-.4.7 0 1.2.2 1.7.7s.7 1 .7 1.7c0 .9-.4 1.6-1.3 2.4-.9.7-2.1 1.4-3.6 1.9s-3 .8-4.6.8c-2.7 0-5-.6-7-1.7s-3.5-2.7-4.6-4.6-1.6-4.2-1.6-6.6c0-2.8.6-5.2 1.7-7.2s2.7-3.7 4.6-4.8 3.9-1.7 6-1.7 4.1.6 6 1.7 3.4 2.7 4.5 4.7c.9 1.9 1.5 4.1 1.5 6.3zm-12.2-7.5c-3.7 0-5.9 1.7-6.6 5.2h12.6v-.3c-.1-1.3-.8-2.5-2-3.5s-2.5-1.4-4-1.4zm30.3-5.2c1 0 1.8.3 2.4.8.7.5 1 1.2 1 1.9 0 1-.3 1.7-.8 2.2-.5.5-1.1.8-1.8.7-.5 0-1-.1-1.6-.3-.2-.1-.4-.1-.6-.2-.4-.1-.7-.1-1.1-.1-.8 0-1.6.3-2.4.8s-1.4 1.3-1.9 2.3-.7 2.3-.7 3.7v11.4c0 .8-.3 1.5-.8 2.1-.5.6-1.2.8-2.1.8s-1.5-.3-2.1-.8c-.5-.6-.8-1.3-.8-2.1V28.8c0-.8.3-1.5.8-2.1.5-.6 1.2-.8 2.1-.8s1.5.3 2.1.8c.5.6.8 1.3.8 2.1v.6c.7-1.3 1.8-2.3 3.2-3 1.3-.7 2.8-1 4.3-1z"
                                                fillRule="evenodd"
                                                clipRule="evenodd"
                                                fill="#4a4a4a"
                                            />
                                        </svg>
                                    </a>
                                </div>
                                {/* END NAVBAR LOGO */}
                                <div className="navbar-nav flex-row order-md-last">
                                    <div className="nav-item dropdown">
                                        <a
                                            href="#"
                                            className="nav-link d-flex lh-1 p-0 px-2"
                                            data-bs-toggle="dropdown"
                                            aria-label="Open user menu"
                                        >
                                            <span
                                                className="avatar avatar-sm"
                                                style={{ backgroundImage: "url(./static/avatars/003f.jpg)" }}
                                            >
                                                {" "}
                                            </span>
                                            <div className="d-none d-xl-block ps-2">
                                                <div>Sangeeth Nandakumar</div>
                                                <div className="mt-1 small text-secondary">Tax Accountant</div>
                                            </div>
                                        </a>
                                        <div
                                            className="dropdown-menu dropdown-menu-end dropdown-menu-arrow"
                                            data-bs-theme="light"
                                        >
                                            <a href="#" className="dropdown-item">
                                                Status
                                            </a>
                                            <a href="./profile.html" className="dropdown-item">
                                                Profile
                                            </a>
                                            <a href="#" className="dropdown-item">
                                                Feedback
                                            </a>
                                            <div className="dropdown-divider" />
                                            <a href="./settings.html" className="dropdown-item">
                                                Settings
                                            </a>
                                            <a href="./sign-in.html" className="dropdown-item">
                                                Logout
                                            </a>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </header>
                        <header className="navbar-expand-md">
                            <div className="collapse navbar-collapse" id="navbar-menu">
                                <div className="navbar">
                                    <div className="container-xl">
                                        <div className="row flex-column flex-md-row flex-fill align-items-center">
                                            <div className="col col-md-auto">
                                                <ul className="navbar-nav">
                                                    <li className="nav-item">
                                                        <a
                                                            className="nav-link"
                                                            href="#"
                                                            data-bs-toggle="offcanvas"
                                                            data-bs-target="#offcanvasSettings"
                                                        >
                                                            <span className="badge badge-sm bg-red text-red-fg">
                                                                New
                                                            </span>
                                                            <span className="nav-link-icon d-md-none d-lg-inline-block">
                                                                {/* Download SVG icon from http://tabler.io/icons/icon/settings */}
                                                                <svg
                                                                    xmlns="http://www.w3.org/2000/svg"
                                                                    width={24}
                                                                    height={24}
                                                                    viewBox="0 0 24 24"
                                                                    fill="none"
                                                                    stroke="currentColor"
                                                                    strokeWidth={2}
                                                                    strokeLinecap="round"
                                                                    strokeLinejoin="round"
                                                                    className="icon icon-1"
                                                                >
                                                                    <path d="M10.325 4.317c.426 -1.756 2.924 -1.756 3.35 0a1.724 1.724 0 0 0 2.573 1.066c1.543 -.94 3.31 .826 2.37 2.37a1.724 1.724 0 0 0 1.065 2.572c1.756 .426 1.756 2.924 0 3.35a1.724 1.724 0 0 0 -1.066 2.573c.94 1.543 -.826 3.31 -2.37 2.37a1.724 1.724 0 0 0 -2.572 1.065c-.426 1.756 -2.924 1.756 -3.35 0a1.724 1.724 0 0 0 -2.573 -1.066c-1.543 .94 -3.31 -.826 -2.37 -2.37a1.724 1.724 0 0 0 -1.065 -2.572c-1.756 -.426 -1.756 -2.924 0 -3.35a1.724 1.724 0 0 0 1.066 -2.573c-.94 -1.543 .826 -3.31 2.37 -2.37c1 .608 2.296 .07 2.572 -1.065z" />
                                                                    <path d="M9 12a3 3 0 1 0 6 0a3 3 0 0 0 -6 0" />
                                                                </svg>
                                                            </span>
                                                            <span className="nav-link-title"> Settings </span>
                                                        </a>
                                                    </li>
                                                </ul>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </header>

                    </div>
                    <div className="page-body">
                        <div className="container-xl">
                            <div className="row g-2 align-items-center">
                                <div className="col">
                                    <h2 className="page-title">Vertical layout</h2>
                                </div>
                            </div>
                            <div className="row row-deck row-cards mt-4">
                                <div className="col-sm-6 col-lg-3">
                                    <div className="card">
                                        <div className="card-body" style={{ height: "10rem" }} />
                                    </div>
                                </div>
                                <div className="col-sm-6 col-lg-3">
                                    <div className="card">
                                        <div className="card-body" style={{ height: "10rem" }} />
                                    </div>
                                </div>
                                <div className="col-sm-6 col-lg-3">
                                    <div className="card">
                                        <div className="card-body" style={{ height: "10rem" }} />
                                    </div>
                                </div>
                                <div className="col-sm-6 col-lg-3">
                                    <div className="card">
                                        <div className="card-body" style={{ height: "10rem" }} />
                                    </div>
                                </div>
                                <div className="col-lg-6">
                                    <div className="row row-cards">
                                        <div className="col-12">
                                            <div className="card">
                                                <div className="card-body" style={{ height: "10rem" }} />
                                            </div>
                                        </div>
                                        <div className="col-12">
                                            <div className="card">
                                                <div className="card-body" style={{ height: "10rem" }} />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div className="col-lg-6">
                                    <div className="card">
                                        <div className="card-body" style={{ height: "10rem" }} />
                                    </div>
                                </div>
                                <div className="col-12">
                                    <div className="card">
                                        <div className="card-body" style={{ height: "10rem" }} />
                                    </div>
                                </div>
                                <div className="col-md-12 col-lg-8">
                                    <div className="card">
                                        <div className="card-body" style={{ height: "10rem" }} />
                                    </div>
                                </div>
                                <div className="col-md-6 col-lg-4">
                                    <div className="card">
                                        <div className="card-body" style={{ height: "10rem" }} />
                                    </div>
                                </div>
                                <div className="col-md-6 col-lg-4">
                                    <div className="card">
                                        <div className="card-body" style={{ height: "10rem" }} />
                                    </div>
                                </div>
                                <div className="col-md-12 col-lg-8">
                                    <div className="card">
                                        <div className="card-body" style={{ height: "10rem" }} />
                                    </div>
                                </div>
                                <div className="col-12">
                                    <div className="card">
                                        <div className="card-body" style={{ height: "10rem" }} />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

        </>
    )
}

export default App
```
