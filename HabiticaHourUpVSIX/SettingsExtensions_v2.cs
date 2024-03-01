using HabiticaHourUpVSIX;
using HabiticaHourUpVSIX.AppSettings.Abstractions;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

public static partial class SettingsExtensions
{
	// Object as key because unknown T of SettingsWithSaving<T>
	private static readonly Dictionary<(object, string propertyName), PropertyInfo> _cache = [];
	private static readonly Dictionary<object, Type> _cachedTypes = [];

	public static void SetWithSave<T, TProperty>(this SettingsWithSaving<T> obj1,
											  Expression<Func<T, TProperty>> expression,
											  TProperty value) where T : struct
	{
		if (expression.Body is not MemberExpression memberExpression)
			throw new InvalidOperationException("Invalid expression. Should have property that returns.");
		string memberName = memberExpression.Member.Name;

		var settingsReadBoxed = (object)obj1.Read();
		var property = GetProperty(obj1, memberName);

		property.SetValue(settingsReadBoxed, value);

		var settingsToWrite = (T)settingsReadBoxed;
		obj1.Write(settingsToWrite);
		obj1.Save();

		PropertyInfo GetProperty(object obj1, string memberName)
		{
			Type type = _cachedTypes.GetOrSet(obj1, () => typeof(T));
			PropertyInfo property = _cache.GetOrSet((obj1, memberName), () => type.GetProperty(memberName));

			return property;
		}
	}
}