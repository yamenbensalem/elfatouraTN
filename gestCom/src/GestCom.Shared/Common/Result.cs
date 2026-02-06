namespace GestCom.Shared.Common;

/// <summary>
/// Représente le résultat d'une opération avec succès/échec
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    protected Result(bool isSuccess, string error)
    {
        if (isSuccess && error != string.Empty)
            throw new InvalidOperationException("Un résultat réussi ne peut pas avoir d'erreur");
        if (!isSuccess && error == string.Empty)
            throw new InvalidOperationException("Un résultat échoué doit avoir un message d'erreur");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, string.Empty);
    public static Result<T> Failure<T>(string error) => new(default!, false, error);
}

/// <summary>
/// Résultat générique avec valeur de retour
/// </summary>
public class Result<T> : Result
{
    public T Value { get; }

    protected internal Result(T value, bool isSuccess, string error) : base(isSuccess, error)
    {
        Value = value;
    }
}
