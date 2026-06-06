using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using AvalonDock.Core.Serialization;
using AvalonDock.Core.Serialization.Dto;
using AvalonDock.Layout;

namespace AvalonDock.Serialization
{
	/// <summary>
	/// Maps between the WPF layout tree and serialization DTOs.
	/// </summary>
	public class LayoutDtoMapper : ILayoutDtoMapper
	{
		private static readonly GridLengthConverter GridLengthConverter = new GridLengthConverter();

		/// <inheritdoc/>
		public LayoutRootDto ToDto(ISerializableLayoutRoot layout)
		{
			if (layout == null) throw new ArgumentNullException(nameof(layout));
			return MapRootToDto((LayoutRoot)layout);
		}

		/// <inheritdoc/>
		public ISerializableLayoutRoot FromDto(LayoutRootDto dto)
		{
			if (dto == null) throw new ArgumentNullException(nameof(dto));
			return MapDtoToRoot(dto);
		}

		private LayoutRootDto MapRootToDto(LayoutRoot root)
		{
			var dto = new LayoutRootDto
			{
				RootPanel = root.RootPanel != null ? MapPanelToDto(root.RootPanel) : null,
				TopSide = MapAnchorSideToDto(root.TopSide),
				RightSide = MapAnchorSideToDto(root.RightSide),
				LeftSide = MapAnchorSideToDto(root.LeftSide),
				BottomSide = MapAnchorSideToDto(root.BottomSide),
			};

			foreach (var fw in root.FloatingWindows)
				dto.FloatingWindows.Add(MapFloatingWindowToDto(fw));

			foreach (var hidden in root.Hidden)
				dto.Hidden.Add(MapAnchorableToDto(hidden));

			return dto;
		}

		private LayoutPanelDto MapPanelToDto(LayoutPanel panel)
		{
			var dto = new LayoutPanelDto
			{
				Orientation = panel.Orientation.ToString(),
				CanDock = panel.CanDock,
			};
			CopyPositionableToDto(panel, dto);

			foreach (var child in panel.Children)
				dto.Children.Add(MapPanelChildToDto(child));

			return dto;
		}

		private LayoutPositionableGroupDto MapPanelChildToDto(ILayoutPanelElement child)
		{
			switch (child)
			{
				case LayoutPanel p:
					return MapPanelToDto(p);
				case LayoutDocumentPaneGroup dpg:
					return MapDocumentPaneGroupToDto(dpg);
				case LayoutDocumentPane dp:
					return MapDocumentPaneToDto(dp);
				case LayoutAnchorablePaneGroup apg:
					return MapAnchorablePaneGroupToDto(apg);
				case LayoutAnchorablePane ap:
					return MapAnchorablePaneToDto(ap);
				default:
					throw new NotSupportedException($"Unknown panel child type: {child?.GetType().Name}");
			}
		}

		private LayoutDocumentPaneGroupDto MapDocumentPaneGroupToDto(LayoutDocumentPaneGroup group)
		{
			var dto = new LayoutDocumentPaneGroupDto
			{
				Orientation = group.Orientation.ToString(),
			};
			CopyPositionableToDto(group, dto);

			foreach (var child in group.Children)
			{
				switch (child)
				{
					case LayoutDocumentPane pane:
						dto.Children.Add(MapDocumentPaneToDto(pane));
						break;
					case LayoutDocumentPaneGroup nested:
						dto.Children.Add(MapDocumentPaneGroupToDto(nested));
						break;
				}
			}

			return dto;
		}

		private LayoutDocumentPaneDto MapDocumentPaneToDto(LayoutDocumentPane pane)
		{
			var dto = new LayoutDocumentPaneDto
			{
				Id = ((ILayoutPaneSerializable)pane).Id,
				ShowHeader = pane.ShowHeader,
			};
			CopyPositionableToDto(pane, dto);

			foreach (var child in pane.Children)
			{
				switch (child)
				{
					case LayoutDocument doc:
						dto.Children.Add(MapDocumentToDto(doc));
						break;
					case LayoutAnchorable anch:
						dto.Children.Add(MapAnchorableToDto(anch));
						break;
				}
			}

			return dto;
		}

		private LayoutAnchorablePaneGroupDto MapAnchorablePaneGroupToDto(LayoutAnchorablePaneGroup group)
		{
			var dto = new LayoutAnchorablePaneGroupDto
			{
				Orientation = group.Orientation.ToString(),
			};
			CopyPositionableToDto(group, dto);

			foreach (var child in group.Children)
			{
				switch (child)
				{
					case LayoutAnchorablePane pane:
						dto.Children.Add(MapAnchorablePaneToDto(pane));
						break;
					case LayoutAnchorablePaneGroup nested:
						dto.Children.Add(MapAnchorablePaneGroupToDto(nested));
						break;
				}
			}

			return dto;
		}

		private LayoutAnchorablePaneDto MapAnchorablePaneToDto(LayoutAnchorablePane pane)
		{
			var dto = new LayoutAnchorablePaneDto
			{
				Id = ((ILayoutPaneSerializable)pane).Id,
				Name = pane.Name,
			};
			CopyPositionableToDto(pane, dto);

			foreach (var child in pane.Children)
				dto.Children.Add(MapAnchorableToDto(child));

			return dto;
		}

		private LayoutDocumentDto MapDocumentToDto(LayoutDocument doc)
		{
			var dto = new LayoutDocumentDto
			{
				Description = doc.Description,
				CanMove = doc.CanMove,
			};
			CopyContentToDto(doc, dto);
			return dto;
		}

		private LayoutAnchorableDto MapAnchorableToDto(LayoutAnchorable anch)
		{
			var dto = new LayoutAnchorableDto
			{
				CanHide = anch.CanHide,
				CanAutoHide = anch.CanAutoHide,
				AutoHideWidth = anch.AutoHideWidth,
				AutoHideHeight = anch.AutoHideHeight,
				AutoHideMinWidth = anch.AutoHideMinWidth,
				AutoHideMinHeight = anch.AutoHideMinHeight,
				CanDockAsTabbedDocument = anch.CanDockAsTabbedDocument,
				CanMove = anch.CanMove,
			};
			CopyContentToDto(anch, dto);
			return dto;
		}

		private void CopyContentToDto(LayoutContent content, LayoutContentDto dto)
		{
			dto.Title = content.Title;
			dto.ContentId = content.ContentId;
			dto.IsSelected = content.IsSelected;
			dto.IsLastFocusedDocument = content.IsLastFocusedDocument;
			dto.ToolTip = content.ToolTip is string s ? s : null;
			dto.FloatingLeft = content.FloatingLeft;
			dto.FloatingTop = content.FloatingTop;
			dto.FloatingWidth = content.FloatingWidth;
			dto.FloatingHeight = content.FloatingHeight;
			dto.IsMaximized = content.IsMaximized;
			dto.CanClose = content.CanClose;
			dto.CanCloseDefault = content._canCloseDefault;
			dto.CanFloat = content.CanFloat;
			dto.CanShowOnHover = content.CanShowOnHover;

			if (content.LastActivationTimeStamp != null)
				dto.LastActivationTimeStamp = content.LastActivationTimeStamp.Value.ToString(CultureInfo.InvariantCulture);

			// PreviousContainer info
			var prevContainer = content as ILayoutPreviousContainer;
			if (prevContainer?.PreviousContainer is ILayoutPaneSerializable paneSerializable)
			{
				dto.PreviousContainerId = paneSerializable.Id;
				dto.PreviousContainerIndex = content.PreviousContainerIndex;
			}
			else
			{
				var prevContainerId = ((ILayoutPreviousContainer)content).PreviousContainerId;
				if (prevContainerId != null)
				{
					dto.PreviousContainerId = prevContainerId;
					dto.PreviousContainerIndex = content.PreviousContainerIndex;
				}
			}
		}

		private void CopyPositionableToDto<T>(LayoutPositionableGroup<T> source, LayoutPositionableGroupDto dto)
			where T : class, ILayoutElement
		{
			var dockWidth = source.DockWidth;
			if (dockWidth.Value != 1.0 || !dockWidth.IsStar)
				dto.DockWidth = GridLengthConverter.ConvertToInvariantString(dockWidth.IsAbsolute ? new GridLength(source.FixedDockWidth) : dockWidth);

			var dockHeight = source.DockHeight;
			if (dockHeight.Value != 1.0 || !dockHeight.IsStar)
				dto.DockHeight = GridLengthConverter.ConvertToInvariantString(dockHeight.IsAbsolute ? new GridLength(source.FixedDockHeight) : dockHeight);

			dto.DockMinWidth = source.DockMinWidth;
			dto.DockMinHeight = source.DockMinHeight;
			dto.FloatingWidth = source.FloatingWidth;
			dto.FloatingHeight = source.FloatingHeight;
			dto.FloatingLeft = source.FloatingLeft;
			dto.FloatingTop = source.FloatingTop;
			dto.IsMaximized = source.IsMaximized;
		}

		private LayoutAnchorSideDto MapAnchorSideToDto(LayoutAnchorSide side)
		{
			var dto = new LayoutAnchorSideDto();
			if (side == null) return dto;

			foreach (var group in side.Children)
				dto.Children.Add(MapAnchorGroupToDto(group));

			return dto;
		}

		private LayoutAnchorGroupDto MapAnchorGroupToDto(LayoutAnchorGroup group)
		{
			var dto = new LayoutAnchorGroupDto
			{
				Id = ((ILayoutPaneSerializable)group).Id,
			};

			if (((ILayoutPreviousContainer)group).PreviousContainer is ILayoutPaneSerializable pane)
				dto.PreviousContainerId = pane.Id;

			foreach (var child in group.Children)
				dto.Children.Add(MapAnchorableToDto(child));

			return dto;
		}

		private LayoutFloatingWindowDto MapFloatingWindowToDto(LayoutFloatingWindow fw)
		{
			switch (fw)
			{
				case LayoutDocumentFloatingWindow dfw:
					return new LayoutDocumentFloatingWindowDto
					{
						RootPanel = dfw.RootPanel != null ? MapDocumentPaneGroupToDto(dfw.RootPanel) : null,
					};
				case LayoutAnchorableFloatingWindow afw:
					return new LayoutAnchorableFloatingWindowDto
					{
						RootPanel = afw.RootPanel != null ? MapAnchorablePaneGroupToDto(afw.RootPanel) : null,
					};
				default:
					throw new NotSupportedException($"Unknown floating window type: {fw?.GetType().Name}");
			}
		}

		private LayoutRoot MapDtoToRoot(LayoutRootDto dto)
		{
			var root = new LayoutRoot();

			if (dto.RootPanel != null)
				root.RootPanel = MapDtoToPanel(dto.RootPanel);

			if (dto.TopSide != null)
				root.TopSide = MapDtoToAnchorSide(dto.TopSide);
			if (dto.RightSide != null)
				root.RightSide = MapDtoToAnchorSide(dto.RightSide);
			if (dto.LeftSide != null)
				root.LeftSide = MapDtoToAnchorSide(dto.LeftSide);
			if (dto.BottomSide != null)
				root.BottomSide = MapDtoToAnchorSide(dto.BottomSide);

			root.FloatingWindows.Clear();
			if (dto.FloatingWindows != null)
			{
				foreach (var fwDto in dto.FloatingWindows)
					root.FloatingWindows.Add(MapDtoToFloatingWindow(fwDto));
			}

			root.Hidden.Clear();
			if (dto.Hidden != null)
			{
				foreach (var hiddenDto in dto.Hidden)
					root.Hidden.Add(MapDtoToAnchorable(hiddenDto));
			}

			return root;
		}

		private LayoutPanel MapDtoToPanel(LayoutPanelDto dto)
		{
			var panel = new LayoutPanel
			{
				Orientation = ParseOrientation(dto.Orientation),
				CanDock = dto.CanDock,
			};
			ApplyPositionableFromDto(dto, panel);

			if (dto.Children != null)
			{
				foreach (var child in dto.Children)
					panel.Children.Add(MapDtoToPanelChild(child));
			}

			return panel;
		}

		private ILayoutPanelElement MapDtoToPanelChild(LayoutPositionableGroupDto child)
		{
			switch (child)
			{
				case LayoutPanelDto p:
					return MapDtoToPanel(p);
				case LayoutDocumentPaneGroupDto dpg:
					return MapDtoToDocumentPaneGroup(dpg);
				case LayoutDocumentPaneDto dp:
					return MapDtoToDocumentPane(dp);
				case LayoutAnchorablePaneGroupDto apg:
					return MapDtoToAnchorablePaneGroup(apg);
				case LayoutAnchorablePaneDto ap:
					return MapDtoToAnchorablePane(ap);
				default:
					throw new NotSupportedException($"Unknown DTO panel child type: {child?.GetType().Name}");
			}
		}

		private LayoutDocumentPaneGroup MapDtoToDocumentPaneGroup(LayoutDocumentPaneGroupDto dto)
		{
			var group = new LayoutDocumentPaneGroup
			{
				Orientation = ParseOrientation(dto.Orientation),
			};
			ApplyPositionableFromDto(dto, group);

			if (dto.Children != null)
			{
				foreach (var child in dto.Children)
				{
					switch (child)
					{
						case LayoutDocumentPaneDto paneDto:
							group.Children.Add(MapDtoToDocumentPane(paneDto));
							break;
						case LayoutDocumentPaneGroupDto nestedDto:
							group.Children.Add(MapDtoToDocumentPaneGroup(nestedDto));
							break;
					}
				}
			}

			return group;
		}

		private LayoutDocumentPane MapDtoToDocumentPane(LayoutDocumentPaneDto dto)
		{
			var pane = new LayoutDocumentPane
			{
				ShowHeader = dto.ShowHeader,
			};
			((ILayoutPaneSerializable)pane).Id = dto.Id;
			ApplyPositionableFromDto(dto, pane);

			if (dto.Children != null)
			{
				foreach (var child in dto.Children)
				{
					switch (child)
					{
						case LayoutDocumentDto docDto:
							pane.Children.Add(MapDtoToDocument(docDto));
							break;
						case LayoutAnchorableDto anchDto:
							pane.Children.Add(MapDtoToAnchorable(anchDto));
							break;
					}
				}
			}

			return pane;
		}

		private LayoutAnchorablePaneGroup MapDtoToAnchorablePaneGroup(LayoutAnchorablePaneGroupDto dto)
		{
			var group = new LayoutAnchorablePaneGroup
			{
				Orientation = ParseOrientation(dto.Orientation),
			};
			ApplyPositionableFromDto(dto, group);

			if (dto.Children != null)
			{
				foreach (var child in dto.Children)
				{
					switch (child)
					{
						case LayoutAnchorablePaneDto paneDto:
							group.Children.Add(MapDtoToAnchorablePane(paneDto));
							break;
						case LayoutAnchorablePaneGroupDto nestedDto:
							group.Children.Add(MapDtoToAnchorablePaneGroup(nestedDto));
							break;
					}
				}
			}

			return group;
		}

		private LayoutAnchorablePane MapDtoToAnchorablePane(LayoutAnchorablePaneDto dto)
		{
			var pane = new LayoutAnchorablePane
			{
				Name = dto.Name,
			};
			((ILayoutPaneSerializable)pane).Id = dto.Id;
			ApplyPositionableFromDto(dto, pane);

			if (dto.Children != null)
			{
				foreach (var child in dto.Children)
					pane.Children.Add(MapDtoToAnchorable(child));
			}

			return pane;
		}

		private LayoutDocument MapDtoToDocument(LayoutDocumentDto dto)
		{
			var doc = new LayoutDocument
			{
				Description = dto.Description,
				CanMove = dto.CanMove,
			};
			ApplyContentFromDto(dto, doc);
			return doc;
		}

		private LayoutAnchorable MapDtoToAnchorable(LayoutAnchorableDto dto)
		{
			var anch = new LayoutAnchorable
			{
				CanHide = dto.CanHide,
				CanAutoHide = dto.CanAutoHide,
				AutoHideWidth = dto.AutoHideWidth,
				AutoHideHeight = dto.AutoHideHeight,
				AutoHideMinWidth = dto.AutoHideMinWidth,
				AutoHideMinHeight = dto.AutoHideMinHeight,
				CanDockAsTabbedDocument = dto.CanDockAsTabbedDocument,
				CanMove = dto.CanMove,
			};
			ApplyContentFromDto(dto, anch);
			return anch;
		}

		private void ApplyContentFromDto(LayoutContentDto dto, LayoutContent content)
		{
			if (dto.Title != null)
				content.Title = dto.Title;
			if (dto.ContentId != null)
				content.ContentId = dto.ContentId;
			content.IsSelected = dto.IsSelected;
			content.IsLastFocusedDocument = dto.IsLastFocusedDocument;
			content.FloatingLeft = dto.FloatingLeft;
			content.FloatingTop = dto.FloatingTop;
			content.FloatingWidth = dto.FloatingWidth;
			content.FloatingHeight = dto.FloatingHeight;
			content.IsMaximized = dto.IsMaximized;
			content.CanClose = dto.CanClose;
			content.CanFloat = dto.CanFloat;
			content.CanShowOnHover = dto.CanShowOnHover;

			if (dto.LastActivationTimeStamp != null)
				content.LastActivationTimeStamp = DateTime.Parse(dto.LastActivationTimeStamp, CultureInfo.InvariantCulture);

			if (dto.PreviousContainerId != null)
			{
				((ILayoutPreviousContainer)content).PreviousContainerId = dto.PreviousContainerId;
				content.PreviousContainerIndex = dto.PreviousContainerIndex;
			}
		}

		private void ApplyPositionableFromDto<T>(LayoutPositionableGroupDto dto, LayoutPositionableGroup<T> target)
			where T : class, ILayoutElement
		{
			if (dto.DockWidth != null)
				target.DockWidth = (GridLength)GridLengthConverter.ConvertFromInvariantString(dto.DockWidth);
			if (dto.DockHeight != null)
				target.DockHeight = (GridLength)GridLengthConverter.ConvertFromInvariantString(dto.DockHeight);
			target.DockMinWidth = dto.DockMinWidth;
			target.DockMinHeight = dto.DockMinHeight;
			target.FloatingWidth = dto.FloatingWidth;
			target.FloatingHeight = dto.FloatingHeight;
			target.FloatingLeft = dto.FloatingLeft;
			target.FloatingTop = dto.FloatingTop;
			target.IsMaximized = dto.IsMaximized;
		}

		private LayoutAnchorSide MapDtoToAnchorSide(LayoutAnchorSideDto dto)
		{
			var side = new LayoutAnchorSide();
			if (dto.Children != null)
			{
				foreach (var groupDto in dto.Children)
					side.Children.Add(MapDtoToAnchorGroup(groupDto));
			}

			return side;
		}

		private LayoutAnchorGroup MapDtoToAnchorGroup(LayoutAnchorGroupDto dto)
		{
			var group = new LayoutAnchorGroup();
			((ILayoutPaneSerializable)group).Id = dto.Id;

			if (dto.PreviousContainerId != null)
				((ILayoutPreviousContainer)group).PreviousContainerId = dto.PreviousContainerId;

			if (dto.Children != null)
			{
				foreach (var child in dto.Children)
					group.Children.Add(MapDtoToAnchorable(child));
			}

			return group;
		}

		private LayoutFloatingWindow MapDtoToFloatingWindow(LayoutFloatingWindowDto dto)
		{
			switch (dto)
			{
				case LayoutDocumentFloatingWindowDto dfw:
					var docFw = new LayoutDocumentFloatingWindow();
					if (dfw.RootPanel != null)
						docFw.RootPanel = MapDtoToDocumentPaneGroup(dfw.RootPanel);
					return docFw;

				case LayoutAnchorableFloatingWindowDto afw:
					var anchFw = new LayoutAnchorableFloatingWindow();
					if (afw.RootPanel != null)
						anchFw.RootPanel = MapDtoToAnchorablePaneGroup(afw.RootPanel);
					return anchFw;

				default:
					throw new NotSupportedException($"Unknown floating window DTO type: {dto?.GetType().Name}");
			}
		}

		private static Orientation ParseOrientation(string value)
		{
			if (string.IsNullOrEmpty(value))
				return Orientation.Horizontal;
			return (Orientation)Enum.Parse(typeof(Orientation), value, true);
		}
	}
}