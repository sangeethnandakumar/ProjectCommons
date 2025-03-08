# Azure Container Apps + GitHub Container

1. Create A Service Principal In EntraAD & Generate Secret
Note down the secret value

![image](https://github.com/user-attachments/assets/8885181a-edcf-471d-887f-93398ef01264)

2. Setup RBAC
Goto resource group --> IAM --> Add Role

![image](https://github.com/user-attachments/assets/6d9ce4de-8e2e-48c5-9106-05454bdf1bca)

3. Generate GitHub PAT Token
Generate a GitHub PAT token from Settings --> Developer Settings --> Classic Token.

> Ensure token has read, write, delete access to Packages

![image](https://github.com/user-attachments/assets/aac4c340-6dba-488f-a4d7-eed81c4771cc)

4. Add PAT & Azure Service Credential To GitHub actin secrets

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

5. GitHub YAML
```yaml
name: Auto Deployment
on:
  push:
    branches: [ "main", "master" ]
  workflow_dispatch:
env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository_owner }}/billingapi  #Change
  RESOURCE_GROUP: Hms  #Change
  APP_NAME: billing-api  #Change
  ENVIRONMENT: hms-api-env  #Change
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
          dotnet-version: "9.0.x"  #Change
      - name: Build
        run: dotnet build --configuration Release
      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v3
      - name: Generate Unique Image Tag
        run: echo "IMAGE_TAG=$(date +'%M%S_%p_%d_%m_%Y')" >> $GITHUB_ENV
      - name: Log in to GitHub Container Registry
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GH_PAT_TOKEN }}  
      - name: Build and push Docker image
        uses: docker/build-push-action@v6
        with:
          context: .
          push: true
          tags: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ env.IMAGE_TAG }}
      - name: Output Image Details
        run: echo "::set-output name=image_tag::${{ env.IMAGE_TAG }}"
    outputs:
      image_tag: ${{ steps.output-image-details.outputs.image_tag }}
      
  deploy:
    needs: build
    runs-on: ubuntu-latest
    steps:
      - name: Azure login
        uses: azure/login@v2
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}
      - name: Deploy to Azure Container Apps (Replace Existing Revision)
        uses: Azure/container-apps-deploy-action@v2
        with:
          imageToDeploy: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ needs.build.outputs.image_tag }}
          containerAppName: ${{ env.APP_NAME }}
          resourceGroup: ${{ env.RESOURCE_GROUP }}
          containerAppEnvironment: ${{ env.ENVIRONMENT }}
          targetPort: 8080  #Change
          ingress: external  #Change
          registryUrl: ${{ env.REGISTRY }}
          registryUsername: ${{ github.actor }}
          registryPassword: ${{ secrets.GH_PAT_TOKEN }}
          #Chang Envs
          environmentVariables: |
            ENV_VAR_1=value1
            ENV_VAR_2=value2
```
