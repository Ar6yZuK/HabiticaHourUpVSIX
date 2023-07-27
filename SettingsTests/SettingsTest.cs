using HabiticaHourUpVSIX.AppSettings;
using HabiticaHourUpVSIX.AppSettings.Abstractions;
using HabiticaHourUpVSIX.AppSettings.Models;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Moq;

namespace SettingsTests;

public class Tests
{
	[Test]
	public async Task SessionTest()
	{
		var settings = new SessionSettings();

		bool invoked = false;

		SessionSettingsModel read = await settings.ReadAsync();
		SessionSettingsModel valueToWrite = new(1);
		await settings.WriteAsync(valueToWrite);
		SessionSettingsModel read2 = await settings.ReadAsync();

		Assert.Multiple(() =>
		{
			Assert.That(invoked);
			Assert.That(read, Is.EqualTo(default(SessionSettingsModel)));
			Assert.That(read2, Is.EqualTo(valueToWrite));
		});
	}

	[Test]
	public async Task TestAddTicks()
	{
		var settings = new HabiticaSettings();
		bool invoked = false;
		settings.OnSavingAsync += x => { invoked = true; Console.WriteLine(x); return Task.CompletedTask; };

		HabiticaSettingsModel settingsRead = await settings.ReadAsync();
		HabiticaSettingsModel settingsToWrite = settingsRead with { TotalTicks = settingsRead.TotalTicks + 1 };
		await settings.WriteAsync(settingsToWrite);
		await settings.SaveAsync();
		HabiticaSettingsModel settingsRead2 = await settings.ReadAsync();

		Assert.Multiple(() =>
		{
			Assert.That(invoked);
			Assert.That(settingsRead, Is.EqualTo(default(HabiticaSettingsModel)));
			Assert.That(settingsRead2, Is.EqualTo(settingsToWrite));
		});

		await settings.WriteAsync(default);
		await settings.SaveAsync();
	}
	[Test]
	public async Task Test()
	{
		
	}
}
public record struct Test(int W);
public record class TestClass(int W);