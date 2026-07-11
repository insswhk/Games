import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { ThemeProvider, createTheme } from '@mui/material/styles'
import './index.css'
import App from './App.tsx'
import { AuthProvider } from './auth/AuthContext.tsx'

const theme = createTheme({
  palette: {
    mode: 'light',
    primary: { main: '#172554' },
    secondary: { main: '#7c3aed' },
    background: { default: '#f4f6f8' },
  },
  shape: { borderRadius: 12 },
  typography: {
    fontFamily: ['Inter', 'system-ui', 'Segoe UI', 'Roboto', 'sans-serif'].join(','),
  },
  components: {
    MuiCard: {
      styleOverrides: {
        root: {
          boxShadow: '0 10px 30px rgba(15, 23, 42, 0.08)',
        },
      },
    },
  },
})

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ThemeProvider theme={theme}>
      <BrowserRouter>
        <AuthProvider>
          <App />
        </AuthProvider>
      </BrowserRouter>
    </ThemeProvider>
  </StrictMode>,
)
