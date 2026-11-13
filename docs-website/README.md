# FluentMigrator Documentation Website

This directory contains the VitePress documentation website for FluentMigrator.

## Development

### Prerequisites

- Node.js 20+
- npm

### Setup

```bash
cd docs-website
npm install
```

### Development Server

```bash
npm run docs:dev
```

The site will be available at `http://localhost:5173`

### Build

```bash
npm run docs:build
```

The built site will be in `.vitepress/dist/`

### Preview Built Site

```bash
npm run docs:preview
```

### Dependency overrides

VitePress 1.x depends on Vite 5, which (together with its bundled esbuild) has known
security advisories that are only fixed in Vite 6.4.6+. `package.json` therefore uses an
npm `overrides` entry to force a patched Vite 6 release. Remove the override once
VitePress ships a stable release that depends on a patched Vite on its own.

## Contributing

To contribute to the documentation:

1. Make your changes to the markdown files
2. Test locally with `npm run docs:dev`
3. Build to verify: `npm run docs:build`
4. Submit your changes
