# Generate An SSL certificate
Use this command to generate an SSL certificate

```bash
sudo certbot certonly --standalone -d example.com
```

# Delete all Docker Processes
```bash
sudo docker stop seq
sudo docker rm seq
```

# Stop all running Docker Processes
```bash
sudo docker stop $(sudo docker ps -a -q)
```

# Start New Process In Docker By Accepting License. Forweard Docker-port 80 into local-port 5000
```bash
sudo docker run \
    --restart always \
    --detach \
    --name seq \
    --env ACCEPT_EULA=Y \
    --publish 5000:80 \
    datalust/seq
```
