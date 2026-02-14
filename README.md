# CalTwo

A simple calculator built with F#, Fable, Elmish, and Feliz.

## Features

- Basic arithmetic operations (add, subtract, multiply, divide)
- Keyboard and mouse input support
- Accessible (WCAG 2.1 Level A compliant)
- Deployed to GitHub Pages

## Demo

Live Demo: https://ohama.github.io/CalTwo/

## Prerequisites

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- [Node.js 20+ (LTS)](https://nodejs.org/)

## Setup

Clone the repository and install dependencies:

```bash
git clone https://github.com/ohama/CalTwo.git
cd CalTwo

# Restore .NET tools
dotnet tool restore

# Install dependencies
npm install

# Start dev server
npm run dev
```

Visit http://localhost:5173/

## Testing

Run tests locally before committing:

```bash
# Unit tests
npm test

# E2E tests
npm run test:e2e

# All tests
npm run test:all
```

## Building

Build for production and preview locally:

```bash
# Production build
npm run build

# Preview production build
npm run preview
```

## Deployment

Automatically deploys to GitHub Pages on push to `main` via GitHub Actions.

## Accessibility

- Keyboard navigation (Tab, Enter, Space, Arrow keys)
- Screen reader support (ARIA labels)
- Visible focus indicators for keyboard users
- WCAG 2.1 Level A compliant

## Tech Stack

- **F#** - Programming language
- **Fable** - F# to JavaScript compiler
- **Elmish** - MVU (Model-View-Update) architecture
- **Feliz** - React DSL for F#
- **Vite** - Build tool and dev server
- **React** - UI rendering

## License

MIT

## Contributing

Pull requests welcome! Please run `npm run test:all` before submitting.
