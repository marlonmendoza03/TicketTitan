import { FullConfig } from '@playwright/test';

async function globalTeardown(config: FullConfig) {
  console.log('🧹 Global teardown...');
  // Cleanup if needed (e.g., delete test data from database)
  console.log('✅ Global teardown complete');
}

export default globalTeardown;