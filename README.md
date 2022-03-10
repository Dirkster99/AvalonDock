| Downloads                                                                                                                                               | NuGet Packages
| :------------------------------------------------------------------------------------------------------------------------------------------------------ | :--------------------------------------------------------------------------------
| [![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.svg)](http://nuget.org/packages/Dirkster.AvalonDock)                                      | [Dirkster.AvalonDock](http://nuget.org/packages/Dirkster.AvalonDock)
| [![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.Themes.Aero.svg)](http://nuget.org/packages/Dirkster.AvalonDock.Themes.Aero)              | [Dirkster.AvalonDock.Themes.Aero](http://nuget.org/packages/Dirkster.AvalonDock.Themes.Aero)
| [![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.Themes.Expression.svg)](http://nuget.org/packages/Dirkster.AvalonDock.Themes.Expression)  | [Dirkster.AvalonDock.Themes.Expression](http://nuget.org/packages/Dirkster.AvalonDock.Themes.Expression)
| [![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.Themes.Metro.svg)](http://nuget.org/packages/Dirkster.AvalonDock.Themes.Metro)            | [Dirkster.AvalonDock.Themes.Metro](http://nuget.org/packages/Dirkster.AvalonDock.Themes.Metro)
| [![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.Themes.VS2010.svg)](http://nuget.org/packages/Dirkster.AvalonDock.Themes.VS2010)          | [Dirkster.AvalonDock.Themes.VS2010](http://nuget.org/packages/Dirkster.AvalonDock.Themes.VS2010)
| [![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.Themes.VS2013.svg)](http://nuget.org/packages/Dirkster.AvalonDock.Themes.VS2013)          | [Dirkster.AvalonDock.Themes.VS2013](http://nuget.org/packages/Dirkster.AvalonDock.Themes.VS2013) (see [Wiki](https://github.com/Dirkster99/AvalonDock/wiki/WPF-VS-2013-Dark-Light-Demo-Client) )

![Net4](https://badgen.net/badge/Framework/.Net&nbsp;4/blue) ![NetCore3](https://badgen.net/badge/Framework/NetCore&nbsp;3/blue) ![Net4](https://badgen.net/badge/Framework/.NET&nbsp;5/blue)

## Master Branch
[![Build status](https://ci.appveyor.com/api/projects/status/kq2wyupx5hm7fok2/branch/master?svg=true)](https://ci.appveyor.com/project/Dirkster99/avalondock/branch/master)[![Release](https://img.shields.io/github/release/Dirkster99/avalondock.svg)](https://github.com/Dirkster99/avalondock/releases/latest)&nbsp;[Continuous Integration](https://ci.appveyor.com/project/Dirkster99/AvalonDock/build/artifacts)

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

## Fixes Added in Version 4.70.1

- [#336 Keep ActiveContent when switching RootPanel](https://github.com/Dirkster99/AvalonDock/pull/336)   (thanx to [Khaos66](https://github.com/Khaos66))
- [#334 fix #333 x64-issue: x86-specific functions are used when project is compiled for x64-architecture](https://github.com/Dirkster99/AvalonDock/pull/334)   (thanx to [Jan cuellius](https://github.com/cuellius))

## Features and Fixes Added in Version 4.70.0

- [#331 FixDockAsDocument fix bug with CanExecute and Execute for DockAsDocument](https://github.com/Dirkster99/AvalonDock/pull/331)   (thanx to [askgthb](https://github.com/askgthb))
- [#328 NullCheck for currentActiveContent ](https://github.com/Dirkster99/AvalonDock/pull/328)   (thanx to [Ben bbuerger](https://github.com/bbuerger))
- [#327 Add default width and height of LayoutAnchorable](https://github.com/Dirkster99/AvalonDock/pull/327)   (thanx to [Anders Chen](https://github.com/AndersChen123))
- [#326 A more complete fix to per-monitor DPI issues](https://github.com/Dirkster99/AvalonDock/pull/326)   (thanx to [Robin rwg0](https://github.com/rwg0))
- [#324 Navigator Window Accessibility fixes](https://github.com/Dirkster99/AvalonDock/pull/324)   (thanx to [Siegfried Pammer](https://github.com/siegfriedpammer))

## Features and Fixes Added in Version 4.60.1

- [#314 Fix NavigatorWindow not working if there is only one document](https://github.com/Dirkster99/AvalonDock/pull/314)   (thanx to [Siegfried Pammer](https://github.com/siegfriedpammer))
- [#308 Code Clean-Up Serialization](https://github.com/Dirkster99/AvalonDock/pull/308)   (thanx to [RadvileSaveraiteFemtika](https://github.com/RadvileSaveraiteFemtika))
- [#317 Aded LayoutItem null check when processing mouseMiddleClickButton](https://github.com/Dirkster99/AvalonDock/pull/317)    (thanx to [JuanCar Orozco](https://github.com/Skaptor))

## Features and Fixes Added in Version 4.60.0

- [#278 Rename pt-BR to pt (make Brazilian Portuguese default to Portuguese)](https://github.com/Dirkster99/AvalonDock/pull/278)   (thanx to [mpondo](https://github.com/mpondo))
- [#272 Fix Mismatched ResourceKey on VS2013 Theme](https://github.com/Dirkster99/AvalonDock/pull/272)   (thanx to [Reisen Usagi](https://github.com/usagirei))
- [#274 Support custom styles for LayoutGridResizerControl](https://github.com/Dirkster99/AvalonDock/pull/274)   (thanx to [mpondo](https://github.com/mpondo))
- [#276 Support minimizing floating windows independently of main window](https://github.com/Dirkster99/AvalonDock/pull/276)   (thanx to [mpondo](https://github.com/mpondo))
- [#284 Vs2013 theme improvement](https://github.com/Dirkster99/AvalonDock/pull/284)   (thanx to [oktrue](https://github.com/oktrue))
- [#288 Fix close from taskbar for floating window](https://github.com/Dirkster99/AvalonDock/pull/288)   (thanx to [mpondo](https://github.com/mpondo))
- [#291 Fix Issue #281 floating window host: UI automation name](https://github.com/Dirkster99/AvalonDock/pull/291)   (thanx to [rmadsen-ks](https://github.com/rmadsen-ks))

## Features and Fixes Added in Version 4.51.1

- [#262 Contextmenus on dpi-aware application have a wrong scaling](https://github.com/Dirkster99/AvalonDock/issues/262)   (thanx to [moby42](https://github.com/moby42))
- [#259 Fixing problems with tests running with XUnit StaFact](https://github.com/Dirkster99/AvalonDock/pull/259)   (thanx to [Erik Ovegård](https://github.com/eriove))

- [#266 Adding a key for AnchorablePaneTitle](https://github.com/Dirkster99/AvalonDock/pull/266)   (thanx to [Zachary Canann](https://github.com/zcanann))

- [#267 Optional show hidden LayoutAnchorable on hover](https://github.com/Dirkster99/AvalonDock/pull/267)   (thanx to [Cory Todd](https://github.com/corytodd))

## Features Added in PRE-VIEW Version 4.51.0

- [#214 Migrate from netcoreapp3.0 to net5.0-windows](https://github.com/Dirkster99/AvalonDock/pull/214)  (thanx to [Magnus Lindhe](https://github.com/mgnslndh))

## Fixes added in Version 4.50.3

- [#163 IsSelected vs IsActive behavior changed from 3.x to 4.1/4.2?](https://github.com/Dirkster99/AvalonDock/issues/163) (thanx to [triman](https://github.com/triman))

- [#244 Right click on tab header closes tab unexpectedly](https://github.com/Dirkster99/AvalonDock/issues/244) (thanx to [Olly Atkins](https://github.com/oatkins))

- [#208 Maximized floating windows sit under the task bar](https://github.com/Dirkster99/AvalonDock/issues/208) (thanx to [Flynn1179](https://github.com/Flynn1179))

- [#255 Don't create FloatingWindows twice](https://github.com/Dirkster99/AvalonDock/pull/255) (thanx to [Khaos66](https://github.com/Khaos66))

## Fixes added in Version 4.50.2

- [#221 Default window style interfere with resizer window](https://github.com/Dirkster99/AvalonDock/issues/221) (thanx to [Magnus Lindhe](https://github.com/mgnslndh))
- ~~[#224 Reverted Fixed a bug that freezed when changing DocumentPane Orientation](https://github.com/Dirkster99/AvalonDock/pull/224) (thanx to [sukamoni](https://github.com/sukamoni))  
  See pull request for issues with this PR~~

- [#240 NullReferenceException in LayoutDocumentControl.OnModelChanged](https://github.com/Dirkster99/AvalonDock/issues/240) (thanx to [Khaos66](https://github.com/Khaos66))  
- [#225 Keyboard up/down in textbox in floating anchorable focusing DropDownControlArea](https://github.com/Dirkster99/AvalonDock/issues/225) (thanx to [Muhahe](https://github.com/Muhahe) [LyonJack](https://github.com/LyonJack) [bdachev](https://github.com/bdachev))
- [#229 Ensure DocumentPaneGroup (fix crash when documentpane on layoutGroup)](https://github.com/Dirkster99/AvalonDock/pull/229) (thanx to [sukamoni](https://github.com/sukamoni))  

## Fixes added in Version 4.50.1

- [#210 LayoutAnchorable with CanDockAsTabbedDocument="False" docks to LayoutDocumentPane when Pane is empty](https://github.com/Dirkster99/AvalonDock/issues/210) (thanx to [Łukasz Holetzke](https://github.com/goldie83))
- [#195 DocumentClosed event issue](https://github.com/Dirkster99/AvalonDock/issues/195) (thanx to [Skaptor](https://github.com/Skaptor))
- [#205 Fix issue where the ActiveContent binding doesn't update two ways when removing a document.](https://github.com/Dirkster99/AvalonDock/pull/205) (thanx to [PatrickHofman](https://github.com/PatrickHofman))

## Fixes added in Version 4.5

- [#199 Add to LayoutDocument CanHide property returning false](https://github.com/Dirkster99/AvalonDock/pull/199) (thanx to [bdachev](https://github.com/bdachev))
- [#138 Trying dock a floating window inside a document pane leads to its disappearing of window's content.](https://github.com/Dirkster99/AvalonDock/pull/138) (thanx to [cuellius](https://github.com/https://github.com/cuellius))
- [#197 [Bug] Tabs start getting dragged around if visual tree load times are too high](https://github.com/Dirkster99/AvalonDock/pull/138) (thanx to [X39](https://github.com/https://github.com/X39))
- [Bug fix for issue #194 App doesn't close after LayoutAnchorable AutoHide and docking it again](https://github.com/Dirkster99/AvalonDock/pull/203) (thanx to [sphet](https://github.com/https://github.com/sphet))

## Fixes & Features added in Version 4.4

- [#182 CanClose property of new LayoutAnchorableItem is different from its LayoutAnchorable](https://github.com/Dirkster99/AvalonDock/pull/183)  (thanx to [skyneps](https://github.com/skyneps))
- [#184 All documents disappear if document stops close application in Caliburn.Micro](https://github.com/Dirkster99/AvalonDock/issues/184)  (thanx to [ryanvs](https://github.com/ryanvs))

- Thanx to [bdachev](https://github.com/bdachev):  
  - [#186 Raise PropertyChanged notification when LayoutContent.IsFloating changes](https://github.com/Dirkster99/AvalonDock/pull/186) (ensure change of the [IsFloating](https://github.com/Dirkster99/AvalonDock/wiki/LayoutContent#properties) property when the Documents state changes)  
  - [#187 Allow to serialize CanClose if set to true for LayoutAnchorable instance](https://github.com/Dirkster99/AvalonDock/pull/187)  
  - [#188 Handle CanClose and CanHide in XAML](https://github.com/Dirkster99/AvalonDock/pull/188)  
  - [#190 Added additional check in LayoutGridControl.UpdateRowColDefinitions to avoid exception.](https://github.com/Dirkster99/AvalonDock/pull/190)  
  - [#192 Default MenuItem style not changed by VS2013 Theme](https://github.com/Dirkster99/AvalonDock/pull/192)


- Removed the additional [ToolTip](https://github.com/Dirkster99/AvalonDock/commit/5554de5c4bfadc37f974ba29803dc792b54f00d0) and [ContextMenu](https://github.com/Dirkster99/AvalonDock/commit/103e1068bc9f5bae8fef275a0e785393b4115764) styles from the Generic.xaml in VS2013 [more details here](https://github.com/Dirkster99/AvalonDock/pull/170#issuecomment-674253874)
- [#189 Removal of DictionaryTheme breaks my application](https://github.com/Dirkster99/AvalonDock/issues/189)  (thanx to [hamohn](https://github.com/hamohn))

## Fixes & Features added in Version 4.3

- Localized labels in [NavigatorWindow](https://github.com/Dirkster99/AvalonDock/wiki/NavigatorWindow)

- [#170 Several Improvements](https://github.com/Dirkster99/AvalonDock/pull/170) (thanx to [刘晓青 LyonJack](https://github.com/LyonJack))  
  - Improved VS 2013 Theme and ease of reusing controls  
  - [Fix Issue #85 Floating Window Title Flashing](https://github.com/Dirkster99/AvalonDock/issues/85)  
  - [Fix Issue #71 Hiding and showing anchorable in document's pane throws an exception](https://github.com/Dirkster99/AvalonDock/issues/71)  
  - [Fix Issue #135 ActiveContent not switching correctly for floating window](https://github.com/Dirkster99/AvalonDock/issues/135)  
  - [Fix Issue #165 ActiveContent not stable](https://github.com/Dirkster99/AvalonDock/issues/165)  
  - [Fix Issue #171 LayoutDocument leaks on close](https://github.com/Dirkster99/AvalonDock/issues/171)  
  - **Breaking Change**  
    [Fix Issue #174 The SetWindowSizeWhenOpened Feature is broken](https://github.com/Dirkster99/AvalonDock/issues/174)
  - [Fix Issue #177 ToolBar TabItem color error](https://github.com/Dirkster99/AvalonDock/issues/177)

- [#59 InvalidOperationException when deserializing layout](https://github.com/Dirkster99/AvalonDock/issues/59#issuecomment-642934204)

- [#136 Layout "locking" method for Anchorables (tool windows) Part II via Style of LayoutAnchorableItem](https://github.com/Dirkster99/AvalonDock/issues/136)

- [#136 Layout "locking" method for Anchorables (tool windows) Part III Added CanDock for LayoutAnchorable and LayoutDocument](https://github.com/Dirkster99/AvalonDock/issues/136)
    [commit 6b611fa7fdce4f6dcfed1cf00c3b9193000ffe16](https://github.com/Dirkster99/AvalonDock/commit/6b611fa7fdce4f6dcfed1cf00c3b9193000ffe16)

- [#169 - Autohide LayoutAnchorable causes CPU load on idle](https://github.com/Dirkster99/AvalonDock/issues/169)

## Fixes & Features  added in Version 4.2

- [#136 Layout "locking" method for Anchorables (tool windows)](https://github.com/Dirkster99/AvalonDock/issues/136)

- [# 159 Docking manager in TabControl can cause InvalidOperationException](https://github.com/Dirkster99/AvalonDock/issues/159)

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
