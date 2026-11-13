# Getting started

runna turns a set of build targets into a command line: each target is a type, dependencies run first, and every
target gets `--help` from its doc comment. It requires .NET 11.

```bash
dotnet add package Runna      # C#
dotnet add package Runna.Fs   # F#
```

Both examples below define the same three targets: `test` needs `build`, which needs `clean`.

## C#

A [file-based app](https://learn.microsoft.com/dotnet/core/sdk/file-based-apps) next to your solution is enough. Save
this as `build.cs`:

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

Targets are the cases of a C# `union`, one `record` each, and the `/// <summary>` becomes the target's description.
Make the file executable once, then run it like any CLI:

```bash
chmod +x build.cs
./build.cs --help
./build.cs test
```

`dotnet run build.cs -- test` works the same, on every platform.

## F#

F# targets are the cases of a DU, so a case you forget in `Bind` is a compiler warning (FS0025). Each target body is
a [Proc.Fs](https://www.nuget.org/packages/Proc.Fs) `shell { }` block. Put the build in a small project,
`build/build.fsproj`:

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

and `build/Program.fs`:

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

```bash
dotnet run --project build -- --help
dotnet run --project build -- test
```

## What it prints

Either one prints the same help, with each target's dependencies:

```text
build

  Usage:
    build <command|target> [options] [<target> [options]...]

  Targets:  (atomic steps — can also be run directly)
  clean    Delete build output
  build    Build the solution  (depends on: clean)
  test     Build, then run the tests  (depends on: build)
```

and `test --graph` draws what `test` would run, without running it:

```text
test  Build, then run the tests
└─ build
   └─ clean

Runs: clean → build → test
```

## Next

- [Choosing what runs](running.md): several targets on one command line, `-s` and `--skip`.
- [Drawing the plan](graph.md): `--graph` as a tree, Mermaid or JSON.
- [Running targets side by side](parallel.md): parallel stages, `-j` and `--keep-going`.
- [Conditions, ordering and cleanup](modifiers.md): `OnlyWhen`, `After`, `Cleanup`, `NeedsEnv` and `NeedsTool`.
- [Wrappers](wrappers.md): one task per target for mise, make and just.

The package READMEs cover the rest of the DSL, such as typed per-target flags, global options, namespaces and pipeline
commands: [`Runna`](https://github.com/nullean/runna/blob/main/src/Nullean.Runna/README.md) for C# and
[`Runna.Fs`](https://github.com/nullean/runna/blob/main/src/Nullean.Runna.Fs/README.md) for F#.
