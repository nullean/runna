namespace Nullean.Runna;

/// <summary>
/// Base class for nested CLI namespace classes on a <see cref="RunnaBuild"/> subclass.
/// Declare a public instance property of the nested type on <see cref="RunnaBuild"/> to register it.
/// The CLI segment name is derived from the property name (PascalCase → kebab-case).
/// <example>
/// <code>
/// class Build : RunnaBuild
/// {
///     public Schema Schema { get; } = new();
///
///     public class Schema : Namespace
///     {
///         public Target Update { get; } = _ => _.Executes(() => { });
///     }
/// }
/// </code>
/// </example>
/// </summary>
public abstract class Namespace
{
}
