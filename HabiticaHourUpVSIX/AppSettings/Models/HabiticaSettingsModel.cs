#nullable enable

namespace HabiticaHourUpVSIX.AppSettings.Models;

public record struct HabiticaSettingsModel(TimeSpan LastWorkTimeLeft, DateTime LastCloseDateTime, int TotalTicks);