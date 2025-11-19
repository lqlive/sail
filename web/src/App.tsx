import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Layout from './components/Layout/Layout';
import Dashboard from './pages/Dashboard';
import RoutesPage from './pages/Routes';
import RouteDetail from './pages/Routes/RouteDetail';
import RouteEdit from './pages/Routes/RouteEdit';
import ClustersPage from './pages/Clusters';
import ClusterDetail from './pages/Clusters/ClusterDetail';
import ClusterEdit from './pages/Clusters/ClusterEdit';
import CertificatesPage from './pages/Certificates';
import CertificateEdit from './pages/Certificates/CertificateEdit';
import MiddlewaresPage from './pages/Middlewares';
import MiddlewareEdit from './pages/Middlewares/MiddlewareEdit';
import Settings from './pages/Settings';
import './App.css';

const AppContent: React.FC = () => {
  return (
    <Router>
      <Layout>
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/routes" element={<RoutesPage />} />
          <Route path="/routes/new" element={<RouteEdit />} />
          <Route path="/routes/:id/edit" element={<RouteEdit />} />
          <Route path="/routes/:id" element={<RouteDetail />} />
          <Route path="/clusters" element={<ClustersPage />} />
          <Route path="/clusters/new" element={<ClusterEdit />} />
          <Route path="/clusters/:id/edit" element={<ClusterEdit />} />
          <Route path="/clusters/:id" element={<ClusterDetail />} />
          <Route path="/certificates" element={<CertificatesPage />} />
          <Route path="/certificates/new" element={<CertificateEdit />} />
          <Route path="/certificates/:id" element={<CertificateEdit />} />
          <Route path="/middlewares" element={<MiddlewaresPage />} />
          <Route path="/middlewares/new" element={<MiddlewareEdit />} />
          <Route path="/middlewares/:id" element={<MiddlewareEdit />} />
          <Route path="/settings" element={<Settings />} />
        </Routes>
      </Layout>
    </Router>
  );
};

const App: React.FC = () => {
  return <AppContent />;
};

export default App; 