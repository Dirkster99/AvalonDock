namespace AvalonDock.Core
{
	/// <summary>
	/// Extension methods for <see cref="DockZone"/>.
	/// </summary>
	public static class DockZoneExtensions
	{
		/// <summary>
		/// Maps a <see cref="DockZone"/> to the <see cref="ToolboxSide"/> it belongs to.
		/// </summary>
		/// <param name="zone">The dock zone.</param>
		/// <returns>The corresponding side.</returns>
		public static ToolboxSide ToSide(this DockZone zone)
		{
			switch (zone)
			{
				case DockZone.LeftTop:
				case DockZone.LeftBottom:
					return ToolboxSide.Left;
				case DockZone.RightTop:
				case DockZone.RightBottom:
					return ToolboxSide.Right;
				case DockZone.BottomLeft:
				case DockZone.BottomRight:
					return ToolboxSide.Bottom;
				default:
					return ToolboxSide.Left;
			}
		}
	}
}