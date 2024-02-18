# Docker Commands

### All Images
```powershell
docker images
```

### All Active Containers
```powershell
docker ps
```

### All Containers
```powershell
docker ps -a
```

### Start/Stop a Container
```powershell
docker start [CONTAINER_ID]
docker stop [CONTAINER_ID]
```

### Remove a Container
```powershell
docker rm [CONTACINER_ID_]
```

### Force remove an Image
```powershell
docker rmi -f [CONTACINER_ID_]
```

## Remove all volumes
```powershell
docker rm -vf $(docker ps -a -q)
docker volume prune -f
```

### View Logs of a Container
```powershell
docker logs [CONTACINER_ID_]
```

### Statitics
```powershell
docker stats
```

### Run a Container
```powershell
docker run -d -p [HOST_PORT]:[CONTAINER_PORT] -v [HOST_PATH]:[CONTAINER_PATH] --name [CONTAINER_NAME] [IMAGE_NAME]
```

### Execute Command
```powershell
docker exec -it [CONTAINER_ID] [COMMAND]
```

### Disk Usage
```powershell
docker system df
```

### Remove Unused Data
```powershell
docker system prune -a
```

<hr/>

# Common Installations

### Redis
##### Mount Volume And Start Redis (With Password)

```powershell
docker run -d -p 6379:6379 -v /docker_volumes/redis:/data --name 'rediscache' redis redis-server --requirepass ******
```

#
## Mongo
##### Mount Volume And Start MongoDB (With Username & Password)

```powershell
docker run -d -p 27017:27017 -v /docker_volumes/mongo:/data/db --name mongodb -e MONGO_INITDB_ROOT_USERNAME=root -e MONGO_INITDB_ROOT_PASSWORD=****** mongo
```

### Seq
##### Mount Volume And Start Seq Log Server (With Username & Password)

```powershell
docker run -d -p 5341:80 -v /docker_volumes/seq:/data --name seq -e ACCEPT_EULA=Y -e SEQ_FIRSTRUN_ADMINUSERNAME='********' -e SEQ_FIRSTRUN_ADMINPASSWORDHASH="$(echo '********' | docker run --rm -i datalust/seq config hash)" datalust/seq
```

### WordPress + MySQL setup (By connecting with network)
##### Mount Volume And Wite A Network. Then Start WordPress & MySQL (With Username & Password)

```powershell
docker network create 'wordpress-network'

docker run -d --name mysql --network 'wordpress-network' -e MYSQL_ROOT_PASSWORD='*******' -e MYSQL_DATABASE='wordpress' -e MYSQL_USER='wordpressuser' -e MYSQL_PASSWORD='*******' -v /docker_volumes/mysql:/var/lib/mysql mysql

docker run -d --name wordpress --network 'wordpress-network' -p 5000:80 -e WORDPRESS_DB_HOST='mysql' -e WORDPRESS_DB_USER='wordpressuser' -e WORDPRESS_DB_PASSWORD='*******' -e WORDPRESS_DB_NAME='wordpress' -v /docker_volumes/wordpress:/var/www/html wordpress
```
