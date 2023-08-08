using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabiticaHourUpVSIX.AppSettings.Models;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
#nullable enable

namespace HabiticaHourUpVSIX;
[INotifyPropertyChanged]
public partial class SettingsWindow : UserControl
{
	private readonly HabiticaHourUpVSIXPackage _package;

	public int TotalTicks
	{
		get => _package.HabiticaSettingsReader.Read().TotalTicks;
		set => _package.HabiticaSettingsReader.SetTotalTicksWithSave(value);
	}
	public int SessionTicks
	{
		get => _package.SessionSettingsReader.Read().Ticks;
		set => _package.SessionSettingsReader.SetTicks(value);
	}

	private readonly TimeSpanValidator _timeToTickValidator;

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
		set => _package.UserSettingsReader.SetDivisorWithSave(value);
	}

	public bool IsAutoScoreUp
	{
		get => _package.UserSettingsReader.Read().IsAutoScoreUp;
		set
		{
			if (value)
				if (HabiticaUserIdPasswordBox.Password.Length is 0
					|| HabiticaApiKeyPasswordBox.Password.Length is 0 
					|| TaskIdToScoreUp.Length is 0)
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

	public SettingsWindow(HabiticaHourUpVSIXPackage habiticaHourUpVSIXPackage)
	{
		this.DataContext = this;
		_package = habiticaHourUpVSIXPackage;

		_package.HabiticaSettingsReader.OnSaving += Settings_OnSaving;

		var maxTimeToTickValue = TimeSpan.FromMilliseconds(uint.MaxValue - 1);
		_timeToTickValidator = new(TimeToTick, minValue:TimeSpan.Zero, maxTimeToTickValue);

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
		var userID = passwordBox.Password;
		_package.CredentialsSettings.SetUserIDWithSave(userID);
	}

	private void HabiticaApiKeyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
	{
		var passwordBox = (PasswordBox)sender;
		var apiKey = passwordBox.Password;
		_package.CredentialsSettings.SetApiKeyWithSave(apiKey);
	}

	private void Settings_OnSaving(HabiticaSettingsModel arg)
	{
		this.OnPropertyChanged(nameof(TotalTicks));
		this.OnPropertyChanged(nameof(SessionTicks));
	}

	[RelayCommand]
	private void ShowTimeToTick()
		=> OnPropertyChanged(nameof(TimeToTick));
}