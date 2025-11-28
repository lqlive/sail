---
layout: default
title: Sail Documentation
---

<div style="text-align: center; margin: 2rem 0;">
  <h1 style="font-size: 3rem; margin-bottom: 1rem;">⛵ Sail</h1>
  <p style="font-size: 1.5rem; color: #666; margin-bottom: 2rem;">Modern API Gateway built on YARP</p>
  <div>
    <a href="https://github.com/lqlive/sail" class="btn" style="display: inline-block; margin: 0.5rem;">View on GitHub</a>
    <a href="articles/getting-started.html" class="btn" style="display: inline-block; margin: 0.5rem;">Get Started</a>
    <a href="articles/architecture.html" class="btn" style="display: inline-block; margin: 0.5rem;">Architecture</a>
  </div>
</div>

Welcome to the documentation for **Sail**! Sail is a modern, high-performance API Gateway built on top of [YARP (Yet Another Reverse Proxy)](https://microsoft.github.io/reverse-proxy/). It provides dynamic configuration management with a web-based admin interface, enabling real-time updates to routes, clusters, and policies without restart.

Please provide your feedback by going to [the GitHub repository](https://github.com/lqlive/sail).

## Why Sail

Sail was created to address the need for a flexible, production-ready API Gateway that combines the power of YARP with modern cloud-native patterns. Key motivations include:

- **Dynamic Configuration**: Traditional API gateways require restarts for configuration changes. Sail uses MongoDB Change Streams to propagate configuration updates in real-time across all gateway instances.
- **Developer Experience**: A modern React-based web UI makes it easy to manage complex routing, authentication, and traffic control policies without editing YAML files.
- **Cloud-Native Ready**: Built on .NET 9 and designed for container deployments with Docker Compose and Kubernetes support.
- **Production-Grade Features**: Includes authentication, rate limiting, retries, timeouts, CORS, TLS/SNI, and more out of the box.

## Key Features

<div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 1.5rem; margin: 2rem 0;">

<div class="feature-box">
  <h3>🚀 Dynamic Configuration</h3>
  <ul>
    <li>Real-time updates without gateway restarts</li>
    <li>MongoDB-backed persistent storage</li>
    <li>gRPC-based configuration distribution</li>
    <li>Change Streams for instant synchronization</li>
  </ul>
</div>

<div class="feature-box">
  <h3>🎨 Web-Based Administration</h3>
  <ul>
    <li>Modern React 18 UI with TypeScript</li>
    <li>Visual route and cluster management</li>
    <li>Policy configuration interface</li>
    <li>Real-time monitoring and status updates</li>
  </ul>
</div>

<div class="feature-box">
  <h3>🔀 Advanced Routing</h3>
  <ul>
    <li>Path-based routing with pattern matching</li>
    <li>Header-based routing</li>
    <li>Host-based routing</li>
    <li>Query parameter routing</li>
    <li>Priority-based route selection</li>
  </ul>
</div>

<div class="feature-box">
  <h3>🔒 Security & Authentication</h3>
  <ul>
    <li>JWT Bearer token authentication</li>
    <li>OpenID Connect integration</li>
    <li>Dynamic authentication policies</li>
    <li>TLS/HTTPS with SNI support</li>
    <li>Certificate management API</li>
  </ul>
</div>

<div class="feature-box">
  <h3>⚡ Traffic Control</h3>
  <ul>
    <li>CORS policy management</li>
    <li>Rate limiting with customizable policies</li>
    <li>Request/response timeouts</li>
    <li>Automatic retry with exponential backoff</li>
    <li>Load balancing strategies</li>
  </ul>
</div>

<div class="feature-box">
  <h3>🔍 Service Discovery</h3>
  <ul>
    <li>Integration with service discovery systems</li>
    <li>Dynamic destination resolution</li>
    <li>Health check support</li>
  </ul>
</div>

</div>

## Architecture Overview

Sail consists of three main components:

```
┌─────────────────┐
│   Web UI        │  React-based Admin Interface
│   (React 18)    │
└────────┬────────┘
         │ REST API
         ▼
┌─────────────────┐
│  Sail API       │  Management API & gRPC Services
│  (ASP.NET Core) │
└────────┬────────┘
         │
         ▼
┌─────────────────┐      Change Streams      ┌─────────────────┐
│    MongoDB      │◄─────────────────────────┤ Sail Compass    │
│                 │                           │ (Config Watcher)│
└─────────────────┘                           └────────┬────────┘
                                                       │ gRPC
                                                       ▼
                                              ┌─────────────────┐
                                              │  Sail Proxy     │
                                              │  (YARP Gateway) │
                                              └─────────────────┘
```

### Components

- **Sail API (`Sail`)**: Management REST API and gRPC services for configuration CRUD operations
- **Sail Compass (`Sail.Compass`)**: Watches MongoDB for configuration changes and updates the gateway
- **Sail Proxy (`Sail.Proxy`)**: The actual gateway runtime that handles client requests
- **Web UI (`web/`)**: React-based admin interface for visual configuration management

## Technology Stack

- **Runtime**: .NET 9.0
- **Web Framework**: ASP.NET Core
- **Reverse Proxy**: YARP (Yet Another Reverse Proxy)
- **Database**: MongoDB 4.4+ with Change Streams
- **Communication**: gRPC, REST API
- **Frontend**: React 18, TypeScript, Vite, Tailwind CSS

## Getting Started

### Quick Start with Docker Compose

The fastest way to get started is using Docker Compose:

```bash
git clone https://github.com/lqlive/sail.git
cd sail
docker-compose up -d
```

Access the Web UI at `http://localhost:5173`

### Local Development Setup

**Prerequisites:**
- .NET 9.0 SDK or later
- MongoDB 4.4 or later
- Node.js 18+ (for Web UI)

**1. Start MongoDB**
```bash
docker run -d -p 27017:27017 --name mongodb mongo:latest
```

**2. Run the Management API**
```bash
cd src/Sail
dotnet run
```

**3. Run the Proxy Gateway**
```bash
cd src/Sail.Proxy
dotnet run
```

**4. Run the Web UI**
```bash
cd web
npm install
npm run dev
```

## Project Structure

```
sail/
├── src/
│   ├── Sail/                 # Management API & gRPC services
│   ├── Sail.Core/            # Core domain models & interfaces
│   ├── Sail.Compass/         # Configuration subscriber & updater
│   ├── Sail.Proxy/           # Gateway runtime (YARP host)
│   └── Sail.Database.MongoDB/# MongoDB persistence layer
├── web/                      # React admin UI
├── shared/protos/            # gRPC service definitions
├── tests/                    # Unit and functional tests
└── samples/                  # Example applications
```

## Next Steps

- Learn about [Configuration](articles/configuration.md) to understand routes, clusters, and policies
- Explore [Authentication](articles/authentication.md) setup for JWT and OpenID Connect
- Read about [Rate Limiting](articles/rate-limiting.md) and traffic control
- Check out [Deployment](articles/deployment.md) guides for production environments

## Contributing

We welcome contributions! Some of the best ways to contribute are:

- Try out Sail and file issues for bugs or feature requests
- Improve documentation
- Submit pull requests for bug fixes or new features

Check out the [GitHub repository](https://github.com/lqlive/sail) to get started.

## Reporting Security Issues

Security issues and bugs should be reported privately via [GitHub Security Advisories](https://github.com/lqlive/sail/security/advisories).

## Related Projects

- [YARP](https://github.com/microsoft/reverse-proxy) - The reverse proxy foundation
- [MongoDB](https://www.mongodb.com/) - Configuration storage with change streams
- [ASP.NET Core](https://github.com/dotnet/aspnetcore) - Web framework

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/lqlive/sail/blob/main/LICENSE) file for details.

---

*Built with ❤️ using .NET and YARP*
