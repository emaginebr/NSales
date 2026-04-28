# Agents

Specialized agent definitions with scoped responsibilities
(per constitution Principle II).

- **Language**: English only.
- **Authoring**: Claude Code flat-file convention — each agent is a single
  Markdown file at `agents/<agent-name>.md` (kebab-case). Mandatory YAML
  frontmatter fields: `name`, `description`, `tools`. `model` is optional.
  Body states role, composed skills (by reference), and boundaries
  (out-of-scope deferral behavior).

Authoritative sources:
- [.specify/memory/constitution.md](../.specify/memory/constitution.md)
- [specs/001-repo-restructure-agents/contracts/agent-schema.md](../specs/001-repo-restructure-agents/contracts/agent-schema.md)
