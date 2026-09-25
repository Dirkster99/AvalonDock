using System.Linq;
using AvalonDock.Core.Serialization;
using AvalonDock.Core.Serialization.Dto;
using AvalonDock.Layout;
using AvalonDock.Serialization;
using NUnit.Framework;

namespace AvalonDockTest
{
	/// <summary>
	/// Tolerance tests for <see cref="LayoutDtoMapper"/>. A stored layout can carry values this
	/// version cannot read - written by an older release, under a different culture, or edited by
	/// hand. A single unreadable attribute must cost that attribute, not the whole layout.
	/// </summary>
	[TestFixture]
	public class LayoutDtoMapperToleranceTests
	{
		private static readonly ILayoutDtoMapper Mapper = new LayoutDtoMapper();

		/// <summary>Builds a root DTO holding a single anchorable pane with one anchorable.</summary>
		/// <param name="configure">Applied to the anchorable DTO before mapping.</param>
		/// <param name="configurePane">Applied to the pane DTO before mapping.</param>
		/// <returns>The root DTO.</returns>
		private static LayoutRootDto RootDto(
			System.Action<LayoutAnchorableDto> configure = null,
			System.Action<LayoutAnchorablePaneDto> configurePane = null)
		{
			var anchorable = new LayoutAnchorableDto { Title = "Tool 1", ContentId = "tool1" };
			configure?.Invoke(anchorable);

			var pane = new LayoutAnchorablePaneDto { Id = "pane1" };
			pane.Children.Add(anchorable);
			configurePane?.Invoke(pane);

			var root = new LayoutRootDto { RootPanel = new LayoutPanelDto { Orientation = "Horizontal" } };
			root.RootPanel.Children.Add(pane);
			return root;
		}

		/// <summary>Finds the single anchorable of a mapped layout.</summary>
		/// <param name="root">The mapped layout.</param>
		/// <returns>The anchorable.</returns>
		private static LayoutAnchorable Anchorable(LayoutRoot root) =>
			root.Descendents().OfType<LayoutAnchorable>().Single(a => a.ContentId == "tool1");

		/// <summary>
		/// A timestamp in a format this version cannot read must not abort the restore. It is dropped:
		/// it only orders the document navigator.
		/// </summary>
		[Test]
		public void FromDto_WithUnreadableTimeStamp_KeepsTheLayout()
		{
			var dto = RootDto(a => a.LastActivationTimeStamp = "not-a-timestamp");

			LayoutRoot root = null;
			Assert.DoesNotThrow(() => root = (LayoutRoot)Mapper.FromDto(dto));

			Assert.That(Anchorable(root).Title, Is.EqualTo("Tool 1"));
			Assert.That(Anchorable(root).LastActivationTimeStamp, Is.Null);
		}

		/// <summary>A timestamp written in invariant culture is still read.</summary>
		[Test]
		public void FromDto_WithInvariantTimeStamp_ReadsIt()
		{
			var dto = RootDto(a => a.LastActivationTimeStamp = "06/25/2026 14:16:56");

			var root = (LayoutRoot)Mapper.FromDto(dto);

			Assert.That(Anchorable(root).LastActivationTimeStamp, Is.EqualTo(
				new System.DateTime(2026, 6, 25, 14, 16, 56)));
		}

		/// <summary>An unreadable dock length falls back to the default instead of throwing.</summary>
		[Test]
		public void FromDto_WithUnreadableDockWidth_KeepsTheLayout()
		{
			var dto = RootDto(configurePane: p => p.DockWidth = "not-a-length");

			LayoutRoot root = null;
			Assert.DoesNotThrow(() => root = (LayoutRoot)Mapper.FromDto(dto));

			Assert.That(Anchorable(root).Title, Is.EqualTo("Tool 1"));
		}

		/// <summary>A readable dock length is still applied.</summary>
		[Test]
		public void FromDto_WithDockWidth_ReadsIt()
		{
			var dto = RootDto(configurePane: p => p.DockWidth = "200");

			var root = (LayoutRoot)Mapper.FromDto(dto);
			var pane = root.Descendents().OfType<LayoutAnchorablePane>().Single();

			Assert.That(pane.DockWidth.Value, Is.EqualTo(200));
		}

		/// <summary>An unknown orientation falls back to horizontal instead of throwing.</summary>
		[Test]
		public void FromDto_WithUnknownOrientation_FallsBackToHorizontal()
		{
			var dto = RootDto();
			dto.RootPanel.Orientation = "Diagonal";

			LayoutRoot root = null;
			Assert.DoesNotThrow(() => root = (LayoutRoot)Mapper.FromDto(dto));

			Assert.That(root.RootPanel.Orientation,
				Is.EqualTo(System.Windows.Controls.Orientation.Horizontal));
		}

		/// <summary>A known orientation is still honoured.</summary>
		[Test]
		public void FromDto_WithVerticalOrientation_ReadsIt()
		{
			var dto = RootDto();
			dto.RootPanel.Orientation = "Vertical";

			var root = (LayoutRoot)Mapper.FromDto(dto);

			Assert.That(root.RootPanel.Orientation,
				Is.EqualTo(System.Windows.Controls.Orientation.Vertical));
		}
	}
}
