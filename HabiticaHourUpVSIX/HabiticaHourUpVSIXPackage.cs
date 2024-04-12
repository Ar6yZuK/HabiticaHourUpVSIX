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
	internal readonly string _vsixSettingsPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/Ar6yZuK/VSIX/{Vsix.Name}/";

	private readonly TimeSpan OneMinute = TimeSpan.FromMinutes(1d);
	private readonly TimeSpan OneDay = TimeSpan.FromDays(1d);

	private IAudioPlayer _soundPlayer;

	private SettingsInFile<SessionSettingsModel> LastSessionSettingsReader => new($"{_vsixSettingsPath}last_session.json", default);

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

		SessionSettingsReader = new SettingsInFile<SessionSettingsModel>($"{_vsixSettingsPath}current_session.json", default);
		// TODO: Maybe encrypt HabiticaCredentials somehow
		CredentialsSettings = new CachedSettingsInFile<HabiticaCredentials>($"{_vsixSettingsPath}credentials.json", new("", ""), timeOfCache);

		UserSettingsModel defaultUserSettings = new(TimeSpan.FromHours(1), "", IsAutoScoreUp: false, ShowErrorOnFailure: true, BeepOnSuccess: true, "");
		UserSettingsReader = new CachedSettingsInFile<UserSettingsModel>($"{_vsixSettingsPath}user_settings.json", defaultUserSettings, timeOfCache);
		UserSettingsReader.OnSaving += UserSettingsReader_OnSaving;

		HabiticaSettingsReader = new CachedSettingsInFile<HabiticaSettingsModel>($"{_vsixSettingsPath}local_settings.json", new(), timeOfCache);

		Timer = new MyTimer();
		Timer.Tick += Tick;

		HabiticaSettingsModel habiticaSettings = HabiticaSettingsReader.Read();
		UserSettingsModel vsSettings = UserSettingsReader.Read();

		TimeSpan tickAfter = vsSettings.Divisor;
		Timer.Change(tickAfter, vsSettings.Divisor);

		await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
		VS.Events.SolutionEvents.OnAfterCloseSolution += OnClose;

		var lastSessionModel = LastSessionSettingsReader.Read();
		await SetLastSessionIfUserAgreeAsync(lastSessionModel);
	}

	private async Task SetLastSessionIfUserAgreeAsync(SessionSettingsModel lastSessionModel)
	{
		TimeSpan lastWorkTimeLeft = GetLastWorkTimeOrDivisor(lastSessionModel, out bool lastWorkTimeLeftIsNull);

		if (lastWorkTimeLeft < OneMinute)
			return;

		DateTime? lastCloseDateTime = lastSessionModel.CloseDateTime;

		TimeSpan? lastCloseAgo = DateTime.Now - lastCloseDateTime;
		string lastCloseAgoFormat = @"hh\:mm\:ss";
		if (lastCloseAgo >= OneDay)
			lastCloseAgoFormat = @"dd\:hh\:mm\:ss";

		string setLastNextTickMessage = $"""
On last session next tick was: {(lastWorkTimeLeftIsNull ? $"{lastWorkTimeLeft}(divisor)" : lastWorkTimeLeft.ToString())}. 
Last session was on: {lastCloseDateTime?.ToString() ?? "?"}({lastCloseAgo?.ToString(lastCloseAgoFormat) ?? "?"} ago)
With ticks sent: {lastSessionModel.TicksSent} and ticks happened: {lastSessionModel.Ticks}
Do you want to load it?
""";

		var model = new InfoBarModel(setLastNextTickMessage, new[] { new InfoBarHyperlink("Click here to load") });

		var infoBar = await VS.InfoBar.CreateAsync(model);
		// if infoBar was null, maybe solution not loaded, then we wait for it.
		if (infoBar is null)
		{
			VS.Events.SolutionEvents.OnAfterOpenSolution += SolutionOpened;
			return;
			async void SolutionOpened(Solution? _)
			{
				VS.Events.SolutionEvents.OnAfterOpenSolution -= SolutionOpened;
				await SetLastSessionIfUserAgreeAsync(lastSessionModel);
			}
		}

		// Maybe close infoBar on click
		infoBar.ActionItemClicked += (s, e) =>
		{
			// TODO: Maybe make sum of Ticks and TicksSent
			this.SessionSettingsReader.Write(lastSessionModel);
			// Maybe add overload of ITimer/MyTimer.Change method without period parameter
			Timer.Change(lastWorkTimeLeft, UserSettingsReader.Read().Divisor);

			/*((InfoBar)s).Close();*/
		};

		await infoBar.TryShowInfoBarUIAsync();

		TimeSpan GetLastWorkTimeOrDivisor(SessionSettingsModel sessionModel, out bool lastWorkTimeLeftIsNull)
		{
			if (sessionModel.WorkTimeLeft.HasValue)
			{
				lastWorkTimeLeftIsNull = false;
				return sessionModel.WorkTimeLeft.Value;
			}

			lastWorkTimeLeftIsNull = true;
			return UserSettingsReader.Read().Divisor;
		}
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
		var lastSessionSettingsInFile = LastSessionSettingsReader;

		var session = SessionSettingsReader.Read() with { WorkTimeLeft = Timer.NextTick, CloseDateTime = DateTime.Now };

		// Saving in file last session at once, per close visual studio.
		lastSessionSettingsInFile.Write(session);
		// Save does nothing in SettingsInFile<>...
		//lastSessionSettingsInFile.Save();

		SessionSettingsReader.Write(default);
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