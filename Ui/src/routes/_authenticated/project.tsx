import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/_authenticated/project')({
  component: RouteComponent,
})

function RouteComponent() {
  return <div>Hello "/_authenticated/project"!</div>
}
