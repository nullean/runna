# runna

The `Runna` (C#) and `Runna.Fs` (F#) packages are typed build target DSLs built on [argh](https://argh.nullean.net).
Targets are union cases (or DU cases in F#), dependencies run first, and every target gets Argh-style `--help`.

```bash
./build.cs --help
./build.cs test --filter Unit
./build.cs release -s        # skip the gates, run only the composed targets
./build.cs release --graph   # draw the plan and run nothing
./build.cs lint test pack    # several targets, in order, shared dependencies once
./build.cs release --skip docs
```

[**Getting started**](getting-started.md) - Install, and a first build in C# and in F#.

[**Choosing what runs**](running.md) - Several targets on one command line, `-s` and `--skip`.

[**Running targets side by side**](parallel.md) - Parallel stages, `-j`, `--keep-going`, and the output of targets
that run at once.

[**Conditions, ordering and cleanup**](modifiers.md) - `OnlyWhen`, `After`, `Cleanup`, and checks for environment
variables and tools before the run starts.

[**Drawing the plan**](graph.md) - `--graph` draws what a run would do, skipped targets included, and runs nothing.

[**Wrappers**](wrappers.md) - Generate one task per target for [mise](mise.md), [make](makefile.md) and
[just](just.md), and let mise hooks keep them up to date.

The package READMEs cover the target DSLs:

- [`Runna`](https://github.com/nullean/runna/blob/main/src/Nullean.Runna/README.md): C# targets as `union` cases,
  or properties on a `RunnaBuild` class.
- [`Runna.Fs`](https://github.com/nullean/runna/blob/main/src/Nullean.Runna.Fs/README.md): F# targets as DU
  cases, with `shell { }` target blocks.
