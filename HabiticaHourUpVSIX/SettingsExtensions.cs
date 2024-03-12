using HabiticaHourUpVSIX.AppSettings.Abstractions;
using HabiticaHourUpVSIX.AppSettings.Models;

namespace HabiticaHourUpVSIX;

// Need read before write, because there may be data that needs to be left untouched
public static partial class SettingsExtensions
{
	public static void SetTotalTicksWithSave(this SettingsWithSaving<HabiticaSettingsModel> obj1, int ticksToSet)
	{
		HabiticaSettingsModel settingsRead = obj1.Read();
		var settingsToWrite = settingsRead with { TotalTicks = ticksToSet};

		obj1.Write(settingsToWrite);
		obj1.Save();
	}
	public static void ReduceTotalTicksWithSave(this SettingsWithSaving<HabiticaSettingsModel> obj1, int ticksToReduce)
	{
		HabiticaSettingsModel settingsRead = obj1.Read();
		var settingsToWrite = settingsRead with { TotalTicks = settingsRead.TotalTicks - ticksToReduce };

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
	public static void SetAutoSend(this SettingsWithSaving<UserSettingsModel> obj1, bool autoScoreUpToSet)
	{
		UserSettingsModel settingsRead = obj1.Read();
		var settingsToWrite = settingsRead with { IsAutoScoreUp = autoScoreUpToSet };

		obj1.Write(settingsToWrite);
		obj1.Save();
	}
	public static void SetTaskId(this SettingsWithSaving<UserSettingsModel> obj1, string taskIdToSet)
	{
		UserSettingsModel settingsRead = obj1.Read();
		var settingsToWrite = settingsRead with { TaskIDToScoreUp = taskIdToSet };

		obj1.Write(settingsToWrite);
		obj1.Save();
	}

	public static void SetSessionTicks(this Settings<SessionSettingsModel> obj1, int ticksToSet)
	{
		SessionSettingsModel settingsRead = obj1.Read();
		var settingsToWrite = settingsRead with { Ticks = ticksToSet };

		obj1.Write(settingsToWrite);
	}
	public static void SetSessionTicksSent(this Settings<SessionSettingsModel> obj1, int ticksToSet)
	{
		SessionSettingsModel settingsRead = obj1.Read();
		var settingsToWrite = settingsRead with { TicksSent = ticksToSet };

		obj1.Write(settingsToWrite);
	}
	public static void AddSessionTicksSent(this Settings<SessionSettingsModel> obj1, int ticksToAdd)
	{
		SessionSettingsModel settingsRead = obj1.Read();
		var settingsToWrite = settingsRead with { TicksSent = settingsRead.TicksSent + ticksToAdd };

		obj1.Write(settingsToWrite);
	}
	public static void SetShowErrorOnFailureWithSave(this SettingsWithSaving<UserSettingsModel> obj1, bool showError)
	{
		var settingsRead = obj1.Read();
		var settingsToWrite = settingsRead with { ShowErrorOnFailure = showError };

		obj1.Write(settingsToWrite);
		obj1.Save();
	}

	public static void SetUserIDWithSave(this SettingsWithSaving<HabiticaCredentials> obj1, string userId)
	{
		HabiticaCredentials settingsRead = obj1.Read();
		var settingsToWrite = settingsRead with { UserId = userId };

		obj1.Write(settingsToWrite);
		obj1.Save();
	}
	public static void SetApiKeyWithSave(this SettingsWithSaving<HabiticaCredentials> obj1, string apiKey)
	{
		HabiticaCredentials settingsRead = obj1.Read();
		var settingsToWrite = settingsRead with { ApiKey = apiKey };

		obj1.Write(settingsToWrite);
		obj1.Save();
	}
}
