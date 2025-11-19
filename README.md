# Sail Gateway

[![GitHub](https://img.shields.io/badge/GitHub-lqlive%2Fsail-blue?logo=github)](https://github.com/lqlive/sail)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)

A modern, high-performance API Gateway built on top of [YARP (Yet Another Reverse Proxy)](https://microsoft.github.io/reverse-proxy/) with dynamic configuration management and a beautiful web-based admin interface.

> 🔗 **GitHub Repository**: [https://github.com/lqlive/sail](https://github.com/lqlive/sail)

## ✨ Features

### Core Capabilities
- **Dynamic Configuration**: Update routes, clusters, and policies in real-time without restarting the gateway
- **Visual Management**: Modern React-based UI for managing all gateway configurations
- **MongoDB Storage**: Persistent configuration storage with Change Streams for real-time updates
- **gRPC Configuration API**: High-performance configuration distribution across multiple gateway instances
- **Hot Reload**: Configuration changes propagate instantly to running instances

### Routing & Traffic Management
- **Flexible Routing**: Path-based, header-based, host-based, and query parameter routing
- **Load Balancing**: Support for multiple YARP load balancing policies
- **Health Checks**: Active and passive health checking configuration
- **Session Affinity**: Cookie-based session affinity support
- **Cluster Management**: Define and manage upstream service clusters

### Security & Authentication
- **JWT Bearer Authentication**: Built-in JWT token validation with configurable policies
- **OpenID Connect**: Full OAuth2/OIDC flow support for authentication
- **Dynamic Auth Policies**: Create and manage multiple authentication schemes on-the-fly
- **HTTPS/TLS**: SSL/SNI certificate management with support for multiple domains

### Traffic Control
- **CORS Policies**: Flexible CORS configuration with origin, method, and header control
- **Rate Limiting**: Fixed window and sliding window rate limiting per route
- **Middleware Management**: Centralized CORS and rate limiter policy management
- **Timeout Management**: Configure timeouts per route

### Observability & Monitoring
- **Dashboard**: Overview of routes, clusters, and services status
- **Configuration Management**: Track and manage all gateway configurations
- **Service Discovery**: Consul integration for automatic service registration (experimental)

## 🏗️ Architecture

```
┌─────────────────┐
│   Web UI        │  React + Vite + Tailwind CSS
│   (Port 5173)   │
└────────┬────────┘
         │ REST API
         │
┌────────▼────────────────────────────────────┐
│         Sail Management API                 │
│  - Routes, Clusters, Certificates API       │
│  - Authentication Policies API              │
│  - Middleware Configuration API             │
└────────┬────────────────────────────────────┘
         │
         │ MongoDB
┌────────▼────────┐       gRPC Stream      ┌──────────────┐
│   MongoDB       │◄──────────────────────►│ Sail Compass │
│   - Routes      │  Change Stream Watch   │  (Subscriber)│
│   - Clusters    │                        └──────┬───────┘
│   - Policies    │                               │
└─────────────────┘                               │
                                                  │ Updates
                                        ┌─────────▼──────────┐
                                        │   Sail Proxy       │
                                        │  (Gateway Runtime) │
                                        │  - YARP Engine     │
                                        │  - Dynamic Config  │
                                        └────────────────────┘
```

## 🚀 Quick Start

### Prerequisites
- .NET 9.0 SDK or later
- MongoDB 4.4 or later
- Node.js 18+ (for Web UI development)

### Running with Docker Compose (Recommended)

```bash
# Clone the repository
git clone https://github.com/lqlive/sail.git
cd sail

# Start all services
docker-compose up -d

# Access the Web UI
open http://localhost:5173
```

### Running Locally

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

## 📖 Documentation

### Configuration Examples

**Creating a Route**
```bash
curl -X POST http://localhost:5000/api/routes?api-version=1.0 \
  -H "Content-Type: application/json" \
  -d '{
    "name": "api-route",
    "clusterId": "backend-cluster",
    "match": {
      "path": "/api/{**catch-all}"
    },
    "enabled": true
  }'
```

**Creating a JWT Authentication Policy**
```bash
curl -X POST http://localhost:5000/api/authentication-policies?api-version=1.0 \
  -H "Content-Type: application/json" \
  -d '{
    "name": "jwt-policy",
    "type": "JwtBearer",
    "enabled": true,
    "jwtBearer": {
      "authority": "https://your-auth-server.com",
      "audience": "your-api",
      "requireHttpsMetadata": true,
      "validateIssuer": true,
      "validateAudience": true,
      "validateLifetime": true
    }
  }'
```

### Project Structure
```
sail/
├── src/
│   ├── Sail/                 # Management API & gRPC services
│   ├── Sail.Core/            # Core domain models & interfaces
│   ├── Sail.Compass/         # Configuration subscriber & updater
│   ├── Sail.Proxy/           # Gateway runtime (YARP host)
│   └── Sail.Storage.MongoDB/ # MongoDB persistence layer
├── web/                      # React admin UI
├── shared/
│   └── protos/               # gRPC service definitions
└── tests/                    # Unit & integration tests
```

## 🛠️ Technology Stack

- **Backend**: .NET 9, ASP.NET Core, YARP
- **Storage**: MongoDB with Change Streams
- **Communication**: gRPC, REST API
- **Reactive**: Rx.NET (Reactive Extensions)
- **Frontend**: React 18, TypeScript, Vite, Tailwind CSS
- **Service Discovery**: Consul (optional)

## 🧩 Key Components

### Sail (Management API)
- RESTful API for all CRUD operations
- gRPC streaming service for real-time configuration updates
- MongoDB persistence with automatic change stream notifications

### Sail.Compass (Configuration Subscriber)
- Subscribes to MongoDB change streams
- Transforms configuration changes into YARP-compatible format
- Pushes updates to in-memory configuration store

### Sail.Proxy (Gateway Runtime)
- Hosts the YARP reverse proxy engine
- Applies dynamic configurations without restart
- Handles all inbound traffic with configured routes and policies

### Web UI
- Modern React-based responsive admin interface
- Real-time configuration management for all entities
- Visual creation and editing of routes, clusters, certificates, middlewares, and auth policies
- Built with Tailwind CSS for beautiful, consistent design

## 📝 Configuration Management

Sail supports multiple configuration sources:
- **MongoDB**: Primary persistent storage with change stream support
- **appsettings.json**: Static configuration for development
- **Environment Variables**: Runtime overrides for containerized deployments

## 🔒 Security Features

- **Dynamic Authentication**: Add/remove JWT Bearer and OpenID Connect schemes without restart
- **Certificate Management**: Manage SSL/TLS certificates and SNI mappings via API
- **Policy-Based Access**: Configure multiple authentication policies per route
- **HTTPS Redirect**: Automatic HTTP to HTTPS redirection support

## 🗺️ Roadmap

The following features are planned for future releases:

- [ ] **Circuit Breaking**: Automatic failover and retry policies
- [ ] **Advanced Transformations**: Request/response body modifications
- [ ] **A/B Testing**: Canary deployments and traffic splitting
- [ ] **Multi-tenancy**: Support for multiple isolated configurations
- [ ] **Metrics & Tracing**: Prometheus metrics and distributed tracing
- [ ] **WebSocket Support**: Full WebSocket proxying capabilities

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request to [https://github.com/lqlive/sail](https://github.com/lqlive/sail).

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [YARP](https://github.com/microsoft/reverse-proxy) - The foundation of this gateway
- [MongoDB](https://www.mongodb.com/) - For change streams and reliable storage
- [React](https://react.dev/) - For the beautiful admin interface

## 📧 Support

- 💬 [GitHub Discussions](https://github.com/lqlive/sail/discussions)
- 🐛 [Issue Tracker](https://github.com/lqlive/sail/issues)
- 📖 [Documentation](https://github.com/lqlive/sail)

---

**Built with ❤️ using .NET and YARP**
