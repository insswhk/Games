import {
  Box,
  Divider,
  Link as MuiLink,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import type { ComponentPropsWithoutRef } from 'react'
import ReactMarkdown from 'react-markdown'
import { Link as RouterLink } from 'react-router-dom'
import remarkGfm from 'remark-gfm'

function isExternal(href: string) {
  return /^(https?:)?\/\//.test(href) || href.startsWith('mailto:') || href.startsWith('#')
}

export function MarkdownContent({ content }: { content: string }) {
  return (
    <Box sx={{ '& > *:first-of-type': { mt: 0 } }}>
      <ReactMarkdown
        remarkPlugins={[remarkGfm]}
        components={{
          h1: ({ children }) => (
            <Typography variant="h4" sx={{ fontWeight: 800, mt: 4, mb: 2 }}>{children}</Typography>
          ),
          h2: ({ children }) => (
            <Typography variant="h5" sx={{ fontWeight: 800, mt: 4, mb: 1.5 }}>{children}</Typography>
          ),
          h3: ({ children }) => (
            <Typography variant="h6" sx={{ fontWeight: 700, mt: 3, mb: 1 }}>{children}</Typography>
          ),
          p: ({ children }) => (
            <Typography variant="body1" sx={{ my: 1.5, lineHeight: 1.7 }}>{children}</Typography>
          ),
          ul: ({ children }) => (
            <Box component="ul" sx={{ my: 1.5, pl: 3 }}>{children}</Box>
          ),
          ol: ({ children }) => (
            <Box component="ol" sx={{ my: 1.5, pl: 3 }}>{children}</Box>
          ),
          li: ({ children }) => (
            <Typography component="li" variant="body1" sx={{ my: 0.5, lineHeight: 1.7 }}>{children}</Typography>
          ),
          a: ({ href, children }) => {
            const target = href ?? '#'
            if (isExternal(target)) {
              return <MuiLink href={target} target="_blank" rel="noopener noreferrer">{children}</MuiLink>
            }
            const slug = target.replace(/^\.?\//, '').replace(/\.md$/, '')
            return <MuiLink component={RouterLink} to={`/documentation/${slug}`}>{children}</MuiLink>
          },
          blockquote: ({ children }) => (
            <Box
              sx={{
                my: 2,
                pl: 2,
                py: 0.5,
                borderLeft: '4px solid',
                borderColor: 'secondary.main',
                bgcolor: 'grey.50',
                color: 'text.secondary',
                borderRadius: 1,
              }}
            >
              {children}
            </Box>
          ),
          code: ({ className, children }) => {
            const isBlock = Boolean(className)
            if (isBlock) {
              return (
                <Box
                  component="code"
                  sx={{
                    display: 'block',
                    fontFamily: 'monospace',
                    fontSize: '0.85rem',
                    whiteSpace: 'pre',
                    overflowX: 'auto',
                  }}
                >
                  {children}
                </Box>
              )
            }
            return (
              <Box
                component="code"
                sx={{
                  fontFamily: 'monospace',
                  fontSize: '0.85rem',
                  bgcolor: 'grey.100',
                  px: 0.75,
                  py: 0.25,
                  borderRadius: 1,
                }}
              >
                {children}
              </Box>
            )
          },
          pre: ({ children }) => (
            <Paper
              variant="outlined"
              sx={{ my: 2, p: 2, bgcolor: 'grey.900', color: 'grey.100', overflowX: 'auto', borderRadius: 2 }}
            >
              {children}
            </Paper>
          ),
          hr: () => <Divider sx={{ my: 3 }} />,
          table: ({ children }) => (
            <TableContainer component={Paper} variant="outlined" sx={{ my: 2 }}>
              <Table size="small">{children}</Table>
            </TableContainer>
          ),
          thead: ({ children }) => <TableHead>{children}</TableHead>,
          tbody: ({ children }) => <TableBody>{children}</TableBody>,
          tr: ({ children }) => <TableRow>{children}</TableRow>,
          th: ({ children }) => <TableCell sx={{ fontWeight: 700 }}>{children}</TableCell>,
          td: ({ children }: ComponentPropsWithoutRef<'td'>) => <TableCell>{children}</TableCell>,
        }}
      >
        {content}
      </ReactMarkdown>
    </Box>
  )
}
