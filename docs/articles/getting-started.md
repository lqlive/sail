---
title: Getting Started
---

# Getting Started with Sail

This guide will walk you through setting up Sail and creating your first API gateway configuration.

## Installation

### Option 1: Docker Compose (Recommended)

The easiest way to run Sail is using Docker Compose, which starts all required services:

```bash
# Clone the repository
git clone https://github.com/lqlive/sail.git
cd sail

# Start all services
docker-compose up -d
```

This will start:
- MongoDB (port 27017)
- Sail Management API (port 5000)
- Sail Proxy Gateway (port 8080/8443)
- Web UI (port 5173)

### Option 2: Local Development

For local development, you'll need:

**Prerequisites:**
- .NET 9.0 SDK
- MongoDB 4.4+
- Node.js 18+

**Steps:**

1. **Start MongoDB**
```bash
docker run -d -p 27017:27017 --name sail-mongodb mongo:latest
```

2. **Configure Connection String**

Edit `src/Sail/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb://localhost:27017/sail"
  }
}
```

3. **Run Management API**
```bash
cd src/Sail
dotnet run
```

4. **Run Proxy Gateway**
```bash
cd src/Sail.Proxy
dotnet run
```

5. **Run Web UI**
```bash
cd web
npm install
npm run dev
```

## First Configuration

### Access the Admin UI

Open your browser and navigate to `http://localhost:5173`

### Create a Backend Cluster

1. Click on **Clusters** in the navigation menu
2. Click **Add Cluster**
3. Configure your cluster:

```json
{
  "id": "backend-api",
  "destinations": {
    "destination1": {
      "address": "https://api.example.com"
    }
  },
  "loadBalancingPolicy": "RoundRobin"
}
```

### Create a Route

1. Click on **Routes** in the navigation menu
2. Click **Add Route**
3. Configure your route:

```json
{
  "routeId": "api-route",
  "clusterId": "backend-api",
  "match": {
    "path": "/api/{**catch-all}"
  }
}
```

### Test Your Gateway

Send a request through the gateway:

```bash
curl http://localhost:8080/api/users
```

The gateway will forward the request to your backend at `https://api.example.com/api/users`.

## Next Steps

- Learn about [Advanced Routing](routing.md) patterns
- Configure [Authentication](authentication.md) policies
- Set up [Rate Limiting](rate-limiting.md)
- Add [CORS](cors.md) policies
- Configure [TLS/HTTPS](https.md)

## Common Issues

### MongoDB Connection Failed

If you see connection errors, verify MongoDB is running:

```bash
docker ps | grep mongo
```

### Port Already in Use

If ports 5000, 8080, or 5173 are already in use, you can change them in:
- `src/Sail/appsettings.json` (Management API)
- `src/Sail.Proxy/appsettings.json` (Gateway)
- `web/vite.config.ts` (Web UI)

### Gateway Not Receiving Configuration

Ensure the Sail.Proxy is properly connected to the Management API via gRPC. Check the logs for connection errors.

