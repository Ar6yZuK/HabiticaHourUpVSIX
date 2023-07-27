using Microsoft.Extensions.Logging;
using System.Threading;
#nullable enable

namespace HabiticaHourUpVSIX;

public class MyTimer
{
	private readonly WorkingTimeCounterTimer _workingTimeCounter;
	private readonly TimeSpan _lastWorkTime;
	private readonly Timer _timer;

	private DateTime? _lastTick;
	public TimeSpan? LastTickAgo => _lastTick.HasValue ? DateTime.Now - _lastTick.Value : null;

	public event Action? TimerCallback;

	public TimeSpan Divisor { get; private set; }
	public bool Enabled { get; private set; }

	public MyTimer(TimeSpan lastWorkTime, TimeSpan divisor)
	{
		_lastWorkTime = lastWorkTime;
		Divisor = divisor;

		_workingTimeCounter = new WorkingTimeCounterTimer(_lastWorkTime, Divisor);

		_timer = new Timer(delegate
		{
			_lastTick = DateTime.Now;
			TimerCallback?.Invoke();
		});
	}
	public bool Start()
		=> SetTimer(_lastWorkTime, Divisor, out _);

	/// <summary>
	/// Set divisor. Calculates first tick for <paramref name="workTime"/>. Logs calculation
	/// </summary>
	public bool SetTimer(TimeSpan workTime, TimeSpan divisor, out long ticksCalculated)
	{
		Divisor = divisor;
		var tickAfter = _workingTimeCounter.CalculateNextTickAfter(workTime, out ticksCalculated);

		return Enabled = _timer.Change(tickAfter, divisor);
	}
	public bool SetTimer(TimeSpan workTime, out long ticksCalculated)
		=> SetTimer(workTime, Divisor, out ticksCalculated);

	/// <summary>
	/// Enables timer if can.
	/// </summary>
	public bool TickAfter(TimeSpan tickAfter)
	{
		_lastTick = DateTime.Now - tickAfter;
		return Enabled = _timer.Change(tickAfter, Divisor);
	}

	public bool Stop()
	{
		Enabled = false;
		return _timer.Change(Timeout.Infinite, Timeout.Infinite);
	}

	#region Trash2
	public TimeSpan CalculateTickAfter(out long ticksCalculated)
	{
		if (_workingTimeCounter.Divisor != Divisor)
			_workingTimeCounter.Divisor = Divisor;

		return _workingTimeCounter.CalculateNextTickAfter(LastTickAgo ?? _lastWorkTime, out ticksCalculated);
		#region Trash
		//ticksCalculated = _workingTimeCounter.DivideWorkTime(workTime);

		//return CalculateTickAfter2(_workingTimeCounter.WorkTime);
		//TimeSpan CalculateTickAfter(TimeSpan workTime)
		//{
		//	if (!_lastTick.HasValue)
		//	{
		//		lastTickAgo = null;
		//		return Divisor - workTime;
		//	}

		//	lastTickAgo = DateTime.Now - _lastTick.Value;
		//	if (lastTickAgo > Divisor)
		//		return Divisor - workTime;

		//	TimeSpan workTimeAfterTick = workTime - lastTickAgo.Value;

		//	return Divisor - workTimeAfterTick;
		//}
		//TimeSpan CalculateTickAfter2(TimeSpan workTime)
		//{
		//	// lastTickAgo = 1:00
		//	// divisor = 0:30
		//	// workTime = 0:10
		//	// return divisor - workTime = 0:20

		//	// lastTickAgo = 1:00
		//	// divisor = 5:00
		//	// workTime = 1:00
		//	// return divisor - lastTickAgo = 4:00

		//	if (!_lastTick.HasValue)
		//	{
		//		lastTickAgo = null;
		//		return Divisor - workTime;
		//	}

		//	lastTickAgo = DateTime.Now - _lastTick.Value;
		//	if (lastTickAgo > Divisor)
		//		return Divisor - workTime;

		//	return Divisor - lastTickAgo.Value;
		//}
		#endregion
	}
	#endregion
}