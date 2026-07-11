import { Card, CardContent, Grid, List, ListItemButton, ListItemText, Stack, Typography } from '@mui/material'
import { Navigate, useParams } from 'react-router-dom'
import { Link as RouterLink } from 'react-router-dom'
import { MarkdownContent } from '../components/MarkdownContent'
import { docs, findDoc } from '../docs/registry'

export function DocumentationPage() {
  const { slug } = useParams<{ slug: string }>()

  if (!slug) {
    return <Navigate to={`/documentation/${docs[0].slug}`} replace />
  }

  const active = findDoc(slug)

  return (
    <Stack spacing={3}>
      <Stack>
        <Typography variant="h4" sx={{ fontWeight: 800 }}>Documentation</Typography>
        <Typography color="text.secondary">
          Guides for setting up, understanding, and operating Game Center CRM.
        </Typography>
      </Stack>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 3 }}>
          <Card>
            <CardContent sx={{ p: 1 }}>
              <List aria-label="Documentation topics">
                {docs.map((doc) => (
                  <ListItemButton
                    key={doc.slug}
                    component={RouterLink}
                    to={`/documentation/${doc.slug}`}
                    selected={doc.slug === slug}
                  >
                    <ListItemText primary={doc.title} secondary={doc.summary} />
                  </ListItemButton>
                ))}
              </List>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, md: 9 }}>
          <Card>
            <CardContent sx={{ p: { xs: 2, md: 4 } }}>
              {active
                ? <MarkdownContent content={active.content} />
                : <Typography color="text.secondary">Select a topic to view its documentation.</Typography>}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Stack>
  )
}
