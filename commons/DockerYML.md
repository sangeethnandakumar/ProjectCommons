## Step 1
Create `Dockerfile.prod`
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
EXPOSE 8080
FROM base AS final
WORKDIR /app

COPY ../../publish/ .
RUN mkdir -p Shared

ENTRYPOINT ["dotnet", "parinaybharat.api.dll"]
```

## Shared Folder + Static Assets
If you need to create multiple sub folders inside Shared folder, It's a good idea to do it on app start
```csharp
CreateSharedDirectories();
if(!app.Environment.IsDevelopment())
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot")),
        RequestPath = "",
        ServeUnknownFileTypes = true,
    });
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Shared", "Ebooks")),
    RequestPath = "/preview",
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/epub+zip",
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();

static void CreateSharedDirectories()
{
    var staticAssetLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Shared", "Ebooks");
    if (!Directory.Exists(staticAssetLocation))
    {
        Directory.CreateDirectory(staticAssetLocation);
    }
}
```

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
  IMAGE_NAME           : 'api.parinaybharat'
  NGINX_DOMAIN         : 'api.parinaybharat.twileloop.com'
  SERVER_PORT          : 6000
  DOCKER_FILE_LOCATION : './parinaybharat.api' 
  PROJECT_PATH         : './parinaybharat.api/parinaybharat.api.csproj'
  HOST_VOLUME_PATH     : '/docker_volumes/api.parinaybharat.twileloop.com'
  
  # Dotnet Config
  DOTNET_VERSION       : '9.0.100'
  CONTAINER_PORT       : 8080
  
  # Linux Config  
  SERVER_HOST          : 'twileloop.com'
  SERVER_USERNAME      : 'root'  
  SERVER_SSH           : ${{ secrets.DEPLOY_KEY }}

  # List of Docker environment variables
  DOCKER_ENVS: |
    ApplicationInsights__ConnectionString=${{ secrets.CON_APP_INSIGHTS }}
    KeyVault__TenantId=${{ secrets.KVLT_TENANT_ID }}
    KeyVault__ClientId=${{ secrets.KVLT_CLIENT_ID }}
    KeyVault__ClientSecret=${{ secrets.KVLT_CLIENT_SECRET }}
    KeyVault__VaultUri=${{ secrets.KVLT_VAULT_URI }}
    KeyVault__CacheExpirationMinutes=${{ secrets.KVLT_CACHE_EXPIRATION }}


jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Cache .NET build output
        uses: actions/cache@v3
        with:
          path: ${{ env.PROJECT_PATH }}/obj
          key: ${{ runner.os }}-dotnet-${{ hashFiles('**/*.csproj') }}
          restore-keys: |
            ${{ runner.os }}-dotnet-
      
      - name: Cache NuGet packages
        uses: actions/cache@v3
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
          restore-keys: |
            ${{ runner.os }}-nuget-
      
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

      - name: Cache Docker layers
        uses: actions/cache@v3
        with:
          path: /tmp/.buildx-cache
          key: ${{ runner.os }}-buildx-${{ hashFiles('**/Dockerfile.prod') }}
          restore-keys: |
            ${{ runner.os }}-buildx-
      - name: Build Docker Image
        run: docker build --cache-from=type=local,src=/tmp/.buildx-cache --build-arg BUILDKIT_INLINE_CACHE=1 -f ${{ env.DOCKER_FILE_LOCATION }}/Dockerfile.prod -t ${{ env.IMAGE_NAME }} .
      
      - name: Save Docker Image
        run: docker save ${{ env.IMAGE_NAME }} | gzip > ${{ env.IMAGE_NAME }}.tar.gz

      - name: Install rsync
        run: sudo apt-get install -y rsync
        
      - name: Transfer using rsync with progress
        run: |
          echo "${{ env.SERVER_SSH }}" > ssh_key
          chmod 600 ssh_key
          rsync -avz --progress -e "ssh -i ssh_key -o StrictHostKeyChecking=no" \
            "./${{ env.IMAGE_NAME }}.tar.gz" \
            ${{ env.SERVER_USERNAME }}@${{ env.SERVER_HOST }}:/tmp/
          rm ssh_key

      - name: Create Host-Volume Directory + Env file
        uses: appleboy/ssh-action@master
        with:
          host: ${{ env.SERVER_HOST }}
          username: ${{ env.SERVER_USERNAME }}
          key: ${{ env.SERVER_SSH }}
          script: |
            # Check if the host volume directory exists
            if [ ! -d "${{ env.HOST_VOLUME_PATH }}" ]; then
              # If it doesn't exist, create the host volume directory
              mkdir -p "${{ env.HOST_VOLUME_PATH }}"
            fi

            # Create or replace the prod.env file
            : > "${{ env.HOST_VOLUME_PATH }}/prod.env"

            # Split the DOCKER_ENVS string into individual variables using newline as delimiter
            echo "${{ env.DOCKER_ENVS }}" | while IFS= read -r env_var; do
              if [ ! -z "$env_var" ]; then
                echo "$env_var" >> "${{ env.HOST_VOLUME_PATH }}/prod.env"
              fi
            done

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
            # Load the Docker image from the tar file
            docker load < /tmp/${{ env.IMAGE_NAME }}.tar.gz
            # Run the Docker image with the specified port and volume mappings
            docker run -d --env-file "${{ env.HOST_VOLUME_PATH }}/prod.env" -p "${{ env.SERVER_PORT }}:${{ env.CONTAINER_PORT }}" -v "/docker_volumes/${{ env.IMAGE_NAME }}:/app/Shared" --name "${{ env.IMAGE_NAME }}" --cpus 0.5 -u $(id -u):$(id -g) "${{ env.IMAGE_NAME }}"
            
      - name: Configure NGINX
        uses: appleboy/ssh-action@master
        with:
          host: ${{ env.SERVER_HOST }}
          username: ${{ env.SERVER_USERNAME }}
          key: ${{ env.SERVER_SSH }}
          script: |
            CONFIG_PATH="/etc/nginx/sites-enabled/${{ env.NGINX_DOMAIN }}"
            # Always write the config file
            cat << EOF > $CONFIG_PATH
            server {
                listen 80;
                server_name ${{ env.NGINX_DOMAIN }};
                return 301 https://\$host\$request_uri;
            }
    
            server {
                listen 443 ssl http2;
                server_name ${{ env.NGINX_DOMAIN }};

                client_max_body_size 200M;
    
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


      - name: Stop NGINX
        uses: appleboy/ssh-action@master
        with:
          host: ${{ env.SERVER_HOST }}
          username: ${{ env.SERVER_USERNAME }}
          key: ${{ env.SERVER_SSH }}
          script: sudo systemctl stop nginx

      - name: Setup SSL Certificate
        uses: appleboy/ssh-action@master
        with:
          host: ${{ env.SERVER_HOST }}
          username: ${{ env.SERVER_USERNAME }}
          key: ${{ env.SERVER_SSH }}
          script: |
            # Check if there is an SSL certificate for the domain
            if ! sudo certbot certificates | grep -B1 -A2 ${{ env.NGINX_DOMAIN }} | grep -q "VALID"; then
              # If not, generate a new SSL certificate
              sudo certbot certonly --standalone -d ${{ env.NGINX_DOMAIN }} --non-interactive --agree-tos --email your-email@example.com --http-01-port=80
            else
              echo "SSL certificate for ${{ env.NGINX_DOMAIN }} already exists and is valid."
            fi

      - name: Start NGINX
        uses: appleboy/ssh-action@master
        with:
          host: ${{ env.SERVER_HOST }}
          username: ${{ env.SERVER_USERNAME }}
          key: ${{ env.SERVER_SSH }}
          script: sudo systemctl start nginx

```
