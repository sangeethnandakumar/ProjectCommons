# CertBot Commands

### Generate SSL
```powershell
sudo certbot certonly --standalone -d example.com
```

### Renew SSL
```powershell
sudo certbot renew --cert-name example.com --force-renewal
```

### Auto Renew All Expired Certificates
```powershell
//Stop NGINX
sudo systemctl stop nginx

//Renew All Certificates
sudo certbot renew

//Restart NGINX
sudo systemctl start nginx
```

### Verify
```powershell
sudo certbot certificates
```

<hr/>

> SSL Path will be usualy:
``` 
/etc/letsencrypt/live/example.com/fullchain.pem;
/etc/letsencrypt/live/example.com/privkey.pem;
```
