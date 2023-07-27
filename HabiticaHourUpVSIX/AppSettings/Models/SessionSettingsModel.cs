#nullable enable

namespace HabiticaHourUpVSIX.AppSettings.Models;

public record struct SessionSettingsModel(int Ticks);
public record class SessionSettingsModelClass(int Ticks)
{
	public static SessionSettingsModelClass Default { get; } = new SessionSettingsModelClass(0);
}
