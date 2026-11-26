import { test as setup, expect } from '@playwright/test';
import fs from 'fs';
import path from 'path';

const BASE_URL = process.env.E2E_BASE_URL || 'http://localhost:5173';
const TEST_GOOGLE_EMAIL = process.env.E2E_GOOGLE_EMAIL || '';
const TEST_GOOGLE_PASSWORD = process.env.E2E_GOOGLE_PASSWORD || '';
const E2E_AUTH_COOKIE = process.env.E2E_AUTH_COOKIE || '';
const E2E_CSRF_COOKIE = process.env.E2E_CSRF_COOKIE || '';
const NAVIGATION_TIMEOUT = 30000;

const authFile = path.join(__dirname, 'playwright/.auth/user.json');

async function loginWithGoogle(page) {
  await page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle' });

  const googleButton = page.locator('button:has-text("Sign in with Google"), a:has-text("Sign in with Google")');
  await expect(googleButton).toBeVisible({ timeout: NAVIGATION_TIMEOUT });
  await googleButton.click();

  const popupPromise = page.waitForEvent('popup', { timeout: NAVIGATION_TIMEOUT });
  const popup = await popupPromise;

  await popup.waitForURL(/accounts\.google\.com/, { timeout: NAVIGATION_TIMEOUT });

  await popup.fill('input[type="email"]', TEST_GOOGLE_EMAIL);
  await popup.click('button:has-text("Next"), #identifierNext');

  await popup.waitForSelector('input[type="password"]', { timeout: NAVIGATION_TIMEOUT });
  await popup.fill('input[type="password"]', TEST_GOOGLE_PASSWORD);
  await popup.click('button:has-text("Next"), #passwordNext');

  try {
    await popup.waitForSelector('button:has-text("Continue"), button:has-text("Allow")', { timeout: 5000 });
    await popup.click('button:has-text("Continue"), button:has-text("Allow")');
  } catch {
    // Consent screen might not appear for pre-authorized apps
  }

  await popup.waitForEvent('close', { timeout: NAVIGATION_TIMEOUT });
  await page.waitForURL(`${BASE_URL}/`, { timeout: NAVIGATION_TIMEOUT });
  await page.waitForLoadState('networkidle');

  await expect(page.locator('button:has-text("Logout"), [data-testid="user-menu"]')).toBeVisible({ timeout: NAVIGATION_TIMEOUT });
}

async function loginViaCookie(page, context) {
  if (!E2E_AUTH_COOKIE) {
    throw new Error('E2E_AUTH_COOKIE not set. Run loginWithGoogle once, extract cookie, then use this method.');
  }

  await context.addCookies([
    {
      name: 'TicketTitan.Auth',
      value: E2E_AUTH_COOKIE,
      domain: 'localhost',
      path: '/',
      httpOnly: true,
      secure: false,
      sameSite: 'Lax',
    },
    {
      name: 'TicketTitan.AntiForgery',
      value: E2E_CSRF_COOKIE || '',
      domain: 'localhost',
      path: '/',
      httpOnly: true,
      secure: false,
      sameSite: 'Lax',
    }
  ]);

  await page.goto(`${BASE_URL}/`, { waitUntil: 'networkidle' });
  await expect(page.locator('button:has-text("Logout"), [data-testid="user-menu"]')).toBeVisible({ timeout: NAVIGATION_TIMEOUT });
}

setup('authenticate and save storage state', async ({ page, context }) => {
  // Ensure auth directory exists
  const authDir = path.dirname(authFile);
  if (!fs.existsSync(authDir)) {
    fs.mkdirSync(authDir, { recursive: true });
  }

  // Check if we already have a valid auth file
  if (fs.existsSync(authFile)) {
    try {
      const storedState = JSON.parse(fs.readFileSync(authFile, 'utf8'));
      // Check if cookies are still valid (not expired)
      const cookies = storedState.cookies || [];
      const authCookie = cookies.find(c => c.name === 'TicketTitan.Auth');
      if (authCookie && authCookie.expires > Date.now() / 1000) {
        console.log('✅ Valid auth state already exists, skipping login');
        return;
      }
    } catch {
      // Invalid auth file, will re-authenticate
    }
  }

  if (TEST_GOOGLE_EMAIL && TEST_GOOGLE_PASSWORD) {
    console.log('🔐 Authenticating via Google OAuth...');
    await loginWithGoogle(page);
  } else if (E2E_AUTH_COOKIE) {
    console.log('🔐 Authenticating via cookie...');
    await loginViaCookie(page, context);
  } else {
    throw new Error('No authentication method configured. Set E2E_GOOGLE_EMAIL/E2E_GOOGLE_PASSWORD or E2E_AUTH_COOKIE');
  }

  // Save storage state
  await context.storageState({ path: authFile });
  console.log(`✅ Authentication state saved to ${authFile}`);
});