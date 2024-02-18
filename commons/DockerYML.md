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
  IMAGE_NAME           : 'mobile-api'
  NGINX_DOMAIN         : 'api.twileloop.com'
  SERVER_PORT          : 5003
  CONTAINER_PORT       : 8080
  
  # Dotnet Config
  DOTNET_VERSION       : '8.0.101'
  DOCKER_FILE_LOCATION : './MobileAPI'
  PROJECT_PATH         : './MobileAPI/MobileAPI.csproj'
  
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
            # Check if the Docker image exists
            if [ "$(docker images -q ${{ env.IMAGE_NAME }})" ]; then
              # Check if a container from this image is currently running
              if [ "$(docker ps -aq -f ancestor=${{ env.IMAGE_NAME }})" ]; then
                # Stop and remove the running container
                docker stop $(docker ps -aq -f ancestor=${{ env.IMAGE_NAME }})
                docker rm $(docker ps -aq -f ancestor=${{ env.IMAGE_NAME }})
              fi
              # Remove the Docker image
              docker rmi ${{ env.IMAGE_NAME }}
            fi
            # Check if the host volume directory exists
            if [ -d "${{ env.HOST_VOLUME_PATH }}" ]; then
              # If it exists, clear the host volume directory
              rm -rf ${{ env.HOST_VOLUME_PATH }}/*
            fi
            # Load the Docker image from the tar file
            docker load < /tmp/${{ env.IMAGE_NAME }}.tar.gz
            # Ensure the host volume directory exists
            mkdir -p /docker_volumes/${{ env.IMAGE_NAME }}
            # Run the Docker image with the specified port and volume mappings
            docker run -d -p ${{ env.SERVER_PORT }}:${{ env.CONTAINER_PORT }} -v /docker_volumes/${{ env.IMAGE_NAME }}:/app/publish --name ${{ env.IMAGE_NAME }} ${{ env.IMAGE_NAME }}

      - name: Configure NGINX
        uses: appleboy/ssh-action@master
        with:
          host: ${{ env.SERVER_HOST }}
          username: ${{ env.SERVER_USERNAME }}
          key: ${{ env.SERVER_SSH }}
          script: |
            # Check if the NGINX config exists
            if [ ! -f "/etc/nginx/sites-enabled/${{ env.NGINX_DOMAIN }}" ]; then
              # If it doesn't exist, create the NGINX config
              cat << EOF > /etc/nginx/sites-enabled/${{ env.NGINX_DOMAIN }}
              server {
                  listen 80;
                  server_name ${{ env.NGINX_DOMAIN }};
                  return 301 https://\$host\$request_uri;
              }
      
              server {
                  listen 443 ssl http2;
                  server_name ${{ env.NGINX_DOMAIN }};
      
                  location / {
                      proxy_pass http://localhost:${{ env.SERVER_PORT }};
                      proxy_set_header Upgrade \$http_upgrade;
                      proxy_set_header Connection "upgrade";
                      proxy_cache off;
                      proxy_http_version 1.1;
                      proxy_buffering off;
                      proxy_read_timeout 100s;
                      proxy_set_header Host \$host;
                      proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
                      proxy_set_header X-Forwarded-Proto \$scheme;
                      proxy_set_header X-Real-IP \$remote_addr;
                  }
      
                  ssl_certificate /etc/letsencrypt/live/${{ env.NGINX_DOMAIN }}/fullchain.pem;
                  ssl_certificate_key /etc/letsencrypt/live/${{ env.NGINX_DOMAIN }}/privkey.pem;
              }
            EOF
            fi

      - name: Stop NGINX
        uses: appleboy/ssh-action@master
        with:
          host: ${{ env.SERVER_HOST }}
          username: ${{ env.SERVER_USERNAME }}
          key: ${{ env.SERVER_SSH }}
          script: sudo systemctl stop nginx

      - name: Kill Running Certbot Processes
        uses: appleboy/ssh-action@master
        with:
          host: ${{ env.SERVER_HOST }}
          username: ${{ env.SERVER_USERNAME }}
          key: ${{ env.SERVER_SSH }}
          script: sudo killall certbot || true
      
      - name: Setup SSL Certificate
        uses: appleboy/ssh-action@master
        with:
          host: ${{ env.SERVER_HOST }}
          username: ${{ env.SERVER_USERNAME }}
          key: ${{ env.SERVER_SSH }}
          script: |
            # Check if the SSL certificate exists and is not expired
            if ! sudo certbot certificates | grep -B1 -A2 ${{ env.NGINX_DOMAIN }} | grep -q "VALID"; then
              # If it doesn't exist or is expired, create or renew the SSL certificate
              sudo certbot certonly --standalone -d ${{ env.NGINX_DOMAIN }} --non-interactive --agree-tos --email your-email@example.com --http-01-port=80
            fi
      
      - name: Start NGINX
        uses: appleboy/ssh-action@master
        with:
          host: ${{ env.SERVER_HOST }}
          username: ${{ env.SERVER_USERNAME }}
          key: ${{ env.SERVER_SSH }}
          script: sudo systemctl start nginx
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
