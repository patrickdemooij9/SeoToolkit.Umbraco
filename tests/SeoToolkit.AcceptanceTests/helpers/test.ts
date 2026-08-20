import { test as umbracoTest } from '@umbraco-cms/acceptance-test-helpers';
import { expect } from '@playwright/test';

/**
 * Browser console noise that is not a SeoToolkit defect.
 *
 * Keep this list SHORT and justified — every entry is a class of regression the smoke
 * suite can no longer see. Add a comment saying why, and prefer fixing the source.
 */
const IGNORED_CONSOLE_ERRORS: RegExp[] = [
  // Chromium reports the missing favicon as a console error on every backoffice page.
  /favicon\.ico/i,
  // Benign browser layout notice, not an application error.
  /ResizeObserver loop/i,
  // The Clean starter kit loads Font Awesome and Google Fonts from CDNs that are
  // unreachable on a locked-down CI runner. Not our code.
  /use\.fontawesome\.com|fonts\.googleapis\.com|cdn\.jsdelivr\.net/i,
];

function isRelevant(message: string): boolean {
  return !IGNORED_CONSOLE_ERRORS.some((pattern) => pattern.test(message));
}

type SeoToolkitFixtures = {
  /** Every console error and uncaught page error seen during the test. */
  consoleErrors: string[];
  /** Auto-fixture: fails the test if any unignored console error was recorded. */
  failOnConsoleErrors: void;
};

/**
 * Extends the Umbraco fixture (which supplies umbracoApi / umbracoUi) with a console
 * error guard. A Lit bundle that fails to import, a missing custom element, or a broken
 * API call all surface here as a console error rather than as a visible failure, so
 * asserting on them is what makes these smoke tests actually catch regressions.
 */
export const test = umbracoTest.extend<SeoToolkitFixtures>({
  consoleErrors: async ({ page }, use) => {
    const errors: string[] = [];

    page.on('console', (message) => {
      if (message.type() === 'error') {
        errors.push(message.text());
      }
    });
    page.on('pageerror', (error) => {
      errors.push(`[pageerror] ${error.message}`);
    });

    await use(errors);
  },

  failOnConsoleErrors: [
    async ({ consoleErrors }, use, testInfo) => {
      await use();

      // Don't pile a second failure onto a test that already failed for a better reason.
      if (testInfo.status !== testInfo.expectedStatus) {
        return;
      }

      const relevant = consoleErrors.filter(isRelevant);
      expect(relevant, `Unexpected browser console errors:\n${relevant.join('\n')}`).toEqual([]);
    },
    { auto: true },
  ],
});

export { expect };
