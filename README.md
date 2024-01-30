# Docker remove all volumes
```bash
docker rm -vf $(docker ps -a -q)
docker volume prune -f
```


# Generate SSL CERT
Use this command to generate an SSL certificate
```bash
sudo certbot certonly --standalone -d example.com
```
# Renew Specific SSL CERT
```bash
sudo certbot renew --cert-name twileloop.com --force-renewal
```

<hr/>

# Redis
## Mount Volume And Start Redis (With Password)

```bash
docker run -d -p 6379:6379 -v /docker_volumes/redis:/data --name 'rediscache' redis redis-server --requirepass ******
```

# Mongo
## Mount Volume And Start MongoDB (With Username & Password)

```bash
docker run -d -p 27017:27017 -v /docker_volumes/mongo:/data/db --name mongodb -e MONGO_INITDB_ROOT_USERNAME=root -e MONGO_INITDB_ROOT_PASSWORD=****** mongo
```

# Seq
## Mount Volume And Start Seq Log Server (With Username & Password)

```bash
docker run -d -p 5341:80 -v /docker_volumes/seq:/data --name seq -e ACCEPT_EULA=Y -e SEQ_FIRSTRUN_ADMINUSERNAME='********' -e SEQ_FIRSTRUN_ADMINPASSWORDHASH="$(echo '********' | docker run --rm -i datalust/seq config hash)" datalust/seq
```


# Auto Renew All Expired Certificates
Stop Apache
```bash
sudo systemctl stop apache2
```
Update SSL Certificates using certbot (Let's Encrypt CLI tool)
```bash
sudo certbot renew
```
Verify
```bash
sudo certbot certificates
```
Restart Apache
```bash
sudo systemctl start apache2
```
<hr/>

# Docker Commands

### STOP ALL
```bash
sudo docker stop $(sudo docker ps -a -q)
```

### STOP
```bash
sudo docker stop seq
```

### REMOVE
```bash
sudo docker rm seq
```

# START
```bash
sudo docker run \
    --restart always \
    --detach \
    --name seq \
    --env ACCEPT_EULA=Y \
    --publish 5000:80 \
    datalust/seq
```
