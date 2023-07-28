using System.Threading;
#nullable enable

namespace HabiticaHourUpVSIX;
public class MyTimer : IDisposable
{
	private readonly Timer _timer;

	public event Action? Tick;

	public TimeSpan Divisor { get; private set; }
	public DateTime Next { get; private set; }

	public TimeSpan NextTick => Next - DateTime.Now;

	public MyTimer(TimeSpan dueTime, TimeSpan divisor)
	{
		_timer = new Timer(
			delegate
			{
				Next = DateTime.Now.Add(Divisor);
				Tick?.Invoke();
			},
			state:null, dueTime, divisor);

		Divisor = divisor;
		Next = DateTime.Now.Add(dueTime);
	}

	public bool Change(TimeSpan dueTime, TimeSpan divisor)
	{
		Divisor = divisor;
		Next = DateTime.Now.Add(dueTime);
		return _timer.Change(dueTime, divisor);
	}

	public void Dispose() => _timer.Dispose();
}