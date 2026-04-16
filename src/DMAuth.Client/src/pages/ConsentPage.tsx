import { useSearchParams, Navigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

const SCOPE_DESCRIPTIONS: Record<string, { label: string; description: string }> = {
  openid: {
    label: 'OpenID',
    description: 'Verify your identity',
  },
  profile: {
    label: 'Profile',
    description: 'Read your display name and username',
  },
  email: {
    label: 'Email',
    description: 'Read your email address',
  },
  'offline_access': {
    label: 'Offline access',
    description: 'Stay signed in after you close your browser',
  },
}

export function ConsentPage() {
  const { isAuthenticated, isLoading, user } = useAuth()
  const [searchParams] = useSearchParams()

  const clientId = searchParams.get('client_id') ?? ''
  const redirectUri = searchParams.get('redirect_uri') ?? ''
  const scope = searchParams.get('scope') ?? ''
  const state = searchParams.get('state') ?? ''
  const codeChallenge = searchParams.get('code_challenge') ?? ''
  const codeChallengeMethod = searchParams.get('code_challenge_method') ?? ''
  const nonce = searchParams.get('nonce') ?? ''

  const requestedScopes = scope.split(' ').filter(Boolean)

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-indigo-600 border-t-transparent" />
      </div>
    )
  }

  if (!isAuthenticated) {
    const returnUrl = encodeURIComponent(window.location.pathname + window.location.search)
    return <Navigate to={`/login?returnUrl=${returnUrl}`} replace />
  }

  if (!clientId || !redirectUri || !scope || !state || !codeChallenge || !codeChallengeMethod) {
    return (
      <div className="flex min-h-screen items-center justify-center px-4">
        <div className="max-w-sm text-center">
          <p className="text-sm text-red-600">Invalid authorization request — missing required parameters.</p>
        </div>
      </div>
    )
  }

  const handleDeny = () => {
    const url = new URL(redirectUri)
    url.searchParams.set('error', 'access_denied')
    url.searchParams.set('error_description', 'The user denied the authorization request.')
    url.searchParams.set('state', state)
    window.location.href = url.toString()
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4">
      <div className="w-full max-w-sm">
        <div className="mb-8 text-center">
          <span className="text-2xl font-semibold tracking-tight text-indigo-600">DM Auth</span>
        </div>

        <div className="rounded-xl border border-gray-200 bg-white p-8 shadow-sm">
          <div className="mb-6 text-center">
            <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-indigo-100">
              <svg className="h-6 w-6 text-indigo-600" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 5.25a3 3 0 0 1 3 3m3 0a6 6 0 0 1-7.029 5.912c-.563-.097-1.159.026-1.563.43L10.5 17.25H8.25v2.25H6v2.25H2.25v-2.818c0-.597.237-1.17.659-1.591l6.499-6.499c.404-.404.527-1 .43-1.563A6 6 0 0 1 21.75 8.25Z" />
              </svg>
            </div>
            <h1 className="text-lg font-semibold text-gray-900">Authorize access</h1>
            <p className="mt-1 text-sm text-gray-500">
              <span className="font-mono text-xs bg-gray-100 px-1.5 py-0.5 rounded text-gray-700">{clientId}</span>
              {' '}is requesting access to your account
            </p>
          </div>

          <div className="mb-6">
            <p className="mb-3 text-xs font-medium uppercase tracking-wide text-gray-500">
              Requested permissions
            </p>
            <ul className="flex flex-col gap-2">
              {requestedScopes.map(scopeValue => {
                const info = SCOPE_DESCRIPTIONS[scopeValue]
                return (
                  <li key={scopeValue} className="flex items-start gap-3 rounded-lg border border-gray-100 bg-gray-50 px-3 py-2.5">
                    <svg className="mt-0.5 h-4 w-4 shrink-0 text-indigo-500" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" d="m4.5 12.75 6 6 9-13.5" />
                    </svg>
                    <div>
                      <p className="text-sm font-medium text-gray-800">
                        {info?.label ?? scopeValue}
                      </p>
                      {info?.description && (
                        <p className="text-xs text-gray-500">{info.description}</p>
                      )}
                    </div>
                  </li>
                )
              })}
            </ul>
          </div>

          <p className="mb-6 text-center text-xs text-gray-400">
            Authorizing as{' '}
            <span className="font-medium text-gray-600">{user?.displayName ?? user?.email}</span>
          </p>

          {/* Approve — native form POST so the browser follows the 302 redirect */}
          <form method="post" action="/connect/consent" className="mb-3">
            <input type="hidden" name="client_id" value={clientId} />
            {requestedScopes.map(s => (
              <input key={s} type="hidden" name="scope" value={s} />
            ))}
            <input type="hidden" name="redirect_uri" value={redirectUri} />
            <input type="hidden" name="state" value={state} />
            <input type="hidden" name="code_challenge" value={codeChallenge} />
            <input type="hidden" name="code_challenge_method" value={codeChallengeMethod} />
            {nonce && <input type="hidden" name="nonce" value={nonce} />}

            <button
              type="submit"
              className="w-full rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 transition-colors"
            >
              Allow access
            </button>
          </form>

          <button
            type="button"
            onClick={handleDeny}
            className="w-full rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 transition-colors"
          >
            Deny
          </button>
        </div>
      </div>
    </div>
  )
}
