# Launch Settings For Docker To Bind To A Specific Port
Binds to port 5000 & 5001 always

```json
 "Container (Dockerfile)": {
   "commandName": "Docker",
   "launchBrowser": true,
   "launchUrl": "{Scheme}://{ServiceHost}:{ServicePort}/swagger",
   "environmentVariables": {
     "ASPNETCORE_HTTPS_PORTS": "5000",
     "ASPNETCORE_HTTP_PORTS": "5001"
   },
   "publishAllPorts": true,
   "useSSL": true,
   "httpPort": 5000,
   "sslPort": 5001
 }
```
