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

## Contributing

To contribute to the documentation:

1. Make your changes to the markdown files
2. Test locally with `npm run docs:dev`
3. Build to verify: `npm run docs:build`
4. Submit your changes
