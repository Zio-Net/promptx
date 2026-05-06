# PromptX

PromptX file format — .NET parsing library & VS Code extension.

## Structure

```
src/Zionet.Shared.Prompting/    # .NET NuGet library for parsing .promptx files
vscode-extension/promptx-vscode/ # VS Code extension for syntax highlighting & diagnostics
schema/                          # Shared PromptX JSON schema
```

## Components

### Zionet.Shared.Prompting (NuGet)

A .NET library that provides:
- File-based prompt loading and resolution
- YAML frontmatter parsing and validation
- Chat body parsing (system/user/assistant/developer roles)
- Mustache-style variable extraction and rendering
- JSON Schema validation for `.promptx` files

### PromptX VS Code Extension

A VS Code extension that provides:
- Syntax highlighting for `.promptx` files
- Inline diagnostics for invalid frontmatter and body structure
- Language configuration (comments, brackets, etc.)

## Getting Started

### NuGet Package

```bash
dotnet add package Zionet.Shared.Prompting
```

### VS Code Extension

```bash
cd vscode-extension/promptx-vscode
npm install
npm run package
npm run install-local
```
