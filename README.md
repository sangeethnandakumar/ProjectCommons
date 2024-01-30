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
