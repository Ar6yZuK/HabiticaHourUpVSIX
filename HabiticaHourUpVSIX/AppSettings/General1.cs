using Ar6yZuK.MethodHelpers;
using System.ComponentModel;
using System.Runtime.InteropServices;
#nullable enable

namespace HabiticaHourUpVSIX.AppSettings;
internal partial class OptionsProvider
{
	// Register the options with this attribute on your package class:
	// [ProvideOptionPage(typeof(OptionsProvider.General1Options), "HabiticaHourUpVSIX.windows", "General1", 0, 0, true, SupportsProfiles = true)]
	[ComVisible(true)]
	public class General1Options : BaseOptionPage<General1> { } 
}

public class General1 : BaseOptionModel<General1>
{
	private static readonly TimeSpan DivisorDefault = TimeSpan.Parse(DivisorDefaultString);
	private const string DivisorDefaultString = "01:00:00";
	public static TimeSpan DivisorMinValue { get; } = TimeSpan.FromSeconds(1);
	public static TimeSpan DivisorMaxValue { get; } = TimeSpan.FromMilliseconds(uint.MaxValue - 1);

	private TimeSpan _divisor = DivisorDefault;
	[DefaultValue(typeof(TimeSpan), DivisorDefaultString)]
	public TimeSpan Divisor 
	{ 
		get => _divisor;
		// Validation min/max value. Set min/max if not valid
		set => _divisor = value < DivisorMinValue ? DivisorMinValue : value > DivisorMaxValue ? DivisorMaxValue : value;
	}
	protected override string SerializeValue(object? value, Type type, string propertyName)
	{
		if (value is TimeSpan t)
			return t.ToString();

		return base.SerializeValue(value, type, propertyName);
	}
	protected override object? DeserializeValue(string serializedData, Type type, string propertyName)
	{
		if (TimeSpan.TryParse(serializedData, out var t))
			return t;

		return base.DeserializeValue(serializedData, type, propertyName);
	}
}