# Azure Container Apps + GitHub Container

## 1. Create A Service Principal In EntraAD & Generate Secret
Note down the secret value

![image](https://github.com/user-attachments/assets/8885181a-edcf-471d-887f-93398ef01264)

## 2. Setup RBAC
Goto resource group --> IAM --> Add Role

![image](https://github.com/user-attachments/assets/6d9ce4de-8e2e-48c5-9106-05454bdf1bca)

## 3. Generate GitHub PAT Token
Generate a GitHub PAT token from Settings --> Developer Settings --> Classic Token.

> Ensure token has read, write, delete access to Packages

![image](https://github.com/user-attachments/assets/aac4c340-6dba-488f-a4d7-eed81c4771cc)

## 4. Add PAT & Azure Service Credential To GitHub actin secrets

AZURE_CREDENTIALS
```json
{
  "clientId": "XXXXXXXXXXXXXX",  #Service Principal client id from EntraAD
  "clientSecret": "XXXXXXXXXXXXXX", #Service Principal client secret value (*Not secret id)
  "subscriptionId": "XXXXXXXXXXXXX",  #From subscriptions
  "tenantId": "XXXXXXXXXXXX"  #From tenents
}
```

GH_PAT_TOKEN
```json
ghp_XXXXXXXXXXXXXXXXXXXXXXXXX
```

![image](https://github.com/user-attachments/assets/b45f54d6-83cc-4b2c-b554-4d13d86b4f3f)

## 6. Create containerapp.yaml
```yaml
properties:
  configuration:
    ingress:
      external: true
      targetPort: 8080
      allowInsecure: false
      traffic:
        - latestRevision: true
          weight: 100
  template:
    containers:
      - name: billing-api
        image: placeholder-image # Will be replaced during deployment
        resources:
          cpu: 0.25
          memory: 0.5Gi
        env:
          - name: ASPNETCORE_ENVIRONMENT
            value: Production
          - name: ANOTHER_ENV
            value: Sangeeth
    scale:
      minReplicas: 0
      maxReplicas: 2
      rules:
        - name: http-rule
          http:
            metadata:
              concurrentRequests: '10'
```

## 5. GitHub YAML
```yaml
name: My Deployment of BillingAPI

on:
  push:
    branches: [ "main", "master" ]
  workflow_dispatch:

env:
  REGISTRY: ghcr.io
  REGISTRY_URL: https://ghcr.io
  IMAGE_NAME: ${{ github.repository_owner }}/billingapi
  RESOURCE_GROUP: Hms
  APP_NAME: billing-api
  ENVIRONMENT: hms-api-env

jobs:
  build:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write
    steps:
      - name: Checkout repository
        uses: actions/checkout@v4

      - name: Set up .NET Core
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "9.0.x"

      - name: Build
        run: dotnet build --configuration Release

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v3

      - name: Generate Unique Image Tag
        run: echo "IMAGE_TAG=$(date +'%M%S_%p_%d_%m_%Y')" >> $GITHUB_ENV

      - name: Log in to GitHub Container Registry
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY_URL }}
          username: ${{ github.actor }}
          password: ${{ secrets.GH_PAT_TOKEN }}

      - name: Build and push Docker image
        uses: docker/build-push-action@v6
        with:
          context: .
          push: true
          tags: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ env.IMAGE_TAG }}

      - name: Set output
        id: output-image-details
        run: echo "image_tag=${{ env.IMAGE_TAG }}" >> $GITHUB_OUTPUT

    outputs:
      image_tag: ${{ steps.output-image-details.outputs.image_tag }}

  deploy:
    needs: build
    runs-on: ubuntu-latest
    steps:
      - name: Checkout repository
        uses: actions/checkout@v4
        
      - name: Update YAML with image
        run: |
          IMAGE="${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ needs.build.outputs.image_tag }}"
          # Update image in the YAML file
          sed -i "s|image: placeholder-image|image: $IMAGE|g" ./containerapp.yaml
          echo "Updated containerapp.yaml with image: $IMAGE"

      - name: Azure login
        uses: azure/login@v2
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}

      - name: Deploy to Azure Container Apps
        uses: Azure/container-apps-deploy-action@v2
        with:
          containerAppName: ${{ env.APP_NAME }}
          resourceGroup: ${{ env.RESOURCE_GROUP }}
          containerAppEnvironment: ${{ env.ENVIRONMENT }}
          registryUrl: ${{ env.REGISTRY }}
          registryUsername: ${{ github.actor }}
          registryPassword: ${{ secrets.GH_PAT_TOKEN }}
          yamlConfigPath: ./containerapp.yaml
```
