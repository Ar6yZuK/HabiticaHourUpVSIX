using HabiticaHourUpVSIX.AppSettings.Abstractions;
using Microsoft.Win32;
using System.Reflection;

namespace SettingsTests;
public class Tests
{
	private static IEnumerable<Assembly> GetTestingAssembly()
	{
		yield return GetVSIXAssembly();
		yield return GetUnitTestAssembly();

		Assembly GetVSIXAssembly() => typeof(HabiticaHourUpVSIX.HabiticaHourUpVSIXPackage).Assembly;
		Assembly GetUnitTestAssembly() => typeof(Tests).Assembly;
	}

	[Theory]
	public void DoBothArgumentsOfGenerics_SettingsWithSaving_ContainProperties([ValueSource(nameof(GetTestingAssembly))] Assembly assembly)
	{
		// Get types from assembly that derived from SettingsWithSaving<TSource, TDest>
		Type[] settingsTypes = GetDerivedFromSettingsTypes(inAssembly: assembly);
		(string? source, string propertyName, bool contains)[] result = AllPropertiesInTDestAreContainedInTSource(settingsTypes);

		Assert.That(result,
			Is.All.Matches(((string, string, bool contains) x) => x.contains),
			message:"(source type in which no contains propertyName, string propertyName, bool contains)");

		static Type[] GetDerivedFromSettingsTypes(Assembly inAssembly)
		{
			var types = inAssembly.DefinedTypes.Where(x =>
			{
				if (x.BaseType is null)
					return false;

				if (!x.BaseType.IsGenericType)
					return false;

				return x.BaseType.GetGenericTypeDefinition().Equals(typeof(SettingsWithSaving<,>));
			});

			var baseTypes = types.Select(x => x.BaseType!)
				.Where(t =>
				{
					var type = typeof(SettingsWithSaving<,>);

					return t.GetGenericTypeDefinition().Equals(type);
				});

			return baseTypes.ToArray();
		}

		static (string? source, string propertyName, bool contains)[] AllPropertiesInTDestAreContainedInTSource(Type[] settingsTypes)
			=> settingsTypes.SelectMany(t =>
				{
					var generics = t.GetGenericArguments();
					var generic1 = generics[0];
					var generic2 = generics[1];

					var properties1 = generic1.GetProperties()
						.Select(x => x.Name).ToArray();
					var properties2 = generic2.GetProperties()
						.Select(x => x.Name).ToArray();

					return properties2.Select(
							x => (source: generic1.FullName, propertyName: $"{generic2.FullName}.{x}", contains: properties1.Contains(x))
							);
				}).ToArray();
	}

	[Test]
	public void TestSet()
	{
		const int value = 2;

		var test = new TestingSettings();
		Assert.That(test.Test.IntProperty, Is.Not.EqualTo(value));

		test.SetWithSave(x => x.IntProperty, value);

		Assert.That(test.Test.IntProperty, Is.EqualTo(value));
	}

	// For debug testing. May be deleted
	//[Test]
	public void TestSetCache()
	{
		const int value = 2;

		var test = new TestingSettings();
		Assert.That(test.Test.IntProperty, Is.Not.EqualTo(value));

		test.SetWithSave(x => x.IntProperty, value);
		test.SetWithSave(x => x.IntProperty, value);

		Assert.That(test.Test.IntProperty, Is.EqualTo(value));
	}
}
public record struct Test(int IntProperty);
public record class TestClass(int IntProperty);

class TestingSettings : SettingsWithSaving<TestClass, Test>
{
	public TestClass Test = new(0);
	protected override TestClass Source => Test;
}