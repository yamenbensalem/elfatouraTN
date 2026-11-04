using System.Collections.Concurrent;

namespace Web_GestCom.Services;

/// <summary>
/// Limiteur de débit en mémoire pour les formulaires publics anonymes (demande d'abonnement).
/// Fenêtre glissante par clé (typiquement l'adresse IP). Singleton, comme LoginProtectionService :
/// l'état est perdu au redémarrage du processus, ce qui est acceptable pour un simple frein anti-abus.
/// </summary>
public interface IPublicFormRateLimiter
{
    /// <summary>Enregistre une tentative et retourne false si la clé a déjà atteint <paramref name="maxParFenetre"/> dans la fenêtre.</summary>
    bool TryAcquire(string cle, int maxParFenetre, TimeSpan fenetre, DateTimeOffset maintenant);
}

public sealed class InMemoryPublicFormRateLimiter : IPublicFormRateLimiter
{
    private const int NettoyageAuDela = 10_000;

    private readonly ConcurrentDictionary<string, Queue<DateTimeOffset>> _tentatives = new();

    public bool TryAcquire(string cle, int maxParFenetre, TimeSpan fenetre, DateTimeOffset maintenant)
    {
        if (_tentatives.Count > NettoyageAuDela)
            Nettoyer(fenetre, maintenant);

        var file = _tentatives.GetOrAdd(cle, _ => new Queue<DateTimeOffset>());
        lock (file)
        {
            while (file.Count > 0 && maintenant - file.Peek() >= fenetre)
                file.Dequeue();

            if (file.Count >= maxParFenetre)
                return false;

            file.Enqueue(maintenant);
            return true;
        }
    }

    private void Nettoyer(TimeSpan fenetre, DateTimeOffset maintenant)
    {
        foreach (var (cle, file) in _tentatives)
        {
            bool vide;
            lock (file)
            {
                while (file.Count > 0 && maintenant - file.Peek() >= fenetre)
                    file.Dequeue();
                vide = file.Count == 0;
            }
            if (vide)
                _tentatives.TryRemove(cle, out _);
        }
    }
}
