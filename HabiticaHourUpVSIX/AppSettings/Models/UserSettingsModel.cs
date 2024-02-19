namespace HabiticaHourUpVSIX.AppSettings.Models;
public record struct UserSettingsModel(TimeSpan Divisor, string TaskIDToScoreUp, bool IsAutoScoreUp, bool ShowErrorOnFailure);