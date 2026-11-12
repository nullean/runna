using Nullean.Runna.Discovery;
using Nullean.Runna.Execution;
using Nullean.Runna.Help;
using Nullean.Runna.Parsing;

namespace Nullean.Runna;

/// <summary>Entry point for Runna-based build scripts.</summary>
public static class RunnaApp
{
	/// <summary>
	/// Discovers all targets on <typeparamref name="TBuild"/>, parses <paramref name="args"/>, and executes the requested target.
	/// Returns an exit code suitable for passing to <c>Environment.Exit</c> or returning from top-level statements.
	/// </summary>
	public static async Task<int> Execute<TBuild>(string[] args) where TBuild : RunnaBuild, new()
	{
		TBuild build;
		BuildGraph graph;

		try
		{
			build = new TBuild();
			graph = BuildScanner.Scan(build);
			GraphValidator.Validate(graph);
		}
		catch (RunnaException ex)
		{
			Console.Error.WriteLine(ex.Message);
			return ex.ExitCode;
		}

		if (Wrappers.RunnaIntrinsics.TryRun(args, graph) is { } intrinsicExit)
			return intrinsicExit;

		var scriptName = DetectScriptName();

		if (args.Length == 0)
		{
			RunnaHelpPrinter.PrintRoot(graph, scriptName);
			return 0;
		}

		ParsedArgs parsed;
		try
		{
			parsed = ArgvParser.Parse(args, graph, build);
		}
		catch (RunnaException ex)
		{
			Console.Error.WriteLine(ex.Message);
			return ex.ExitCode;
		}

		if (parsed.ShowVersion)
		{
			var asm = typeof(TBuild).Assembly;
			var ver = asm.GetName().Version?.ToString() ?? "0.0.0";
			Console.WriteLine(ver);
			return 0;
		}

		if (parsed.ShowHelp)
		{
			if (string.IsNullOrEmpty(parsed.HelpTarget) || !graph.ByRoute.TryGetValue(parsed.HelpTarget, out var helpNode))
			{
				if (string.IsNullOrEmpty(parsed.HelpTarget) || !RunnaHelpPrinter.PrintNamespace(parsed.HelpTarget, graph, scriptName))
					RunnaHelpPrinter.PrintRoot(graph, scriptName);
			}
			else if (helpNode.Kind == TargetKind.Command)
				RunnaHelpPrinter.PrintCommand(helpNode, graph, scriptName);
			else
				RunnaHelpPrinter.PrintTarget(helpNode, graph, scriptName);
			return 0;
		}

		if (parsed.Target is null)
		{
			if (parsed.Graph is { } format)
				return DepGraphExecutor.DrawWholeGraph(graph, format);
			RunnaHelpPrinter.PrintRoot(graph, scriptName);
			return 0;
		}

		return await DepGraphExecutor.ExecuteAsync(parsed.Target, parsed, graph);
	}

	private static string DetectScriptName()
	{
		var entry = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "runna";
		return entry;
	}
}
