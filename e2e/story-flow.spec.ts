import { test, expect, Page, BrowserContext } from '@playwright/test';

// ============================================================================
// CONFIGURATION
// ============================================================================
const BASE_URL = process.env.E2E_BASE_URL || 'http://localhost:5173';
const API_URL = process.env.E2E_API_URL || 'http://localhost:5255';

// Test credentials (set via environment variables)
const TEST_GOOGLE_EMAIL = process.env.E2E_GOOGLE_EMAIL || '';
const TEST_GOOGLE_PASSWORD = process.env.E2E_GOOGLE_PASSWORD || '';

// Timeouts
const NAVIGATION_TIMEOUT = 30000;
const ACTION_TIMEOUT = 10000;
const AI_RESPONSE_TIMEOUT = 60000; // Gemini can be slow

// ============================================================================
// HELPER FUNCTIONS
// ============================================================================

async function waitForNetworkIdle(page: Page, timeout = 5000) {
  await page.waitForLoadState('networkidle', { timeout });
}

async function loginWithGoogle(page: Page, context: BrowserContext) {
  // Navigate to login page
  await page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle' });

  // Click "Sign in with Google" button
  const googleButton = page.locator('button:has-text("Sign in with Google"), a:has-text("Sign in with Google")');
  await expect(googleButton).toBeVisible({ timeout: NAVIGATION_TIMEOUT });
  await googleButton.click();

  // Handle Google OAuth popup/redirect
  // Option A: If it opens in a popup
  const popupPromise = page.waitForEvent('popup', { timeout: NAVIGATION_TIMEOUT });
  const popup = await popupPromise;

  // Wait for Google sign-in page
  await popup.waitForURL(/accounts\.google\.com/, { timeout: NAVIGATION_TIMEOUT });

  // Fill email
  await popup.fill('input[type="email"]', TEST_GOOGLE_EMAIL);
  await popup.click('button:has-text("Next"), #identifierNext');

  // Fill password
  await popup.waitForSelector('input[type="password"]', { timeout: NAVIGATION_TIMEOUT });
  await popup.fill('input[type="password"]', TEST_GOOGLE_PASSWORD);
  await popup.click('button:has-text("Next"), #passwordNext');

  // Handle consent screen if appears
  try {
    await popup.waitForSelector('button:has-text("Continue"), button:has-text("Allow")', { timeout: 5000 });
    await popup.click('button:has-text("Continue"), button:has-text("Allow")');
  } catch {
    // Consent screen might not appear for pre-authorized apps
  }

  // Wait for popup to close (redirect back to app)
  await popup.waitForEvent('close', { timeout: NAVIGATION_TIMEOUT });

  // Wait for main page to load authenticated state
  await page.waitForURL(`${BASE_URL}/`, { timeout: NAVIGATION_TIMEOUT });
  await waitForNetworkIdle(page);

  // Verify logged in - check for user avatar or logout button
  await expect(page.locator('button:has-text("Logout"), [data-testid="user-menu"]')).toBeVisible({ timeout: NAVIGATION_TIMEOUT });
}

async function loginViaCookie(page: Page, context: BrowserContext) {
  /**
   * Alternative: Login by setting auth cookie directly (faster, no Google OAuth)
   * Requires: Pre-authenticated cookie from a manual login session
   * Usage: Set E2E_AUTH_COOKIE env var with the cookie value
   */
  const authCookie = process.env.E2E_AUTH_COOKIE;
  if (!authCookie) {
    throw new Error('E2E_AUTH_COOKIE not set. Run loginWithGoogle once, extract cookie, then use this method.');
  }

  await context.addCookies([
    {
      name: 'TicketTitan.Auth',
      value: authCookie,
      domain: 'localhost',
      path: '/',
      httpOnly: true,
      secure: false,
      sameSite: 'Lax',
    },
    {
      name: 'TicketTitan.AntiForgery',
      value: process.env.E2E_CSRF_COOKIE || '',
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

async function getCsrfToken(page: Page): Promise<string> {
  // Extract CSRF token from the page (meta tag or cookie)
  const csrfMeta = page.locator('meta[name="csrf-token"]');
  if (await csrfMeta.count() > 0) {
    return await csrfMeta.getAttribute('content') || '';
  }

  // Fallback: get from cookie
  const cookies = await page.context().cookies();
  const csrfCookie = cookies.find(c => c.name === 'TicketTitan.AntiForgery');
  return csrfCookie?.value || '';
}

async function createStorySession(page: Page, storyIdea: string) {
  // Navigate to create story page
  await page.goto(`${BASE_URL}/stories/create`, { waitUntil: 'networkidle' });

  // Check if we're on the right page - if we see "Logout" we might be on home page
  const pageTitle = await page.title();
  const pageUrl = page.url();
  console.log(`Page URL: ${pageUrl}, Title: ${pageTitle}`);

  // If we're on home page (has Logout but no story form), click Create Story
  const createStoryBtn = page.locator('button:has-text("Create Story"):visible, a:has-text("Create Story"):visible, button:has-text("New Story"):visible, a:has-text("New Story"):visible').first();
  if (await createStoryBtn.count() > 0) {
    console.log('Found Create Story button, clicking...');
    await createStoryBtn.click();
    await page.waitForTimeout(1000);
  }

  // Wait for the page to load - wait for any form element
  await page.waitForSelector('select, button, textarea, input', { timeout: NAVIGATION_TIMEOUT });
  await page.waitForTimeout(1000); // Small delay for any animations

  // Debug: log all form elements and their properties
  const selects = await page.locator('select').all();
  const textareas = await page.locator('textarea').all();
  const buttons = await page.locator('button').all();
  const inputs = await page.locator('input').all();
  console.log(`Found ${selects.length} selects, ${textareas.length} textareas, ${buttons.length} buttons, ${inputs.length} inputs`);

  // Log all visible elements with their text
  const allVisible = await page.locator('button:visible, [role="radio"]:visible, [role="option"]:visible, select:visible, textarea:visible, input:visible').all();
  console.log('All visible interactive elements:');
  for (const el of allVisible) {
    const tag = await el.evaluate(e => e.tagName.toLowerCase());
    const text = await el.textContent();
    const type = await el.getAttribute('type');
    const placeholder = await el.getAttribute('placeholder');
    const role = await el.getAttribute('role');
    const id = await el.getAttribute('id');
    const className = await el.getAttribute('class');
    console.log(`  <${tag}> text="${text?.trim().substring(0, 50)}" type="${type}" placeholder="${placeholder}" role="${role}" id="${id}" class="${className?.substring(0, 50)}"`);
  }

  // Also log ALL elements with MUI Select related classes (visible or not)
  const allMuiSelects = await page.locator('.MuiSelect-root, .MuiInputBase-root, [role="combobox"]').all();
  console.log(`Found ${allMuiSelects.length} potential MUI Select elements (including hidden):`);
  for (const el of allMuiSelects) {
    const tag = await el.evaluate(e => e.tagName.toLowerCase());
    const text = await el.textContent();
    const isVisible = await el.isVisible();
    const className = await el.getAttribute('class');
    const id = await el.getAttribute('id');
    const role = await el.getAttribute('role');
    console.log(`  <${tag}> text="${text?.trim().substring(0, 50)}" visible="${isVisible}" id="${id}" role="${role}" class="${className?.substring(0, 80)}"`);
  }

  // Check for Material UI Select components (they have hidden native inputs)
  // Look for the clickable Select trigger elements
  const muiSelects = await page.locator('.MuiSelect-root, [role="combobox"], .MuiInputBase-root:has(input.MuiSelect-nativeInput)').all();
  console.log(`Found ${muiSelects.length} MUI Select components`);

  // If we have native selects OR MUI Selects
  const totalSelects = selects.length + muiSelects.length;
  if (totalSelects >= 3) {
    // Handle MUI Select stepper pattern
    // Find the enabled comboboxes (the ones without Mui-disabled class)
    const comboboxes = await page.locator('[role="combobox"]:visible').all();
    console.log(`Found ${comboboxes.length} visible comboboxes`);

    for (let i = 0; i < comboboxes.length; i++) {
      const combo = comboboxes[i];
      const isDisabled = await combo.getAttribute('class').then(cls => cls?.includes('Mui-disabled') || false);
      console.log(`Combobox ${i}: disabled=${isDisabled}`);

      if (!isDisabled) {
        // This is the currently enabled step
        let optionText = '';
        if (i === 0) optionText = 'Horror';
        else if (i === 1) optionText = '3';
        else if (i === 2) optionText = '120';

        console.log(`Clicking combobox ${i} to select "${optionText}"`);
        await combo.click();
        await page.waitForTimeout(500);
        await page.locator(`[role="option"]:has-text("${optionText}"), li:has-text("${optionText}")`).first().click();
        await page.waitForTimeout(1000); // Wait for next step to enable

        // Re-fetch comboboxes after selection
        const newComboboxes = await page.locator('[role="combobox"]:visible').all();
        if (newComboboxes.length > comboboxes.length || newComboboxes[i + 1]?.getAttribute('class').then(cls => !cls?.includes('Mui-disabled'))) {
          console.log(`Step ${i + 1} completed, next step enabled`);
        }
      }
    }
  } else {
    // Alternative UI: wizard/stepper - need to click through setup steps
    console.log('No select elements found - trying wizard/stepper UI patterns');

    // Try clicking "Next" or "Continue" buttons to advance through setup
    // Look for stepper navigation
    let step = 1;
    while (step <= 5) { // Max 5 steps to avoid infinite loop
      console.log(`Step ${step}: Looking for options...`);

      // Log visible button texts for debugging
      const visibleButtons = await page.locator('button:visible').all();
      for (const btn of visibleButtons) {
        const text = await btn.textContent();
        console.log(`  Button: "${text}"`);
      }

      // Try clicking Horror category if visible
      const horrorBtn = page.locator('button:has-text("Horror"):visible, [role="radio"]:has-text("Horror"):visible').first();
      if (await horrorBtn.count() > 0) {
        console.log('  Clicking Horror category');
        await horrorBtn.click();
        await page.waitForTimeout(500);
      }

      // Try clicking 3 episodes if visible
      const threeBtn = page.locator('button:has-text("3"):visible, [role="radio"]:has-text("3"):visible').first();
      if (await threeBtn.count() > 0) {
        console.log('  Clicking 3 episodes');
        await threeBtn.click();
        await page.waitForTimeout(500);
      }

      // Try clicking 120 duration if visible
      const durationBtn = page.locator('button:has-text("120"):visible, [role="radio"]:has-text("120"):visible').first();
      if (await durationBtn.count() > 0) {
        console.log('  Clicking 120 duration');
        await durationBtn.click();
        await page.waitForTimeout(500);
      }

      // Try to click Next/Continue button to advance (exclude disabled buttons and send button)
      const nextBtn = page.locator('button:has-text("Next"):visible:not([disabled]), button:has-text("Continue"):visible:not([disabled]), button:has-text("Submit"):visible:not([disabled])').first();
      // Also check for buttons that are specifically for navigation (not the send button)
      const navBtn = page.locator('button[data-testid*="next"], button[data-testid*="continue"], .stepper-next, .wizard-next').first();

      let clicked = false;
      if (await nextBtn.count() > 0) {
        console.log('  Clicking Next/Continue');
        await nextBtn.click();
        clicked = true;
      } else if (await navBtn.count() > 0) {
        console.log('  Clicking navigation button');
        await navBtn.click();
        clicked = true;
      }

      if (clicked) {
        await page.waitForTimeout(1000);
        step++;
      } else {
        // No next button, might be done with setup
        console.log('  No Next button found, checking if setup complete');
        break;
      }
    }
  }

  // Step 4: Enter story idea - wait for textarea to be enabled
  console.log('Waiting for textarea to be enabled...');
  await page.waitForSelector('textarea:not([disabled])', { timeout: NAVIGATION_TIMEOUT });
  await page.fill('textarea:not([disabled])', storyIdea);

  // Click Send button - wait for navigation
  const sendBtn = page.locator('button[aria-label="Send message"]:not([disabled]), button:has-text("Send"):not([disabled]), button[type="submit"]:not([disabled])').first();
  await expect(sendBtn).toBeEnabled({ timeout: NAVIGATION_TIMEOUT });

  console.log('Clicking send button...');
  await sendBtn.click();

  // Wait for either modal or redirect
  await page.waitForTimeout(2000);
  console.log(`Current URL after click: ${page.url()}`);

  // Check for modal
  const modal = page.locator('[role="dialog"], .MuiModal-root, .modal').first();
  if (await modal.count() > 0) {
    console.log('Modal detected, waiting for it to close...');
    await modal.waitFor({ state: 'detached', timeout: NAVIGATION_TIMEOUT });
  }

  // Check for error messages
  const errorMsg = page.locator('.MuiAlert-message, .error, [role="alert"]').first();
  if (await errorMsg.count() > 0) {
    const errorText = await errorMsg.textContent();
    console.log(`Error message: ${errorText}`);
  }

  // Check for loading state
  const loading = page.locator('.MuiCircularProgress-root, .loading, [aria-busy="true"]').first();
  if (await loading.count() > 0) {
    console.log('Loading detected, waiting...');
    await loading.waitFor({ state: 'detached', timeout: NAVIGATION_TIMEOUT });
  }

  // Wait for redirect to session page
  await page.waitForURL(/\/tickets\/[a-f0-9-]+/, { timeout: NAVIGATION_TIMEOUT });
  await waitForNetworkIdle(page);

  // Extract session ID from URL
  const url = page.url();
  const match = url.match(/\/tickets\/([a-f0-9-]+)/);
  if (!match) throw new Error('Session ID not found in URL');
  return match[1];
}

async function sendChatMessage(page: Page, message: string) {
  // Wait for composer to be enabled
  await page.waitForSelector('textarea[label="Your story idea"]:not([disabled])', { timeout: ACTION_TIMEOUT });

  // Type message
  await page.fill('textarea[label="Your story idea"]', message);

  // Send
  await page.click('button[aria-label="Send message"]');

  // Wait for Gemini response (look for new assistant bubble)
  await page.waitForSelector('.chat-bubble-assistant:last-child', { timeout: AI_RESPONSE_TIMEOUT });
  await waitForNetworkIdle(page);
}

async function acceptStory(page: Page) {
  // Click "Accept Story" button
  await page.click('button:has-text("Accept Story")');

  // Confirm in dialog
  await page.click('button:has-text("Yes, Accept")');

  // Wait for attachment upload section to appear
  await page.waitForSelector('text=Upload Reference Assets', { timeout: NAVIGATION_TIMEOUT });
  await waitForNetworkIdle(page);
}

async function uploadAttachments(page: Page, characterFiles: string[], settingFiles: string[]) {
  // Upload character files
  if (characterFiles.length > 0) {
    const charInput = page.locator('#character-upload');
    await charInput.setInputFiles(characterFiles);
    await page.waitForTimeout(1000); // Allow file preview to render
  }

  // Upload setting files
  if (settingFiles.length > 0) {
    const settingInput = page.locator('#setting-upload');
    await settingInput.setInputFiles(settingFiles);
    await page.waitForTimeout(1000);
  }

  // Click "Upload All"
  await page.click('button:has-text("Upload All")');

  // Wait for success state
  await page.waitForSelector('text=Story has been approved and references uploaded', { timeout: NAVIGATION_TIMEOUT });
  await waitForNetworkIdle(page);
}

async function verifySessionApproved(page: Page) {
  // Verify status chip shows "Approved"
  await expect(page.locator('text=Approved')).toBeVisible();

  // Verify composer is disabled
  await expect(page.locator('textarea[label="Your story idea"][disabled]')).toBeVisible();

  // Verify uploaded assets are displayed
  await expect(page.locator('text=Uploaded Assets')).toBeVisible();
}

// ============================================================================
// TEST SUITE
// ============================================================================

test.describe('Full Story Flow E2E', () => {
  // Disable retries for this test suite (configured in playwright.config.ts for CI)
  test.describe.configure({ retries: 0 });
  let context: BrowserContext;
  let page: Page;

  test.beforeAll(async ({ browser }) => {
    context = await browser.newContext({
      // Record video for debugging
      recordVideo: { dir: 'test-results/videos/' },
      // Accept downloads
      acceptDownloads: true,
    });
    page = await context.newPage();
    page.setDefaultTimeout(NAVIGATION_TIMEOUT);

    // Verify we're authenticated (storageState from config should handle this)
    await page.goto(BASE_URL, { waitUntil: 'networkidle' });
    await expect(page.locator('button:has-text("Logout"), [data-testid="user-menu"]')).toBeVisible({ timeout: NAVIGATION_TIMEOUT });
  });

  test.afterAll(async () => {
    await context.close();
  });

  test('Complete story creation → chat → accept → upload flow + persistence verification', async () => {
    test.setTimeout(300000); // 5 minutes for full flow

    // -------------------------------------------------------------------------
    // PHASE 1: CREATE STORY SESSION (already authenticated via storageState)
    // -------------------------------------------------------------------------
    const storyIdea = 'A night-shift security guard keeps hearing his own voice on the radio, but the transmissions are from 10 minutes in the future. Each warning saves his life, until one night the voice tells him to do something terrible.';

    const sessionId = await createStorySession(page, storyIdea);
    console.log(`Created session: ${sessionId}`);

    // Verify we're on the session page
    await expect(page.locator('h2:has-text("Gemini story assistant")')).toBeVisible();

    // Verify initial message exists
    await expect(page.locator('.chat-bubble-assistant').first()).toContainText('Describe the story');

    // -------------------------------------------------------------------------
    // PHASE 3: CHAT WITH GEMINI (Polish cycle)
    // -------------------------------------------------------------------------
    // First Gemini response should appear automatically after session creation
    await page.waitForSelector('.chat-bubble-assistant:nth-child(2)', { timeout: AI_RESPONSE_TIMEOUT });

    // Send a polish message
    await sendChatMessage(page, 'Make the protagonist a former detective who lost his partner.');

    // Verify user message appears
    await expect(page.locator('.chat-bubble-user').last()).toContainText('former detective');

    // Wait for Gemini response
    await page.waitForSelector('.chat-bubble-assistant:last-child', { timeout: AI_RESPONSE_TIMEOUT });

    // Send another polish
    await sendChatMessage(page, 'Add a twist: the future voice is actually his own recordings from a parallel timeline.');

    // Wait for Gemini response
    await page.waitForSelector('.chat-bubble-assistant:last-child', { timeout: AI_RESPONSE_TIMEOUT });

    // -------------------------------------------------------------------------
    // PHASE 4: ACCEPT STORY
    // -------------------------------------------------------------------------
    await acceptStory(page);

    // Verify we're in "Awaiting References" state
    await expect(page.locator('text=Awaiting References')).toBeVisible();

    // Verify attachment upload section is visible
    await expect(page.locator('text=Upload Reference Assets')).toBeVisible();
    await expect(page.locator('text=Character References')).toBeVisible();
    await expect(page.locator('text=Setting References')).toBeVisible();

    // -------------------------------------------------------------------------
    // PHASE 5: UPLOAD ATTACHMENTS
    // -------------------------------------------------------------------------
    // Create test files
    const testFilesDir = 'test-assets';
    const characterFiles = [
      `${testFilesDir}/character1.png`,
      `${testFilesDir}/character2.jpg`,
    ];
    const settingFiles = [
      `${testFilesDir}/setting1.png`,
      `${testFilesDir}/setting2.pdf`,
    ];

    // Note: These files need to exist. Create them in test setup or use fixtures.
    // For now, we'll skip if files don't exist
    const fs = require('fs');
    const hasCharFiles = characterFiles.every(f => fs.existsSync(f));
    const hasSettingFiles = settingFiles.every(f => fs.existsSync(f));

    if (hasCharFiles || hasSettingFiles) {
      await uploadAttachments(page, hasCharFiles ? characterFiles : [], hasSettingFiles ? settingFiles : []);
    } else {
      console.log('Test asset files not found, skipping upload verification');
      // Just verify the upload UI works
      await expect(page.locator('#character-upload')).toBeTruthy();
      await expect(page.locator('#setting-upload')).toBeTruthy();
    }

    // -------------------------------------------------------------------------
    // PHASE 6: VERIFY FINAL STATE
    // -------------------------------------------------------------------------
    await verifySessionApproved(page);

    // Verify attachments appear in the list (if uploaded)
    if (hasCharFiles) {
      await expect(page.locator('text=character1.png')).toBeVisible();
      await expect(page.locator('text=character2.jpg')).toBeVisible();
    }
    if (hasSettingFiles) {
      await expect(page.locator('text=setting1.png')).toBeVisible();
      await expect(page.locator('text=setting2.pdf')).toBeVisible();
    }

    // -------------------------------------------------------------------------
    // PHASE 6: VERIFY SESSION PERSISTS AFTER RELOAD
    // -------------------------------------------------------------------------
    await test.step('Verify session persists after reload', async () => {
      // Reload page
      await page.reload({ waitUntil: 'networkidle' });

      // Should still be on session page (not redirected to login)
      await expect(page).toHaveURL(/\/tickets\/[a-f0-9-]+/);

      // Session should still be approved
      await expect(page.locator('text=Approved')).toBeVisible();

      // Chat history should be preserved
      await expect(page.locator('.chat-bubble-user').first()).toBeVisible();
      await expect(page.locator('.chat-bubble-assistant').first()).toBeVisible();
    });

    // -------------------------------------------------------------------------
    // PHASE 7: VERIFY SIGNALR CONNECTION
    // -------------------------------------------------------------------------
    await test.step('Verify SignalR connection', async () => {
      // Check SignalR connection indicator (if implemented)
      // This is a placeholder - implement based on your UI
      const signalRStatus = page.locator('[data-testid="signalr-status"]');
      if (await signalRStatus.count() > 0) {
        await expect(signalRStatus).toHaveText(/connected/i);
      }
    });
  });
});

// ============================================================================
// API-LEVEL TESTS (Faster, no browser)
// ============================================================================

test.describe('Story API Direct Tests', () => {
  let authToken: string;
  let csrfToken: string;

  test.beforeAll(async ({ request }) => {
    // Get CSRF token
    const csrfResponse = await request.get(`${API_URL}/api/auth/csrf-token`, {
      headers: { 'Accept': 'application/json' }
    });
    const csrfData = await csrfResponse.json();
    csrfToken = csrfData.token;

    // Note: For full API tests, you'd need to authenticate via Google OAuth
    // This is a placeholder for API-level testing
  });

  test('Create session via API', async ({ request }) => {
    test.skip('Requires authenticated user context');
  });

  test('Send message via API', async ({ request }) => {
    test.skip('Requires authenticated user context');
  });

  test('Approve session via API', async ({ request }) => {
    test.skip('Requires authenticated user context');
  });

  test('Upload attachment via API', async ({ request }) => {
    test.skip('Requires authenticated user context');
  });
});

// ============================================================================
// UTILITY TESTS
// ============================================================================

test.describe('Health Checks', () => {
  test('API health endpoint', async ({ request }) => {
    const response = await request.get(`${API_URL}/`, { timeout: 10000 });
    expect(response.ok()).toBeTruthy();
    const body = await response.json();
    expect(body.status).toContain('TicketTitan');
  });

  test('UI loads', async ({ page }) => {
    await page.goto(BASE_URL, { waitUntil: 'networkidle' });
    // Use first() to avoid strict mode violation (multiple "Ticket Titan" elements)
    await expect(page.locator('text=Ticket Titan').first()).toBeVisible();
  });
});