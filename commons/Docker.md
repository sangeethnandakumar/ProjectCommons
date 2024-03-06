# Docker Commands

## Docker
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Instaread.BestSellingScrapper.API.csproj", "./"]
RUN dotnet restore "./Instaread.BestSellingScrapper.API.csproj"
COPY . .
RUN dotnet build "./Instaread.BestSellingScrapper.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Instaread.BestSellingScrapper.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p Shared

ENTRYPOINT ["dotnet", "Instaread.BestSellingScrapper.API.dll"]
```

## COPY/PASTE
```powershell
//Promethieus Config
docker cp prometheus:/etc/prometheus/prometheus.yml /docker_volumes/temp/prometheus.yml
docker cp /docker_volumes/temp/prometheus.yml prometheus:/etc/prometheus/prometheus.yml

//Another example
docker cp 9c0c28810d4a:/app/Ebooks /backups/Ebooks
```

## Install Docker
```powershell
sudo apt update
```
```powershell
sudo apt install apt-transport-https ca-certificates curl software-properties-common
```
```powershell
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
```
```powershell
echo "deb [signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
```
```powershell
sudo apt update
```
```powershell
sudo apt install docker-ce docker-ce-cli containerd.io
```

## Common Installations

<hr/>

### Promethius
##### It need root access within the container, So pass `-u 0:0` to docker
```powershell
docker run -d --name=prometheus -p 9090:9090 -u 0:0 -v /docker_volumes/prometheus:/opt/bitnami/prometheus/data bitnami/prometheus
```


### Graphana
```powershell
docker run -d -u 0:0 --name=grafana -p 3000:3000 -e "GF_SECURITY_ADMIN_USER=root" -e "GF_SECURITY_ADMIN_PASSWORD=*****" -v /docker_volumes/grafana:/var/lib/grafana grafana/grafana
```

### Redis
##### Mount Volume And Start Redis (With Password)

```powershell
docker run -d -u 0:0 -p 6379:6379 -v /docker_volumes/redis:/data --name 'redis' redis redis-server --requirepass '****'
```

### Mongo
##### Mount Volume And Start MongoDB (With Username & Password)

```powershell
docker run -d -p 27017:27017 -v /docker_volumes/mongo:/data/db --name mongodb -e MONGO_INITDB_ROOT_USERNAME='root' -e MONGO_INITDB_ROOT_PASSWORD='*****' mongo
```

### Seq
##### Mount Volume And Start Seq Log Server (With Username & Password)

```powershell
docker run -d -p 5341:80 -v /docker_volumes/seq:/data --name seq -e ACCEPT_EULA=Y -e SEQ_FIRSTRUN_ADMINUSERNAME='root' -e SEQ_FIRSTRUN_ADMINPASSWORDHASH="$(echo '*****' | docker run --rm -i datalust/seq config hash)" datalust/seq
```

### WordPress + MySQL setup (By connecting with network)
##### Mount Volume And Wite A Network. Then Start WordPress & MySQL (With Username & Password)

```powershell
docker network create 'wordpress-network'

docker run -d --name mysql --network 'wordpress-network' -e MYSQL_ROOT_PASSWORD='*******' -e MYSQL_DATABASE='wordpress' -e MYSQL_USER='wordpressuser' -e MYSQL_PASSWORD='*******' -v /docker_volumes/mysql:/var/lib/mysql mysql

docker run -d --name wordpress --network 'wordpress-network' -p 5000:80 -e WORDPRESS_DB_HOST='mysql' -e WORDPRESS_DB_USER='wordpressuser' -e WORDPRESS_DB_PASSWORD='*******' -e WORDPRESS_DB_NAME='wordpress' -v /docker_volumes/wordpress:/var/www/html wordpress
```
