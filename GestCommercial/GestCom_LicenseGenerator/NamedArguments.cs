namespace GestCom_LicenseGenerator;

/// <summary>
/// Minimal <c>--name value</c> style parser for the <c>issue</c> subcommand — deliberately not a
/// CLI framework dependency, this tool only ever has a handful of flags.
/// </summary>
public sealed class NamedArguments
{
    private readonly Dictionary<string, string> _values;

    private NamedArguments(Dictionary<string, string> values)
    {
        _values = values;
    }

    public static NamedArguments Parse(IReadOnlyList<string> args, int startIndex)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var index = startIndex; index < args.Count; index += 2)
        {
            var flag = args[index];
            if (!flag.StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException($"Argument inattendu : {flag}");

            if (index + 1 >= args.Count)
                throw new ArgumentException($"L'argument {flag} attend une valeur.");

            values[flag[2..]] = args[index + 1];
        }

        return new NamedArguments(values);
    }

    public string Require(string name) =>
        _values.TryGetValue(name, out var value)
            ? value
            : throw new ArgumentException($"Argument obligatoire manquant : --{name}");

    public string? GetOrDefault(string name) => _values.GetValueOrDefault(name);
}
