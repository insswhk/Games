import { Stack, Typography } from '@mui/material'
import { DataPage } from './DataPage'

export function BonusPointsPage() {
  return (
    <Stack spacing={3}>
      <Stack>
        <Typography variant="h4" sx={{ fontWeight: 800 }}>Bonus Points</Typography>
        <Typography color="text.secondary">Reportable bonus points issued by customer.</Typography>
      </Stack>
      <DataPage
        title="Bonus Points Summary"
        description="Current report uses the default API date scope; use Reports for filtered analysis."
        endpoint="/reports/bonus-points-summary"
        columns={[
          { key: 'customerCode', label: 'Customer Code' },
          { key: 'customerName', label: 'Customer' },
          { key: 'pointsIssued', label: 'Points Issued' },
        ]}
      />
    </Stack>
  )
}
