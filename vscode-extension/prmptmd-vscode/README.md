# Prompt File Tools

Syntax highlighting and validation for structured prompt files in Visual Studio Code.

This extension adds editor support for `.prmpt.md` files, including syntax highlighting,
YAML frontmatter awareness, role-block parsing for chat prompts, variable highlighting,
and inline diagnostics for common formatting and validation errors.

Once installed, any file ending in `.prmpt.md` is recognized with syntax highlighting and inline diagnostics.

## Features

- Dedicated `prmptmd` language for `.prmpt.md` files.
- YAML frontmatter highlighting.
- Chat role highlighting for `system:`, `user:`, `assistant:`, and `developer:` blocks.
- Variable highlighting for placeholders such as `{{question}}`.
- Diagnostics for invalid frontmatter fields, invalid `type` values, and malformed chat role markers.

## PrmptMD Syntax

A PrmptMD file has two parts:

1. A required YAML frontmatter block, delimited by `---` lines.
2. A body whose shape depends on the frontmatter `type`.

### Frontmatter

```prmptmd
---
description: Short human-readable description of what this prompt does.
type: chat
---
```

Allowed fields:

| Field         | Required | Allowed values        | Notes                                                |
| ------------- | -------- | --------------------- | ---------------------------------------------------- |
| `type`        | yes      | `prompt` or `chat`    | Determines how the body is parsed.                   |
| `description` | no       | any string            | Free-form description.                               |

The frontmatter must:

- Start on the very first line of the file with `---`.
- End with a closing `---` on its own line.
- Be valid YAML.

The prompt **name** comes from the containing folder name, and the prompt **version** comes from the file name (for example `v1.prmpt.md`). These are not declared in frontmatter.

### Body — `type: prompt`

The body is plain text. It may reference variables using `{{name}}`:

```prmptmd
---
description: Summarize a piece of text in a target word count.
type: prompt
---

Summarize the following text in {{maxWords}} words or fewer:

{{text}}
```

### Body — `type: chat`

The body is a sequence of role blocks. Each block starts with one of the following labels at column `0`, on its own line, immediately followed by `:`:

- `system:`
- `user:`
- `assistant:`
- `developer:`

Everything until the next role label belongs to that block. Roles may repeat. Anything before the first role label must be blank.

```prmptmd
---
description: Simple Q and A prompt with multiple role blocks.
type: chat
---

system:
You are a helpful assistant. Answer briefly and accurately.

developer:
Keep the answer concise and grounded in the prompt.

user:
{{question}}

assistant:
I can help with that.

user:
Keep the answer short.
```

### Variables

Variables use double curly braces and a simple identifier: `{{question}}`, `{{maxWords}}`, `{{text}}`.


## Diagnostics

The extension reports errors directly in the editor for:

- Missing or unterminated `---` frontmatter delimiters.
- Frontmatter that is not a YAML mapping or contains YAML syntax errors.
- Unknown frontmatter fields (only `description` and `type` are allowed).
- Use of removed fields like `name` or `version`, with a message explaining where the value now comes from.
- Missing or invalid `type` value.
- Chat prompts with no role block, with role labels indented past column `0`, or with unknown role labels.
- Non-blank content before the first role label in a chat prompt.

## Shared Source Of Truth

The canonical schema lives in `src/Zionet.Prompting/Assets/Schemas/prmptmd.schema.json`.

Keep editor validation rules in this extension aligned with that shared schema and the prompting library.

## Not Included

The extension does not currently provide:

- Completion items or snippets.
- Comments inside the prompt body.

## Build And Install

You can package the extension as a `.vsix` or publish it through the Visual Studio Code Marketplace.

Prerequisites:

- Node.js and `npm`.
- The VS Code `code` CLI on your `PATH` (in VS Code: *Shell Command: Install 'code' command in PATH*).

From the `vscode-extension/prmptmd-vscode` folder:

```bash
npm install
npm run package
code --install-extension prmptmd-vscode-0.4.0.vsix --force
```

Then reload VS Code (*Developer: Reload Window*) and open any `.prmpt.md` file to verify highlighting and diagnostics.

## License

MIT
