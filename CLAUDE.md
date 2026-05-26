# CLAUDE.md — dotnet-otel

## Language and spelling

Use U.S. English in all prose, comments, commit messages, and docs.

## Code conventions

- Target **net10.0**; use latest C# language features where they improve clarity.
- `Nullable` and `ImplicitUsings` enabled on all projects.
- No inline XML comments on code that is self-explanatory. Add `<summary>` XML docs on all `internal` and `public` types, constructors, methods, and non-trivial fields.
- Use **source-generated logging** (`[LoggerMessage]`) for all `ILogger` calls — never `LogInformation(...)` directly (CA1873).
- Split large partial classes by concern: e.g. `Foo.cs` for logic, `Foo.Logging.cs` for `[LoggerMessage]` declarations.
- No defensive null-checks on DI-injected dependencies — trust the container.
- No comments that restate what the code already says.

## Configuration

- Secrets (API keys) go in **user secrets**, never in `appsettings.json`.
- Endpoint overrides go in `appsettings.json` with sensible defaults in code.
- `Properties/launchSettings.json` sets `DOTNET_ENVIRONMENT=Development`.

## Git commits

- Subject line: ≤ 50 characters, imperative mood, no period.
- Body (optional): wrap at 72 characters, explain *why* not *what*.
- Total commit message: **under 50 words**.
- Always add `Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>` when AI-assisted.

## Pull request descriptions

- **Under 200 words.**
- Include: what changed, why, and a short test/verification note.
- No filler phrases ("this PR...", "in this change...").
