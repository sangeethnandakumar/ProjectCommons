# NGinx Website Config For HTTP-3 QUIC

```conf
server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    listen 443 quic reuseport;
    listen [::]:443 quic reuseport;

    server_name twileloop.com www.twileloop.com;

    ssl_certificate /etc/letsencrypt/live/twileloop.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/twileloop.com/privkey.pem;
    ssl_trusted_certificate /etc/letsencrypt/live/twileloop.com/chain.pem;

    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_prefer_server_ciphers off;

    add_header Alt-Svc 'h3=":443"; ma=86400';

    location / {
        proxy_pass http://localhost:3000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

```
