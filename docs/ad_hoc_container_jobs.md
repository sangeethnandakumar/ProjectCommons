# ACA Container - AdHoc Runs

```
name: Deploy To Azure Container Apps Jobs

on:
  push:
    branches: [ "main", "master" ]
  workflow_dispatch:

env:
  # Registry Configuration
  REGISTRY: ghcr.io
  REGISTRY_URL: https://ghcr.io
  IMAGE_NAME: ${{ github.repository_owner }}/job-invoicing  # Change this for each project
  
  # Azure Configuration
  RESOURCE_GROUP: fastgst.in                              # Change this for each project
  ACA_JOB_NAME: job-invoicing                             # Change this for each project
  ACA_ENV: env-job-invoicing                              # Change this for each project
  ACA_LOCATION: centralindia                              # Change this for your preferred location
  
  # .NET Configuration
  DOTNET_VERSION: "9.0.x"                                 # Change this for your .NET version
  
  # Job Configuration
  JOB_TYPE: Manual                                        # Manual (on-demand) or Scheduled
  CPU: 0.5                                               # Change CPU allocation for jobs
  MEMORY: 1Gi                                            # Change memory allocation for jobs
  PARALLELISM: 1                                         # Number of parallel job executions
  COMPLETION_COUNT: 1                                     # Number of successful completions needed
  TRIGGER_TYPE: Manual                                    # Manual or Schedule
  
  # Environment Variables for Container Job
  ASPNETCORE_ENVIRONMENT: Production                      # Change environment
  CUSTOM_ENV_VAR: Sangeeth                               # Add/change your custom env vars
  JOB_TIMEOUT: 3600                                      # Job timeout in seconds (1 hour)

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
        run: dotnet build job.invoicing.sln --configuration Release

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

      - name: Check if Container App Job exists and create/update accordingly
        run: |
          EXISTS=$(az containerapp job show -g ${{ env.RESOURCE_GROUP }} -n ${{ env.ACA_JOB_NAME }} --query id -o tsv 2>/dev/null || echo "")
          if [ -z "$EXISTS" ]; then
            echo "Container App Job not found. Creating..."
            
            # Create the job with registry credentials
            az containerapp job create \
              -g ${{ env.RESOURCE_GROUP }} \
              -n ${{ env.ACA_JOB_NAME }} \
              --environment ${{ env.ACA_ENV }} \
              --image ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ needs.build.outputs.image_tag }} \
              --registry-server ${{ env.REGISTRY }} \
              --registry-username ${{ github.actor }} \
              --registry-password ${{ secrets.GH_PAT_TOKEN }} \
              --trigger-type ${{ env.TRIGGER_TYPE }} \
              --replica-timeout ${{ env.JOB_TIMEOUT }} \
              --replica-retry-limit 3 \
              --replica-completion-count ${{ env.COMPLETION_COUNT }} \
              --parallelism ${{ env.PARALLELISM }} \
              --cpu ${{ env.CPU }} \
              --memory ${{ env.MEMORY }} \
              --env-vars ASPNETCORE_ENVIRONMENT=${{ env.ASPNETCORE_ENVIRONMENT }} ANOTHER_ENV=${{ env.CUSTOM_ENV_VAR }}
              
            echo "Container App Job created successfully!"
          else
            echo "Container App Job exists. Updating..."
            
            # Update the existing job with new image
            az containerapp job update \
              -g ${{ env.RESOURCE_GROUP }} \
              -n ${{ env.ACA_JOB_NAME }} \
              --image ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ needs.build.outputs.image_tag }} \
              --registry-server ${{ env.REGISTRY }} \
              --registry-username ${{ github.actor }} \
              --registry-password ${{ secrets.GH_PAT_TOKEN }} \
              --cpu ${{ env.CPU }} \
              --memory ${{ env.MEMORY }} \
              --env-vars ASPNETCORE_ENVIRONMENT=${{ env.ASPNETCORE_ENVIRONMENT }} ANOTHER_ENV=${{ env.CUSTOM_ENV_VAR }}
              
            echo "Container App Job updated successfully!"
          fi

      - name: Display Job Information
        run: |
          echo "=== Container App Job Details ==="
          az containerapp job show -g ${{ env.RESOURCE_GROUP }} -n ${{ env.ACA_JOB_NAME }} --query "{name:name,location:location,provisioningState:properties.provisioningState,triggerType:properties.configuration.triggerType}" -o table
          
          echo ""
          echo "=== Job Configuration ==="
          echo "Job Name: ${{ env.ACA_JOB_NAME }}"
          echo "Resource Group: ${{ env.RESOURCE_GROUP }}"
          echo "Environment: ${{ env.ACA_ENV }}"
          echo "Image: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ needs.build.outputs.image_tag }}"
          echo "Trigger Type: ${{ env.TRIGGER_TYPE }}"
          echo "CPU: ${{ env.CPU }}"
          echo "Memory: ${{ env.MEMORY }}"
          
          echo ""
          echo "=== Manual Execution Command ==="
          echo "To run this job manually, use:"
          echo "az containerapp job start -n ${{ env.ACA_JOB_NAME }} -g ${{ env.RESOURCE_GROUP }}"

```
