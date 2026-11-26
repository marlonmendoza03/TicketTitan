import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: '.',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  timeout: 120000, // 2 minutes per test
  expect: {
    timeout: 10000,
  },

  use: {
    baseURL: process.env.E2E_BASE_URL || 'http://localhost:5173',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    // Use saved authentication state
    storageState: 'playwright/.auth/user.json',
  },

  projects: [
    {
      name: 'setup',
      testMatch: /.*auth-setup\.ts/,
      teardown: 'teardown',
    },
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
      dependencies: ['setup'],
    },
    // Uncomment for multi-browser testing
    // {
    //   name: 'firefox',
    //   use: { ...devices['Desktop Firefox'] },
    //   dependencies: ['setup'],
    // },
    // {
    //   name: 'webkit',
    //   use: { ...devices['Desktop Safari'] },
    //   dependencies: ['setup'],
    // },
    {
      name: 'teardown',
      testMatch: /.*global-teardown\.ts/,
    },
  ],

  globalSetup: require.resolve('./global-setup.ts'),
});