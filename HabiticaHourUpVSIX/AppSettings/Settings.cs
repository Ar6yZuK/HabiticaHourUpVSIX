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