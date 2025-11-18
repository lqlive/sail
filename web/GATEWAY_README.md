# Gateway Frontend

A modern, minimalist web interface for managing API Gateway configurations.

## Features

### Dashboard
- **Overview Statistics**: View total routes, clusters, and certificates
- **Health Monitoring**: Track destination health status
- **System Status**: Monitor active routes, clusters, and certificate validity

### Routes Management
- **Route Listing**: Browse all configured routes with search and filtering
- **Route Details**: View and manage individual route configurations
- **Status Indicators**: Quickly identify active/inactive routes
- **Configuration Display**: See path patterns, methods, hosts, and order

### Clusters Management
- **Cluster Overview**: View all backend clusters with health status
- **Load Balancing**: Display configured load balancing policies
- **Destination Tracking**: Monitor all destinations per cluster
- **Health Checks**: View health check configuration and status

### Certificates Management
- **Certificate Inventory**: List all SSL/TLS certificates
- **Expiration Monitoring**: Identify certificates expiring soon
- **Certificate Details**: View issuer, subject, thumbprint, and validity dates
- **Status Indicators**: Visual alerts for expiring certificates

## Design Principles

- **Minimalist UI**: Clean, white background with subtle borders
- **Clear Typography**: Readable fonts with proper hierarchy
- **Responsive Design**: Works seamlessly on desktop, tablet, and mobile
- **Intuitive Navigation**: Simple top navigation with clear sections
- **Consistent Patterns**: Reusable design patterns throughout

## Tech Stack

- **React 18** with TypeScript
- **Vite** for fast development and building
- **TailwindCSS** for styling
- **React Router** for navigation
- **Heroicons** for iconography

## Getting Started

```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview
```

## Current Status

All pages are implemented with mock data. API integration is ready to be connected to your backend endpoints.

### Mock Data Features
- Dashboard shows realistic gateway statistics
- Routes page displays sample routing rules
- Clusters page shows backend server groups
- Certificates page displays SSL/TLS certificates with expiration tracking

## Next Steps

1. Connect to actual Gateway API endpoints
2. Implement create/edit/delete operations
3. Add real-time updates via WebSocket or polling
4. Implement authentication and authorization
5. Add more detailed configuration options
6. Implement route and cluster testing tools

## API Integration Points

The following endpoints will need to be implemented:

- `GET /api/stats` - Dashboard statistics
- `GET /api/routes` - List all routes
- `GET /api/routes/:id` - Get route details
- `GET /api/clusters` - List all clusters
- `GET /api/clusters/:id` - Get cluster details
- `GET /api/certificates` - List all certificates

## License

MIT

