# runna

Typed build targets for .NET: `Nullean.Runna` (C#, targets are the cases of a `union`) and `Nullean.Runna.Fs` (F#,
targets are DU cases), built on [argh](https://github.com/nullean/argh) for help and argument parsing. The docs site
(`docs/`, runna.nullean.net) is a docs-builder site published by `.github/workflows/gh-pages.yml`.

## Layout

- `src/Nullean.Runna`: the runtime. `RunnaApp<TUnion>` (union targets) and `RunnaApp.Execute<TBuild>` (a `RunnaBuild`
  class with `Target` properties), the dependency graph, the run plan, parallel stages, help, and the mise, make and
  just wrappers (`__wrappers sync|check`).
- `src/Nullean.Runna.Generator`: the source generator that registers each union's case table. It ships inside the
  `Nullean.Runna` package as an analyzer, so a `#:project` reference to `src/Nullean.Runna` does not run it; union
  targets need the package (or a project reference with `OutputItemType="Analyzer"`, as the tests use).
- `src/Nullean.Runna.Fs`: the F# binding over the runtime. `Nullean.Runna` grants it `InternalsVisibleTo` by public key,
  so both are signed with `build/keys/keypair.snk`.
- `examples/RunnaBuild`: a complete C# build script in the `RunnaBuild` class style; build it and try `--help` and `--graph`.
- `docs/`: the site, one page per topic. `docs/runna-landing.html` replaces the generated home page.

Argh comes from NuGet (`Nullean.Argh.Core` in `Directory.Packages.props`). runna uses only its public API: the help
formatting (`CliHelpFormatting`), fuzzy matching (`FuzzyMatch`) and `[Argument]`. A change that needs more from argh is
an argh release first.

## Commands

mise installs the SDK pinned in `global.json`. Run everything through it:

```bash
mise run build    # build the solution
mise run test     # build, then run the tests
mise run pack     # build, pack the NuGet packages, validate them, report public API changes
mise run ci       # exactly what CI's build job runs: test, pack, validate, diff the public API, then curb check
mise run docs     # build the docs as the deploy does (theme, landing page) and serve them; `-- --noserve` builds only
mise run format   # format the C# with curb
mise run check    # fail on formatting drift (curb) and workflow findings (zizmor)
mise run clean    # delete build output
mise run release  # show the release draft; `mise run release publish` releases
```

`mise tasks ls` lists them all. The tasks call the F# build in `build/scripts`, written with this repository's own
`Nullean.Runna.Fs` (a `ProjectReference`, so a change to it is exercised by the build too). Run a target directly with
`dotnet run --project build/scripts -- <target>`, and `-s` to skip what it requires. Packages and API reports go to
`build/output`.

`nupkg-validator` stays last in `dotnet-tools.json`: SDK 11's `dotnet tool restore` fails on a clean machine when
that RID-specific tool package comes before `assembly-differ`.

In `build/scripts`, commands are Proc.Fs: a target's body is `target [deps] { sh $"…" }`, captures are `text`/`lines`,
and helpers that compute a value are `shell { }` blocks. No `ProcNet` calls, no `exec "dotnet" [list]` wrappers.

## Releasing

release-drafter keeps a draft release up to date from the labels of merged pull requests. **Publishing the draft**
releases: it creates the tag (`0.1.0`, no `v`), the `release` job in `ci.yml` packs at that version once main's CI has
passed on that commit, and the `publish` job pushes to nuget.org and appends the public API reports
(`build/output/breaking-changes-*.md`) to the release notes. The push has to run in a job of `ci.yml` itself: nuget.org's
trusted publishing policy rejects a token issued to the shared workflow. Pushing a tag on its own does nothing.
`mise run release` shows the draft. The [releasing](.claude/skills/releasing/SKILL.md) and
[breaking-changes](.claude/skills/breaking-changes/SKILL.md) skills have the details. Every pull request carries
exactly one release label.

## Testing

Both test projects are TUnit with AwesomeAssertions, on Microsoft.Testing.Platform (`global.json` selects it). Tests
that swap `Console.Out` are `[NotInParallel]`. The F# tests discover by reflection (F# has no source generators).

```bash
dotnet test -c Release
dotnet test --project tests/Nullean.Runna.Tests -c Release --treenode-filter "/*/*/WrapperTests/*"
```

## Rules

- The word "make" in this repository means GNU make (the `make` wrapper, `runna.mk`). The tool is runna.
- Every package's skill is `src/<Project>/skills/<name>/SKILL.md`; write it from the code and compile its snippets.
- The build uses `<UseArtifactsOutput>true</UseArtifactsOutput>`: outputs go to `.artifacts/bin/<Project>/` and
  `.artifacts/obj/`. Do not reference `bin/` or `obj/` paths.
