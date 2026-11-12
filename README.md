<p align="center">
  <img src="docs/images/png/nullean-lockup-544x192.png" width="420" alt="runna">
</p>

<p align="center">Typed build targets for .NET: your targets are the cases of a C# <code>union</code> or an F# DU, dependencies run first, and every target gets real <code>--help</code>.</p>

<p align="center">
  <a href="https://www.nuget.org/packages/Nullean.Runna"><img src="https://img.shields.io/nuget/v/Nullean.Runna?style=flat-square" alt="NuGet"></a>
  <a href="https://github.com/nullean/runna/actions/workflows/ci.yml"><img src="https://img.shields.io/github/actions/workflow/status/nullean/runna/ci.yml?branch=main&style=flat-square&label=ci" alt="CI"></a>
  <a href="https://runna.nullean.net"><img src="https://img.shields.io/badge/docs-runna.nullean.net-20a0a0?style=flat-square" alt="Docs"></a>
</p>

```
$ ./build.cs release --graph
release  Verify gates, pack, generate notes, diff APIs
├─ requires
│  ├─ pristine-check
│  └─ test
└─ composes
   ├─ pkg generate
   ├─ pkg validate
   ├─ release-notes
   └─ api-changes

Runs: pristine-check → test → pkg generate → pkg validate → release-notes → api-changes
```

## Install

```sh
dotnet add package Nullean.Runna
```

## Quick start

A file-based app next to your solution is enough. It requires .NET 11.

```csharp
#!/usr/bin/env -S dotnet run --
#:package Nullean.Runna@0.1.0
#:property GenerateDocumentationFile=true
#:property NoWarn=CS1591
using Nullean.Runna;
using static Nullean.Runna.Shell;

var app = new RunnaApp<Target>("build");

app.Bind(t => t.Value switch
{
    Clean => app.Target(() => Exec("dotnet clean")),
    Build => app.Target([new Clean()], () => Exec("dotnet build")),
    Test  => app.Target([new Build()], () => Exec("dotnet test")),
    _ => throw new InvalidOperationException($"unmapped target {t.Value}")
});

return await app.RunAsync(args);

/// <summary>Delete build output</summary>
record Clean;
/// <summary>Build the solution</summary>
record Build;
/// <summary>Build, then run the tests</summary>
record Test;

union Target(Clean, Build, Test);
```

```sh
./build.cs --help
./build.cs test
./build.cs test --graph
```

## Packages

| Package | What it is | |
|---|---|---|
| [`Nullean.Runna`](src/Nullean.Runna/README.md) | C# build targets as the cases of a `union` | [![NuGet](https://img.shields.io/nuget/v/Nullean.Runna?style=flat-square)](https://www.nuget.org/packages/Nullean.Runna) |
| [`Nullean.Runna.Fs`](src/Nullean.Runna.Fs/README.md) | F# build targets as DU cases | [![NuGet](https://img.shields.io/nuget/v/Nullean.Runna.Fs?style=flat-square)](https://www.nuget.org/packages/Nullean.Runna.Fs) |

## Learn more

- Documentation: https://runna.nullean.net
- Built on [argh](https://argh.nullean.net), the source-generated CLI framework
- [Releases](https://github.com/nullean/runna/releases)
- [Contributing](https://github.com/nullean/.github/blob/main/CONTRIBUTING.md)

MIT licensed. Copyright Nullean and contributors.
