import AccountBalanceWalletIcon from '@mui/icons-material/AccountBalanceWallet'
import CasinoIcon from '@mui/icons-material/Casino'
import GroupsIcon from '@mui/icons-material/Groups'
import PointOfSaleIcon from '@mui/icons-material/PointOfSale'
import TrendingDownIcon from '@mui/icons-material/TrendingDown'
import TrendingUpIcon from '@mui/icons-material/TrendingUp'
import { Card, CardContent, Grid, Stack, Typography } from '@mui/material'
import { useEffect, useState } from 'react'
import { api } from '../api/client'
import type { DashboardKpi } from '../types'

const currency = new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' })

export function DashboardPage() {
  const [kpis, setKpis] = useState<DashboardKpi | null>(null)

  useEffect(() => {
    api.get<DashboardKpi>('/reports/dashboard').then((response) => setKpis(response.data))
  }, [])

  const cards = [
    { label: 'Total Cash In', value: currency.format(kpis?.totalCashIn ?? 0), icon: <TrendingUpIcon color="success" /> },
    { label: 'Total Cash Out', value: currency.format(kpis?.totalCashOut ?? 0), icon: <TrendingDownIcon color="error" /> },
    { label: 'Net Profit Today', value: currency.format(kpis?.netProfitToday ?? 0), icon: <PointOfSaleIcon color="primary" /> },
    { label: 'Bonus Points Issued', value: String(kpis?.bonusPointsIssued ?? 0), icon: <AccountBalanceWalletIcon color="secondary" /> },
    { label: 'Active Customers', value: String(kpis?.activeCustomers ?? 0), icon: <GroupsIcon color="primary" /> },
    { label: 'Active Games', value: String(kpis?.activeGames ?? 0), icon: <CasinoIcon color="warning" /> },
  ]

  return (
    <Stack spacing={3}>
      <Stack>
        <Typography variant="h4" sx={{ fontWeight: 800 }}>Dashboard</Typography>
        <Typography color="text.secondary">Live KPIs for cashier activity, customer balances, and games.</Typography>
      </Stack>
      <Grid container spacing={2}>
        {cards.map((card) => (
          <Grid key={card.label} size={{ xs: 12, sm: 6, lg: 4 }}>
            <Card>
              <CardContent>
                <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center' }}>
                  <Stack>
                    <Typography variant="body2" color="text.secondary">{card.label}</Typography>
                    <Typography variant="h4" sx={{ fontWeight: 800 }}>{card.value}</Typography>
                  </Stack>
                  {card.icon}
                </Stack>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Stack>
  )
}
