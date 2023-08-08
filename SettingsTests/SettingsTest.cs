using HabiticaHourUpVSIX.AppSettings.Abstractions;
using HabiticaHourUpVSIX.AppSettings.Models;

namespace SettingsTests;

public class Tests
{
	[Test]
	public void SessionTest()
	{
		// Cant be test because settings are internal
		throw new NotImplementedException();
		SettingsWithSaving<object, SessionSettingsModel> settings = null!;

		//var settings = new SessionSettings();

		bool invoked = false;

		SessionSettingsModel read = settings.Read();
		SessionSettingsModel valueToWrite = new(1);
		settings.Write(valueToWrite);
		SessionSettingsModel read2 = settings.Read();

		Assert.Multiple(() =>
		{
			Assert.That(invoked);
			Assert.That(read, Is.EqualTo(default(SessionSettingsModel)));
			Assert.That(read2, Is.EqualTo(valueToWrite));
		});
	}

	[Test]
	public void TestAddTicks()
	{
		// Cant be test because settings are internal
		throw new NotImplementedException();
		SettingsWithSaving<object, HabiticaSettingsModel> settings = null!;

		//var settings = new HabiticaSettings();

		bool invoked = false;
		settings.OnSaving += x => { invoked = true; Console.WriteLine(x); };

		HabiticaSettingsModel settingsRead = settings.Read();
		HabiticaSettingsModel settingsToWrite = settingsRead with { TotalTicks = settingsRead.TotalTicks + 1 };
		settings.Write(settingsToWrite);
		settings.Save();
		HabiticaSettingsModel settingsRead2 = settings.Read();

		Assert.Multiple(() =>
		{
			Assert.That(invoked);
			Assert.That(settingsRead, Is.EqualTo(default(HabiticaSettingsModel)));
			Assert.That(settingsRead2, Is.EqualTo(settingsToWrite));
		});

		settings.Write(default);
		settings.Save();
	}
}
public record struct Test(int W);
public record class TestClass(int W);