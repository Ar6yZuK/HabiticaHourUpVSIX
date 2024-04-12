using CommunityToolkit.Mvvm.Input;
using HabiticaHourUpVSIX.AppSettings.Models;
using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
#nullable enable

namespace HabiticaHourUpVSIX;
public partial class SettingsWindow : UserControl, INotifyPropertyChanged
{
	private readonly HabiticaHourUpVSIXPackage _package;
	private readonly TimeSpanValidator _timeToTickValidator;
	private readonly TimeSpanValidator _divisorValidator;

	public event PropertyChangedEventHandler? PropertyChanged;

	public int TotalTicks
	{
		get => _package.HabiticaSettingsReader.Read().TotalTicks;
		set => _package.HabiticaSettingsReader.SetTotalTicksWithSave(value);
	}
	public int SessionTicks
	{
		get => _package.SessionSettingsReader.Read().Ticks;
		set => _package.SessionSettingsReader.SetSessionTicks(value);
	}
	public int SessionTicksSent
	{
		get => _package.SessionSettingsReader.Read().TicksSent;
		set => _package.SessionSettingsReader.SetSessionTicksSent(value);
	}
	public bool ShowErrorOnFailure
	{
		get => _package.UserSettingsReader.Read().ShowErrorOnFailure;
		// On notify it reads on get
		set => _package.UserSettingsReader.SetShowErrorOnFailureWithSave(value);
	}

	public TimeSpan TimeToTick
	{
		get => _package.Timer.NextTick;
		set
		{
			_timeToTickValidator.Value = value;
			_package.Timer.Change(_timeToTickValidator.Value, Divisor);
		}
	}

	public TimeSpan Divisor
	{
		get => _package.UserSettingsReader.Read().Divisor;
		set
		{
			_divisorValidator.Value = value;
			_package.UserSettingsReader.SetDivisorWithSave(_divisorValidator.Value);
		}
	}

	public bool IsAutoScoreUp
	{
		get => _package.UserSettingsReader.Read().IsAutoScoreUp;
		set
		{
			if (value && (HabiticaUserIdPasswordBox.Password.Length is 0
					|| HabiticaApiKeyPasswordBox.Password.Length is 0
					|| TaskIdToScoreUp.Length is 0)
				)
			{
				VS.MessageBox.ShowError($"Please set task ID, Habitica user ID, and Habitica user API-key.");
				return;
			}

			_package.UserSettingsReader.SetAutoSend(value);
		}
	}
	public string TaskIdToScoreUp
	{
		get => _package.UserSettingsReader.Read().TaskIDToScoreUp;
		set => _package.UserSettingsReader.SetTaskId(value);
	}

	public bool BeepOnSuccess
	{
		get => _package.UserSettingsReader.Read().BeepOnSuccess;
		set
		{
			if (value && string.IsNullOrEmpty(OnBeepAudioPath))
			{
				// Hardcode string, maybe later create custom control with property "Description"
				VS.MessageBox.ShowError("Please at first set beep-audio file path.");
				return;
			}

			_package.UserSettingsReader.SetWithSave(x => x.BeepOnSuccess, value);
		}
	}
	public string OnBeepAudioPath
	{
		get => _package.UserSettingsReader.Read().BeepAudioPath;
		set => _package.UserSettingsReader.SetWithSave(x => x.BeepAudioPath, value);
	}

	public SettingsWindow(HabiticaHourUpVSIXPackage habiticaHourUpVSIXPackage)
	{
		this.DataContext = this;
		_package = habiticaHourUpVSIXPackage;

		_package.HabiticaSettingsReader.OnSaving += 
			(HabiticaSettingsModel _) => 
			{
				OnPropertyChanged(nameof(TotalTicks));
			};
		_package.SessionSettingsReader.OnChanged += 
			(SessionSettingsModel _) => 
			{
				OnPropertyChanged(nameof(SessionTicksSent));
				OnPropertyChanged(nameof(SessionTicks));
			};

		_package.HabiticaClient.OnSuccessfullySend += delegate { OnPropertyChanged(nameof(SessionTicksSent)); };

		_package.UserSettingsReader.OnSaving += delegate { OnPropertyChanged(nameof(ShowErrorOnFailure)); };

		var maxTimeToTickValue = TimeSpan.FromMilliseconds(uint.MaxValue - 1);
		_timeToTickValidator = new(TimeToTick, minValue: TimeSpan.Zero, maxTimeToTickValue);
		var maxDivisorValue = TimeSpan.FromMilliseconds(uint.MaxValue - 1);
		_divisorValidator = new(Divisor, minValue: TimeSpan.FromSeconds(30), maxValue: maxDivisorValue);
		_package.Timer.Changed += delegate { OnPropertyChanged(nameof(TimeToTick)); };

		InitializeComponent();

		var credentials = _package.CredentialsSettings.Read();
		HabiticaApiKeyPasswordBox.Password = credentials.ApiKey;
		HabiticaUserIdPasswordBox.Password = credentials.UserId;

		HabiticaApiKeyPasswordBox.PasswordChanged += HabiticaApiKeyPasswordBox_PasswordChanged;
		HabiticaUserIdPasswordBox.PasswordChanged += HabiticaUserIdPasswordBox_PasswordChanged;
	}

	private void HabiticaUserIdPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
	{
		var passwordBox = (PasswordBox)sender;
		string userID = passwordBox.Password;
		_package.CredentialsSettings.SetUserIDWithSave(userID);
	}

	private void HabiticaApiKeyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
	{
		var passwordBox = (PasswordBox)sender;
		string apiKey = passwordBox.Password;
		_package.CredentialsSettings.SetApiKeyWithSave(apiKey);
	}

	[RelayCommand]
	private void ShowTimeToTick()
		=> OnPropertyChanged(nameof(TimeToTick));
	[RelayCommand]
	private void GetAudioBeepPathDialog()
	{
		var dialog = new OpenFileDialog
		{
			Multiselect = false,
			// Example: "Файлы рисунков (*.bmp, *.jpg)|*.bmp;*.jpg|Все файлы (*.*)|*.*"
			// If you need to empty description, set space before '|' otherwise you got empty file name.
			Filter = " |*.wav;*.mp3"
		};

		// ShowDialog return nullable bool
		if (dialog.ShowDialog() is true)
		{
			OnBeepAudioPath = dialog.FileName;
			OnPropertyChanged(nameof(OnBeepAudioPath));
		}
	}
	[RelayCommand]
	private void TestBeep()
		=> _package.PlayBeep();

	protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}