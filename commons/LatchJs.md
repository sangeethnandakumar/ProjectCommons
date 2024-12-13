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

    // Initialize the SignalR connection
    initializeConnection() {
        this.connection = new HubConnectionBuilder()
            .withUrl(`${this.apiUrl}/signalrhub`, { withCredentials: false })
            .withAutomaticReconnect()
            .build();

        this.connection.start()
            .then(() => {
                console.log("Connected to SignalR server");
                // Register the user each time the connection starts
                if (this.userId) {
                    this.registerUser(this.userId);
                }
            })
            .catch((error) => console.error("SignalR Connection Error: ", error));
    }

    // Method to set the user ID and register it with the server
    setUserId(userId) {
        this.userId = userId;
        // Register the user if the connection is already started
        if (this.connection?.state === "Connected") {
            this.registerUser(userId);
        }
    }

    // Method to register the user with the server
    registerUser(userId) {
        this.connection.invoke("RegisterUser", userId)
            .catch(err => console.error("Error registering user:", err));
    }

    // Subscribe to notifications with a callback
    on(component, methodName, callback) {
        this.connection.on(`${component}_${methodName}`, callback);
    }

    // Unsubscribe from a listener
    off(component, methodName, callback) {
        this.connection.off(`${component}_${methodName}`, callback);
    }

    // Send a message (unicast) to the server with the user's connection ID
    unicast(userId, component, methodName, ...args) {
        this.connection.invoke("UnicastAsync", userId, `${component}_${methodName}`, ...args)
            .catch(err => console.error("Error sending unicast message:", err));
    }

    // Invoking multicast
    multicast(userIds, component, methodName, ...args) {
        this.connection.invoke("MulticastAsync", userIds, `${component}_${methodName}`, ...args)
            .catch(err => console.error("Error sending multicast message:", err));
    }

    // Broadcast to all clients
    broadcast(component, methodName, ...args) {
        this.connection.invoke("BroadcastAsync", component, methodName, ...args)
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

// Initialize Latch with your API URL
const latch = new Latch(import.meta.env.VITE_APP_API_URL);

function App() {

    useEffect(() => {

        // Fetch user info from localStorage and set userId in Latch
        const claims = JSON.parse(localStorage.getItem('userClaims'));
        const loggedInUserId = claims?.profileId;

        if (loggedInUserId) {
            latch.setUserId(loggedInUserId);
        }

        // Listeners for notifications and progress
        const listenForNotifications = msg => console.log(msg);
        const listenForProgress = msg => console.log(msg);

        latch.on("Dashboard", "Notifications", listenForNotifications);
        latch.on("Dashboard", "Progress", listenForProgress);

        return () => {
            latch.off("Dashboard", "Notifications", listenForNotifications);
            latch.off("Dashboard", "Progress", listenForProgress);
        };

    }, [navigate]);

    return (
        <>
            <MainPage />
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
        // Concurrent dictionary to map user IDs to connection IDs
        private static ConcurrentDictionary<string, string> _userConnections = new ConcurrentDictionary<string, string>();

        // Method to register a user and update the connection ID
        public async Task RegisterUser(string userId)
        {
            // Remove any old connection IDs associated with the user
            if (_userConnections.ContainsKey(userId))
            {
                _userConnections.TryRemove(userId, out _);
            }

            // Add the new connection ID
            _userConnections[userId] = Context.ConnectionId;
            await Clients.Client(Context.ConnectionId).SendAsync("Registered", Context.ConnectionId);
        }

        // Method to send a message to a specific user (unicast)
        public async Task UnicastAsync(string userId, string component, string methodName, object payload)
        {
            if (_userConnections.TryGetValue(userId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync($"{component}_{methodName}", payload);
            }
        }

        // Method to send a message to multiple users (multicast)
        public async Task MulticastAsync(List<string> userIds, string component, string methodName, object payload)
        {
            var connectionIds = userIds
                .Where(userId => _userConnections.TryGetValue(userId, out _))
                .Select(userId => _userConnections[userId])
                .ToList();

            if (connectionIds.Count > 0)
            {
                await Clients.Clients(connectionIds).SendAsync($"{component}_{methodName}", payload);
            }
        }

        // Method to send a message to all connected clients (broadcast)
        public async Task BroadcastAsync(string component, string methodName, object payload)
        {
            await Clients.All.SendAsync($"{component}_{methodName}", payload);
        }

        // Clean up connection mappings when a user disconnects
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var user = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (user.Key != null)
            {
                _userConnections.TryRemove(user.Key, out _);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
```

### Usage
```csharp
using Asp.Versioning;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using ParinayBharat.Api.Application.Features.Meta.GetMetaItems;
using ParinayBharat.Api.Domain.Constants;
using ParinayBharat.Api.SignalR;

namespace ParinayBharat.Api.Presentation.Modules
{
    public sealed class TestModule : CarterModule
    {
        private readonly SignalRHub hub;

        public TestModule(SignalRHub hub)
        {
            WithTags("Meta");
            this.hub = hub;
        }

        public override void AddRoutes(IEndpointRouteBuilder app)
        {

            group.MapGet("/signalr", async (int apiVersion, IMediator mediator) =>
            {
                // Send unicast message to PB-1000
                await hub.UnicastAsync("PB-1000", "Dashboard", "Progress", new
                {
                    Message = $"{Guid.NewGuid()}: Hello PB-1000",
                });

                // Send unicast message to PB-1001
                await hub.UnicastAsync("PB-1001", "Dashboard", "Progress", new
                {
                    Message = $"{Guid.NewGuid()}: Hello PB-1001",
                });

                return Results.Ok();
            }).AllowAnonymous();

        }
    }
}

```
