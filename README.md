<p align="center">
  <img src="docs/images/png/runna-lockup-544x278.png" width="420" alt="runna">
</p>

<p align="center">Typed build targets for .NET: your targets are the cases of a C# <code>union</code> or an F# DU, dependencies run first, and every target gets real <code>--help</code>.</p>

<p align="center">
  <a href="https://www.nuget.org/packages/Runna"><img src="https://img.shields.io/nuget/v/Runna?style=flat-square&label=Runna" alt="Runna on NuGet"></a>
  <a href="https://www.nuget.org/packages/Runna.Fs"><img src="https://img.shields.io/nuget/v/Runna.Fs?style=flat-square&label=Runna.Fs" alt="Runna.Fs on NuGet"></a>
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
dotnet add package Runna      # C#
dotnet add package Runna.Fs   # F#
```

## Quick start

The same three targets in either language: `test` needs `build`, which needs `clean`. It requires .NET 11.

### C#

A file-based app next to your solution is enough.

```csharp
#!/usr/bin/env -S dotnet --
#:package Runna@0.2.0
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
./build.cs test --graph
```

### F#

Targets are the cases of a DU, so a case you forget in `Bind` is a compiler warning (FS0025), and each target body is a
[Proc.Fs](https://www.nuget.org/packages/Proc.Fs) `shell { }` block. Put it in a small project, `build/build.fsproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net11.0</TargetFramework>
    <!-- the /// comments become each target's description in the help -->
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="Program.fs" />
    <PackageReference Include="Runna.Fs" Version="0.2.0" />
  </ItemGroup>
</Project>
```

```fsharp
open Nullean.Runna.Fs
open Proc.Fs

type Target =
    /// Delete build output
    | Clean
    /// Build the solution
    | Build
    /// Build, then run the tests
    | Test

[<EntryPoint>]
let main argv =
    let app = RunnaApp<Target>("build", None)

    app.Bind <| function
        | Clean -> target [] { sh $"dotnet clean" }
        | Build -> target [Clean] { sh $"dotnet build" }
        | Test  -> target [Build] { sh $"dotnet test" }

    app.RunAsync(argv).GetAwaiter().GetResult()
```

```sh
dotnet run --project build -- --help
dotnet run --project build -- test --graph
```

Both print the same help and plan. The package READMEs go further: typed per-target flags, namespaces, pipeline
commands, parallel stages and the [Runna](src/Nullean.Runna/README.md) / [Runna.Fs](src/Nullean.Runna.Fs/README.md)
specifics.

## Packages

| Package | What it is | |
|---|---|---|
| [`Runna`](src/Nullean.Runna/README.md) | C# build targets as the cases of a `union` | [![NuGet](https://img.shields.io/nuget/v/Runna?style=flat-square)](https://www.nuget.org/packages/Runna) |
| [`Runna.Fs`](src/Nullean.Runna.Fs/README.md) | F# build targets as DU cases | [![NuGet](https://img.shields.io/nuget/v/Runna.Fs?style=flat-square)](https://www.nuget.org/packages/Runna.Fs) |

## Learn more

- Documentation: https://runna.nullean.net
- Built on [argh](https://argh.nullean.net), the source-generated CLI framework
- [Releases](https://github.com/nullean/runna/releases)
- [Contributing](https://github.com/nullean/.github/blob/main/CONTRIBUTING.md)

MIT licensed. Copyright Nullean and contributors.
