using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.RpcContracts.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.File.GZip;
using Serilog.Templates;
using System.IO;

namespace HabiticaHourUpVSIX;
public class AssemblyFileLoggerProvider : FileLoggerProvider
{
	private readonly static string _assemblyPath = System.Reflection.Assembly.GetAssembly(typeof(HabiticaHourUpVSIXPackage)).Location;

	public AssemblyFileLoggerProvider(string logFileName = "logs.log") : base(Path.Combine(Path.GetDirectoryName(_assemblyPath), logFileName))
	{
	}
}
public class FileLoggerProvider
{
	private const string _loggerTemplate =
		"[{@t:HH:mm:ss.fff zzzUTC dd\\d MM\\m yyyy} {@l:u3}{#if SourceContext is not null} ({SourceContext}){#end}{#each s in Scope} {s}{#delimit},{#end}]\n\t{@m}\n{@x}";

	protected readonly ExpressionTemplate _formatter = new(_loggerTemplate);
	public string LogPath { get; }

	public FileLoggerProvider(string logPath)
	{
		LogPath = logPath;
	}

	protected Logger CreateSerilog()
	{
		return new LoggerConfiguration()
			.MinimumLevel.Verbose()
			.WriteTo.File(path: LogPath, hooks:new GZipHooks(), formatter: _formatter, rollingInterval:RollingInterval.Day)
			.CreateLogger();
	}

	private ILoggerFactory CreateFactoryWith(Logger logger)
	{
		return new LoggerFactory()
			.AddSerilog(logger, true);
	}

	public ILogger<T> CreateLogger<T>()
	{
		var logger = CreateSerilog();

		var result = CreateFactoryWith(logger)
			.CreateLogger<T>();

		return result;
	}
	//public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
	//{
	//	var logger = CreateSerilog();

	//	var result = CreateFactoryWith(logger)
	//		.CreateLogger(categoryName);

	//	return result;
	//}
}