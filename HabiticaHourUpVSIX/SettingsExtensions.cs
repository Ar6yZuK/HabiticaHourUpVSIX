using HabiticaHourUpVSIX.AppSettings.Abstractions;
using HabiticaHourUpVSIX.AppSettings.Models;

namespace HabiticaHourUpVSIX;

// Need read before write, because there may be data that needs to be left untouched
public static class SettingsExtensions
{
	public static void SetLastTickAfterWithSave(this SettingsWithSaving<HabiticaSettingsModel> obj1, TimeSpan lastTickToSet)
	{
		HabiticaSettingsModel settingsRead = obj1.Read();
		var settingsToWrite = settingsRead with { LastTickAfter = lastTickToSet };

		obj1.Write(settingsToWrite);
		obj1.Save();
	}

	public static void SetTicksWithSave(this SettingsWithSaving<HabiticaSettingsModel> obj1, int ticksToSet)
	{
		HabiticaSettingsModel settingsRead = obj1.Read();
		var settingsToWrite = settingsRead with { TotalTicks = ticksToSet};

		obj1.Write(settingsToWrite);
		obj1.Save();
	}
	public static void SetDivisorWithSave(this SettingsWithSaving<UserSettingsModel> obj1, TimeSpan divisorToSet)
	{
		UserSettingsModel settingsRead = obj1.Read();
		var settingsToWrite = settingsRead with { Divisor = divisorToSet };

		obj1.Write(settingsToWrite);
		obj1.Save();
	}

	public static void SetTicks(this Settings<SessionSettingsModel> obj1, int ticksToSet)
	{
		SessionSettingsModel settingsRead = obj1.Read();
		var settingsToWrite = settingsRead with { Ticks = ticksToSet };

		obj1.Write(settingsToWrite);
	}
}
