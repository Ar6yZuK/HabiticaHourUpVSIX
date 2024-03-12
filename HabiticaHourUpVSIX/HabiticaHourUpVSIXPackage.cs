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
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
#nullable enable

namespace HabiticaHourUpVSIX;
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
[ProvideMenuResource("Menus.ctmenu", 1)]
[Guid(PackageGuids.HabiticaHourUpVSIXString)]
[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
[ProvideToolWindow(typeof(SettingsToolWindow.Pane))]
public sealed class HabiticaHourUpVSIXPackage : ToolkitPackage
{
	private readonly TimeSpan OneMinute = TimeSpan.FromMinutes(1d);
	private readonly TimeSpan OneDay = TimeSpan.FromDays(1d);

	private IAudioPlayer _soundPlayer;

	public SettingsWithSaving<HabiticaSettingsModel> HabiticaSettingsReader { get; private set; }
	public SettingsWithSaving<HabiticaCredentials> CredentialsSettings { get; private set; }
	public SettingsWithSaving<UserSettingsModel> UserSettingsReader { get; private set; }
	public Settings<SessionSettingsModel> SessionSettingsReader { get; private set; }
	public HabiticaClientBase HabiticaClient { get; private set; }
	public ITimer Timer { get; private set; }

	protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
	{
		await this.RegisterCommandsAsync();
		this.RegisterToolWindows();

		HabiticaClient = new HabiticaClient(this);
		_soundPlayer = new AudioPlayer();

		HabiticaClient.OnSuccessfullySend += delegate { SessionSettingsReader.AddSessionTicksSent(1); };
		HabiticaClient.OnSuccessfullySend += delegate
		{
			var userSettings = UserSettingsReader.Read();
			if (!userSettings.BeepOnSuccess)
				return;

			// UserSettingsReader is CachedSettings that means Play Beep can read from settings without performance decreasing
			PlayBeep();
		};

		// timeOfCache is expected time of load
		var timeOfCache = TimeSpan.FromSeconds(10);
		string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		string vsixSettingsPath = $"{appDataPath}/Ar6yZuK/VSIX/{Vsix.Name}/";

		SessionSettingsReader = new SessionSettings();
		// TODO: Maybe encrypt HabiticaCredentials somehow
		CredentialsSettings = new CachedSettingsInFile<HabiticaCredentials>($"{vsixSettingsPath}credentials.json", new("", ""), timeOfCache);

		UserSettingsModel defaultUserSettings = new(TimeSpan.FromHours(1), "", IsAutoScoreUp: false, ShowErrorOnFailure: true, BeepOnSuccess: true, "");
		UserSettingsReader = new CachedSettingsInFile<UserSettingsModel>($"{vsixSettingsPath}user_settings.json", defaultUserSettings, timeOfCache);
		UserSettingsReader.OnSaving += UserSettingsReader_OnSaving;

		HabiticaSettingsReader = new CachedSettingsInFile<HabiticaSettingsModel>($"{vsixSettingsPath}local_settings.json", new(), timeOfCache);

		Timer = new MyTimer();
		Timer.Tick += Tick;

		HabiticaSettingsModel habiticaSettings = HabiticaSettingsReader.Read();
		UserSettingsModel vsSettings = UserSettingsReader.Read();

		TimeSpan tickAfter = vsSettings.Divisor;
		Timer.Change(tickAfter, vsSettings.Divisor);

		await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
		VS.Events.SolutionEvents.OnAfterCloseSolution += OnClose;

		await SetLastTimerIfUserAgreeAsync(habiticaSettings, vsSettings.Divisor);
	}

	private async Task SetLastTimerIfUserAgreeAsync(HabiticaSettingsModel habiticaSettings, TimeSpan divisor)
	{
		var lastWorkTimeLeft = habiticaSettings.LastWorkTimeLeft;
		if (lastWorkTimeLeft < OneMinute)
			return;

		var lastCloseAgo = DateTime.Now - habiticaSettings.LastCloseDateTime;
		string lastCloseAgoFormat = @"hh\:mm\:ss";
		if (lastCloseAgo >= OneDay)
			lastCloseAgoFormat = @"dd\:hh\:mm\:ss";

		string setLastNextTickMessage = $"""
On last session next tick was: {lastWorkTimeLeft}. 
Do you want to load it?
Last session was on: {habiticaSettings.LastCloseDateTime}({lastCloseAgo.ToString(lastCloseAgoFormat)} ago)
""";

		var model = new InfoBarModel(setLastNextTickMessage, new[] { new InfoBarHyperlink("Click here to load") });

		var infoBar = await VS.InfoBar.CreateAsync(model);
		// if infoBar was null, maybe solution not loaded, then we wait for it.
		if (infoBar is null)
		{
			VS.Events.SolutionEvents.OnAfterOpenSolution += SolutionOpened;
			return;
			async void SolutionOpened(Solution? obj)
			{
				VS.Events.SolutionEvents.OnAfterOpenSolution -= SolutionOpened;
				// We read because there may have been changes in settings outside(settings file may be changed)
				await SetLastTimerIfUserAgreeAsync(HabiticaSettingsReader.Read(), UserSettingsReader.Read().Divisor);
			}
		}

		// Maybe close infoBar on click
		infoBar.ActionItemClicked += (s, e) => { Timer.Change(lastWorkTimeLeft, divisor); /*((InfoBar)s).Close();*/ };

		await infoBar.TryShowInfoBarUIAsync();
	}

	internal void PlayBeep()
	{
		var userSettings = UserSettingsReader.Read();

		_soundPlayer.AudioPath = userSettings.BeepAudioPath;
		if (string.IsNullOrWhiteSpace(userSettings.BeepAudioPath))
		{
			VS.MessageBox.ShowError($"{nameof(userSettings.BeepOnSuccess)} was enabled, but {nameof(userSettings.BeepAudioPath)} was empty");
			return;
		}
		if (!File.Exists(userSettings.BeepAudioPath))
		{
			VS.MessageBox.ShowError("Attempt to play audio with non-existent audio file");
			return;
		}

		_soundPlayer.Play();
	}

	private void Tick()
	{
		AddTicksToAllSettings(1);

		var userSettingsModel = UserSettingsReader.Read();
		if (!userSettingsModel.IsAutoScoreUp)
			return;

		JoinableTaskFactory.RunAsync(
			async delegate
			{
				const string ErrorTitleMessage = "Habitica score up failure\nPress cancel for do not show error again";

				// Set as no have error
				VSConstants.MessageBoxResult? mboxResult = null;
				try
				{
					var scoreUpResult = await HabiticaClient.SendOneTickAsync();
					if (scoreUpResult.TryPickT1(out var notSuccess, out _) && UserSettingsReader.Read().ShowErrorOnFailure)
					{
						mboxResult = await VS.MessageBox.ShowErrorAsync(ErrorTitleMessage, $"{notSuccess.Error}:\n{notSuccess.Message}");
						return;
					}
				}
				catch (Exception ex) when (UserSettingsReader.Read().ShowErrorOnFailure)
				{
					mboxResult = await VS.MessageBox.ShowErrorAsync(ErrorTitleMessage + "\nError will logged in output\nMaybe error with internet connection", ex.ToString());
					throw;
				}
				finally
				{
					if (mboxResult is VSConstants.MessageBoxResult.IDCANCEL)
						UserSettingsReader.SetShowErrorOnFailureWithSave(false);
				}
			}).FireAndForget();
	}
	private void OnClose()
	{
		HabiticaSettingsReader.SetWithSave(x => x.LastWorkTimeLeft, Timer.NextTick);
		HabiticaSettingsReader.SetWithSave(x => x.LastCloseDateTime, DateTime.Now);
	}

	private void UserSettingsReader_OnSaving(UserSettingsModel userSettingsModel)
	{
		Timer.Change(Timer.NextTick, userSettingsModel.Divisor);
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

	protected override void Dispose(bool disposing)
	{
		VS.Events.SolutionEvents.OnAfterCloseSolution -= OnClose;
		Timer?.Dispose();
		base.Dispose(disposing);
	}
}