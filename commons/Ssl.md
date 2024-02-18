# CertBot Commands

### Generate SSL
```bash
sudo certbot certonly --standalone -d example.com
```

### Renew SSL
```bash
sudo certbot renew --cert-name example.com --force-renewal
```

### Auto Renew All Expired Certificates
```bash
sudo certbot renew
```

### Verify
```bash
sudo certbot certificates
```

<hr/>

> SSL Path will be usualy:
``` 
/etc/letsencrypt/live/example.com/fullchain.pem;
/etc/letsencrypt/live/example.com/privkey.pem;
```