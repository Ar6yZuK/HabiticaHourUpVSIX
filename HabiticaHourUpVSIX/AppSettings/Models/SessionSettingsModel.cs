#nullable enable

namespace HabiticaHourUpVSIX.AppSettings.Models;

public record struct SessionSettingsModel(TimeSpan? WorkTimeLeft, DateTime? CloseDateTime, int Ticks, int TicksSent);
public record class SessionSettingsModelClass(TimeSpan? WorkTimeLeft, DateTime? CloseDateTime, int Ticks, int TicksSent);
