import { FullConfig } from '@playwright/test';
import fs from 'fs';
import path from 'path';

async function globalSetup(config: FullConfig) {
  console.log('🔧 Global setup starting...');

  // Create test assets directory
  const testAssetsDir = path.join(__dirname, 'test-assets');
  if (!fs.existsSync(testAssetsDir)) {
    fs.mkdirSync(testAssetsDir, { recursive: true });
  }

  // Create dummy test files if they don't exist
  const testFiles = [
    { name: 'character1.png', content: 'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==' }, // 1x1 transparent PNG
    { name: 'character2.jpg', content: '/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQH/2wBDAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQH/wAARCAABAAEDASIAAhEBAxEB/8QAFQABAQAAAAAAAAAAAAAAAAAAAAn/xAAUEAEAAAAAAAAAAAAAAAAAAAAA/8QAFQEBAQAAAAAAAAAAAAAAAAAAAAX/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwA/AB8A/9k=' }, // 1x1 JPEG
    { name: 'setting1.png', content: 'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==' },
    { name: 'setting2.pdf', content: '%PDF-1.4\n1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] >>\nendobj\nxref\n0 4\n0000000000 65535 f\n0000000009 00000 n\n0000000058 00000 n\n0000000115 00000 n\ntrailer\n<< /Size 4 /Root 1 0 R >>\nstartxref\n149\n%%EOF' }, // Minimal PDF
  ];

  for (const file of testFiles) {
    const filePath = path.join(testAssetsDir, file.name);
    if (!fs.existsSync(filePath)) {
      const buffer = Buffer.from(file.content, 'base64');
      fs.writeFileSync(filePath, buffer);
      console.log(`  Created test asset: ${file.name}`);
    }
  }

  // Verify API is accessible
  try {
    const response = await fetch(`${process.env.E2E_API_URL || 'http://localhost:5255'}/`);
    if (response.ok) {
      console.log('✅ API is accessible');
    } else {
      console.warn('⚠️ API returned non-OK status:', response.status);
    }
  } catch (error) {
    console.warn('⚠️ Could not reach API:', error);
  }

  // Verify UI is accessible
  try {
    const response = await fetch(`${process.env.E2E_BASE_URL || 'http://localhost:5173'}/`);
    if (response.ok) {
      console.log('✅ UI is accessible');
    } else {
      console.warn('⚠️ UI returned non-OK status:', response.status);
    }
  } catch (error) {
    console.warn('⚠️ Could not reach UI:', error);
  }

  console.log('✅ Global setup complete');
}

export default globalSetup;