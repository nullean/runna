using System.Reflection;

namespace Nullean.Runna;

internal static class RunnaVersion
{
	/// <summary>The informational version of the build script's assembly, as Argh's own <c>--version</c> prints it.</summary>
	public static string OfEntryAssembly(Assembly? fallback = null)
	{
		var assembly = Assembly.GetEntryAssembly() ?? fallback;
		if (assembly is null)
			return "0.0.0";
		return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? assembly.GetName().Version?.ToString() ?? "0.0.0";
	}
}
