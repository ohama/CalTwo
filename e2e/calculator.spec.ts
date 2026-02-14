import { test, expect } from '@playwright/test';

test.describe('Calculator E2E', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('displays initial state of 0', async ({ page }) => {
    await expect(page.getByTestId('display')).toHaveText('0');
  });

  test('adds two numbers: 2 + 3 = 5', async ({ page }) => {
    await page.getByRole('button', { name: '2' }).click();
    await page.getByRole('button', { name: '+' }).click();
    await page.getByRole('button', { name: '3' }).click();
    await page.getByRole('button', { name: '=' }).click();
    await expect(page.getByTestId('display')).toHaveText('5');
  });

  test('subtracts: 9 - 4 = 5', async ({ page }) => {
    await page.getByRole('button', { name: '9' }).click();
    await page.getByRole('button', { name: '-' }).click();
    await page.getByRole('button', { name: '4' }).click();
    await page.getByRole('button', { name: '=' }).click();
    await expect(page.getByTestId('display')).toHaveText('5');
  });

  test('multiplies: 6 × 7 = 42', async ({ page }) => {
    await page.getByRole('button', { name: '6' }).click();
    await page.getByRole('button', { name: '×' }).click();
    await page.getByRole('button', { name: '7' }).click();
    await page.getByRole('button', { name: '=' }).click();
    await expect(page.getByTestId('display')).toHaveText('42');
  });

  test('divides: 8 ÷ 2 = 4', async ({ page }) => {
    await page.getByRole('button', { name: '8' }).click();
    await page.getByRole('button', { name: '÷' }).click();
    await page.getByRole('button', { name: '2' }).click();
    await page.getByRole('button', { name: '=' }).click();
    await expect(page.getByTestId('display')).toHaveText('4');
  });

  test('shows Error on divide by zero', async ({ page }) => {
    await page.getByRole('button', { name: '5' }).click();
    await page.getByRole('button', { name: '÷' }).click();
    await page.getByRole('button', { name: '0' }).click();
    await page.getByRole('button', { name: '=' }).click();
    await expect(page.getByTestId('display')).toHaveText('Error');
  });

  test('clears display with C button', async ({ page }) => {
    await page.getByRole('button', { name: '5' }).click();
    await page.getByRole('button', { name: '3' }).click();
    await expect(page.getByTestId('display')).toHaveText('53');
    await page.getByRole('button', { name: 'C' }).click();
    await expect(page.getByTestId('display')).toHaveText('0');
  });

  test('handles decimal input', async ({ page }) => {
    await page.getByRole('button', { name: '3' }).click();
    await page.getByRole('button', { name: '.' }).click();
    await page.getByRole('button', { name: '5' }).click();
    await expect(page.getByTestId('display')).toHaveText('3.5');
  });

  test('chains operations left to right: 2 + 3 × 4 = 20', async ({ page }) => {
    await page.getByRole('button', { name: '2' }).click();
    await page.getByRole('button', { name: '+' }).click();
    await page.getByRole('button', { name: '3' }).click();
    await page.getByRole('button', { name: '×' }).click();
    // At this point 2+3=5 should be evaluated, display shows 5
    await expect(page.getByTestId('display')).toHaveText('5');
    await page.getByRole('button', { name: '4' }).click();
    await page.getByRole('button', { name: '=' }).click();
    await expect(page.getByTestId('display')).toHaveText('20');
  });

  test('backspace removes last digit', async ({ page }) => {
    await page.getByRole('button', { name: '1' }).click();
    await page.getByRole('button', { name: '2' }).click();
    await page.getByRole('button', { name: '3' }).click();
    await expect(page.getByTestId('display')).toHaveText('123');
    await page.getByRole('button', { name: '←' }).click();
    await expect(page.getByTestId('display')).toHaveText('12');
  });
});
