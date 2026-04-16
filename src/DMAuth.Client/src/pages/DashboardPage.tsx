import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { clientApi } from '../api/clientApi'
import { useAuth } from '../auth/useAuth'

function StatCard({
  label,
  value,
  loading,
}: {
  label: string
  value: string | number
  loading: boolean
}) {
  return (
    <div className="rounded-xl border border-gray-200 bg-white px-6 py-5">
      <p className="text-sm text-gray-500">{label}</p>
      {loading ? (
        <div className="mt-2 h-7 w-16 animate-pulse rounded bg-gray-200" />
      ) : (
        <p className="mt-1 text-2xl font-semibold text-gray-900">{value}</p>
      )}
    </div>
  )
}

export function DashboardPage() {
  const { user } = useAuth()

  const { data: clients, isLoading: clientsLoading } = useQuery({
    queryKey: ['clients'],
    queryFn: () => clientApi.listClients().then(r => r.data),
  })

  return (
    <div className="max-w-3xl">
      <div className="mb-8">
        <h1 className="text-2xl font-semibold text-gray-900">
          Welcome back, {user?.displayName}
        </h1>
        <p className="mt-1 text-sm text-gray-500">{user?.email}</p>
      </div>

      <div className="mb-8 grid grid-cols-2 gap-4">
        <StatCard
          label="Registered clients"
          value={clients?.length ?? 0}
          loading={clientsLoading}
        />
        <StatCard
          label="Username"
          value={user?.username ?? '—'}
          loading={false}
        />
      </div>

      <div>
        <h2 className="mb-3 text-sm font-medium text-gray-700">Quick actions</h2>
        <div className="flex flex-wrap gap-3">
          <Link
            to="/clients/new"
            className="inline-flex items-center gap-2 rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 transition-colors"
          >
            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
            </svg>
            New client
          </Link>
          <Link
            to="/clients"
            className="inline-flex items-center gap-2 rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors"
          >
            Manage clients
          </Link>
          <Link
            to="/profile"
            className="inline-flex items-center gap-2 rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors"
          >
            Edit profile
          </Link>
        </div>
      </div>
    </div>
  )
}
