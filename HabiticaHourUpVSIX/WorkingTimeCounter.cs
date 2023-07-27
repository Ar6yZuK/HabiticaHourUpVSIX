using Microsoft.Extensions.Logging;
#nullable enable

namespace HabiticaHourUpVSIX;

public class WorkingTimeCounter
{
	public TimeSpan WorkTime { get; private set; }
	public TimeSpan Divisor { get; set; }

	public WorkingTimeCounter(TimeSpan workTime, TimeSpan divisor)
	{
		WorkTime = workTime;
		Divisor = divisor;
	}

	/// <summary>
	/// Divides <paramref name="additionalWorkTime"/> + <see cref="WorkTime"/> by Divisor and returns the result of the division.
	/// Writes the remainder of the division to <see cref="WorkTime"/>
	/// </summary>
	public long DivideWorkTime(TimeSpan additionalWorkTime = default)
	{
		long totalWorkTimeTicks = additionalWorkTime.Ticks + WorkTime.Ticks;
		long quotient = Math.DivRem(totalWorkTimeTicks, Divisor.Ticks, out long remainderTicks);

		TimeSpan remainder = TimeSpan.FromTicks(remainderTicks);

		WorkTime = remainder;

		return quotient;
	}
}
public class WorkingTimeCounterTimer : WorkingTimeCounter
{
	public WorkingTimeCounterTimer(TimeSpan workTime, TimeSpan divisor) : base(workTime, divisor)
	{
	}

	public TimeSpan CalculateNextTickAfter(TimeSpan workTime, out long ticksCalculated)
	{
		ticksCalculated = DivideWorkTime(workTime);

		return Divisor - WorkTime;
	}
	public TimeSpan CalculateNextTickAfter(out long ticksCalculated)
		=> CalculateNextTickAfter(TimeSpan.Zero, out ticksCalculated);
}
public class WorkingOpenTimeCounter : WorkingTimeCounter
{
	public DateTime OpenTime { get; }

	public WorkingOpenTimeCounter(TimeSpan lastWorkTime, DateTime openTime, TimeSpan divisor) : base(lastWorkTime, divisor)
	{
		OpenTime = openTime;
	}

	/// <summary>
	/// Calculates the workTime starting from <see cref="OpenTime"/> and up to <see cref="DateTime.Now"/>
	/// </summary>
	public long DivideLastWorkTime()
		=> DivideWorkTime(DateTime.Now - OpenTime);
}