using System.Threading;
#nullable enable

namespace HabiticaHourUpVSIX;

public interface ITimer : IDisposable
{
	event Action? Tick;

	DateTime? Next { get; }
	TimeSpan NextTick { get; }

	bool Change(TimeSpan dueTime, TimeSpan period);
}
public class MyTimer : ITimer, IDisposable
{
	private readonly Timer _timer;
	private bool _disposedValue;

	public event Action? Tick;

	public TimeSpan Period { get; protected set; }
	public DateTime? Next { get; protected set; }

	public TimeSpan NextTick => Next - DateTime.Now ?? Timeout.InfiniteTimeSpan;

	public MyTimer()
	{
		_timer = new Timer(
			delegate
			{
				Next = DateTime.Now.Add(Period);
				Tick?.Invoke();
			});
	}

	public bool Change(TimeSpan dueTime, TimeSpan period)
	{
		Period = period;
		Next = DateTime.Now.Add(dueTime);
		return _timer.Change(dueTime, period);
	}

	#region Dispose
	protected virtual void Dispose(bool disposing)
	{
		if (_disposedValue)
			return;

		if (disposing)
		{
			_timer.Dispose();
			// TODO: освободить управляемое состояние (управляемые объекты)
		}

		// TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
		// TODO: установить значение NULL для больших полей
		_disposedValue = true;
	}

	// // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
	// ~MyTimer()
	// {
	//     // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
	//     Dispose(disposing: false);
	// }

	public void Dispose()
	{
		// Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
	#endregion
}