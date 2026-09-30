namespace Microsoft.Windows.Shell
{
	public class WindowChrome : System.Windows.Shell.WindowChrome
	{
		public bool ShowSystemMenu { get; set; } = true;

		public static new WindowChrome GetWindowChrome(System.Windows.Window window) =>
			System.Windows.Shell.WindowChrome.GetWindowChrome(window) as WindowChrome;
	}
}
