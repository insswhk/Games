import { Alert, Box, Card, CardContent, Stack, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Typography } from '@mui/material'
import { useEffect, useState } from 'react'
import { api } from '../api/client'
import type { DataRow } from '../types'

interface DataPageProps {
  title: string
  description: string
  endpoint: string
  columns: { key: string; label: string }[]
}

export function DataPage({ title, description, endpoint, columns }: DataPageProps) {
  const [rows, setRows] = useState<DataRow[]>([])
  const [error, setError] = useState('')

  useEffect(() => {
    setError('')
    api.get<DataRow[]>(endpoint)
      .then((response) => setRows(response.data))
      .catch(() => setError('Unable to load data. Verify permissions and API connectivity.'))
  }, [endpoint])

  return (
    <Stack spacing={3}>
      <Stack>
        <Typography variant="h4" fontWeight={800}>{title}</Typography>
        <Typography color="text.secondary">{description}</Typography>
      </Stack>
      {error && <Alert severity="error">{error}</Alert>}
      <Card className="data-section">
        <CardContent>
          <TableContainer component={Box}>
            <Table size="small" aria-label={`${title} table`}>
              <TableHead>
                <TableRow>
                  {columns.map((column) => <TableCell key={column.key}>{column.label}</TableCell>)}
                </TableRow>
              </TableHead>
              <TableBody>
                {rows.map((row) => (
                  <TableRow key={String(row.id)}>
                    {columns.map((column) => (
                      <TableCell key={column.key}>{formatCell(row[column.key])}</TableCell>
                    ))}
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </CardContent>
      </Card>
    </Stack>
  )
}

function formatCell(value: DataRow[string]) {
  if (typeof value === 'boolean') return value ? 'Yes' : 'No'
  if (typeof value === 'number') return value.toLocaleString()
  return value ?? ''
}
