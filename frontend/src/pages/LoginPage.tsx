import LockIcon from '@mui/icons-material/Lock'
import { Alert, Avatar, Box, Button, Card, CardContent, Stack, TextField, Typography } from '@mui/material'
import { FormEvent, useState } from 'react'
import { Navigate, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

export function LoginPage() {
  const { login, user } = useAuth()
  const navigate = useNavigate()
  const [userName, setUserName] = useState('admin')
  const [password, setPassword] = useState('Admin@12345')
  const [error, setError] = useState('')

  if (user) return <Navigate to="/" replace />

  async function onSubmit(event: FormEvent) {
    event.preventDefault()
    setError('')
    try {
      await login(userName, password)
      navigate('/')
    } catch {
      setError('Unable to sign in. Check your credentials and API connection.')
    }
  }

  return (
    <Box sx={{ minHeight: '100svh', display: 'grid', placeItems: 'center', bgcolor: 'grey.100', p: 2 }}>
      <Card sx={{ width: '100%', maxWidth: 420 }}>
        <CardContent>
          <Stack spacing={3} component="form" onSubmit={onSubmit}>
            <Stack alignItems="center" spacing={1}>
              <Avatar sx={{ bgcolor: 'primary.main' }}><LockIcon /></Avatar>
              <Typography variant="h5" fontWeight={800}>Game Center CRM</Typography>
              <Typography variant="body2" color="text.secondary">Financial-grade game club operations</Typography>
            </Stack>
            {error && <Alert severity="error">{error}</Alert>}
            <TextField label="User name" value={userName} onChange={(event) => setUserName(event.target.value)} required autoComplete="username" />
            <TextField label="Password" value={password} onChange={(event) => setPassword(event.target.value)} required type="password" autoComplete="current-password" />
            <Button type="submit" size="large" variant="contained">Sign in</Button>
            <Typography variant="caption" color="text.secondary">
              Seed logins: admin/Admin@12345, manager/Manager@12345, cashier1/Cashier@12345.
            </Typography>
          </Stack>
        </CardContent>
      </Card>
    </Box>
  )
}
