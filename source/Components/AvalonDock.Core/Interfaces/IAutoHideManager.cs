#nullable disable
using System;

namespace AvalonDock.Core
{
	/// <summary>
	/// UI-agnostic interface for auto-hide window management.
	/// Controls showing/hiding of auto-hide (side-docked) tool windows.
	/// </summary>
	/// <remarks>
	/// <para>The WPF implementation uses DispatcherTimer for delayed hiding.</para>
	/// <para>ViewModels can use this to programmatically show/hide tool windows.</para>
	/// </remarks>
	public interface IAutoHideManager
	{
		/// <summary>Gets or sets the delay in milliseconds before auto-hiding.</summary>
		int AutoHideDelayMs { get; set; }

		/// <summary>Shows the auto-hide window for the anchorable with the given content ID.</summary>
		/// <param name="contentId">The content ID of the anchorable to show.</param>
		void ShowAutoHide(string contentId);

		/// <summary>Hides the currently shown auto-hide window.</summary>
		void HideAutoHide();

		/// <summary>Gets a value indicating whether an auto-hide window is currently visible.</summary>
		bool IsAutoHideVisible { get; }

		/// <summary>Gets the content ID of the currently shown auto-hide anchorable, or null.</summary>
		string CurrentAutoHideContentId { get; }
	}
}