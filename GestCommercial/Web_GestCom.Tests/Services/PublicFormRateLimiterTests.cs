using Web_GestCom.Services;
using Xunit;

namespace Web_GestCom.Tests.Services;

public class PublicFormRateLimiterTests
{
    private static readonly TimeSpan Heure = TimeSpan.FromHours(1);
    private static readonly DateTimeOffset T0 = new(2026, 11, 4, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void TryAcquire_ShouldAllowUpToTheMaximumThenRefuse()
    {
        // Arrange
        var limiter = new InMemoryPublicFormRateLimiter();

        // Act
        var results = Enumerable.Range(0, 4).Select(i => limiter.TryAcquire("ip-1", 3, Heure, T0.AddMinutes(i))).ToList();

        // Assert
        Assert.Equal([true, true, true, false], results);
    }

    [Fact]
    public void TryAcquire_ShouldFreeASlotOnceTheWindowHasPassed()
    {
        // Arrange
        var limiter = new InMemoryPublicFormRateLimiter();
        limiter.TryAcquire("ip-1", 1, Heure, T0);

        // Act
        var dansLaFenetre = limiter.TryAcquire("ip-1", 1, Heure, T0.AddMinutes(59));
        var apresLaFenetre = limiter.TryAcquire("ip-1", 1, Heure, T0.AddMinutes(61));

        // Assert
        Assert.False(dansLaFenetre);
        Assert.True(apresLaFenetre);
    }

    [Fact]
    public void TryAcquire_ShouldTrackKeysIndependently()
    {
        // Arrange
        var limiter = new InMemoryPublicFormRateLimiter();
        limiter.TryAcquire("ip-1", 1, Heure, T0);

        // Act + Assert
        Assert.True(limiter.TryAcquire("ip-2", 1, Heure, T0));
        Assert.False(limiter.TryAcquire("ip-1", 1, Heure, T0));
    }

    [Fact]
    public void TryAcquire_WhenRefused_ShouldNotExtendTheBlock()
    {
        // Arrange — un refus ne compte pas comme une tentative : le robot qui insiste ne prolonge pas
        // indéfiniment le blocage d'un visiteur légitime qui partage la même IP.
        var limiter = new InMemoryPublicFormRateLimiter();
        limiter.TryAcquire("ip-1", 1, Heure, T0);
        limiter.TryAcquire("ip-1", 1, Heure, T0.AddMinutes(30));
        limiter.TryAcquire("ip-1", 1, Heure, T0.AddMinutes(50));

        // Act
        var apresLaPremiere = limiter.TryAcquire("ip-1", 1, Heure, T0.AddMinutes(61));

        // Assert
        Assert.True(apresLaPremiere);
    }
}
