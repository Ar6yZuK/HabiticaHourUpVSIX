using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabiticaHourUpVSIX.AppSettings.Abstractions;
using HabiticaHourUpVSIX.AppSettings.Models;
using Microsoft.Build.Framework.XamlTypes;
using Microsoft.Extensions.Logging;
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
	private readonly WorkingTimeCounterTimer _workingTimeCounter;
	private readonly ILogger<SettingsWindow> _logger;
	private readonly Timer _timer;

	public bool TicksEnabled
	{
		get => _habiticaHourUpVSIXPackage.Timer.Enabled;
		set => _ = value ? _habiticaHourUpVSIXPackage.Timer.Start() : _habiticaHourUpVSIXPackage.Timer.Stop();
	}
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
		get
		{
			using var scope = _logger.BeginScope("Time to tick Calculation");

			var timeToNextTick = _habiticaHourUpVSIXPackage.Timer.CalculateTickAfter(out var ticksCalculated);
			_logger.LogDebug("Ticks calculated:{tickCalculated}", ticksCalculated);
			return timeToNextTick;
		}
		set
		{
			var success = _habiticaHourUpVSIXPackage.Timer.TickAfter(value);
		}
	}
	private bool _timeToTickEdit;

	public bool IsTimeToTickEdit
	{
		get => _timeToTickEdit;
		set
		{
			if(_timeToTickEdit = value)
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
		_logger = _habiticaHourUpVSIXPackage.LoggerProvider.CreateLogger<SettingsWindow>();

		_workingTimeCounter = new WorkingTimeCounterTimer(_habiticaHourUpVSIXPackage.HabiticaSettingsReader.Read().LastWorkTime, Divisor);
		_habiticaHourUpVSIXPackage.HabiticaSettingsReader.OnSaving += Settings_OnSaving;
		_timer = new Timer(1000);
		_timer.Elapsed += _timer_Elapsed;

		InitializeComponent();
		IsTimeToTickEdit = false;
	}

	private void Settings_OnSaving(HabiticaSettingsModel arg)
	{
		this.OnPropertyChanged(nameof(TotalTicks));
	}
	private void _timer_Elapsed(object sender, ElapsedEventArgs e)
	{
		if (_habiticaHourUpVSIXPackage.Timer.Enabled)
		{
			this.OnPropertyChanged(nameof(TimeToTick));
		}
	}
}