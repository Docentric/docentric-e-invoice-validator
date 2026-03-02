# ZUGFeRD Mustang UI build automation

Download mustang-cli from Maven Central:
https://mvnrepository.com/artifact/org.mustangproject/Mustang-CLI
or directly from their site:
https://www.mustangproject.org/commandline/ ==> https://www.mustangproject.org/deploy/Mustang-CLI-2.22.0.jar

## What this solution does
This repository contains the front-end for the ZUGFeRD Mustang UI, a Vite + React + TypeScript application styled with Tailwind CSS and shadcn/ui components. The new automation scripts streamline getting from source code to the generated production HTML that ships in `dist/`. Each script installs the required Node.js dependencies (if needed) and triggers the framework's production build so you always end up with the final static assets.

## Why it was created
Delivering a reliable "final HTML" build previously required running ad-hoc commands. The added scripts remove guesswork by providing:

- **Cross-platform build coverage** &mdash; dedicated launchers for Linux/macOS (Bash), Windows PowerShell, and traditional Windows Command Prompt.
- **Repeatable environment setup** &mdash; a single initialisation script validates the Node.js toolchain and installs dependencies before the first build.
- **Documented workflow** &mdash; this README now explains the motivation, tooling, and usage for quick onboarding.

## Technology stack
- [Vite](https://vitejs.dev/) with the React SWC plugin for lightning-fast builds
- React 18 and TypeScript for type-safe UI development
- Tailwind CSS and shadcn/ui for design system components

## Prerequisites
- Node.js 18 or later (npm is bundled with Node.js)
- Access to a shell that matches your operating system (Bash, PowerShell, or Command Prompt)

## Initialise the tooling
Run the initialisation script once after cloning to verify Node.js and install all dependencies:

```bash
./scripts/init-tools.sh
```

The script checks that `node` and `npm` are on your `PATH`, reports their versions, and installs the dependencies defined in `package.json`/`package-lock.json`.

## Build the final HTML bundle
Use the script that matches your environment. Each command must be executed from the repository root (or with the appropriate relative path).

### Linux and macOS (Bash)
```bash
./scripts/build-linux.sh
```

### Windows PowerShell
```powershell
pwsh ./scripts/build.ps1
```

### Windows Command Prompt
```cmd
scripts\build.cmd
```

Every script ensures dependencies are present, invokes `npm run build`, and reports where the generated HTML and assets live (`dist/`).

## How the build works
Vite bundles the React application starting from `src/main.tsx`, resolves imports through the TypeScript compiler, and outputs an optimised static site. Tailwind CSS is processed through PostCSS using the Tailwind configuration in `tailwind.config.ts`, ensuring styles are tree-shaken. The resulting `dist/` directory contains the production-ready HTML, CSS, and JavaScript artefacts that can be deployed to any static host or integrated into larger systems.

## Local development (optional)
While the scripts focus on production output, you can still run the development server for iterative work:

```bash
npm run dev
```

This serves the application with hot module replacement at `http://localhost:5173` by default.
