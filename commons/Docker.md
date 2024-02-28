# Docker Commands

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
sudo docker run -d --name prometheus -p 9090:9090 -u 0:0 -v /docker_volumes/prometheus:/opt/bitnami/prometheus/data bitnami/prometheus:2.49.1-debian-12-r9
```

### Graphana
```powershell
docker run -d --name=grafana -p 3000:3000 grafana/grafana
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
