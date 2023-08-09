using Ar6yZuK.Habitica.Response;
using Ar6yZuK.Habitica.Response.Tasks;
using HabiticaHourUpVSIX.AppSettings.Models;
using OneOf;
using System.Threading;
using System.Threading.Tasks;

namespace HabiticaHourUpVSIX;
public class HabiticaClient : IHabiticaClient
{
	private readonly HabiticaHourUpVSIXPackage _package;

	public HabiticaClient(HabiticaHourUpVSIXPackage package)
	{
		_package = package;
	}

	/// <exception cref="ArgumentException"></exception>
	public async Task<OneOf<TaskScore.Root, NotSuccess.Root>> SendOneTickAsync(CancellationToken cancellationToken = default)
	{
		UserSettingsModel userSettingsRead = _package.UserSettingsReader.Read();
		HabiticaCredentials credentials = _package.CredentialsSettings.Read();

		using var client = new Ar6yZuK.Habitica.HabiticaClient(credentials);
		var result = await ScoreUpInternalAsync(userSettingsRead.TaskIDToScoreUp, client, cancellationToken);

		return result;
	}
	private async Task<OneOf<TaskScore.Root, NotSuccess.Root>> ScoreUpInternalAsync(string taskIDToScoreUp, Ar6yZuK.Habitica.HabiticaClient client, CancellationToken cancellationToken)
	{
		var scoreUpResult = await client.ScoreUp(taskIDToScoreUp, cancellationToken);
		if (scoreUpResult.TryPickT1(out NotSuccess.Root notSuccess, out var success))
			return notSuccess;

		return success;
	}
}
public class HabiticaClientNotSuccess : IHabiticaClient
{
	public Task<OneOf<TaskScore.Root, NotSuccess.Root>> SendOneTickAsync(CancellationToken cancellationToken = default)
		=> Task.FromResult((OneOf<TaskScore.Root, NotSuccess.Root>)new NotSuccess.Root { Error = "NotAuthorized", Message = "There is no account that uses those credentials." });
}
public class HabiticaClientSuccess : IHabiticaClient
{
	public Task<OneOf<TaskScore.Root, NotSuccess.Root>> SendOneTickAsync(CancellationToken cancellationToken = default)
		=> Task.FromResult((OneOf<TaskScore.Root, NotSuccess.Root>)new TaskScore.Root { Success = true });
}

public interface IHabiticaClient
{
	Task<OneOf<TaskScore.Root, NotSuccess.Root>> SendOneTickAsync(CancellationToken cancellationToken = default);
}