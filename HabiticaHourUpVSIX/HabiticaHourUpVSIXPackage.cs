global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using HabiticaHourUpVSIX.AppSettings;
using HabiticaHourUpVSIX.AppSettings.Abstractions;
using HabiticaHourUpVSIX.AppSettings.Models;
using HabiticaHourUpVSIX.Habitica.Abstractions;
using HabiticaHourUpVSIX.ToolWindows;
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
[ProvideToolWindow(typeof(SettingsToolWindow.Pane))]
public sealed class HabiticaHourUpVSIXPackage : ToolkitPackage
{
	public SettingsWithSaving<HabiticaSettingsModel> HabiticaSettingsReader { get; private set; }
	public SettingsWithSaving<HabiticaCredentials> CredentialsSettings { get; private set; }
	public SettingsWithSaving<UserSettingsModel> UserSettingsReader { get; private set; }
	public Settings<SessionSettingsModel> SessionSettingsReader { get; private set; }
	public HabiticaClientBase HabiticaClient { get; private set; }
	public MyTimer Timer { get; private set; }

	public event Action<bool>? ShowErrorChangedEvent;

	protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
	{
		await this.RegisterCommandsAsync();
		this.RegisterToolWindows();

		HabiticaClient = new HabiticaClient(this);

		HabiticaClient.OnSuccessfullySend += delegate { SessionSettingsReader.AddSessionTicksSent(1); };

		SessionSettingsReader = new SessionSettings();
		CredentialsSettings = new CredentialsSettings();

		UserSettingsReader = new UserSettings();
		UserSettingsReader.OnSaving += VSOptionsSettingsReader_OnSaving;

		HabiticaSettingsReader = new HabiticaSettings();
		HabiticaSettingsModel habiticaSettings = HabiticaSettingsReader.Read();

		UserSettingsModel vsSettings = UserSettingsReader.Read();

		Timer = new MyTimer();
		Timer.Tick += Tick;

		TimeSpan tickAfter = habiticaSettings.LastWorkTime <= TimeSpan.Zero ? vsSettings.Divisor : habiticaSettings.LastWorkTime;

		Timer.Change(tickAfter, vsSettings.Divisor);

		HabiticaSettingsReader.SetLastTickAfterWithSave(tickAfter);

		await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
		VS.Events.SolutionEvents.OnAfterCloseSolution += OnClose;
	}

	private void Tick()
	{
		AddTicksToAllSettings(1);

		var userSettingsModel = UserSettingsReader.Read();
		if (userSettingsModel.IsAutoScoreUp)
		{
			JoinableTaskFactory.RunAsync(
				async delegate
				{
					const string ErrorTitleMessage = "Habitica score up failure\nPress cancel for do not show error again";

					// Set as no have error
					VSConstants.MessageBoxResult? mboxResult = null;
					try
					{
						var scoreUpResult = await HabiticaClient.SendOneTickAsync();
						if (scoreUpResult.TryPickT1(out var notSuccess, out _) && SessionSettingsReader.Read().ShowError)
						{
							mboxResult = await VS.MessageBox.ShowErrorAsync(ErrorTitleMessage, $"{notSuccess.Error}:\n{notSuccess.Message}");
							return;
						}
					}
					catch (Exception ex) when(SessionSettingsReader.Read().ShowError)
					{
						mboxResult = await VS.MessageBox.ShowErrorAsync(ErrorTitleMessage + "\nError will logged in output\nMaybe error with internet connection", ex.ToString());
						throw;
					}
					finally
					{
						if(mboxResult is VSConstants.MessageBoxResult.IDCANCEL)
						{
							SessionSettingsReader.SetShowError(false);
							// Use SetShowErrorAndNotify for notify SettingsWindow about changes
							// TODO: Maybe change Settings class, adding event about changing property.
							SetShowErrorAndNotify(false);
						}
					}
				}).FireAndForget();
		}
	}

	private void VSOptionsSettingsReader_OnSaving(UserSettingsModel userSettingsModel)
	{
		Timer.Change(Timer.NextTick, userSettingsModel.Divisor);
	}

	private void OnClose()
	{
		this.HabiticaSettingsReader.SetLastTickAfterWithSave(Timer.NextTick);
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

	public void SetShowErrorAndNotify(bool newValue)
	{
		SessionSettingsReader.SetShowError(newValue);
		ShowErrorChangedEvent?.Invoke(newValue);
	}
}