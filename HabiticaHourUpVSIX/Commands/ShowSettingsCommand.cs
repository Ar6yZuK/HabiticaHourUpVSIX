using HabiticaHourUpVSIX.ToolWindows;

namespace HabiticaHourUpVSIX.Commands;

[Command(PackageIds.OpenSettingsWindow)]
internal sealed class ShowSettingsCommand : BaseCommand<ShowSettingsCommand>
{
	protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
	{
		await SettingsToolWindow.ShowAsync();
	}
}
