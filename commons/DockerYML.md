# YML - Docker Build + Deploy + Run
```yml
name: Build & Deploy Docker Container

on:
  workflow_dispatch:
  push:
    branches:
      - master

env:
  # Docker Config  
  IMAGE_NAME           : 'image-name'
  SERVER_PORT          : 5002
  CONTAINER_PORT       : 8080
  
  # Dotnet Config
  DOTNET_VERSION       : '8.0.101'
  DOCKER_FILE_LOCATION : './Api'
  PROJECT_PATH         : './Api/Api.csproj'
  
  # Linux Config  
  SERVER_HOST: 'twileloop.com'
  SERVER_USERNAME: 'root'  
  SERVER_SSH: ${{ secrets.DEPLOY_KEY }}

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Set up .NET
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore
        run: dotnet restore ${{ env.PROJECT_PATH }}

      - name: Build
        run: dotnet build ${{ env.PROJECT_PATH }} --configuration Release --no-restore

      - name: Publish
        run: dotnet publish ${{ env.PROJECT_PATH }} --configuration Release --no-build --output './publish/'

      - name: Build Docker Image
        run: docker build -t ${{ env.IMAGE_NAME }} ${{ env.DOCKER_FILE_LOCATION }}

      - name: Save Docker Image
        run: docker save ${{ env.IMAGE_NAME }} | gzip > ${{ env.IMAGE_NAME }}.tar.gz

      - name: Copy Docker Image to VM
        uses: appleboy/scp-action@master
        with:
          host: ${{ env.SERVER_HOST }}
          username: ${{ env.SERVER_USERNAME }}
          key: ${{ env.SERVER_SSH }}
          source: "./${{ env.IMAGE_NAME }}.tar.gz"
          target: "/tmp"

      - name: Load and Run Docker Image on VM
        uses: appleboy/ssh-action@master
        with:
          host: ${{ env.SERVER_HOST }}
          username: ${{ env.SERVER_USERNAME }}
          key: ${{ env.SERVER_SSH }}
          script: |
            if [ "$(docker images -q ${{ env.IMAGE_NAME }})" ]; then
              if [ "$(docker ps -aq -f ancestor=${{ env.IMAGE_NAME }})" ]; then
                docker stop $(docker ps -aq -f ancestor=${{ env.IMAGE_NAME }})
                docker rm $(docker ps -aq -f ancestor=${{ env.IMAGE_NAME }})
              fi
              docker rmi ${{ env.IMAGE_NAME }}
              rm -rf /docker_volumes/${{ env.IMAGE_NAME }}
            fi
            docker load < /tmp/${{ env.IMAGE_NAME }}.tar.gz
            mkdir -p /docker_volumes/${{ env.IMAGE_NAME }}
            docker run -d -p ${{ env.SERVER_PORT }}:${{ env.CONTAINER_PORT }} --name ${{ env.IMAGE_NAME }} ${{ env.IMAGE_NAME }}

```
### Docker File Expectation
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Api.csproj", "./"]
RUN dotnet restore "./Api.csproj"
COPY . .
RUN dotnet build "./Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Api.dll"]
```