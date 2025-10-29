import { createFileRoute } from '@tanstack/react-router'
import { SampleDashboard } from '@/components/SampleDashboard.tsx'

export const Route = createFileRoute('/')({ component: App })

function App() {
  return <SampleDashboard />
}
