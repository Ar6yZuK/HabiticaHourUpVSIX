using Ar6yZuK.MethodHelpers;
using HabiticaHourUpVSIX.AppSettings.Abstractions;
using HabiticaHourUpVSIX.AppSettings.Models;
using Microsoft.VisualStudio.Threading;
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
public sealed class VSOptionsSettings : SettingsWithSaving<General1, UserSettingsModel>
{
	private readonly JoinableTaskFactory _joinableTaskFactory;
	private readonly AsyncLazy<General1> _lazySettings;

	protected override General1 Source
	{
		get => _lazySettings.GetValue();
	}

	public VSOptionsSettings(JoinableTaskFactory joinableTaskFactory)
	{
		_joinableTaskFactory = joinableTaskFactory;
		_lazySettings = new AsyncLazy<General1>(General1.GetLiveInstanceAsync, _joinableTaskFactory);

		General1.Saved += x => _joinableTaskFactory.Run(base.SaveAsync); // Notify all subscribers from base SaveAsync
	}

	public override async Task SaveAsync()
	{
		await base.SaveAsync();
		await Source.SaveAsync();
	}
}
public sealed class HabiticaSettings : SettingsWithSaving<Settings1, HabiticaSettingsModel>
{
	protected override Settings1 Source => Settings1.Default;

	public override async Task SaveAsync()
	{
		await base.SaveAsync();
		Source.Save();
	}
}

public sealed class SessionSettings : Settings<SessionSettingsModel>
{
	private readonly SourceDestMapper<SessionSettingsModelClass, SessionSettingsModel> _mapper = new();
	private readonly SessionSettingsModelClass _source = SessionSettingsModelClass.Default;

	public override SessionSettingsModel Read()
		=> _mapper.Map(_source);
	public override Task<SessionSettingsModel> ReadAsync()
		=> Task.Run(Read);

	public override void Write(SessionSettingsModel value)
		=> _mapper.Map(value, _source);
	public override Task WriteAsync(SessionSettingsModel value)
		=> Task.Run(() => Write(value));
}