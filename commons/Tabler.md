# Tabler Setup
For using Tabler in React, Just setup these CDN's in index.html

## Install Tabler via NPM
```cmd
npm install  @tabler/core
```

## Add <head>
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
                                        <span className="nav-link-title"> Home </span>
                                    </a>
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
                                            {/* Download SVG icon from http://tabler.io/icons/icon/package */}
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
                                                <path d="M12 3l8 4.5l0 9l-8 4.5l-8 -4.5l0 -9l8 -4.5" />
                                                <path d="M12 12l8 -4.5" />
                                                <path d="M12 12l0 9" />
                                                <path d="M12 12l-8 -4.5" />
                                                <path d="M16 5.25l-8 4.5" />
                                            </svg>
                                        </span>
                                        <span className="nav-link-title"> Interface </span>
                                    </a>
                                    <div className="dropdown-menu">
                                        <div className="dropdown-menu-columns">
                                            <div className="dropdown-menu-column">
                                                <a className="dropdown-item" href="./accordion.html">
                                                    Accordion
                                                    <span className="badge badge-sm bg-green-lt text-uppercase ms-auto">
                                                        New
                                                    </span>
                                                </a>
                                                <a className="dropdown-item" href="./alerts.html">
                                                    {" "}
                                                    Alerts{" "}
                                                </a>
                                                <div className="dropend">
                                                    <a
                                                        className="dropdown-item dropdown-toggle"
                                                        href="#sidebar-authentication"
                                                        data-bs-toggle="dropdown"
                                                        data-bs-auto-close="false"
                                                        role="button"
                                                        aria-expanded="false"
                                                    >
                                                        Authentication
                                                    </a>
                                                    <div className="dropdown-menu">
                                                        <a href="./sign-in.html" className="dropdown-item">
                                                            {" "}
                                                            Sign in{" "}
                                                        </a>
                                                        <a href="./sign-in-link.html" className="dropdown-item">
                                                            {" "}
                                                            Sign in link{" "}
                                                        </a>
                                                        <a href="./sign-in-illustration.html" className="dropdown-item">
                                                            {" "}
                                                            Sign in with illustration{" "}
                                                        </a>
                                                    </div>
                                                </div>
                                                
                                            </div>                                            
                                        </div>
                                    </div>
                                </li>
                            </ul>

                        </div>
                    </div>
                </aside>
                <div className="page-wrapper">
                    <div className="page-header d-print-none">
                        <div className="container-xl">
                            <div className="row g-2 align-items-center">
                                <div className="col">
                                    <h2 className="page-title">Vertical layout</h2>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div className="page-body">
                        <div className="container-xl">
                            <div className="row row-deck row-cards">
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
