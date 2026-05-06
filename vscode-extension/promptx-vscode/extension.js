const { languages, workspace, Range, Diagnostic, DiagnosticSeverity, Position } = require('vscode');
const { parseDocument, isMap, isScalar } = require('yaml');

const allowedFrontmatterKeys = new Set(['description', 'type']);
const allowedPromptTypes = new Set(['prompt', 'chat']);
const allowedRoleNames = new Set(['system', 'user', 'assistant', 'developer']);
const roleLinePattern = /^(system|user|assistant|developer):[ \t]*$/;
const indentedRolePattern = /^\s+(system|user|assistant|developer):[ \t]*$/;
const unknownRolePattern = /^([A-Za-z_][A-Za-z0-9_-]*):[ \t]*$/;

function activate(context) {
  const diagnostics = languages.createDiagnosticCollection('promptx');

  const validateDocument = document => {
    if (!isPromptxDocument(document)) {
      return;
    }

    diagnostics.set(document.uri, collectDiagnostics(document));
  };

  context.subscriptions.push(
    diagnostics,
    workspace.onDidOpenTextDocument(validateDocument),
    workspace.onDidChangeTextDocument(event => validateDocument(event.document)),
    workspace.onDidCloseTextDocument(document => diagnostics.delete(document.uri))
  );

  workspace.textDocuments.forEach(validateDocument);
}

function deactivate() {
}

function isPromptxDocument(document) {
  return document.languageId === 'promptx' || document.fileName.toLowerCase().endsWith('.promptx');
}

function collectDiagnostics(document) {
  const diagnostics = [];
  const split = splitFrontmatter(document, diagnostics);

  if (!split) {
    return diagnostics;
  }

  const frontmatter = validateFrontmatter(document, split, diagnostics);
  validateBody(document, split, frontmatter, diagnostics);
  return diagnostics;
}

function splitFrontmatter(document, diagnostics) {
  if (document.lineCount === 0 || document.lineAt(0).text !== '---') {
    diagnostics.push(createDiagnostic(
      firstLineRange(document),
      "PromptX file must start with '---' on the first line."
    ));
    return null;
  }

  let closingLine = -1;
  for (let lineIndex = 1; lineIndex < document.lineCount; lineIndex += 1) {
    if (document.lineAt(lineIndex).text === '---') {
      closingLine = lineIndex;
      break;
    }
  }

  if (closingLine < 0) {
    diagnostics.push(createDiagnostic(
      document.lineAt(0).range,
      "PromptX file is missing the closing '---' frontmatter delimiter."
    ));
    return null;
  }

  const yamlStart = closingLine > 0
    ? document.lineAt(1).range.start
    : document.lineAt(0).range.end;
  const yamlEnd = document.lineAt(closingLine).range.start;

  return {
    yamlText: document.getText(new Range(yamlStart, yamlEnd)),
    yamlBaseOffset: document.offsetAt(yamlStart),
    yamlFallbackRange: rangeOrLine(document, closingLine > 0 ? 1 : 0),
    bodyStartLine: closingLine + 1,
  };
}

function validateFrontmatter(document, split, diagnostics) {
  const parsed = parseDocument(split.yamlText, {
    prettyErrors: false,
    uniqueKeys: false,
  });

  for (const error of parsed.errors) {
    diagnostics.push(createDiagnostic(
      diagnosticRangeFromOffsets(document, split.yamlBaseOffset, error.pos, split.yamlFallbackRange),
      error.message
    ));
  }

  if (parsed.errors.length > 0) {
    return { type: undefined };
  }

  if (!isMap(parsed.contents)) {
    diagnostics.push(createDiagnostic(
      split.yamlFallbackRange,
      'Frontmatter must be a YAML mapping.'
    ));
    return { type: undefined };
  }

  const root = parsed.contents;
  const seenKeys = new Set();
  let promptType;

  for (const item of root.items) {
    const keyText = scalarString(item.key);
    if (!keyText) {
      continue;
    }

    seenKeys.add(keyText);

    if (!allowedFrontmatterKeys.has(keyText)) {
      diagnostics.push(createDiagnostic(
        nodeRange(document, split.yamlBaseOffset, item.key, split.yamlFallbackRange),
        createUnknownFrontmatterMessage(keyText)
      ));
      continue;
    }

    if (keyText === 'type') {
      promptType = validatePromptType(document, split, item.value, diagnostics);
    }
  }

  if (!seenKeys.has('type')) {
    diagnostics.push(createDiagnostic(split.yamlFallbackRange, "Frontmatter is missing required field 'type'."));
  }

  return { type: promptType };
}

function validatePromptType(document, split, node, diagnostics) {
  const value = scalarString(node);
  if (!allowedPromptTypes.has(value)) {
    diagnostics.push(createDiagnostic(
      nodeRange(document, split.yamlBaseOffset, node, split.yamlFallbackRange),
      "Invalid 'type' value. Expected one of: chat, prompt."
    ));
  }

  return value;
}

function createUnknownFrontmatterMessage(keyText) {
  if (keyText === 'name') {
    return "Frontmatter field 'name' is no longer supported. Prompt name comes from the containing folder.";
  }

  if (keyText === 'version') {
    return "Frontmatter field 'version' is no longer supported. Prompt version comes from the file name, for example v1.promptx.";
  }

  return `Unknown frontmatter field '${keyText}'. Allowed fields: description, type.`;
}

function validateBody(document, split, frontmatter, diagnostics) {
  if (frontmatter.type !== 'chat') {
    return;
  }

  let foundRole = false;
  let firstPreludeLine = null;

  for (let lineIndex = split.bodyStartLine; lineIndex < document.lineCount; lineIndex += 1) {
    const text = document.lineAt(lineIndex).text;

    if (roleLinePattern.test(text)) {
      foundRole = true;
      continue;
    }

    if (indentedRolePattern.test(text)) {
      diagnostics.push(createDiagnostic(
        document.lineAt(lineIndex).range,
        'Role blocks must start at column 0.'
      ));
      if (firstPreludeLine === null) {
        firstPreludeLine = lineIndex;
      }
      continue;
    }

    const unknownRole = text.match(unknownRolePattern);
    if (unknownRole && !allowedRoleNames.has(unknownRole[1])) {
      diagnostics.push(createDiagnostic(
        document.lineAt(lineIndex).range,
        `Unknown chat role '${unknownRole[1]}:'. Expected one of: system:, user:, assistant:, developer:.`
      ));
      if (firstPreludeLine === null) {
        firstPreludeLine = lineIndex;
      }
      continue;
    }

    if (!foundRole && text.trim() !== '' && firstPreludeLine === null) {
      firstPreludeLine = lineIndex;
    }
  }

  if (!foundRole) {
    diagnostics.push(createDiagnostic(
      rangeOrLine(document, split.bodyStartLine),
      'Chat prompt must contain at least one role block (system:, user:, assistant:, or developer:) at column 0.'
    ));
    return;
  }

  if (firstPreludeLine !== null) {
    diagnostics.push(createDiagnostic(
      document.lineAt(firstPreludeLine).range,
      'Everything before the first chat role line must be blank.'
    ));
  }
}

function scalarString(node) {
  if (!isScalar(node)) {
    return '';
  }

  return node.value == null ? '' : String(node.value);
}

function nodeRange(document, baseOffset, node, fallbackRange) {
  if (!node || !Array.isArray(node.range) || node.range.length < 2) {
    return fallbackRange;
  }

  return diagnosticRangeFromOffsets(document, baseOffset, node.range, fallbackRange);
}

function diagnosticRangeFromOffsets(document, baseOffset, offsets, fallbackRange) {
  if (!Array.isArray(offsets) || offsets.length < 2) {
    return fallbackRange;
  }

  const startOffset = Math.max(0, baseOffset + offsets[0]);
  const rawEndOffset = offsets.length > 2 ? offsets[2] : offsets[1];
  const endOffset = Math.max(startOffset, baseOffset + rawEndOffset);

  return new Range(
    document.positionAt(startOffset),
    document.positionAt(endOffset)
  );
}

function createDiagnostic(range, message) {
  return new Diagnostic(range, message, DiagnosticSeverity.Error);
}

function firstLineRange(document) {
  if (document.lineCount === 0) {
    return new Range(new Position(0, 0), new Position(0, 0));
  }

  const line = document.lineAt(0);
  return line.range.isEmpty ? new Range(line.range.start, line.range.start) : line.range;
}

function rangeOrLine(document, lineIndex) {
  if (document.lineCount === 0) {
    return new Range(new Position(0, 0), new Position(0, 0));
  }

  const safeIndex = Math.min(Math.max(lineIndex, 0), document.lineCount - 1);
  return document.lineAt(safeIndex).range;
}

module.exports = {
  activate,
  deactivate,
};