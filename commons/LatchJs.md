# Latch.js

```js
//Latch.js
//Helper js file, that connects to SignalR server and helps invoke endpoints and subscribe to events

import { HubConnectionBuilder } from "@microsoft/signalr";

class Latch {
    constructor() {
        this.connection = null;
        this.initializeConnection();
    }

    //Connects to SignalR server
    initializeConnection() {
        this.connection = new HubConnectionBuilder()
            .withUrl(`${import.meta.env.VITE_APP_API_URL}/signalrhub`, { withCredentials: false })
            .withAutomaticReconnect()
            .build();
        this.connection.start()
            .catch((error) => console.error(error));
    }

    //Subscribe to notifications with a callback
    on(component, methodName, callback) {
        this.connection.on(`${component}_${methodName}`, callback);
    }

    //Unsubscribe to a lister
    off(component, methodName, callback) {
        this.connection.off(`${component}_${methodName}`, callback);
    }

    //Invoking modes custom
    invoke(component, methodName, ...args) {
        this.connection.invoke(`${component}_${methodName}`, ...args);
    }

    //Invoking modes unicast
    unicast(component, methodName, ...args) {
        const connectionId = this.connection.connectionId;
        this.connection.invoke("UnicastAsync", connectionId, `${component}_${methodName}`, ...args);
    }

    //Invoking modes multicast
    multicast(connectionIds, component, methodName, ...args) {
        this.connection.invoke("MulticastAsync", connectionIds, `${component}_${methodName}`, ...args);
    }

    //Invoking modes broadcast
    broadcast(component, methodName, ...args) {
        this.connection.invoke("BroadcastAsync", `${component}_${methodName}`, ...args);
    }
}

const latch = new Latch();

export default latch;
```

### Usage

- Install SignalR (`npm i @microsoft/signalr`)
- Replace `HubURL` below
- Configure Server Side
- Ready to use

```js
import Latch from '../libs/latch.js';

const App = () => {

    useEffect(() => {

        //Listners
        const listenForNotifications = msg => console.log(msg);
        const listenForProgress = msg => console.log(msg);

        //Attach
        Latch.on("Dashboard", "Notifications", listenForNotifications);
        Latch.on("Dashboard", "Progress", listenForProgress);

        //Detach
        return () => {
            Latch.off("Dashboard", "Notifications", listenForNotifications);
            Latch.off("Dashboard", "Progress", listenForProgress);
        };

    }, []);

    return (
        <>
            <h1>Hello</h1>
        </>
    );
}

export default App;
```

# ServerSide SignalR

### Ngnix Proxy & WebSocket Support
```
server {
    listen 80;
    server_name example.com;
    return 301 https://$host$request_uri;
}

server {
    # Binding Settings
    listen 443 ssl http2;
    server_name example.com;

    location / {
        # Backend URL
        proxy_pass http://localhost:5002/;
        
        # Proxy Settings
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_cache off;
        proxy_http_version 1.1;
        proxy_buffering off;
        proxy_read_timeout 100s;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Real-IP $remote_addr;
    }

    # SSL Settings
    ssl_certificate /etc/letsencrypt/live/example.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/example.com/privkey.pem;
}
```

### Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//----------------------------------------------

//Add SignalR support
builder.Services.AddSignalR(e =>
{
    e.MaximumReceiveMessageSize = long.MaxValue;
});

//Inject SignalRHub as Singleton
builder.Services.AddSingleton<SignalRHub>();

//----------------------------------------------

var app = builder.Build();

//----------------------------------------------
app.MapHub<SignalRHub>("/signalrhub");
//----------------------------------------------

app.Run();
```

### SignalRHub.cs
```csharp
using Microsoft.AspNetCore.SignalR;
using System.Text;

namespace Instaread.BestSellingScrapper.API.Hubs
{
    public class SignalRHub : Hub
    {
        //Unicast
        public async Task UnicastAsync(string connectionId, string component, string methodName, object payload)
        {
            if(Clients is null)
                return;
            await Clients.Client(connectionId).SendAsync($"{component}_{methodName}", payload);
        }

        //Multicast
        public async Task MulticastAsync(List<string> connectionIds, string component, string methodName, object payload)
        {
            if (Clients is null)
                return;
            await Clients.Clients(connectionIds).SendAsync($"{component}_{methodName}", payload);
        }

        //Broadcast
        public async Task BroadcastAsync(string component, string methodName, object payload)
        {
            if (Clients is null)
                return;
            await Clients.All.SendAsync($"{component}_{methodName}", payload);
        }
    }
}
```

### Usage
```csharp
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> logger;
        private readonly SignalRHub hub;

        public HomeController(ILogger<HomeController> logger, SignalRHub hub)
        {
            this.logger = logger;
            this.hub = hub;
        }

        [HttpGet]
        [Route("Test")]
        public async Task<IActionResult> Test()
        {
            logger.LogInformation("This is an info log");
            await hub.BroadcastAsync("Dashboard", "Progress", new
            {
                BooksSkipped = 10,
                AmazonScrapped = 20,
                LibGenScrapped = 11,
                BooksFound = 55
            });
            return Ok();
        }
    }
```
