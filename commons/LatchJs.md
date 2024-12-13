### Topic Subscriptions

![image](https://github.com/user-attachments/assets/42cd16d7-a09d-454d-aa12-42562c61f1f3)



# Latch.js

```js
// Latch.js
import { HubConnectionBuilder } from "@microsoft/signalr";

class Latch {
    constructor(apiUrl) {
        this.connection = null;
        this.userId = null;
        this.apiUrl = apiUrl;
        this.initializeConnection();
    }

    initializeConnection() {
        this.connection = new HubConnectionBuilder()
            .withUrl(`${this.apiUrl}/signalrhub`, { withCredentials: false })
            .withAutomaticReconnect()
            .build();

        this.connection.start()
            .then(() => {
                console.log("Connected to SignalR server");
                if (this.userId) {
                    this.registerUser(this.userId);
                }
            })
            .catch((error) => console.error("SignalR Connection Error: ", error));
    }

    setUserId(userId) {
        this.userId = userId;
        if (this.connection?.state === "Connected") {
            this.registerUser(userId);
        }
    }

    registerUser(userId) {
        this.connection.invoke("RegisterUser", userId)
            .catch(err => console.error("Error registering user:", err));
    }

    on(topic, callback) {
        this.connection.on(topic, callback);
    }

    off(topic, callback) {
        this.connection.off(topic, callback);
    }

    unicast(userId, topic, ...args) {
        this.connection.invoke("UnicastAsync", userId, topic, ...args)
            .catch(err => console.error("Error sending unicast message:", err));
    }

    multicast(userIds, topic, ...args) {
        this.connection.invoke("MulticastAsync", userIds, topic, ...args)
            .catch(err => console.error("Error sending multicast message:", err));
    }

    broadcast(topic, ...args) {
        this.connection.invoke("BroadcastAsync", topic, ...args)
            .catch(err => console.error("Error sending broadcast message:", err));
    }
}

export default Latch;
```

### Usage

- Install SignalR (`npm i @microsoft/signalr`)
- Replace `HubURL` below
- Configure Server Side
- Ready to use

```js
import './App.css';
import { useEffect, useState } from 'react';
import { useNavigate, Outlet } from 'react-router-dom';
import Header from './components/header/Header';
import Footer from './components/footer/Footer';
import BarLoader from 'react-spinners/BarLoader';
import Latch from '../src/helpers/Latch';

// --------------------------------------------------------------------------------------------------------------------
const latch = new Latch(import.meta.env.VITE_APP_API_URL);
// --------------------------------------------------------------------------------------------------------------------

function App() {
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();

    useEffect(() => {
        const urlParams = new URLSearchParams(window.location.search);
        const referrerParam = urlParams.get('referrer');

        if (referrerParam) {
            setLoading(true);
            window.history.replaceState(null, '', window.location.pathname);
            setTimeout(() => {
                setLoading(false);
                navigate(decodeURIComponent(referrerParam));
            }, 2000);
        }

        // --------------------------------------------------------------------------------------------------------------------
                const claims = JSON.parse(localStorage.getItem('userClaims'));
                const loggedInUserId = claims?.profileId;
        
                if (loggedInUserId) {
                    latch.setUserId(loggedInUserId);
                }
        
                // Listeners for different notifications
                const listenForChatNotifications = (notification) => {
                    console.log(`%c[Chat] ${notification.senderName}: ${notification.message}`, 'color: blue; font-weight: bold;');
                };
        
                const listenForGeneralNotifications = (notification) => {
                    console.log(`%c[General] ${notification.message}`, 'color: green; font-weight: bold;');
                };
        
                const listenForProgressNotifications = (notification) => {
                    console.log(`%c[Progress] ${notification.title} - ${notification.subtitle}: ${notification.progress}%`, 'color: orange; font-weight: bold;');
                };
        
                // Attach listeners
                latch.on("LiveChatNotification", listenForChatNotifications);
                latch.on("GeneralNotification", listenForGeneralNotifications);
                latch.on("ProgressNotification", listenForProgressNotifications);
        
                return () => {
                    latch.off("LiveChatNotification", listenForChatNotifications);
                    latch.off("GeneralNotification", listenForGeneralNotifications);
                    latch.off("ProgressNotification", listenForProgressNotifications);
                };
        // --------------------------------------------------------------------------------------------------------------------

    }, [navigate]);

    return (
        <>
            <Header />
            <div className="container">
                {loading ? (
                    <center>
                        <BarLoader />
                        <small>Redirecting you...</small>
                    </center>
                ) : (
                    <Outlet />
                )}
            </div>
            <Footer />
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
using System.Collections.Concurrent;

namespace ParinayBharat.Api.SignalR
{
    public class SignalRHub : Hub
    {
        private static ConcurrentDictionary<string, string> _userConnections = new ConcurrentDictionary<string, string>();

        public async Task RegisterUser(string userId)
        {
            if (_userConnections.ContainsKey(userId))
            {
                _userConnections.TryRemove(userId, out _);
            }
            _userConnections[userId] = Context.ConnectionId;
            await Clients.Client(Context.ConnectionId).SendAsync("Registered", Context.ConnectionId);
        }

        public async Task UnicastAsync(string userId, string topic, object payload)
        {
            if (_userConnections.TryGetValue(userId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync(topic, payload);
            }
        }

        public async Task MulticastAsync(List<string> userIds, string topic, object payload)
        {
            var connectionIds = userIds
                .Where(userId => _userConnections.TryGetValue(userId, out _))
                .Select(userId => _userConnections[userId])
                .ToList();

            if (connectionIds.Count > 0)
            {
                await Clients.Clients(connectionIds).SendAsync(topic, payload);
            }
        }

        public async Task BroadcastAsync(string topic, object payload)
        {
            await Clients.All.SendAsync(topic, payload);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var user = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (user.Key != null)
            {
                _userConnections.TryRemove(user.Key, out _);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendChatNotification(LiveChatNotification notification)
        {
            await UnicastAsync(notification.To, "LiveChatNotification", notification);
        }

        public async Task SendGeneralNotification(LiveGeneralNotification notification)
        {
            await UnicastAsync(notification.To, "GeneralNotification", notification);
        }

        public async Task SendProgressNotification(LiveProgressNotification notification)
        {
            await UnicastAsync(notification.To, "ProgressNotification", notification);
        }
    }

    public class LiveChatNotification
    {
        public string To { get; set; }
        public string From { get; set; }
        public string ChatId { get; set; }
        public string SenderName { get; set; }
        public string Message { get; set; }
    }

    public class LiveGeneralNotification
    {
        public string To { get; set; }
        public string Message { get; set; }
    }

    public class LiveProgressNotification
    {
        public string To { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public int Progress { get; set; }
    }
}
```

### Usage
```csharp
group.MapGet("/signalr", async (int apiVersion, string toUser, IMediator mediator, SignalRHub hub) =>
{
    await hub.SendChatNotification(new LiveChatNotification
    {
        To = toUser,
        From = "PB-1001",
        ChatId = "<CHAT ID>",
        SenderName = "Gayathri",
        Message = "Hello.."
    });

    await hub.SendGeneralNotification(new LiveGeneralNotification
    {
        To = toUser,
        Message = "Your existing plan PRIME+ will expire in next days"
    });

    await hub.SendProgressNotification(new LiveProgressNotification
    {
        To = toUser,
        Title = "Downloading file",
        Subtitle = "Please wait...",
        Progress = 83
    });

    return Results.Ok();
})
.AllowAnonymous();
```
