namespace GestCom.Shared.Exceptions;

/// <summary>
/// Exception levée quand une entité n'est pas trouvée
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"L'entité \"{name}\" ({key}) n'a pas été trouvée.")
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }
}
