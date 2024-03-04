using HabiticaHourUpVSIX.AppSettings.Abstractions;
using HabiticaHourUpVSIX.AppSettings.Models;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
#nullable enable

namespace HabiticaHourUpVSIX.AppSettings;
public class FileSettings : ISettings<UserSettingsModel>
{
	private readonly string _filePath;

	public UserSettingsModel SettingsDefaultModel { get; }
	public FileSettings(string filePath, UserSettingsModel settingsModelDefaultValue)
	{
		SettingsDefaultModel = settingsModelDefaultValue;
		_filePath = filePath;
	}

	private void CreateWrite(UserSettingsModel settingsModel)
	{
		string serializedToWrite = JsonConvert.SerializeObject(settingsModel, Formatting.Indented);
		File.WriteAllText(_filePath, serializedToWrite);
	}
	private Task<UserSettingsModel> CreateDefault()
	{
		CreateWrite(SettingsDefaultModel);
		return Task.FromResult(SettingsDefaultModel);
	}
	public Task<UserSettingsModel> ReadAsync()
	{
		if (!File.Exists(_filePath))
			return CreateDefault();

		var text = File.ReadAllText(_filePath);
		var deserialized = JsonConvert.DeserializeObject<UserSettingsModel?>(text);

		if (!deserialized.HasValue)
			return CreateDefault();

		return Task.FromResult(deserialized.Value);
	}

	public Task WriteAsync(UserSettingsModel value)
	{
		throw new NotImplementedException();
	}

	public UserSettingsModel Read()
	{
		throw new NotImplementedException();
	}

	public void Write(UserSettingsModel value)
	{
		throw new NotImplementedException();
	}
}
internal sealed class UserSettings : SettingsWithSaving<UserSettings3, UserSettingsModel>
{
	protected override UserSettings3 Source
	{
		get => UserSettings3.Default;
	}

	public override void Save()
	{
		base.Save();
		Source.Save();
	}
}
internal class SettingsInFile<T>(string fileName, T defaultSettings) : SettingsWithSaving<T>
	where T : struct
{
	public override T Read()
	{
		if (!File.Exists(fileName))
		{
			Write(defaultSettings);
			return defaultSettings;
		}

		string data = File.ReadAllText(fileName);
		var result = JsonConvert.DeserializeObject<T>(data);

		return result;
	}

	public override void Write(T value)
	{
		string data = JsonConvert.SerializeObject(value, Formatting.Indented);
		WriteDataToFile(fileName, data);

		static void WriteDataToFile(string fileName, string data)
		{
			CreateDirectoryIfNotExists(Path.GetDirectoryName(fileName));
			File.WriteAllText(fileName, data);
			static void CreateDirectoryIfNotExists(string dirPath)
			{
				if (!Directory.Exists(dirPath))
					Directory.CreateDirectory(dirPath);
			}
		}
	}

}
// TODO: Maybe add ICachedSettings interface if needed
internal class CachedSettings<T>(Settings<T> settingsToCache, TimeSpan timeOfCache) : Settings<T>
	where T : struct
{
	private DateTime _expires;
	private T _cachedValue;

	public override T Read()
	{
		if (_expires > DateTime.Now)
			return _cachedValue;

		_expires = DateTime.Now + timeOfCache;
		return _cachedValue = settingsToCache.Read();
	}
	public override void Write(T value)
	{
		_expires = DateTime.Now - TimeSpan.FromSeconds(1d);
		settingsToCache.Write(value);
	}
}
internal class CachedSettingsWithSaving<T>(SettingsWithSaving<T> settings, TimeSpan timeOfCache) : SettingsWithSaving<T>
	where T : struct
{
	private readonly CachedSettings<T> _cachedSettings = new(settings, timeOfCache);

	public override T Read() => _cachedSettings.Read();
	public override void Write(T value) => _cachedSettings.Write(value);
}
internal class CachedSettingsInFile<T>(string fileName, T defaultSettings, TimeSpan timeOfCache) : SettingsWithSaving<T>
	where T : struct
{
	private readonly CachedSettingsWithSaving<T> _cachedSettingsInFile = new(new SettingsInFile<T>(fileName, defaultSettings), timeOfCache);

	public override T Read() => _cachedSettingsInFile.Read();
	public override void Write(T value) => _cachedSettingsInFile.Write(value);
}
internal sealed class HabiticaSettings : SettingsWithSaving<HabiticaSettings1, HabiticaSettingsModel>
{
	protected override HabiticaSettings1 Source => HabiticaSettings1.Default;

	public override void Save()
	{
		base.Save();
		Source.Save();
	}
}
internal sealed class CredentialsSettings : SettingsWithSaving<CredentialsSettings1, HabiticaCredentials>
{
	protected override CredentialsSettings1 Source => CredentialsSettings1.Default;

	public override void Save()
	{
		base.Save();
		Source.Save();
	}
}
internal sealed class SessionSettings : Settings<SessionSettingsModel>
{
	private readonly SourceDestMapper<SessionSettingsModelClass, SessionSettingsModel> _mapper = new();
	private readonly SessionSettingsModelClass _source = SessionSettingsModelClass.Default;

	public override SessionSettingsModel Read()
		=> _mapper.Map(_source);

	public override void Write(SessionSettingsModel value)
		=> _mapper.Map(value, _source);
}