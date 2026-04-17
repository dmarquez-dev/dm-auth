import { describe, it, expect } from 'vitest'
import { screen } from '@testing-library/react'
import { Route, Routes } from 'react-router-dom'
import { PublicLayout } from './PublicLayout'
import { renderWithProviders } from '../test/renderWithProviders'

const setup = () =>
  renderWithProviders(
    <Routes>
      <Route element={<PublicLayout />}>
        <Route path="/login" element={<p>Login content</p>} />
      </Route>
    </Routes>,
    { initialEntries: ['/login'] }
  )

describe('PublicLayout', () => {
  it('renders the DM Auth brand header', () => {
    setup()
    expect(screen.getByText('DM Auth')).toBeInTheDocument()
  })

  it('renders child route content inside the card', () => {
    setup()
    expect(screen.getByText('Login content')).toBeInTheDocument()
  })
})
