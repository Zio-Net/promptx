# PrmptMD

PrmptMD file format — .NET parsing library & VS Code extension.

## Structure

```
src/Zionet.Prompting/    # .NET NuGet library for parsing .prmpt.md files
vscode-extension/prmptmd-vscode/ # VS Code extension for syntax highlighting & diagnostics
schema/                          # Shared PrmptMD JSON schema
```

## Components

### Zionet.Prompting (NuGet)

A .NET library that provides:
- File-based prompt loading and resolution
- YAML frontmatter parsing and validation
- Chat body parsing (system/user/assistant/developer roles)
- Mustache-style variable extraction and rendering
- JSON Schema validation for `.prmpt.md` files

### PrmptMD VS Code Extension

A VS Code extension that provides:
- Syntax highlighting for `.prmpt.md` files
- Inline diagnostics for invalid frontmatter and body structure
- Language configuration (comments, brackets, etc.)

## Getting Started

### NuGet Package

```bash
dotnet add package Zionet.Prompting
```

### VS Code Extension

```bash
cd vscode-extension/prmptmd-vscode
npm install
npm run package
npm run install-local
```
