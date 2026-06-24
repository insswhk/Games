import { Alert, Button, Card, CardContent, Grid, MenuItem, Stack, TextField, Typography } from '@mui/material'
import { useEffect, useState, type FormEvent } from 'react'
import { api } from '../api/client'
import type { CashierDto, CustomerDto, GameModeDto, LocationDto, ShiftType, TransactionType } from '../types'

const transactionTypes: TransactionType[] = ['AddMoney', 'WithdrawMoney', 'BonusPoints']
const shifts: ShiftType[] = ['Day', 'Night']

export function TransactionsPage() {
  const [customers, setCustomers] = useState<CustomerDto[]>([])
  const [cashiers, setCashiers] = useState<CashierDto[]>([])
  const [locations, setLocations] = useState<LocationDto[]>([])
  const [gameModes, setGameModes] = useState<GameModeDto[]>([])
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [form, setForm] = useState({
    customerId: '',
    cashierId: '',
    locationId: '',
    gameModeId: '',
    transactionType: 'AddMoney' as TransactionType,
    shift: 'Day' as ShiftType,
    amount: 0,
    bonusPoints: 0,
    notes: '',
  })

  useEffect(() => {
    Promise.all([
      api.get<CustomerDto[]>('/master-data/customers'),
      api.get<CashierDto[]>('/master-data/cashiers'),
      api.get<LocationDto[]>('/master-data/locations'),
      api.get<GameModeDto[]>('/master-data/game-modes'),
    ]).then(([customerResponse, cashierResponse, locationResponse, modeResponse]) => {
      setCustomers(customerResponse.data)
      setCashiers(cashierResponse.data)
      setLocations(locationResponse.data)
      setGameModes(modeResponse.data)
    }).catch(() => setError('Unable to load transaction lookup data.'))
  }, [])

  async function submit(event: FormEvent) {
    event.preventDefault()
    setError('')
    setMessage('')
    try {
      const { data } = await api.post('/transactions', form)
      setMessage(`Transaction posted. Customer balance: ${data.customerBalanceAfter}, cashier register: ${data.cashierRegisterAfter}`)
    } catch {
      setError('Transaction failed validation. Check balance, register, shift, cashier, location, and game mode.')
    }
  }

  return (
    <Stack spacing={3}>
      <Stack>
        <Typography variant="h4" sx={{ fontWeight: 800 }}>Transactions</Typography>
        <Typography color="text.secondary">Post Add Money, Withdraw Money, and Bonus Point entries through the transaction engine.</Typography>
      </Stack>
      {message && <Alert severity="success">{message}</Alert>}
      {error && <Alert severity="error">{error}</Alert>}
      <Card>
        <CardContent>
          <Grid container spacing={2} component="form" onSubmit={submit}>
            <Grid size={{ xs: 12, md: 6 }}>
              <TextField select fullWidth label="Location" value={form.locationId} required onChange={(event) => setForm({ ...form, locationId: event.target.value })}>
                {locations.map((location) => <MenuItem key={location.id} value={location.id}>{location.clubName}</MenuItem>)}
              </TextField>
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <TextField select fullWidth label="Cashier" value={form.cashierId} required onChange={(event) => setForm({ ...form, cashierId: event.target.value })}>
                {cashiers.map((cashier) => <MenuItem key={cashier.id} value={cashier.id}>{cashier.cashierCode} · {cashier.fullName}</MenuItem>)}
              </TextField>
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <TextField select fullWidth label="Customer" value={form.customerId} required onChange={(event) => setForm({ ...form, customerId: event.target.value })}>
                {customers.map((customer) => <MenuItem key={customer.id} value={customer.id}>{customer.customerCode} · {customer.fullName}</MenuItem>)}
              </TextField>
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <TextField select fullWidth label="Game Mode" value={form.gameModeId} required onChange={(event) => setForm({ ...form, gameModeId: event.target.value })}>
                {gameModes.map((mode) => <MenuItem key={mode.id} value={mode.id}>{mode.code} · {mode.name}</MenuItem>)}
              </TextField>
            </Grid>
            <Grid size={{ xs: 12, md: 4 }}>
              <TextField select fullWidth label="Transaction Type" value={form.transactionType} onChange={(event) => setForm({ ...form, transactionType: event.target.value as TransactionType })}>
                {transactionTypes.map((type) => <MenuItem key={type} value={type}>{type}</MenuItem>)}
              </TextField>
            </Grid>
            <Grid size={{ xs: 12, md: 4 }}>
              <TextField select fullWidth label="Shift" value={form.shift} onChange={(event) => setForm({ ...form, shift: event.target.value as ShiftType })}>
                {shifts.map((shift) => <MenuItem key={shift} value={shift}>{shift}</MenuItem>)}
              </TextField>
            </Grid>
            <Grid size={{ xs: 12, md: 4 }}>
              <TextField fullWidth label="Amount" type="number" value={form.amount} onChange={(event) => setForm({ ...form, amount: Number(event.target.value) })} />
            </Grid>
            <Grid size={{ xs: 12, md: 4 }}>
              <TextField fullWidth label="Bonus Points" type="number" value={form.bonusPoints} onChange={(event) => setForm({ ...form, bonusPoints: Number(event.target.value) })} />
            </Grid>
            <Grid size={{ xs: 12, md: 8 }}>
              <TextField fullWidth label="Notes" value={form.notes} onChange={(event) => setForm({ ...form, notes: event.target.value })} />
            </Grid>
            <Grid size={{ xs: 12 }}>
              <Button type="submit" variant="contained" size="large">Post Transaction</Button>
            </Grid>
          </Grid>
        </CardContent>
      </Card>
    </Stack>
  )
}
