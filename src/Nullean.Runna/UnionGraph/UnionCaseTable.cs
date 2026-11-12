using System.ComponentModel;

namespace Nullean.Runna.UnionGraph;

/// <summary>One leaf case of a <see cref="RunnaApp{TUnion}"/> union, registered by generated code.</summary>
/// <param name="Route">The CLI route, such as <c>["db", "migrate"]</c> for a case of a nested union.</param>
/// <param name="CaseType">The case's record type.</param>
/// <param name="Wrap">Wraps a case instance in <typeparamref name="TUnion"/>, through any nested unions.</param>
/// <param name="CreateDefault">The value passed to <c>Bind</c> when <see cref="RunnaApp{TUnion}"/> reads the graph.</param>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed record UnionCase<TUnion>(string[] Route, Type CaseType, Func<object, TUnion> Wrap, Func<TUnion> CreateDefault);

/// <summary>
/// The cases of <typeparamref name="TUnion"/>, registered at module initialization by the
/// <c>Nullean.Runna</c> source generator for every <c>new RunnaApp&lt;TUnion&gt;(...)</c> it sees.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class UnionCaseTable<TUnion>
{
	internal static IReadOnlyList<UnionCase<TUnion>>? Cases { get; private set; }
	internal static Func<TUnion, Type?>? LeafType { get; private set; }
	internal static Func<TUnion, object?>? LeafValue { get; private set; }

	/// <summary>Called by generated code. <paramref name="leafType"/> returns the innermost case type of a union value.</summary>
	public static void Register(UnionCase<TUnion>[] cases, Func<TUnion, Type?> leafType)
	{
		Cases = cases;
		LeafType = leafType;
	}

	/// <summary>
	/// Called by generated code. <paramref name="leafType"/> returns the innermost case type of a union value, and
	/// <paramref name="leafValue"/> the innermost case value itself.
	/// </summary>
	public static void Register(UnionCase<TUnion>[] cases, Func<TUnion, Type?> leafType, Func<TUnion, object?> leafValue)
	{
		Register(cases, leafType);
		LeafValue = leafValue;
	}

	internal static IReadOnlyList<UnionCase<TUnion>> Required =>
		Cases ??
			throw new RunnaException(
				$"No case table was generated for {typeof(TUnion).Name}. The Nullean.Runna source generator registers it " +
					$"for every 'new RunnaApp<{typeof(TUnion).Name}>(...)' in the project; make sure analyzers are enabled.",
				1
			);
}
