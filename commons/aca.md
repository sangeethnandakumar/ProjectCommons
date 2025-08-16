# Azure Container Apps + GitHub Container

## 1. Create an SP
Note down the secret value

![image](https://github.com/user-attachments/assets/8885181a-edcf-471d-887f-93398ef01264)

## 2. Resource Group IAM
Goto resource group IAM & Add below roles

| Role                                  |
|---------------------------------------|
| Azure Container Instances Contributor |
| Container Apps Contributor             |
| Contributor                            |

<img width="1722" height="352" alt="image" src="https://github.com/user-attachments/assets/48017862-d265-45ea-adcc-c85e4086dc9d" />


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


## 5. Auto Create ACA & Full Deployment YAML
```yaml
name: Deploy To Azure Container Apps

on:
  push:
    branches: [ "main", "master" ]
  workflow_dispatch:

env:
  # Registry Configuration
  REGISTRY: ghcr.io
  REGISTRY_URL: https://ghcr.io
  IMAGE_NAME: ${{ github.repository_owner }}/api-console  # Change this for each project
  
  # Azure Configuration
  RESOURCE_GROUP: fastgst.in                              # Change this for each project
  ACA_NAME: api-console                                   # Change this for each project
  ACA_ENV: env-api-console                                # Change this for each project
  ACA_LOCATION: centralindia                              # Change this for your preferred location
  
  # .NET Configuration
  DOTNET_VERSION: "9.0.x"                                 # Change this for your .NET version
  
  # Container Configuration
  TARGET_PORT: 8080                                       # Change this for your app's port
  CPU: 0.25                                              # Change CPU allocation
  MEMORY: 0.5Gi                                          # Change memory allocation
  MIN_REPLICAS: 0                                        # Change minimum replicas
  MAX_REPLICAS: 2                                        # Change maximum replicas
  
  # Environment Variables for Container
  ASPNETCORE_ENVIRONMENT: Production                      # Change environment
  CUSTOM_ENV_VAR: Sangeeth                               # Add/change your custom env vars

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
          dotnet-version: ${{ env.DOTNET_VERSION }}

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

      - name: Azure login
        uses: azure/login@v2
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}

      - name: Ensure Resource Group exists
        run: |
          RG=$(az group show -n ${{ env.RESOURCE_GROUP }} --query name -o tsv || echo "")
          if [ -z "$RG" ]; then
            echo "Resource Group not found. Creating..."
            az group create -n ${{ env.RESOURCE_GROUP }} -l ${{ env.ACA_LOCATION }}
          fi

      - name: Ensure Container App Environment exists
        run: |
          ENV_ID=$(az containerapp env show -g ${{ env.RESOURCE_GROUP }} -n ${{ env.ACA_ENV }} --query id -o tsv || echo "")
          if [ -z "$ENV_ID" ]; then
            echo "Environment not found. Creating..."
            az containerapp env create \
              -g ${{ env.RESOURCE_GROUP }} \
              -n ${{ env.ACA_ENV }} \
              --location ${{ env.ACA_LOCATION }} \
              --logs-destination none
          fi
          
          # Wait for environment to be fully provisioned
          echo "Waiting for environment to be fully provisioned..."
          for i in {1..30}; do
            PROVISION_STATE=$(az containerapp env show -g ${{ env.RESOURCE_GROUP }} -n ${{ env.ACA_ENV }} --query properties.provisioningState -o tsv 2>/dev/null || echo "Unknown")
            echo "Attempt $i: Environment provisioning state: $PROVISION_STATE"
            
            if [ "$PROVISION_STATE" = "Succeeded" ]; then
              echo "Environment is ready!"
              break
            elif [ "$PROVISION_STATE" = "Failed" ]; then
              echo "Environment provisioning failed!"
              exit 1
            else
              echo "Still provisioning... waiting 30 seconds"
              sleep 30
            fi
            
            if [ $i -eq 30 ]; then
              echo "Timeout waiting for environment to be ready"
              exit 1
            fi
          done
          
          ENV_ID=$(az containerapp env show -g ${{ env.RESOURCE_GROUP }} -n ${{ env.ACA_ENV }} --query id -o tsv)
          echo "ENV_ID=$ENV_ID" >> $GITHUB_ENV

      - name: Check if Container App exists and create/update accordingly
        run: |
          EXISTS=$(az containerapp show -g ${{ env.RESOURCE_GROUP }} -n ${{ env.ACA_NAME }} --query id -o tsv 2>/dev/null || echo "")
          if [ -z "$EXISTS" ]; then
            echo "Container App not found. Creating..."
            
            # Create with registry credentials
            az containerapp create \
              -g ${{ env.RESOURCE_GROUP }} \
              -n ${{ env.ACA_NAME }} \
              --environment ${{ env.ACA_ENV }} \
              --image ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ needs.build.outputs.image_tag }} \
              --registry-server ${{ env.REGISTRY }} \
              --registry-username ${{ github.actor }} \
              --registry-password ${{ secrets.GH_PAT_TOKEN }} \
              --target-port ${{ env.TARGET_PORT }} \
              --ingress external \
              --cpu ${{ env.CPU }} \
              --memory ${{ env.MEMORY }} \
              --min-replicas ${{ env.MIN_REPLICAS }} \
              --max-replicas ${{ env.MAX_REPLICAS }} \
              --env-vars ASPNETCORE_ENVIRONMENT=${{ env.ASPNETCORE_ENVIRONMENT }} ANOTHER_ENV=${{ env.CUSTOM_ENV_VAR }}
              
            echo "CONTAINER_APP_CREATED=true" >> $GITHUB_ENV
          else
            echo "Container App exists. Will update using deploy action."
            echo "CONTAINER_APP_CREATED=false" >> $GITHUB_ENV
          fi

      - name: Deploy/Update Azure Container Apps
        if: env.CONTAINER_APP_CREATED == 'false'
        uses: Azure/container-apps-deploy-action@v2
        with:
          containerAppName: ${{ env.ACA_NAME }}
          resourceGroup: ${{ env.RESOURCE_GROUP }}
          containerAppEnvironment: ${{ env.ACA_ENV }}
          registryUrl: ${{ env.REGISTRY }}
          registryUsername: ${{ github.actor }}
          registryPassword: ${{ secrets.GH_PAT_TOKEN }}
          imageToDeploy: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ needs.build.outputs.image_tag }}
```
