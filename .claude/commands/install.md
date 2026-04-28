---
description: Install skills, agents, and commands from a GitHub repository into the current project's .claude/ directory.
argument-hint: <user>/<repo>[@<branch>]
allowed-tools: Bash, Read
---

# /install — Install Claude Code artifacts from a GitHub repo

Install the artifact folders `skills/`, `agents/`, and `commands/` from a
GitHub repository into the current project's `.claude/` directory.

## Input

Argument: `$ARGUMENTS`

Accepted formats:

- `<user>/<repo>` — shorthand; branch defaults to `main` (e.g., `landim32/awesome-ai-skills`).
- `<user>/<repo>@<branch>` — explicit branch (e.g., `landim32/awesome-ai-skills@develop`).
- Full `https://github.com/<user>/<repo>(.git)` URL with optional `@<branch>` suffix.

If `$ARGUMENTS` is empty or does not parse as one of the above, ask the user
to provide a valid `<user>/<repo>` and stop.

## Execution

1. Parse `$ARGUMENTS` into `REPO_SLUG` (e.g., `landim32/awesome-ai-skills`) and `BRANCH` (default `main`).
2. Verify that `git` is available via `git --version`. If not, abort with a clear error message and stop.
3. Shallow-clone the repository into a temporary directory using a single Bash command:

   ```bash
   TEMP=$(mktemp -d -t claude-install-XXXXXX) && \
   git clone --depth 1 --branch "$BRANCH" --quiet "https://github.com/$REPO_SLUG.git" "$TEMP"
   ```

4. Ensure `.claude/` exists in the current working directory (`mkdir -p .claude`).
5. For each folder in the fixed list `skills`, `agents`, `commands`:
   - If `$TEMP/<folder>` exists and is not empty, create `.claude/<folder>/` if missing and copy its contents recursively (`cp -r "$TEMP/<folder>/." ".claude/<folder>/"`). Collisions MUST be overwritten; files that exist only in the destination MUST be preserved.
   - If `$TEMP/<folder>` is missing or empty, print a warning and skip it.
6. Remove the temporary directory (`rm -rf "$TEMP"`).

## Reporting

After completion, print a concise summary containing:

- The repository slug and branch that was cloned.
- For each of the three target folders: the count of files copied, or `skipped (not present in source)` if the folder was missing.
- The absolute path of the destination `.claude/` directory.

## Boundaries

- DO NOT touch folders other than `skills/`, `agents/`, `commands/`.
- DO NOT create any git commit; leave the changes as uncommitted working-tree edits for the user to review.
- DO NOT delete files in `.claude/` that have no counterpart in the source.
- DO NOT install `rules/` — it is not a standard Claude Code plugin artifact folder and is intentionally out of scope for this command.
- If the clone fails (invalid slug, non-existent branch, network error), abort and report the `git` exit message verbatim.
