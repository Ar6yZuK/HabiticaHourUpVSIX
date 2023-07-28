#nullable enable

namespace HabiticaHourUpVSIX.AppSettings.Models;

public record struct HabiticaSettingsModel(TimeSpan LastTickAfter, int TotalTicks);