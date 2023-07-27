global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using HabiticaHourUpVSIX.AppSettings;
using HabiticaHourUpVSIX.AppSettings.Abstractions;
using HabiticaHourUpVSIX.AppSettings.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
#nullable enable

namespace HabiticaHourUpVSIX;
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
[ProvideMenuResource("Menus.ctmenu", 1)]
[Guid(PackageGuids.HabiticaHourUpVSIXString)]
//[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndNotBuildingAndNotDebugging_string, PackageAutoLoadFlags.BackgroundLoad)]
[ProvideOptionPage(typeof(AppSettings.OptionsProvider.General1Options), "HabiticaHourUpVSIX", "General", 0, 0, true, SupportsProfiles = true)]
[ProvideToolWindow(typeof(ToolWindow1.Pane))]
public sealed class HabiticaHourUpVSIXPackage : ToolkitPackage
{
	public SettingsWithSaving<HabiticaSettingsModel> HabiticaSettingsReader { get; private set; }
	public SettingsWithSaving<UserSettingsModel> VSOptionsSettingsReader { get; private set; }
	public Settings<SessionSettingsModel> SessionSettingsReader { get; private set; }
	public DateTime OpenTime { get; private set; }
	public FileLoggerProvider LoggerProvider { get; private set; }
	public MyTimer Timer { get; private set; }

	private ILogger<HabiticaHourUpVSIXPackage> _mainLogger;
	
	public TimeSpan RealWorkTime => DateTime.Now - OpenTime;

	protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
	{
		await this.RegisterCommandsAsync();
		this.RegisterToolWindows();

		OpenTime = DateTime.Now;

		LoggerProvider = new FileLoggerProvider($"{UserLocalDataPath}/Ar6yZuK/HabiticaHourUpVSIX/logs/log.gz");
		_mainLogger = LoggerProvider.CreateLogger<HabiticaHourUpVSIXPackage>();

		using IDisposable? scope = _mainLogger.BeginScope("Init");
		_mainLogger.LogInformation("Start");

		SessionSettingsReader = new SessionSettings();

		VSOptionsSettingsReader = new VSOptionsSettings(JoinableTaskFactory);
		VSOptionsSettingsReader.OnSaving += VSOptionsSettingsReader_OnSaving;

		HabiticaSettingsReader = new HabiticaSettings();
		HabiticaSettingsModel habiticaSettings = await HabiticaSettingsReader.ReadAsync();
		_mainLogger.LogInformation("Last work time read:{workTime}\n\tTotal ticks read:{ticks}", habiticaSettings.LastWorkTime, habiticaSettings.TotalTicks);

		UserSettingsModel vsSettings = await VSOptionsSettingsReader.ReadAsync();
		Timer = new MyTimer(habiticaSettings.LastWorkTime, vsSettings.Divisor, _mainLogger);
		Timer.TimerCallback += Tick;
		Timer.Start();

		await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
		VS.Events.SolutionEvents.OnAfterCloseSolution += OnClose;
		scope?.Dispose();
	}

	private void Tick()
	{
		using IDisposable? tickScope = _mainLogger.BeginScope(nameof(Tick));
		AddTicksToAllSettings(1);
	}

	private void VSOptionsSettingsReader_OnSaving(UserSettingsModel userSettingsModel) // UserSettingsModel(TimeSpan Divisor)
	{
		using IDisposable? scope = _mainLogger.BeginScope("Saving VS options");

		var timerSuccessful = Timer.SetTimer(RealWorkTime, userSettingsModel.Divisor, out long ticksCalculated); // timer Logs calculation in timer
		_mainLogger.LogDebug("Set timer successful:{successful}", timerSuccessful);

		if (ticksCalculated > 0)
			AddTicksToAllSettings((int)ticksCalculated);
	}

	private async void OnClose()
	{
		using IDisposable? scope = _mainLogger.BeginScope("OnAfterCloseSolution");
		_mainLogger.LogInformation("Work time per session:{workTime}", RealWorkTime);
		
		await JoinableTaskFactory.RunAsync(Closing);

		async Task Closing()
		{
			HabiticaSettingsModel habiticaSettings = await HabiticaSettingsReader.ReadAsync();
			UserSettingsModel vsSettings = await VSOptionsSettingsReader.ReadAsync();
			var wotc = new WorkingOpenTimeCounter(habiticaSettings.LastWorkTime, OpenTime, vsSettings.Divisor);

			var ticks = wotc.DivideLastWorkTime();
			HabiticaSettingsModel settingsToWrite = habiticaSettings with
			{
				LastWorkTime = wotc.WorkTime,
				TotalTicks = habiticaSettings.TotalTicks + (int)ticks
			};
			await HabiticaSettingsReader.WriteAsync(settingsToWrite);
			await HabiticaSettingsReader.SaveAsync();

			SessionSettingsModel sessionSettings = await SessionSettingsReader.ReadAsync();
			_mainLogger.LogDebug(
"""
Ticks per session:{sessionSettings}
	Saved total ticks:{ticks}
""", sessionSettings.Ticks, ticks);

			bool confirm = await VS.MessageBox.ShowConfirmAsync($"Send {settingsToWrite.TotalTicks} ticks to habitica?");
			_mainLogger.LogInformation("Confirm send tick to habitica:{confirm}", confirm);
			if(confirm)
			{
				await HabiticaSettingsReader.WriteAsync(settingsToWrite with { TotalTicks = 0 });
				await HabiticaSettingsReader.SaveAsync();
			}
		};
	}

	private void AddTicksToAllSettings(int addedTicks)
	{
		HabiticaSettingsModel habiticaSettings = HabiticaSettingsReader.Read();
		HabiticaSettingsModel habiticaSettingsToWrite = habiticaSettings with { TotalTicks = habiticaSettings.TotalTicks + addedTicks };
		HabiticaSettingsReader.Write(habiticaSettingsToWrite);

		SessionSettingsModel sessionSettings = SessionSettingsReader.Read();
		SessionSettingsModel sessionSettingsToWrite = sessionSettings with { Ticks = sessionSettings.Ticks + addedTicks };
		SessionSettingsReader.Write(sessionSettingsToWrite);

		HabiticaSettingsReader.Save();

		_mainLogger.LogInformation("Saved addition to ticks {addedTicks}:\n\tTotal ticks: {totalTicks} Session ticks: {sessionTicks}", 
			addedTicks, habiticaSettingsToWrite.TotalTicks, sessionSettingsToWrite.Ticks);
	}
}