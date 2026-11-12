namespace Nullean.Runna;

/// <summary>Thrown from a target body to fail the run with a message and optional exit code.</summary>
public sealed class RunnaException : Exception
{
	public int ExitCode { get; }

	public RunnaException(string message, int exitCode = 1) : base(message) => ExitCode = exitCode;
}
