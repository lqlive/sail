# Sail

[![GitHub](https://img.shields.io/badge/GitHub-lqlive%2Fsail-blue?logo=github)](https://github.com/lqlive/sail)
[![MIT License](https://img.shields.io/github/license/lqlive/sail?color=%230b0&style=flat-square)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)

Sail is a modern, high-performance API Gateway built on top of [YARP (Yet Another Reverse Proxy)](https://microsoft.github.io/reverse-proxy/). It provides dynamic configuration management with a web-based admin interface, enabling real-time updates to routes, clusters, and policies without restart.

## Get started

Follow the [Quick Start](#quick-start) instructions below.

Also check out the [documentation](https://github.com/lqlive/sail) for more information on configuration and deployment.

## Features

- **Dynamic Configuration**: Update routes, clusters, and policies in real-time without restarting
- **Web-Based Management**: Modern React UI for visual configuration management
- **Flexible Routing**: Path, header, host, and query parameter-based routing
- **Authentication**: JWT Bearer and OpenID Connect support with dynamic policies
- **Traffic Control**: CORS, rate limiting, timeouts, and load balancing
- **TLS/HTTPS**: Certificate management with SNI support
- **MongoDB Storage**: Persistent configuration with Change Streams for real-time updates
- **gRPC API**: High-performance configuration distribution across gateway instances

## Quick Start

### Running with Docker Compose (Recommended)

```bash
git clone https://github.com/lqlive/sail.git
cd sail
docker-compose up -d
```

Access the Web UI at `http://localhost:5173`

### Running Locally

**Prerequisites**:
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

## Architecture

```
Web UI (React) → REST API → Sail Management API
                              ↓
                           MongoDB ← Change Streams → Sail Compass
                                                          ↓
                                                    Sail Proxy (YARP)
```

## Project Structure

```
sail/
├── src/
│   ├── Sail/                 # Management API & gRPC services
│   ├── Sail.Core/            # Core domain models & interfaces
│   ├── Sail.Compass/         # Configuration subscriber & updater
│   ├── Sail.Proxy/           # Gateway runtime (YARP host)
│   └── Sail.Storage.MongoDB/ # MongoDB persistence layer
├── web/                      # React admin UI
└── shared/protos/            # gRPC service definitions
```

## Technology Stack

- **Backend**: .NET 9, ASP.NET Core, YARP
- **Storage**: MongoDB with Change Streams
- **Communication**: gRPC, REST API
- **Frontend**: React 18, TypeScript, Vite, Tailwind CSS

## How to engage, contribute, and give feedback

Some of the best ways to contribute are to try things out, file issues, and make pull-requests.

- Check out the [contributing](CONTRIBUTING.md) page to see the best places to log issues and start discussions
- [Build from source](./docs/BuildFromSource.md)

## Reporting security issues and bugs

Security issues and bugs should be reported privately via [GitHub Security Advisories](https://github.com/lqlive/sail/security/advisories).

## Related projects

- [YARP](https://github.com/microsoft/reverse-proxy) - The reverse proxy foundation
- [MongoDB](https://www.mongodb.com/) - Configuration storage with change streams
- [ASP.NET Core](https://github.com/dotnet/aspnetcore) - Web framework

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
