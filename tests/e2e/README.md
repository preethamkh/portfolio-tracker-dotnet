# E2E Tests (Playwright)

Browser-based end-to-end tests for the Blazor frontend.

## Prerequisites

- Node.js 20 LTS
- The API running (`dotnet run --project src/PortfolioTracker.API`)
- The Blazor dev server running (`dotnet run --project BlazorFrontend`)

## Setup (first time)

```bash
cd tests/e2e
npm install
npx playwright install chromium
```

## Running

```bash
npm test           # headless
npm run report     # open the HTML report
```

## Notes

- Tests run sequentially against a shared dev database; each run creates
  a uniquely-addressed test user so reruns stay independent.
- Login/register selectors are tied to form labels; update them if the
  auth pages change.
