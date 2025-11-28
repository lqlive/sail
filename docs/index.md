---
uid: root
title: Sail Documentation
---

# Sail: Modern API Gateway

Welcome to the documentation for **Sail**! Sail is a modern, high-performance API Gateway built on top of [YARP (Yet Another Reverse Proxy)](https://microsoft.github.io/reverse-proxy/). It provides dynamic configuration management with a web-based admin interface, enabling real-time updates to routes, clusters, and policies without restart.

Please provide your feedback by going to [the GitHub repository](https://github.com/lqlive/sail).

## Why Sail

Sail was created to address the need for a flexible, production-ready API Gateway that combines the power of YARP with modern cloud-native patterns. Key motivations include:

- **Dynamic Configuration**: Traditional API gateways require restarts for configuration changes. Sail uses MongoDB Change Streams to propagate configuration updates in real-time across all gateway instances.
- **Developer Experience**: A modern React-based web UI makes it easy to manage complex routing, authentication, and traffic control policies without editing YAML files.
- **Cloud-Native Ready**: Built on .NET 9 and designed for container deployments with Docker Compose and Kubernetes support.
- **Production-Grade Features**: Includes authentication, rate limiting, retries, timeouts, CORS, TLS/SNI, and more out of the box.

## Key Features

### Dynamic Configuration Management
- Real-time updates without gateway restarts
- MongoDB-backed persistent storage
- gRPC-based configuration distribution
- Change Streams for instant synchronization

### Web-Based Administration
- Modern React 18 UI with TypeScript
- Visual route and cluster management
- Policy configuration interface
- Real-time monitoring and status updates

### Advanced Routing
- Path-based routing with pattern matching
- Header-based routing
- Host-based routing
- Query parameter routing
- Priority-based route selection

### Security & Authentication
- JWT Bearer token authentication
- OpenID Connect integration
- Dynamic authentication policies
- TLS/HTTPS with SNI support
- Certificate management API

### Traffic Control
- CORS policy management
- Rate limiting with customizable policies
- Request/response timeouts
- Automatic retry with exponential backoff
- Load balancing strategies

### Service Discovery
- Integration with service discovery systems
- Dynamic destination resolution
- Health check support

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
