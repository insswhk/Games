import AccountBalanceIcon from '@mui/icons-material/AccountBalance'
import AssessmentIcon from '@mui/icons-material/Assessment'
import CasinoIcon from '@mui/icons-material/Casino'
import DashboardIcon from '@mui/icons-material/Dashboard'
import GroupIcon from '@mui/icons-material/Group'
import LocationCityIcon from '@mui/icons-material/LocationCity'
import LogoutIcon from '@mui/icons-material/Logout'
import MenuIcon from '@mui/icons-material/Menu'
import PaymentsIcon from '@mui/icons-material/Payments'
import PersonIcon from '@mui/icons-material/Person'
import StarsIcon from '@mui/icons-material/Stars'
import {
  AppBar,
  Box,
  CssBaseline,
  Divider,
  Drawer,
  IconButton,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Typography,
  useMediaQuery,
  useTheme,
} from '@mui/material'
import { useState, type ReactNode } from 'react'
import { Link, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

const drawerWidth = 280

const navItems = [
  { label: 'Dashboard', form: 'Dashboard', path: '/', icon: <DashboardIcon /> },
  { label: 'Users', form: 'Users', path: '/users', icon: <PersonIcon /> },
  { label: 'Locations', form: 'Locations', path: '/locations', icon: <LocationCityIcon /> },
  { label: 'Cashiers', form: 'Cashiers', path: '/cashiers', icon: <PaymentsIcon /> },
  { label: 'Customers', form: 'Customers', path: '/customers', icon: <GroupIcon /> },
  { label: 'Members', form: 'Members', path: '/members', icon: <StarsIcon /> },
  { label: 'Transactions', form: 'Transactions', path: '/transactions', icon: <AccountBalanceIcon /> },
  { label: 'Bonus Points', form: 'BonusPoints', path: '/bonus-points', icon: <StarsIcon /> },
  { label: 'Expenses', form: 'Expenses', path: '/expenses', icon: <PaymentsIcon /> },
  { label: 'Games Register', form: 'Games', path: '/games', icon: <CasinoIcon /> },
  { label: 'Reports', form: 'Reports', path: '/reports', icon: <AssessmentIcon /> },
]

function DrawerContent({ onNavigate }: { onNavigate?: () => void }) {
  const { canOpen, logout, user } = useAuth()
  const location = useLocation()

  return (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <Toolbar sx={{ alignItems: 'flex-start', flexDirection: 'column', py: 2 }}>
        <Typography variant="h6" sx={{ fontWeight: 800 }}>Game Center CRM</Typography>
        <Typography variant="body2" color="text.secondary">{user?.fullName} · {user?.role}</Typography>
      </Toolbar>
      <Divider />
      <List aria-label="Primary navigation" sx={{ flex: 1 }}>
        {navItems.filter((item) => canOpen(item.form)).map((item) => (
          <ListItemButton
            key={item.path}
            component={Link}
            to={item.path}
            selected={location.pathname === item.path}
            onClick={onNavigate}
          >
            <ListItemIcon>{item.icon}</ListItemIcon>
            <ListItemText primary={item.label} />
          </ListItemButton>
        ))}
      </List>
      <Divider />
      <List>
        <ListItemButton onClick={logout}>
          <ListItemIcon><LogoutIcon /></ListItemIcon>
          <ListItemText primary="Sign out" />
        </ListItemButton>
      </List>
    </Box>
  )
}

export function AppLayout({ children }: { children?: ReactNode }) {
  const [open, setOpen] = useState(false)
  const theme = useTheme()
  const isDesktop = useMediaQuery(theme.breakpoints.up('lg'))

  return (
    <Box sx={{ display: 'flex', minHeight: '100svh', bgcolor: 'grey.100' }}>
      <CssBaseline />
      <AppBar position="fixed" sx={{ zIndex: theme.zIndex.drawer + 1 }}>
        <Toolbar>
          {!isDesktop && (
            <IconButton color="inherit" edge="start" onClick={() => setOpen(true)} aria-label="Open navigation">
              <MenuIcon />
            </IconButton>
          )}
          <Typography variant="h6" noWrap component="div" sx={{ ml: isDesktop ? 0 : 1 }}>
            ERP Dashboard
          </Typography>
        </Toolbar>
      </AppBar>
      <Drawer
        variant={isDesktop ? 'permanent' : 'temporary'}
        open={isDesktop || open}
        onClose={() => setOpen(false)}
        ModalProps={{ keepMounted: true }}
        sx={{ '& .MuiDrawer-paper': { width: drawerWidth, boxSizing: 'border-box' } }}
      >
        <DrawerContent onNavigate={() => setOpen(false)} />
      </Drawer>
      <Box component="main" sx={{ flexGrow: 1, p: { xs: 2, md: 3 }, mt: 8, ml: { lg: `${drawerWidth}px` } }}>
        {children ?? <Outlet />}
      </Box>
    </Box>
  )
}
