namespace Nullean.Runna;

/// <summary>Handle to a global option declared on a <see cref="RunnaBuild"/> subclass. Obtain instances from <see cref="RunnaContext"/> to read values at execution time.</summary>
public interface IOptionRef<out T>
{
	internal string Long { get; }
	internal string? Short { get; }
	internal string? Description { get; }
	internal T DefaultValue { get; }
}
