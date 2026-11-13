# make

`__wrappers sync` writes `.runna/runna.mk`, a GNU make include with one phony target per runna target. Include it from
the Makefile:

```makefile
# Makefile
-include .runna/runna.mk
```

The `-` makes a checkout without the file still parse.

```bash
make test ARGS="--filter Unit"
make pkg/generate
```

To write only this file:

```bash
./build.cs __wrappers sync --only make
```

## The generated targets

```makefile
NULLEAN_RUNNA_RUN ?= dotnet run build.cs --

# Pack the NuGet packages
pkg/generate:
	@$(NULLEAN_RUNNA_RUN) pkg generate $(ARGS)
```

- Arguments go in `ARGS`. The shell parses them, so `ARGS="--filter 'A B'"` passes `A B` as one value.
- `NULLEAN_RUNNA_RUN` is set with `?=`, so a Makefile or the environment can override it.
- The include keeps the Makefile's default goal. Including it at the top does not make a bare `make` run a target.
- `make test pack` starts the script twice, and each run resolves its own dependencies.

See [Wrappers](wrappers.md) for the options and the task names.
