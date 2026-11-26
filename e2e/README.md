# TicketTitan End-to-End Tests

Playwright-based E2E tests covering the complete user flow.

## Quick Start

```bash
# 1. Install dependencies
cd C:\TicketTitan\e2e
npm install

# 2. Install browsers
npm run install:browsers

# 3. Configure environment
cp .env.example .env
# Edit .env with your credentials

# 4. Start both servers (in separate terminals)
# Terminal 1 - API:
cd C:\TicketTitan && dotnet run

# Terminal 2 - UI:
cd C:\ticket-titan-ui && npm run dev

# 5. Run tests
npm test
```

## Test Coverage

| Test | Description |
|------|-------------|
| `Complete story creation → chat → accept → upload flow` | Full user journey |
| `Verify session persists after reload` | Auth persistence |
| `Verify SignalR connection` | Real-time connectivity |
| `Health Checks` | API + UI availability |

## Authentication Methods

### Method 1: Google OAuth (Full Flow)
Set in `.env`:
```bash
E2E_GOOGLE_EMAIL=test@gmail.com
E2E_GOOGLE_PASSWORD=password
```
- Tests real Google OAuth flow
- Slower, but tests actual authentication

### Method 2: Pre-authenticated Cookies (Fast)
1. Manually log in at `http://localhost:5173`
2. DevTools → Application → Cookies → `localhost`
3. Copy `TicketTitan.Auth` value → `E2E_AUTH_COOKIE`
4. Copy `TicketTitan.AntiForgery` value → `E2E_CSRF_COOKIE`
5. Set in `.env`:
```bash
E2E_AUTH_COOKIE=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
E2E_CSRF_COOKIE=CfDJ8...
```
- Much faster (skips Google OAuth)
- Cookies expire ~1 hour, refresh when needed

## Test Assets

Test files are auto-generated in `e2e/test-assets/`:
- `character1.png` / `character2.jpg` - Character references
- `setting1.png` / `setting2.pdf` - Setting references

## Running Specific Tests

```bash
# Run single test file
npx playwright test story-flow.spec.ts

# Run in headed mode (see browser)
npx playwright test --headed

# Debug mode (step through)
npx playwright test --debug

# UI mode (interactive)
npx playwright test --ui

# Show last report
npx playwright show-report
```

## CI/CD Integration

```yaml
# GitHub Actions example
- name: Run E2E Tests
  run: |
    cd e2e
    npm ci
    npm run install:browsers
    npm test
  env:
    E2E_BASE_URL: ${{ secrets.STAGING_UI_URL }}
    E2E_API_URL: ${{ secrets.STAGING_API_URL }}
    E2E_AUTH_COOKIE: ${{ secrets.E2E_AUTH_COOKIE }}
    E2E_CSRF_COOKIE: ${{ secrets.E2E_CSRF_COOKIE }}
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| `401 Unauthorized` | Refresh auth cookie or check Google credentials |
| `Timeout waiting for Gemini` | Increase `AI_RESPONSE_TIMEOUT` in test |
| `File not found` | Run global setup: `npx playwright test --global-setup` |
| `Port in use` | Kill existing processes: `taskkill /F /IM dotnet.exe` |

## Test Structure

```
e2e/
├── story-flow.spec.ts      # Main test scenarios
├── playwright.config.ts    # Playwright configuration
├── global-setup.ts         # Auto-generates test assets
├── global-teardown.ts      # Cleanup
├── package.json            # Dependencies
├── .env.example            # Configuration template
└── test-assets/            # Generated test files
```

## Extending Tests

Add new test cases in `story-flow.spec.ts`:

```typescript
test('New feature test', async () => {
  // Use helper functions:
  // - loginWithGoogle / loginViaCookie
  // - createStorySession
  // - sendChatMessage
  // - acceptStory
  // - uploadAttachments
  // - verifySessionApproved
});
```