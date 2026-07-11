import { Navigate, Route, Routes } from 'react-router-dom'
import { useAuth } from './auth/AuthContext'
import { AppLayout } from './layout/AppLayout'
import { BonusPointsPage } from './pages/BonusPointsPage'
import { DashboardPage } from './pages/DashboardPage'
import { DataPage } from './pages/DataPage'
import { ExpensesPage } from './pages/ExpensesPage'
import { LoginPage } from './pages/LoginPage'
import { ReportsPage } from './pages/ReportsPage'
import { TransactionsPage } from './pages/TransactionsPage'

function ProtectedApp() {
  const { user } = useAuth()
  if (!user) return <Navigate to="/login" replace />

  return <AppLayout />
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<ProtectedApp />}>
        <Route index element={<DashboardPage />} />
        <Route path="users" element={<DataPage title="Users" description="Role-based operators with location scope." endpoint="/master-data/users" columns={[
          { key: 'userName', label: 'User Name' },
          { key: 'fullName', label: 'Full Name' },
          { key: 'role', label: 'Role' },
          { key: 'isActive', label: 'Active' },
        ]} />} />
        <Route path="locations" element={<DataPage title="Locations" description="Multi-club location records." endpoint="/master-data/locations" columns={[
          { key: 'clubName', label: 'Club Name' },
          { key: 'city', label: 'City' },
          { key: 'state', label: 'State' },
          { key: 'country', label: 'Country' },
          { key: 'manager', label: 'Manager' },
          { key: 'caretaker', label: 'Caretaker' },
        ]} />} />
        <Route path="cashiers" element={<DataPage title="Cashiers" description="Cashiers assigned to locations with register balances." endpoint="/master-data/cashiers" columns={[
          { key: 'cashierCode', label: 'Code' },
          { key: 'fullName', label: 'Name' },
          { key: 'locationName', label: 'Location' },
          { key: 'cashRegisterBalance', label: 'Register Balance' },
          { key: 'isActive', label: 'Active' },
        ]} />} />
        <Route path="customers" element={<DataPage title="Customers" description="Customer profiles, balances, referrals, and bonus points." endpoint="/master-data/customers" columns={[
          { key: 'customerCode', label: 'Code' },
          { key: 'fullName', label: 'Name' },
          { key: 'locationName', label: 'Location' },
          { key: 'mobile', label: 'Mobile' },
          { key: 'balance', label: 'Balance' },
          { key: 'bonusPoints', label: 'Bonus Points' },
        ]} />} />
        <Route path="members" element={<DataPage title="Members" description="Membership types and expiry dates." endpoint="/master-data/members" columns={[
          { key: 'membershipNumber', label: 'Membership #' },
          { key: 'membershipType', label: 'Type' },
          { key: 'expiryDate', label: 'Expiry' },
          { key: 'isActive', label: 'Active' },
        ]} />} />
        <Route path="transactions" element={<TransactionsPage />} />
        <Route path="bonus-points" element={<BonusPointsPage />} />
        <Route path="expenses" element={<ExpensesPage />} />
        <Route path="games" element={<DataPage title="Games Register" description="Game assets, suppliers, and maintenance cost tracking." endpoint="/master-data/games" columns={[
          { key: 'gameName', label: 'Game' },
          { key: 'numberOfPlayers', label: 'Players' },
          { key: 'purchaseAmount', label: 'Purchase Amount' },
          { key: 'supplierInfo', label: 'Supplier' },
          { key: 'maintenanceCosts', label: 'Maintenance Costs' },
          { key: 'lastMaintenanceDate', label: 'Last Maintenance' },
        ]} />} />
        <Route path="reports" element={<ReportsPage />} />
      </Route>
    </Routes>
  )
}
