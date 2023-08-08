namespace HabiticaHourUpVSIX.AppSettings.Models;

public class TimeSpanValidator
{
	public readonly TimeSpan MaxValue;
	public readonly TimeSpan MinValue;

	private TimeSpan _value;
	/// <summary>
	/// Return <see cref="MinValue"/> if <paramref name="value"/> less. 
	/// Return <see cref="MaxValue"/> if <paramref name="value"/> larger. 
	/// Otherwise return <paramref name="value"/>.
	/// </summary>
	public TimeSpan Value
	{
		get => _value;
		set => _value = GetValidated(value);
	}

	public TimeSpanValidator(TimeSpan value, TimeSpan minValue, TimeSpan maxValue)
	{
		_value = GetValidated(value);
		MinValue = minValue;
		MaxValue = maxValue;
	}

	
	private TimeSpan GetValidated(TimeSpan value)
		=> value < MinValue ? MinValue : value > MaxValue ? MaxValue : value;
}