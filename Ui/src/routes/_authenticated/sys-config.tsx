import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/_authenticated/sys-config')({
  component: RouteComponent,
})

function RouteComponent() {
  return <div>Hello "/sys-config"!</div>
}
