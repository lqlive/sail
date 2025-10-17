import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import Layout from './components/Layout'
import Dashboard from './pages/Dashboard'
import RoutesPage from './pages/Routes'
import RouteEdit from './pages/RouteEdit'
import Clusters from './pages/Clusters'
import Certificates from './pages/Certificates'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<Navigate to="/dashboard" replace />} />
          <Route path="dashboard" element={<Dashboard />} />
          <Route path="routes" element={<RoutesPage />} />
          <Route path="routes/new" element={<RouteEdit />} />
          <Route path="routes/:id/edit" element={<RouteEdit />} />
          <Route path="clusters" element={<Clusters />} />
          <Route path="certificates" element={<Certificates />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}

export default App

