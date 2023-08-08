#nullable enable

namespace HabiticaHourUpVSIX.AppSettings.Abstractions;

public interface ISettings<T>
{
	T Read();
	void Write(T value);
}
public interface ISaveable<T>
	where T : struct
{
	abstract event Action<T>? OnSaving;
	
	void Save();
}
public abstract class Settings<T> : ISettings<T> where T : struct
{
	public abstract T Read();

	public abstract void Write(T value);
}

public abstract class SettingsWithSaving<T>
	: Settings<T>, ISaveable<T>
	where T : struct
{
	public event Action<T>? OnSaving;

	public virtual void Save()
	{
		T read = Read();
		OnSaving?.Invoke(read);
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