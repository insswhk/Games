import { Alert, Button, Card, CardContent, Grid, MenuItem, Stack, TextField, Typography } from '@mui/material'
import { FormEvent, useEffect, useState } from 'react'
import { api } from '../api/client'
import type { AccountDto, DataRow, ExpenseType, LocationDto } from '../types'
import { DataPage } from './DataPage'

const expenseTypes: ExpenseType[] = ['Startup', 'Rent', 'Salary', 'Furniture', 'Games', 'Refreshments']

export function ExpensesPage() {
  const [accounts, setAccounts] = useState<AccountDto[]>([])
  const [locations, setLocations] = useState<LocationDto[]>([])
  const [refreshKey, setRefreshKey] = useState(0)
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [form, setForm] = useState({ accountId: '', locationId: '', expenseType: 'Rent' as ExpenseType, amount: 0, notes: '' })

  useEffect(() => {
    Promise.all([api.get<AccountDto[]>('/master-data/accounts'), api.get<LocationDto[]>('/master-data/locations')])
      .then(([accountResponse, locationResponse]) => {
        setAccounts(accountResponse.data.filter((account) => account.accountType === 'Expense'))
        setLocations(locationResponse.data)
      })
      .catch(() => setError('Unable to load expense lookup data.'))
  }, [])

  async function submit(event: FormEvent) {
    event.preventDefault()
    setError('')
    setMessage('')
    try {
      await api.post('/expenses', form)
      setMessage('Expense posted and ledger entries created.')
      setRefreshKey((value) => value + 1)
    } catch {
      setError('Expense failed validation. Check account, location, and amount.')
    }
  }

  return (
    <Stack spacing={3}>
      <Stack>
        <Typography variant="h4" fontWeight={800}>Expenses</Typography>
        <Typography color="text.secondary">Record operating expenses with double-entry ledger postings.</Typography>
      </Stack>
      {message && <Alert severity="success">{message}</Alert>}
      {error && <Alert severity="error">{error}</Alert>}
      <Card>
        <CardContent>
          <Grid container spacing={2} component="form" onSubmit={submit}>
            <Grid size={{ xs: 12, md: 4 }}>
              <TextField select fullWidth required label="Location" value={form.locationId} onChange={(event) => setForm({ ...form, locationId: event.target.value })}>
                {locations.map((location) => <MenuItem key={location.id} value={location.id}>{location.clubName}</MenuItem>)}
              </TextField>
            </Grid>
            <Grid size={{ xs: 12, md: 4 }}>
              <TextField select fullWidth required label="Expense Account" value={form.accountId} onChange={(event) => setForm({ ...form, accountId: event.target.value })}>
                {accounts.map((account) => <MenuItem key={account.id} value={account.id}>{account.accountNumber} · {account.accountName}</MenuItem>)}
              </TextField>
            </Grid>
            <Grid size={{ xs: 12, md: 4 }}>
              <TextField select fullWidth label="Expense Type" value={form.expenseType} onChange={(event) => setForm({ ...form, expenseType: event.target.value as ExpenseType })}>
                {expenseTypes.map((type) => <MenuItem key={type} value={type}>{type}</MenuItem>)}
              </TextField>
            </Grid>
            <Grid size={{ xs: 12, md: 4 }}>
              <TextField fullWidth label="Amount" type="number" value={form.amount} onChange={(event) => setForm({ ...form, amount: Number(event.target.value) })} />
            </Grid>
            <Grid size={{ xs: 12, md: 8 }}>
              <TextField fullWidth label="Notes" value={form.notes} onChange={(event) => setForm({ ...form, notes: event.target.value })} />
            </Grid>
            <Grid size={{ xs: 12 }}>
              <Button type="submit" variant="contained">Post Expense</Button>
            </Grid>
          </Grid>
        </CardContent>
      </Card>
      <ExpenseTable key={refreshKey} />
    </Stack>
  )
}

function ExpenseTable() {
  return (
    <DataPage
      title="Expense Register"
      description="Latest posted expenses."
      endpoint="/expenses"
      columns={[
        { key: 'expenseDate', label: 'Date' },
        { key: 'expenseType', label: 'Type' },
        { key: 'amount', label: 'Amount' },
        { key: 'notes', label: 'Notes' },
      ] satisfies { key: keyof DataRow & string; label: string }[]}
    />
  )
}
