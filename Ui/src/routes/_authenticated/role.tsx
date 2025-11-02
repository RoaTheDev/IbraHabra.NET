import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/_authenticated/role')({
  component: RouteComponent,
})

function RouteComponent() {
  return <div>Hello "/_authenticated/role"!</div>
}
