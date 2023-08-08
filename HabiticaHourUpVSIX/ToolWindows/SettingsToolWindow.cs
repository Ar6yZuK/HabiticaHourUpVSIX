using Microsoft.VisualStudio.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HabiticaHourUpVSIX.ToolWindows;
public class SettingsToolWindow : BaseToolWindow<SettingsToolWindow>
{
	public override string GetTitle(int toolWindowId) => "Settings";

	public override Type PaneType => typeof(Pane);

	public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
	{
		return Task.FromResult<FrameworkElement>(new SettingsWindow(Package as HabiticaHourUpVSIXPackage));
	}

	[Guid("aaa595bb-bc87-4359-97a2-0d19be800384")]
	internal class Pane : ToolWindowPane
	{
		public Pane()
		{
			BitmapImageMoniker = KnownMonikers.ToolWindow;
		}
	}
}
