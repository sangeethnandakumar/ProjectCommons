# Generate SSL CERT
Use this command to generate an SSL certificate
```bash
sudo certbot certonly --standalone -d example.com
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
