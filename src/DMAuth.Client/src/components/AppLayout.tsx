import { NavLink, Outlet } from 'react-router-dom'
import { Suspense } from 'react'
import { useAuth } from '../auth/useAuth'

function ContentLoader() {
  return (
    <div className="flex h-full items-center justify-center">
      <div className="h-8 w-8 animate-spin rounded-full border-4 border-indigo-600 border-t-transparent" />
    </div>
  )
}

export function AppLayout() {
  const { user, logout } = useAuth()

  return (
    <div className="flex h-screen bg-gray-50">
      <nav className="w-56 shrink-0 border-r border-gray-200 bg-white px-4 py-6 flex flex-col overflow-y-auto">
        <span className="mb-8 px-2 text-sm font-semibold tracking-wide text-indigo-600 uppercase">
          DM Auth
        </span>
        <ul className="flex flex-col gap-1 flex-1">
          <li>
            <NavLink
              to="/dashboard"
              className={({ isActive }) =>
                `block rounded-md px-3 py-2 text-sm font-medium transition-colors ${
                  isActive
                    ? 'bg-indigo-50 text-indigo-700'
                    : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
                }`
              }
            >
              Dashboard
            </NavLink>
          </li>
          <li>
            <NavLink
              to="/clients"
              className={({ isActive }) =>
                `block rounded-md px-3 py-2 text-sm font-medium transition-colors ${
                  isActive
                    ? 'bg-indigo-50 text-indigo-700'
                    : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
                }`
              }
            >
              Clients
            </NavLink>
          </li>
          <li>
            <NavLink
              to="/profile"
              className={({ isActive }) =>
                `block rounded-md px-3 py-2 text-sm font-medium transition-colors ${
                  isActive
                    ? 'bg-indigo-50 text-indigo-700'
                    : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
                }`
              }
            >
              Profile
            </NavLink>
          </li>
        </ul>
        <div className="border-t border-gray-200 pt-4">
          <p className="px-3 text-xs text-gray-500 truncate">{user?.email}</p>
          <button
            onClick={logout}
            className="mt-2 w-full rounded-md px-3 py-2 text-left text-sm text-gray-600 hover:bg-gray-100 hover:text-gray-900 transition-colors"
          >
            Sign out
          </button>
        </div>
      </nav>
      <main className="flex-1 overflow-y-auto p-8">
        <Suspense fallback={<ContentLoader />}>
          <Outlet />
        </Suspense>
      </main>
    </div>
  )
}
