namespace HabiticaHourUpVSIX.AppSettings.Models;
public record struct UserSettingsModel(TimeSpan Divisor, string TaskIDToScoreUp, bool IsAutoScoreUp)
{
	private readonly TimeSpanValidator _validator = new(value:Divisor, minValue:TimeSpan.FromSeconds(30), maxValue: TimeSpan.FromMilliseconds(uint.MaxValue - 1));

	public readonly TimeSpan Divisor
	{
		get => _validator.Value;
		set => _validator.Value = value;
	}
}
