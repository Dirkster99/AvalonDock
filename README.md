| [![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.svg)](http://nuget.org/packages/Dirkster.AvalonDock) | Dirkster.AvalonDock |
| ------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------- |
| [![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.Themes.Aero.svg)](http://nuget.org/packages/Dirkster.AvalonDock.Themes.Aero)              | Dirkster.AvalonDock.Themes.Aero            |
| [![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.Themes.Expression.svg)](http://nuget.org/packages/Dirkster.AvalonDock.Themes.Expression)  | Dirkster.AvalonDock.Themes.Expression      |
| [![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.Themes.Metro.svg)](http://nuget.org/packages/Dirkster.AvalonDock.Themes.Metro)            | Dirkster.AvalonDock.Themes.Metro           |
| [![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.Themes.VS2010.svg)](http://nuget.org/packages/Dirkster.AvalonDock.Themes.VS2010)          | Dirkster.AvalonDock.Themes.VS2010 |
| [![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.Themes.VS2013.svg)](http://nuget.org/packages/Dirkster.AvalonDock.Themes.VS2013)          | [Dirkster.AvalonDock.Themes.VS2013](https://github.com/Dirkster99/AvalonDock/wiki/WPF-VS-2013-Dark-Light-Demo-Client) |

![Net4](https://badgen.net/badge/Framework/.Net&nbsp;4/blue) ![NetCore3](https://badgen.net/badge/Framework/NetCore&nbsp;3/blue)

## Master Branch
[![Build status](https://ci.appveyor.com/api/projects/status/kq2wyupx5hm7fok2/branch/master?svg=true)](https://ci.appveyor.com/project/Dirkster99/avalondock/branch/master)[![Release](https://img.shields.io/github/release/Dirkster99/avalondock.svg)](https://github.com/Dirkster99/avalondock/releases/latest)&nbsp;[Continues Integration](https://ci.appveyor.com/project/Dirkster99/AvalonDock/build/artifacts)

<a href="https://github.com/Dirkster99/AvalonDock/issues">
    <img src="https://img.shields.io/github/issues-raw/Dirkster99/AvalonDock.svg?style=flat-square">
  </a>
  <a href="https://github.com/Dirkster99/AvalonDock/issues">
    <img src="https://img.shields.io/github/issues-closed-raw/Dirkster99/AvalonDock.svg?style=flat-square">
  </a><br/>

<a href="https://github.com/Dirkster99/AvalonDock/issues">
    <img src="https://img.shields.io/github/issues-pr-raw/Dirkster99/AvalonDock.svg?style=flat-square">
  </a>
  <a href="https://github.com/Dirkster99/AvalonDock/issues">
    <img src="https://img.shields.io/github/issues-pr-closed-raw/Dirkster99/AvalonDock.svg?style=flat-square">
  </a>
  
# AvalonDock
Support this project with a :star: -report an issue, or even better, place a pull request :mailbox: :blush:

My projects <a href="https://dirkster99.github.io/Edi/">Edi</a>, <a href="https://github.com/Dirkster99/Aehnlich">Aehnlich</a>, and [many others](https://github.com/search?p=4&q=%22dirkster.avalondock%22&type=Code) (open source or commercial) are powered by this project.

AvalonDock is a WPF Document and Tool Window layout container that is used to arrange documents
and tool windows in similar ways than many well known IDEs, such as, Eclipse, Visual Studio,
PhotoShop and so forth. Here are some CodeProject articles:

* [AvalonDock [2.0] Tutorial Part 1 - Adding a Tool Window](https://www.codeproject.com/Articles/483507/AvalonDock-Tutorial-Part-Adding-a-Tool-Windo)
* [AvalonDock [2.0] Tutorial Part 2 - Adding a Start Page](https://www.codeproject.com/Articles/483533/AvalonDock-Tutorial-Part-Adding-a-Start-Page)
* [AvalonDock [2.0] Tutorial Part 3 - AvalonEdit in AvalonDock](https://www.codeproject.com/Articles/570313/AvalonDock-Tutorial-Part-AvalonEdit-in-Avalo)
* [AvalonDock [2.0] Tutorial Part 4 - Integrating AvalonEdit Options](https://www.codeproject.com/Articles/570324/AvalonDock-Tutorial-Part-Integrating-AvalonE)
* [AvalonDock [2.0] Tutorial Part 5 - Load/Save Layout with De-Referenced DockingManager](https://www.codeproject.com/Articles/719143/AvalonDock-Tutorial-Part-Load-Save-Layout)

This repository contains **additional bug fixes and a feature added** fork for:
xceedsoftware/wpftoolkit version **3.2-3.6**. Version 4.0 and later are developed indepentently, which is why this library (version 4.0 and later) uses the namespaces and library names that were used in AvalonDock 2.0 and earlier versions. But most importantly, the usage of this AvalonDock project remains free for both, commercial and open source users.

There is also an open source repository https://github.com/dotnetprojects/WpfExtendedToolkit with a fixed and stable version of all other (other than AvalonDock) components from the WPFToolKit.

Be sure to checkout the <a href="https://github.com/Dirkster99/AvalonDock/wiki">Wiki for more details</a>.

## Building AvalonDock from Source

This project supports multitargeting frameworks (NetCore 3 and .Net 4). This means that it requires
Visual Studio Community 2019 or better to build.

# Feature Added - Dark and Light VS 2013 Theme

Please review the <a href="https://github.com/Dirkster99/AvalonDock/wiki">Project Wiki</a> to see more demo screenshots.
All screenshots below are from the <a href="https://github.com/Dirkster99/MLib">MLib</a> based VS 2013 Dark (Accent Color Gold)/Light (Accent Color Blue) theme on Windows 10. Similar theming results should be possible with other theming libraries since the implementation follow these <a href="https://www.codeproject.com/Articles/1236588/File-System-Controls-in-WPF-Version-III">guidelines</a>.

The Docking Buttons are [defined in XAML](https://github.com/Dirkster99/AvalonDock/wiki/OverlayWindow), which ensures a good looking image on all resolutions, even 4K or 8K, and enables us to color theme consistently with the Window 10 <b>Accent Color</b>.

<table width="100%">
   <tr>
      <td>Description</td>
      <td>Dark</td>
      <td>Light</td>
   </tr>
   <tr>
      <td>Dock Document</td>
      <td><img src="https://raw.githubusercontent.com/Dirkster99/Docu/master/AvalonDock/VS2013/AD_MLib/Dark/DockDocument.png" width="400"></td>
      <td><img src="https://raw.githubusercontent.com/Dirkster99/Docu/master/AvalonDock/VS2013/AD_MLib/Light/DockDocument.png" width="400"></td>
   </tr>
   <tr>
      <td>Dock Document</td>
      <td><img src="https://raw.githubusercontent.com/Dirkster99/Docu/master/AvalonDock/VS2013/AD_MLib/Dark/DockDocument_1.png" width="400"></td>
      <td><img src="https://raw.githubusercontent.com/Dirkster99/Docu/master/AvalonDock/VS2013/AD_MLib/Light/DockDocument_1.png" width="400"></td>

   </tr>
   <tr>
      <td>Dock Tool Window</td>
      <td><img src="https://raw.githubusercontent.com/Dirkster99/Docu/master/AvalonDock/VS2013/AD_MLib/Dark/DockToolWindow.png" width="400"></td>
      <td><img src="https://raw.githubusercontent.com/Dirkster99/Docu/master/AvalonDock/VS2013/AD_MLib/Light/DockToolWindow.png" width="400"></td>
   </tr>
   <tr>
      <td>Document</td>
      <td><img src="https://raw.githubusercontent.com/Dirkster99/Docu/master/AvalonDock/VS2013/AD_MLib/Dark/Document.png" width="400"></td>
      <td><img src="https://raw.githubusercontent.com/Dirkster99/Docu/master/AvalonDock/VS2013/AD_MLib/Light/Document.png" width="400"></td>
   </tr>
   <tr>
      <td>Tool Window</td>
      <td><img src="https://raw.githubusercontent.com/Dirkster99/Docu/master/AvalonDock/VS2013/AD_MLib/Dark/ToolWindow.png" width="400"></td>
      <td><img src="https://raw.githubusercontent.com/Dirkster99/Docu/master/AvalonDock/VS2013/AD_MLib/Light/ToolWindow.png" width="400"></td>
   </tr>
</table>

## Theming

Using the *AvalonDock.Themes.VS2013* theme is very easy with *Dark* and *Light* themes.
Just load *Light* or *Dark* brush resources in you resource dictionary to take advantage of existing definitions.

```XAML
    <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="/AvalonDock.Themes.VS2013;component/DarkBrushs.xaml" />
    </ResourceDictionary.MergedDictionaries>
```

```XAML
    <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="/AvalonDock.Themes.VS2013;component/LightBrushs.xaml" />
    </ResourceDictionary.MergedDictionaries>
```

These definitions do not theme all controls used within this library. You should use a standard theming library, such as:
- [MahApps.Metro](https://github.com/MahApps/MahApps.Metro),
- [MLib](https://github.com/Dirkster99/MLib), or
- [MUI](https://github.com/firstfloorsoftware/mui)

to also theme standard elements, such as, button and textblock etc.

# Mile Stone History

## Fixes & Features in master branch (not released to NuGet, yet)

- [# 151 Model.Root.Manager may be null in LayoutDocumentTabItem](https://github.com/Dirkster99/AvalonDock/issues/151) Thanx to [scdmitryvodich](https://github.com/scdmitryvodich)

## Fixes & Features  added in Version 4.1

- [Fix #137 BindingExpression in VS2013 theme](https://github.com/Dirkster99/AvalonDock/issues/137)

- [Feature Added: Auto resizing floating window to content](https://github.com/Dirkster99/AvalonDock/pull/146) [thanx to Erik Ovegård](https://github.com/eriove)

- Feature Added: Virtualizing Tabbed Documents and/or LayoutAnchorables [PR #143](https://github.com/Dirkster99/AvalonDock/pull/143) + [Virtualization Options](https://github.com/Dirkster99/AvalonDock/commit/1a45dbbe66c931e6c87ad769a9b269da4cb290ae)  [thanx to matko238](https://github.com/matko238)  
  - See ``DockingManager.IsVirtualizingAnchorable``, ``DockingManager.IsVirtualizingDocument``, and ``IsVirtualizing`` property on ``LayoutAnchorablePaneControl`` and ``LayoutDocumentPaneControl``.

- [Fixed Issue #149 Flicker/Lag when restoring floating window from Maximized state](https://github.com/Dirkster99/AvalonDock/issues/149) [thanx to skyneps](https://github.com/skyneps)

- [Fixed Issue #150 Restoring floating window position on multiple monitors uses wrong Point for Virtual Screen location](https://github.com/Dirkster99/AvalonDock/issues/150) [thanx to charles-roberts](https://github.com/charles-roberts)

## Fixes and Features added in Version 4.0

- [Fix #98 with floating window without a content #99](https://github.com/Dirkster99/AvalonDock/pull/99) Thanx to [scdmitryvodich](https://github.com/scdmitryvodich)

- Changed coding style to using TABS as indentation
- **Breaking Change** [Changed namespaces to AvalonDock (as authored originally in version 2.0 and earlier)](https://github.com/Dirkster99/AvalonDock/pull/102) See also [Issue #108](https://github.com/Dirkster99/AvalonDock/issues/108)

- [Fix #101 and new fix for #81 with docked pane becomes not visible.](https://github.com/Dirkster99/AvalonDock/issues/101) Thanx to [scdmitryvodich](https://github.com/scdmitryvodich)

- [Feature added: allow documents to be docked in a floating window](https://github.com/Dirkster99/AvalonDock/pull/107) Thanx to [amolf-se](https://github.com/amolf-se) [https://github.com/mkonijnenburg](mkonijnenburg) @ [http://www.amolf.nl](http://www.amolf.nl)

- [Feature added: AutoHideDelay property to control the time until an AutoHide window is reduced back to its anchored representation](https://github.com/Dirkster99/AvalonDock/pull/110) Thanx to [Alexei Stukov](https://github.com/Jiiks)

- [Fix #127 Controls cause memory leaks via event listener](https://github.com/Dirkster99/AvalonDock/issues/127)

- [Fix #111 AvalonDock.LayoutRoot doesn't know how to deserialize...](https://github.com/Dirkster99/AvalonDock/issues/111) Thanx to [scdmitryvodich](https://github.com/scdmitryvodich)

- [Fix #117 Dragging LayoutAnchoreable into outer docking buttons of floating document result in Exception](https://github.com/Dirkster99/AvalonDock/issues/117) Thanx to [scdmitryvodich](https://github.com/scdmitryvodich)

- [Fix #132 Drop FloatingDocumentWindow into DocumentPane is not consistent (when FloatingDocumentWindow contains LayoutAnchorable)](https://github.com/Dirkster99/AvalonDock/issues/132)

## More Patch History
Please review the **Path History** for more more information on patches and feaures in <a href="https://github.com/Dirkster99/AvalonDock/wiki/Patch-History">previously released versions of AvalonDock</a>.
