using GestCom.Application.Common.Interfaces;

namespace GestCom.Infrastructure.Services;

/// <summary>
/// Service pour la gestion des dates et heures
/// </summary>
public class DateTimeService : IDateTimeService
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
}
