import apiReference from '../../../docs/api-reference.md?raw'
import architecture from '../../../docs/architecture.md?raw'
import overview from '../../../docs/overview.md?raw'
import quickStart from '../../../docs/quick-start.md?raw'
import setup from '../../../docs/setup.md?raw'
import userGuide from '../../../docs/user-guide.md?raw'

export interface DocEntry {
  slug: string
  title: string
  summary: string
  content: string
}

export const docs: DocEntry[] = [
  {
    slug: 'overview',
    title: 'Overview',
    summary: 'What the project is and where to find each guide.',
    content: overview,
  },
  {
    slug: 'quick-start',
    title: 'Quick Start',
    summary: 'From clone to a first posted transaction in minutes.',
    content: quickStart,
  },
  {
    slug: 'setup',
    title: 'Setup & Installation',
    summary: 'Full local environment: backend, database, and frontend.',
    content: setup,
  },
  {
    slug: 'architecture',
    title: 'Architecture',
    summary: 'Projects, layers, and how data flows through the system.',
    content: architecture,
  },
  {
    slug: 'user-guide',
    title: 'User Guide',
    summary: 'Roles, screens, and day-to-day workflows.',
    content: userGuide,
  },
  {
    slug: 'api-reference',
    title: 'API Reference',
    summary: 'REST endpoints exposed by the backend API.',
    content: apiReference,
  },
]

export function findDoc(slug: string | undefined): DocEntry | undefined {
  return docs.find((doc) => doc.slug === slug)
}
