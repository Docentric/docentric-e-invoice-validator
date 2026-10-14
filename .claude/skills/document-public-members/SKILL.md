---
name: document-public-members
description: Add missing XML documentation (C#) or TSDoc (TypeScript) to public members in the selected files. Use when asked to document code. Never changes behavior.
---
Find public types and members in the selected file(s) that lack documentation and add it.

- C#: `<summary>`, plus `<param>`, `<returns>`, `<exception>`, `<typeparam>` where applicable; `<inheritdoc />` for overrides and interface implementations; document record parameters, enum values and constants.
- TypeScript: TSDoc on exported functions, hooks and components.
- Explain *why* and behavior, not what the name already says.
- Do not change code behavior.
