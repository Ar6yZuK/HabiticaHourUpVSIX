global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using HabiticaHourUpVSIX.AppSettings;
using HabiticaHourUpVSIX.AppSettings.Abstractions;
using HabiticaHourUpVSIX.AppSettings.Models;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Threading;
using System.Runtime.InteropServices;
using System.Threading;
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
	public MyTimer Timer { get; private set; }
	
	public TimeSpan RealWorkTime => DateTime.Now - OpenTime;

	protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
	{
		await this.RegisterCommandsAsync();
		this.RegisterToolWindows();

		OpenTime = DateTime.Now;

		SessionSettingsReader = new SessionSettings();

		VSOptionsSettingsReader = new VSOptionsSettings(JoinableTaskFactory);
		VSOptionsSettingsReader.OnSaving += VSOptionsSettingsReader_OnSaving;

		HabiticaSettingsReader = new HabiticaSettings();
		HabiticaSettingsModel habiticaSettings = await HabiticaSettingsReader.ReadAsync();

		UserSettingsModel vsSettings = await VSOptionsSettingsReader.ReadAsync();
		Timer = new MyTimer(habiticaSettings.LastTickAfter, vsSettings.Divisor);
		Timer.Tick += Tick;

		await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
		VS.Events.SolutionEvents.OnAfterCloseSolution += OnClose;
	}

	private void Tick()
	{
		AddTicksToAllSettings(1);
	}

	private void VSOptionsSettingsReader_OnSaving(UserSettingsModel userSettingsModel) // UserSettingsModel(TimeSpan Divisor)
	{
		Timer.Change(Timer.NextTick, userSettingsModel.Divisor);
	}

	private void OnClose()
	{
		this.HabiticaSettingsReader.SetLastTickAfterWithSave(Timer.NextTick);
		// TODO: Show window with send ticks to habitica
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
	}
}