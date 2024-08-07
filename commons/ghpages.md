# 1. Name Project Like 'expencetrackerplayground.github.com'

# 2. Update Vite.config with BASE URL

```js
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
    plugins: [react()],
    base: "/expencetrackerplayground.github.co"
})
```

# 3. Update Homepage in packages.json

```json
 "homepage": "https://sangeethnandakumar.github.io/expencetrackerplayground.github.com",
 "scripts": {
   "predeploy": "npm run build",
   "deploy": "gh-pages -d dist"
 },
```

# 4. YML Workflow

```yml
name: Deploy to GitHub Pages

on:
  push:
    branches:
      - master
  workflow_dispatch:

env:
  REACT_APP_DIR: 'expencetrackerplayground'

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout repository
      uses: actions/checkout@v2

    - name: Set up Node.js
      uses: actions/setup-node@v2
      with:
        node-version: '21'

    - name: Install dependencies
      run: npm install
      working-directory: ${{ env.REACT_APP_DIR }}

    - name: Build the React app
      run: npm run build
      working-directory: ${{ env.REACT_APP_DIR }}

    - name: List
      run: ls ${{ env.REACT_APP_DIR }}/dist

    - name: Deploy to GitHub Pages
      uses: peaceiris/actions-gh-pages@v3
      with:
        github_token: ${{ secrets.PAT_TOKEN }}
        publish_dir: ${{ env.REACT_APP_DIR }}/dist
        force_orphan: true
```
