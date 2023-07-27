using Ar6yZuK.MethodHelpers;
using System.Threading.Tasks;
#nullable enable

namespace HabiticaHourUpVSIX.AppSettings.Abstractions;

public interface ISettings<T>
{
	Task<T> ReadAsync();
	Task WriteAsync(T value);
	T Read();
	void Write(T value);
}
public interface ISaveable<T>
	where T : struct
{
	abstract event Func<T, Task>? OnSavingAsync;
	abstract event Action<T>? OnSaving;
	
	Task SaveAsync();
	void Save();
}
public abstract class Settings<T> : ISettings<T> where T : struct
{
	public abstract T Read();
	public abstract Task<T> ReadAsync();

	public abstract void Write(T value);
	public abstract Task WriteAsync(T value);
}

public abstract class SettingsWithSaving<T>
	: Settings<T>, ISaveable<T>
	where T : struct
{
	public event Func<T, Task>? OnSavingAsync;
	public event Action<T>? OnSaving;

	public void Save()
	{
		T read = Read();
		OnSaving?.Invoke(read);
	}

	public virtual async Task SaveAsync()
	{
		T read = await ReadAsync();
		await OnSavingAsync.InvokeMultipleAsync(read);
	}
}
public abstract class SettingsWithSaving<TSource, TDest>
	: SettingsWithSaving<TDest>
	where TDest : struct
	where TSource : class
{
	private readonly SourceDestMapper<TSource, TDest> _mapper = new();

	protected abstract TSource Source { get; }

	public override TDest Read()
		=> _mapper.Map(Source);
	public override void Write(TDest value)
		=> _mapper.Map(value, Source);

	public override Task<TDest> ReadAsync()
		=> Task.Run(Read);
	public override Task WriteAsync(TDest value)
		=> Task.Run(() => Write(value));
}

//public abstract class Settings<TSource, TDest>
//	: ISettings<TDest>
//	where TDest : struct
//	where TSource : class
//{
//	private readonly SourceDestMapper<TSource, TDest> _mapper = new();
//	protected abstract TSource Source { get; }

//	public Task<TDest> ReadAsync()
//		=> _mapper.Map(Source);
//	public Task WriteAsync(TDest value)
//		=> _mapper.Map(value, Source);
//}