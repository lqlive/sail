---
title: Architecture
---

# Sail Architecture

This document describes the architecture and design principles of Sail.

## Overview

Sail is designed as a distributed system with three main components that work together to provide a dynamic, high-performance API gateway:

```
┌──────────────────────────────────────────────────────────────┐
│                        Web UI (React)                        │
│                    http://localhost:5173                     │
└───────────────────────────┬──────────────────────────────────┘
                            │ REST API (HTTP/JSON)
                            │
┌───────────────────────────▼──────────────────────────────────┐
│                   Sail Management API                        │
│                    - REST API Endpoints                      │
│                    - gRPC Services                           │
│                    - Configuration CRUD                      │
│                    http://localhost:5000                     │
└───────────────────────────┬──────────────────────────────────┘
                            │
                            │ MongoDB Connection
                            │
┌───────────────────────────▼──────────────────────────────────┐
│                        MongoDB                               │
│                 - Stores Configuration                       │
│                 - Change Streams                             │
│                 mongodb://localhost:27017                    │
└───────────────────────────┬──────────────────────────────────┘
                            │ Change Streams (Real-time)
                            │
┌───────────────────────────▼──────────────────────────────────┐
│                     Sail Compass                             │
│              (Embedded in Sail.Proxy)                        │
│                 - Watches MongoDB                            │
│                 - Updates YARP Config                        │
└───────────────────────────┬──────────────────────────────────┘
                            │ In-Memory Config Updates
                            │
┌───────────────────────────▼──────────────────────────────────┐
│                      Sail Proxy                              │
│                   (YARP Gateway)                             │
│              - Reverse Proxy Logic                           │
│              - Middleware Pipeline                           │
│              http://localhost:8080                           │
└──────────────────────────────────────────────────────────────┘
                            │
                            │ Proxied Requests
                            │
┌───────────────────────────▼──────────────────────────────────┐
│                    Backend Services                          │
└──────────────────────────────────────────────────────────────┘
```

## Components

### 1. Sail Management API (`src/Sail`)

The Management API is the control plane of Sail. It provides:

**REST API Endpoints:**
- `/api/v1/routes` - Route configuration
- `/api/v1/clusters` - Backend cluster configuration
- `/api/v1/certificates` - TLS certificate management
- `/api/v1/middleware` - Middleware policies (CORS, rate limiting)
- `/api/v1/authentication` - Authentication policies

**gRPC Services:**
- `RouteGrpcService` - Real-time route updates
- `ClusterGrpcService` - Real-time cluster updates
- `CertificateGrpcService` - Certificate distribution
- `MiddlewareGrpcService` - Middleware policy distribution
- `AuthenticationPolicyGrpcService` - Authentication policy distribution

**Technology:**
- ASP.NET Core Minimal APIs
- gRPC for efficient binary communication
- Entity-based domain model

### 2. MongoDB Storage (`src/Sail.Database.MongoDB`)

MongoDB serves as the persistent storage layer with unique features:

**Collections:**
- `Routes` - Route definitions
- `Clusters` - Backend cluster configurations
- `Certificates` - TLS certificates and keys
- `CorsPolicies` - CORS policies
- `RateLimitPolicies` - Rate limiting configurations
- `AuthenticationPolicies` - JWT/OIDC configurations
- `TimeoutPolicies` - Request timeout settings
- `RetryPolicies` - Retry configurations

**Change Streams:**
MongoDB Change Streams enable real-time reactivity. When a configuration document is inserted, updated, or deleted, all connected Sail.Proxy instances are notified instantly without polling.

### 3. Sail Compass (`src/Sail.Compass`)

Compass is the "configuration watcher" component embedded in Sail.Proxy. It:

**Responsibilities:**
- Subscribes to MongoDB Change Streams for each configuration type
- Transforms MongoDB documents into YARP configuration objects
- Updates the in-memory `IProxyConfigProvider` in real-time
- Manages dynamic authentication schemes, CORS policies, rate limiters, etc.

**Observers:**
- `RouteObserver` - Watches route changes
- `ClusterObserver` - Watches cluster changes
- `CertificateObserver` - Watches certificate changes
- `CorsObserver` - Watches CORS policy changes
- `RateLimiterObserver` - Watches rate limiter policy changes
- `AuthenticationObserver` - Watches authentication policy changes
- `TimeoutObserver` - Watches timeout policy changes
- `RetryObserver` - Watches retry policy changes

### 4. Sail Proxy (`src/Sail.Proxy`)

The Proxy is the data plane that handles actual client requests. It:

**Request Pipeline:**
1. TLS/HTTPS termination (with SNI support)
2. HTTPS redirection
3. CORS policy application
4. Authentication (JWT/OIDC)
5. Authorization
6. Rate limiting
7. Request timeouts
8. Retry logic
9. YARP reverse proxy middleware
10. Load balancing
11. Backend request forwarding

**Technology:**
- Built on YARP (Yet Another Reverse Proxy)
- Custom middleware for Sail-specific features
- Dynamic configuration updates without restart

### 5. Web UI (`web/`)

The Web UI is a modern React application providing:

**Features:**
- Visual route and cluster management
- Real-time configuration status
- Policy editors (CORS, rate limiting, authentication)
- Certificate upload and management
- Testing and debugging tools

**Technology:**
- React 18 with TypeScript
- Vite for fast development
- Tailwind CSS for styling
- REST API client

## Data Flow

### Configuration Update Flow

1. **User Action**: Admin updates a route in the Web UI
2. **REST API**: Web UI sends PUT request to Sail Management API
3. **Database Write**: Management API updates MongoDB document
4. **Change Stream**: MongoDB emits change event
5. **Compass Observer**: Sail.Compass receives change notification
6. **Config Update**: Compass updates YARP `IProxyConfigProvider`
7. **YARP Reload**: YARP detects config change and reloads routes
8. **Traffic Flow**: New requests use updated configuration

**Time to Propagate**: Typically < 100ms from database write to live traffic

### Request Flow

1. **Client Request**: `GET https://gateway.example.com/api/users`
2. **TLS Termination**: Sail.Proxy decrypts HTTPS using configured certificate
3. **Route Matching**: YARP matches request to route by path/headers/host
4. **Middleware Pipeline**:
   - CORS: Validates origin and adds headers
   - Authentication: Validates JWT token
   - Authorization: Checks user permissions
   - Rate Limiting: Enforces request quotas
5. **Load Balancing**: Selects healthy backend destination
6. **Proxy Request**: Forwards to backend with optional transforms
7. **Response**: Returns backend response to client

## Design Principles

### 1. Zero-Downtime Configuration

All configuration changes take effect immediately without restarting the gateway. This is achieved through:
- MongoDB Change Streams for real-time notifications
- YARP's `IProxyConfigProvider` abstraction
- Dynamic policy registration (authentication schemes, CORS, rate limiters)

### 2. Separation of Concerns

- **Control Plane (Sail API)**: Configuration management only
- **Data Plane (Sail.Proxy)**: Request handling only
- **Storage (MongoDB)**: Persistent state only

This separation allows:
- Independent scaling of control and data planes
- Multiple gateway instances sharing the same configuration
- Control plane failures don't affect running traffic

### 3. Cloud-Native Ready

- **Containerized**: Docker images for all components
- **Stateless**: Proxy instances are stateless and horizontally scalable
- **Config-Driven**: Everything configurable via API or UI
- **Health Checks**: Built-in health endpoints for orchestrators

### 4. Extensibility

- **Middleware Pipeline**: Easy to add custom middleware
- **Service Discovery**: Plugin architecture for Consul, Kubernetes, etc.
- **Storage Abstraction**: Can support other databases beyond MongoDB
- **Transform Pipeline**: YARP's request/response transform system

## Scalability

### Horizontal Scaling

**Sail.Proxy** is designed to scale horizontally:
- Stateless architecture
- No instance-to-instance communication
- Independent MongoDB Change Stream connections
- Shared configuration via MongoDB

Deploy multiple instances behind a load balancer:

```
                    ┌──────────────┐
      Clients ─────▶│ Load Balancer│
                    └──────┬───────┘
                           │
          ┌────────────────┼────────────────┐
          │                │                │
    ┌─────▼─────┐    ┌─────▼─────┐    ┌────▼──────┐
    │  Proxy 1  │    │  Proxy 2  │    │  Proxy N  │
    └─────┬─────┘    └─────┬─────┘    └────┬──────┘
          │                │                │
          └────────────────┼────────────────┘
                           │
                    ┌──────▼───────┐
                    │   MongoDB    │
                    └──────────────┘
```

### Performance Characteristics

- **Throughput**: Inherits YARP's excellent performance (100k+ RPS per instance)
- **Latency**: Minimal overhead (<1ms) over direct YARP
- **Config Update**: < 100ms propagation time
- **Memory**: Scales with number of routes (~1KB per route)

## Security Considerations

### 1. TLS/HTTPS
- SNI support for multiple certificates
- Automatic certificate selection based on hostname
- Certificate storage in MongoDB with encryption at rest

### 2. Authentication
- JWT Bearer tokens with signature validation
- OpenID Connect integration
- Per-route authentication policies

### 3. API Security
- Management API should be protected (not exposed to internet)
- Consider API keys or OAuth for Management API access
- Use MongoDB authentication in production

### 4. Secrets Management
- Store sensitive data (certificates, keys) encrypted
- Consider integrating with Azure Key Vault, AWS Secrets Manager, etc.
- Rotate credentials regularly

## Monitoring & Observability

### Logging
- Structured logging with ASP.NET Core logging abstractions
- Request/response logging in Proxy
- Configuration change audit logs

### Metrics
- Built-in ASP.NET Core metrics
- YARP telemetry (request counts, latencies, errors)
- Custom metrics for configuration updates

### Distributed Tracing
- OpenTelemetry support
- Trace propagation through gateway
- Integration with Jaeger, Zipkin, etc.

## Future Architecture Enhancements

Potential areas for evolution:

1. **Redis Cache Layer**: Cache configuration for faster startup
2. **Event Sourcing**: Audit trail of all configuration changes
3. **Multi-Tenancy**: Namespace isolation for different teams
4. **Plugin System**: Load custom middleware from assemblies
5. **GraphQL API**: Alternative to REST for configuration management
6. **WebSocket Support**: Real-time configuration updates to Web UI

