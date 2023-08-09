using Ar6yZuK.Habitica.Response;
using Ar6yZuK.Habitica.Response.Tasks;
using OneOf;
using System.Threading;
using System.Threading.Tasks;

namespace HabiticaHourUpVSIX.Habitica.Abstractions;
public abstract class HabiticaClientBase : ISendTickToHabitica, INotifySend
{
	public event Action OnSuccessfullySend;

	public virtual Task<OneOf<TaskScore.Root, NotSuccess.Root>> SendOneTickAsync(CancellationToken cancellationToken = default)
	{
		OnSuccessfullySend?.Invoke();
		return Task.FromResult((OneOf<TaskScore.Root, NotSuccess.Root>)new NotSuccess.Root() { Message = "HabiticaClientBaseNotSend", Error = "" });
	}
}
public interface ISendTickToHabitica
{
	Task<OneOf<TaskScore.Root, NotSuccess.Root>> SendOneTickAsync(CancellationToken cancellationToken = default);
}
public interface INotifySend
{
	abstract event Action OnSuccessfullySend;
}