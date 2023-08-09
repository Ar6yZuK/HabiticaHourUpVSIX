using Ar6yZuK.Habitica.Response;
using Ar6yZuK.Habitica.Response.Tasks;
using HabiticaHourUpVSIX.AppSettings.Models;
using HabiticaHourUpVSIX.Habitica.Abstractions;
using Microsoft.VisualStudio.Threading;
using OneOf;
using System.Threading;
using System.Threading.Tasks;

namespace HabiticaHourUpVSIX;
public class HabiticaClient : HabiticaClientBase
{
	private readonly HabiticaHourUpVSIXPackage _package;

	public HabiticaClient(HabiticaHourUpVSIXPackage package)
	{
		_package = package;
	}

	/// <exception cref="ArgumentException">On taskID is empty</exception>
	public override async Task<OneOf<TaskScore.Root, NotSuccess.Root>> SendOneTickAsync(CancellationToken cancellationToken = default)
	{
		UserSettingsModel userSettingsRead = _package.UserSettingsReader.Read();
		HabiticaCredentials credentials = _package.CredentialsSettings.Read();

		using var client = new Ar6yZuK.Habitica.HabiticaClient(credentials);
		var result = await ScoreUpInternalAsync(userSettingsRead.TaskIDToScoreUp, client, cancellationToken);

		if (result.IsT0)
			base.SendOneTickAsync().Forget();

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
public class HabiticaClientNotSuccess : ISendTickToHabitica
{
	public Task<OneOf<TaskScore.Root, NotSuccess.Root>> SendOneTickAsync(CancellationToken cancellationToken = default)
		=> Task.FromResult((OneOf<TaskScore.Root, NotSuccess.Root>)new NotSuccess.Root { Error = "NotAuthorized", Message = "There is no account that uses those credentials." });
}
public class HabiticaClientSuccess : HabiticaClientBase
{
	public override Task<OneOf<TaskScore.Root, NotSuccess.Root>> SendOneTickAsync(CancellationToken cancellationToken = default)
	{
		base.SendOneTickAsync().Forget();
		return Task.FromResult((OneOf<TaskScore.Root, NotSuccess.Root>)new TaskScore.Root { Success = true });
	}
}