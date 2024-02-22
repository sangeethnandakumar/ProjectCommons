# Docker Commands
## Common Installations

<hr/>

### Promethius
##### It need root access within the container, So pass `-u 0:0` to docker
```powershell
sudo docker run -d --name prometheus -p 9090:9090 -u 0:0 -v /docker_volumes/prometheus:/opt/bitnami/prometheus/data bitnami/prometheus:latest
```

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
