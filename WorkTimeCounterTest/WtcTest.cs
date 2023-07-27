using HabiticaHourUpVSIX;

namespace WorkTimeCounterTest;

public class Tests
{
	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public void Test1()
	{
		var divisor = TimeSpan.FromHours(1);
		var lastWorkTime = TimeSpan.FromMinutes(80);

		var wtc = new WorkingTimeCounter(lastWorkTime, divisor);

		var count = wtc.DivideWorkTime();

		var twentyMinutes = TimeSpan.FromMinutes(20);

		Assert.Multiple(() =>
		{
			Assert.That(wtc.WorkTime, Is.EqualTo(twentyMinutes));
			Assert.That(count, Is.EqualTo(1));
			Assert.That((divisor - wtc.WorkTime).TotalMinutes, Is.EqualTo(40));
		});
	}
	[Test]
	public void Test2()
	{
		var divisor = TimeSpan.FromHours(1);
		var lastWorkTime = TimeSpan.FromMinutes(40);

		var wtc = new WorkingTimeCounter(lastWorkTime, divisor);
		var count = wtc.DivideWorkTime();

		Assert.Multiple(() =>
		{
			Assert.That(count, Is.EqualTo(0));
			Assert.That(wtc.WorkTime, Is.EqualTo(lastWorkTime));
			Assert.That((divisor - wtc.WorkTime).TotalMinutes, Is.EqualTo(20));
		});
	}
	[Test]
	public void Test3()
	{
		var workTime = TimeSpan.FromMinutes(1);
		var divisor = TimeSpan.FromHours(1);

		var wtc = new WorkingTimeCounter(workTime, divisor);
		var count = wtc.DivideWorkTime();

		Assert.That(count, Is.EqualTo(0));
		Assert.That(wtc.WorkTime, Is.EqualTo(workTime));
	}

	[Test]
	public void CalculateFirstTest()
	{
		var lastWorkTime = TimeSpan.FromMinutes(46);
		var divisor = TimeSpan.FromHours(1);
		var wtc = new WorkingTimeCounterTimer(lastWorkTime, divisor);

		TimeSpan tickAfter = wtc.CalculateNextTickAfter(out long ticksCalculated);

		Assert.That(tickAfter, Is.EqualTo(TimeSpan.FromMinutes(14)));
		Assert.That(ticksCalculated, Is.EqualTo(0));
	}
	[Test]
	public void CalculateFirstTickWithNewDivisor()
	{
		var lastWorkTime = TimeSpan.FromMinutes(46);
		var divisor = TimeSpan.FromMinutes(5);
		var wtc = new WorkingTimeCounterTimer(lastWorkTime, divisor);

		TimeSpan tickAfter2 = wtc.CalculateNextTickAfter(out long ticksCalculated2);

		Assert.That(ticksCalculated2, Is.EqualTo(9));
		Assert.That(tickAfter2, Is.EqualTo(TimeSpan.FromMinutes(4)));
	}
	[Test]
	public void CalculateTickAfter()
	{
		// lastDivisor = 01:00:00

		// lastTickAgo	= 13:00
		// workTime		= 16:00
		// divisor		= 5:00

		var workTime = TimeSpan.FromMinutes(16);
		var divisor = TimeSpan.FromHours(1);
		var wtc = new WorkingTimeCounterTimer(workTime:TimeSpan.Zero, divisor);

		wtc.CalculateNextTickAfter(workTime, out long ticksCalculated);

	}
}