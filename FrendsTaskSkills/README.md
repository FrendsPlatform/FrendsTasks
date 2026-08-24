# Frends Task Skills

[Agent Skills](https://agentskills.io) for building Frends Tasks, distributed as Claude Code plugins
through the marketplace defined in [`/.claude-plugin/marketplace.json`](/.claude-plugin/marketplace.json).

Skills package procedural knowledge — the Task Development Guidelines, the template workflow, the
analyzer rules — into a folder that an AI coding agent loads on demand when the task at hand matches.

## Available plugins

| Plugin | Contains | For |
|---|---|---|
| `frends-task-creator` | `frends-task-creator` | Building custom Tasks under your own vendor prefix |

## Installing

```
/plugin marketplace add FrendsPlatform/FrendsTasks
/plugin install frends-task-creator@frends-tasks
```

Task repositories generated from `FrendsTaskTemplate` get this automatically via their
`.claude/settings.json` — see [Automatic installation](#automatic-installation).

## Automatic installation

A task repository that ships this `.claude/settings.json` installs the marketplace and plugin for
anyone who clones it and trusts the folder:

```json
{
  "extraKnownMarketplaces": {
    "frends-tasks": {
      "source": { "source": "github", "repo": "FrendsPlatform/FrendsTasks" }
    }
  },
  "enabledPlugins": {
    "frends-task-creator@frends-tasks": true
  }
}
```

## Using the skills outside Claude Code

`SKILL.md` follows the open [Agent Skills standard](https://agentskills.io), so the folders work
unmodified in Codex, Cursor, Gemini CLI, Copilot and other adopting agents. Only the install
directory differs — copy the skill folder into `.agents/skills/` (vendor-neutral) or the agent's own
path. The plugin marketplace above is Claude Code specific.

## Adding a skill

1. Create the skill folder under a plugin directory: `<plugin>/skills/<skill-name>/SKILL.md`.
   Folder name must match the `name` in the frontmatter.
2. If it belongs to a new plugin, add `<plugin>/.claude-plugin/plugin.json` and a `plugins[]` entry
   in the marketplace manifest.
3. Bump `metadata.version` in the marketplace manifest.

Plugin `name` values are immutable slugs. Once published, renaming breaks every existing install with
a plugin-not-found error.