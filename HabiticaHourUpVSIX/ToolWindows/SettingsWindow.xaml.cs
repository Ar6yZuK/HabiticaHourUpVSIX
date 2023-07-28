using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabiticaHourUpVSIX.AppSettings.Abstractions;
using HabiticaHourUpVSIX.AppSettings.Models;
using Microsoft.Build.Framework.XamlTypes;
using Microsoft.VisualStudio.Threading;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
#nullable enable

namespace HabiticaHourUpVSIX;
[INotifyPropertyChanged]
public partial class SettingsWindow : UserControl
{
	private readonly HabiticaHourUpVSIXPackage _habiticaHourUpVSIXPackage;
	private readonly Timer _timer;

	public int TotalTicks
	{
		get => _habiticaHourUpVSIXPackage.HabiticaSettingsReader.Read().TotalTicks;
		set => _habiticaHourUpVSIXPackage.HabiticaSettingsReader.SetTicksWithSave(value);
	}
	public int SessionTicks
	{
		get => _habiticaHourUpVSIXPackage.SessionSettingsReader.Read().Ticks;
		set => _habiticaHourUpVSIXPackage.SessionSettingsReader.SetTicks(value);
	}
	public TimeSpan TimeToTick
	{
		get => _habiticaHourUpVSIXPackage.Timer.NextTick;
		set => _habiticaHourUpVSIXPackage.Timer.Change(value, Divisor);
	}
	private bool _timeToTickEdit;

	public bool IsTimeToTickEdit
	{
		get => _timeToTickEdit;
		set
		{
			if (_timeToTickEdit = value)
				_timer.Stop();
			else
				_timer.Start();
		}
	}
	public TimeSpan Divisor
	{
		get => _habiticaHourUpVSIXPackage.VSOptionsSettingsReader.Read().Divisor;
		set => _habiticaHourUpVSIXPackage.VSOptionsSettingsReader.SetDivisorWithSave(value);
	}

	public SettingsWindow(HabiticaHourUpVSIXPackage habiticaHourUpVSIXPackage)
	{
		this.DataContext = this;
		_habiticaHourUpVSIXPackage = habiticaHourUpVSIXPackage;

		_timer = new Timer(interval:1000);
		_timer.Elapsed += (s, e) => OnPropertyChanged(nameof(TimeToTick));

		_habiticaHourUpVSIXPackage.HabiticaSettingsReader.OnSaving += Settings_OnSaving;

		InitializeComponent();
	}

	private void Settings_OnSaving(HabiticaSettingsModel arg)
	{
		this.OnPropertyChanged(nameof(TotalTicks));
		this.OnPropertyChanged(nameof(SessionTicks));
	}
}