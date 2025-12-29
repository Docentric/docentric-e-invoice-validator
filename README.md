# Docentric ZuGFeRD & Factur-X Validator (Mustang Project .NET Test Tool)

A small test harness around the [Mustang Project](https://www.mustangproject.org/) CLI, exposing a REST API and a React-based UI for validating **ZuGFeRD** and **Factur-X** invoices (PDF and XML).

- **Backend**: ASP.NET Core minimal API (.NET 10)
- **Frontend**: React + Vite + TypeScript (shadcn/ui)
- **Validator Engine**: Mustang Project CLI (Java)

The goal of this tool is to make it trivial to validate sample documents interactively and via API calls.

---

## Architecture

The solution consists of two main parts:

- `src/Server`  
  ASP.NET Core REST API that:
  - Accepts a file upload (PDF or XML).
  - Calls `java -jar /opt/Mustang-CLI.jar` with `--action validate`.
  - Parses Mustang XML output to derive a simple validation status.
  - Returns a JSON response.

- `src/WebUI`  
  React + Vite SPA that:
  - Provides a UI to upload and validate documents.
  - Calls the REST API endpoints.
  - Is built into static assets and served from `Server/wwwroot` in production.

The Docker image bundles:

- .NET runtime
- OpenJDK (for the Mustang CLI)
- Downloaded `Mustang-CLI-{version}.jar` into `/opt/Mustang-CLI.jar`
- Published ASP.NET Core application

---

## Folder structure

```text
Mustang-Project-DotNet/
├─ README.md
├─ .gitignore
├─ .editorconfig
├─ Dockerfile
├─ docker-build.cmd
├─ docker-run.cmd
├─ src/
│  ├─ Server/
│  │  ├─ Docentric.EInvoice.Validator.RestServer.csproj
│  │  ├─ Program.cs
│  │  ├─ ApiHandlers.cs
│  │  ├─ Models/
│  │  ├─ Properties/
│  │  └─ wwwroot/             # built SPA (not tracked in Git)
│  └─ WebUI/
│     ├─ Docentric.EInvoice.Validator.WebUI.esproj
│     ├─ package.json
│     ├─ tsconfig*.json
│     ├─ eslint.config.js
│     ├─ tailwind.config.ts
│     ├─ vite.config.ts
│     ├─ index.html
│     └─ src/
└─ tests/
   └─ Docentric.EInvoice.Validator.Tests/ (optional, xUnit)
