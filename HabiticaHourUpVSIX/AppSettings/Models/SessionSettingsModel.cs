#nullable enable

namespace HabiticaHourUpVSIX.AppSettings.Models;

public record struct SessionSettingsModel(int Ticks, int TicksSent);
public record class SessionSettingsModelClass(int Ticks, int TicksSent)
{
	public static SessionSettingsModelClass Default { get; } = new SessionSettingsModelClass(0, 0);
}
