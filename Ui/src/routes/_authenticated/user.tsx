import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/_authenticated/user')({
  component: RouteComponent,
})

function RouteComponent() {
  return <div>Hello "/_authenticated/user-management"!</div>
}
