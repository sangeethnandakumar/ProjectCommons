## React SPA Deploymet to Azure CDN

## Enable Static Hosting and setup `$web` container

## Set Storage account name and key in GitHub secrets

## Modify `App.js` to handle redirect properly by looking at `referer`
```jsx
import './App.css';
import { useEffect } from 'react';
import { useNavigate, Outlet } from 'react-router-dom';
import Header from './components/header/Header';
import Footer from './components/footer/Footer';

function App() {
    const navigate = useNavigate();

    useEffect(() => {
        const urlParams = new URLSearchParams(window.location.search);
        const referrer = urlParams.get('referrer');

        if (referrer) {
            // Replace the current URL with the referrer
            window.history.replaceState(null, '', referrer);
            // Navigate internally using React Router
            navigate(referrer);
        }
    }, [navigate]);

    return (
        <>
            <Header />
            <Outlet />
            <Footer />
        </>
    );
}

export default App;
```

## Build nd upload artifact using YML
```yaml
name: Build and Deploy to Azure Storage

on:
  push:
    branches:
      - master
  workflow_dispatch:

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
    - name: Checkout Code
      uses: actions/checkout@v3

    - name: Set up Node.js
      uses: actions/setup-node@v3
      with:
        node-version: '23.3.0'
        cache: 'npm'

    - name: Cache Azure CLI
      id: cache-azure-cli
      uses: actions/cache@v3
      with:
        path: ~/.azure
        key: ${{ runner.os }}-azure-cli-${{ hashFiles('~/.azure/commandIndex.json') }}
        restore-keys: |
          ${{ runner.os }}-azure-cli-

    - name: Cache node modules
      id: cache-npm
      uses: actions/cache@v3
      with:
        path: ~/.npm
        key: ${{ runner.os }}-node-${{ hashFiles('**/package-lock.json') }}
        restore-keys: |
          ${{ runner.os }}-node-

    - name: Install Azure CLI
      if: steps.cache-azure-cli.outputs.cache-hit != 'true'
      run: |
        curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

    - name: Install Dependencies
      run: npm ci

    - name: Set Environment Variables
      run: cp .env.production .env

    - name: Build
      run: npm run build

    - name: Add error.html to dist
      run: echo "<!DOCTYPE html>
          <html lang=\"en\">
          <head>
            <meta charset=\"UTF-8\">
            <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">
            <title>Fallback</title>
          </head>
          <body>
            <script type=\"text/javascript\">
              // Redirect to the main page with the original URL as a query parameter
              var originalUrl = encodeURIComponent(window.location.pathname + window.location.search);
              window.location = \"/?referrer=\" + originalUrl;
            </script>
          </body>
          </html>" > dist/error.html

    - name: Deploy to Azure Blob Storage
      run: |
        az storage blob upload-batch \
          --source dist \
          --destination "\$web" \
          --account-name ${{ secrets.AZURE_STORAGE_ACCOUNT_NAME }} \
          --account-key ${{ secrets.AZURE_STORAGE_ACCOUNT_KEY }} \
          --auth-mode key \
          --overwrite
```
