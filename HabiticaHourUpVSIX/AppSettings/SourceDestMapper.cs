using AutoMapper;
using System.Threading.Tasks;
#nullable enable

namespace HabiticaHourUpVSIX.AppSettings.Abstractions;

internal class SourceDestMapper<TSource, TDest>
{
	private readonly IMapper _mapper = new MapperConfiguration(cfg =>
	{
		cfg.ShouldMapMethod = _ => false;

		cfg.CreateMap<TSource, TDest>();
		cfg.CreateMap<TDest, TSource>();
	}).CreateMapper(); 
	
	public TDest Map(TSource source)
		=> _mapper.Map<TDest>(source);
	public void Map(TDest source, TSource destination)
		=> _mapper.Map(source, destination);
}