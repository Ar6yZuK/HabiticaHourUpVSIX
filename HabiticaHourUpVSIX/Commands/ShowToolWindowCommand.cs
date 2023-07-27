
namespace HabiticaHourUpVSIX.Commands;

[Command(PackageIds.OpenSettingsWindow)]
internal sealed class ShowToolWindowCommand : BaseCommand<ShowToolWindowCommand>
{
	protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
	{
		await ToolWindow1.ShowAsync();
	}
}
