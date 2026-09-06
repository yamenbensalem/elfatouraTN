using GestCom_LicenseGenerator;

var exitCode = args.Length switch
{
    0 => Commands.PrintUsage(),
    _ => args[0].ToLowerInvariant() switch
    {
        "collect" => Commands.Collect(args),
        "issue" => Commands.Issue(args),
        _ => Commands.PrintUsage(),
    },
};

return exitCode;
