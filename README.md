| ------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------- |
| [![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.svg)](http://nuget.org/packages/Dirkster.AvalonDock)                                      | AvalonDock                        |
| [![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.Themes.Expression.svg)](http://nuget.org/packages/Dirkster.AvalonDock.Themes.Expression)  | AvalonDock.Themes.Expression      |
| [![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.Themes.Metro.svg)](http://nuget.org/packages/Dirkster.AvalonDock.Themes.Metro)            | AvalonDock.Themes.Metro           |
| [![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.Themes.VS2013.svg)](http://nuget.org/packages/Dirkster.AvalonDock.Themes.VS2013)          | Dirkster.AvalonDock.Themes.VS2013 |

## Master Branch
[![Build status](https://ci.appveyor.com/api/projects/status/kq2wyupx5hm7fok2/branch/master?svg=true)](https://ci.appveyor.com/project/Dirkster99/avalondock/branch/master)[![Release](https://img.shields.io/github/release/Dirkster99/avalondock.svg)](https://github.com/Dirkster99/avalondock/releases/latest)&nbsp;[Continues Integration](https://ci.appveyor.com/project/Dirkster99/AvalonDock/build/artifacts)

# AvalonDock

Support this project by setting a STAR, reporting an issue, or even better, placing a pull request.

My IDE called <a href="https://dirkster99.github.io/Edi/">Edi</a> is powered by this project.

AvalonDock is a WPF Document and Tool Window layout container that is used to arrange documents
and tool windows in similar ways than many well known IDEs, such as, Eclipse, Visual Studio,
PhotoShop and so forth. Here are some CodeProject articles:

* [AvalonDock [2.0] Tutorial Part 1 - Adding a Tool Window](https://www.codeproject.com/Articles/483507/AvalonDock-Tutorial-Part-Adding-a-Tool-Windo)
* [AvalonDock [2.0] Tutorial Part 2 - Adding a Start Page](https://www.codeproject.com/Articles/483533/AvalonDock-Tutorial-Part-Adding-a-Start-Page)
* [AvalonDock [2.0] Tutorial Part 3 - AvalonEdit in AvalonDock](https://www.codeproject.com/Articles/570313/AvalonDock-Tutorial-Part-AvalonEdit-in-Avalo)
* [AvalonDock [2.0] Tutorial Part 4 - Integrating AvalonEdit Options](https://www.codeproject.com/Articles/570324/AvalonDock-Tutorial-Part-Integrating-AvalonE)
* [AvalonDock [2.0] Tutorial Part 5 - Load/Save Layout with De-Referenced DockingManager](https://www.codeproject.com/Articles/719143/AvalonDock-Tutorial-Part-Load-Save-Layout)

This repository contains **additional bug fixes and a feature added** fork from:
https://github.com/xceedsoftware/wpftoolkit

Be sure to checkout the <a href="https://github.com/Dirkster99/AvalonDock/wiki">Wiki for more details</a>. The repository also contains a Log4Net branch for debugging interactive issues with a close enough branch. There are also Log4Net demo executables in the 3.4.01 (or later) realease for additional debugging fun.

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

Using the *Xceed.Wpf.AvalonDock.Themes.VS2013* theme is very easy with *Dark* and *Light* themes.
Just load *Light* or *Dark* brush resources in you resource dictionary to take advantage of existing definitions.

```XAML
    <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="/Xceed.Wpf.AvalonDock.Themes.VS2013;component/DarkBrushs.xaml" />
    </ResourceDictionary.MergedDictionaries>
```

```XAML
    <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="/Xceed.Wpf.AvalonDock.Themes.VS2013;component/LightBrushs.xaml" />
    </ResourceDictionary.MergedDictionaries>
```

These definitions do not theme all controls used within this library. You should use a standard theming library, such as:
- [MahApps.Metro](https://github.com/MahApps/MahApps.Metro),
- [MLib](https://github.com/Dirkster99/MLib), or
- [MUI](https://github.com/firstfloorsoftware/mui)

to also theme standard elements, such as, button and textblock etc.

# Mile Stone History

## Patch History for AvalonDock Version 3.5

### Increased to Version 3.5.1
- <a href="https://github.com/Dirkster99/AvalonDock/commit/8cb6565db294ed3fcb2ac502172f059d740f013d">@Dirkster99</a>

### Fixed Close Button Position in Generic Theme
- <a href="https://github.com/xceedsoftware/wpftoolkit/pull/1184/files">#1184</a>
- <a href="https://github.com/Dirkster99/AvalonDock/commit/11709c6cbfc190e920fc62302b5922d666dbac29">@Dirkster99</a>

### DockingManager: ModelChange event happens before new LayoutDocumentItem is added
- <a href="https://github.com/xceedsoftware/wpftoolkit/issues/1430">#1430</a>
- <a href="https://github.com/Dirkster99/AvalonDock/commit/6d23da0ce95236ee77e53238cf60e679a3c8fb0e">@Dirkster99</a>

### Added resource file for AvalonDock for nl-BE
- <a href="https://github.com/xceedsoftware/wpftoolkit/issues/1424">#1424</a>
- <a href="https://github.com/Dirkster99/AvalonDock/commit/70995417714f37d84659854245f0caad8e3c6d39">@Dirkster99</a>

### Fixed Crash while loading FLOATING DOCUMENT

Fixed Crash while loading (Deserialize) a layout with FLOATING DOCUMENT window.

NullReferenceException in LayoutDocumentFloatingWindowControl.OnInitialized:
Initialization in 2nd constructor was missing:
   _model = model;
   UpdateThemeResources();
   
- <a href="https://github.com/xceedsoftware/wpftoolkit/issues/1442">Issue/Resolution is similar to #1442</a>
- <a href="https://github.com/Dirkster99/AvalonDock/commit/66fb84bfc1a99ec95d643245a3ecc0006b39f7ed">@Dirkster99</a>

### Removed unused private field in LayoutAutoHideWindowControl
- <a href="https://github.com/Dirkster99/AvalonDock/commit/c4b60646fc0e2beb166f682a1f0e9b8e0532ec8d">@Dirkster99</a>
- <a href="https://github.com/Dirkster99/AvalonDock/commit/1304e0c4fad40f2d057858712c3c5be86bf46509">@Dirkster99</a>

### Prevent crash from setting negative size
- <a href="https://github.com/RecursiveNerd/wpftoolkit/commit/38d36f236727cf48ab0e82e9794e1f927d059695">RecursiveNerd/wpftoolkit</a>
- <a href="https://github.com/Dirkster99/AvalonDock/commit/19a07008cfeb352bba8bf371305e6be83541806d">@Dirkster99</a>

### Fix for issue #1379 as suggested by RecursiveNerd
- <a href="https://github.com/xceedsoftware/wpftoolkit/issues/1379">#1379</a>
- <a href="https://github.com/Dirkster99/AvalonDock/commit/f8be3fa041904b2cb9e55299a4497b288b145d00">@Dirkster99</a>

### Update zh-Hans translation #1383
- <a href="https://github.com/xceedsoftware/wpftoolkit/pull/1383/files">#1383</a>
- <a href="https://github.com/Dirkster99/AvalonDock/commit/f8167d3ab094f65454e702347d50ac5cd1427da2">@Dirkster99</a>

### AvalonDock czech localization #1396
- <a href="https://github.com/xceedsoftware/wpftoolkit/pull/1396/files">#1396</a>
- <a href="https://github.com/Dirkster99/AvalonDock/commit/c67e8583b596d7deacd0bda79f2448a47b5fc6d7">@Dirkster99</a>

### AvalonDock fixed misspelling when serializing DockMinWidth/DockMinHeight
- <a href="https://github.com/xceedsoftware/wpftoolkit/pull/1212">#1212</a>
- <a href="https://github.com/Dirkster99/AvalonDock/commit/9230aad671817ef6e48e6b5dc59a98a583fe1bed">@Dirkster99</a>

### Clear Bindings #1360
- <a href="https://github.com/xceedsoftware/wpftoolkit/issues/1360">#1360</a>
- <a href="https://github.com/Dirkster99/AvalonDock/commit/33655c686faeded1f4bef8d22da8960a2a101b50">@Dirkster99</a>

### Theming ContextMenuEx in Metro
Re-styling this in AvalonDock since the menu on the drop-down button for more documents is otherwise black
- <a href="http://avalondock.codeplex.com/workitem/15743">http://avalondock.codeplex.com/workitem/15743</a>
- <a href="https://github.com/Dirkster99/AvalonDock/commit/fd2186e31e34fa2c62dee7d0bd356f86c8f4729a">@Dirkster99</a>

### Drag and Drop on (scaled) 4K display
Drag and Drop of Document or ToolWindow content does not always work on (scaled) 4K display
- <a href="https://github.com/xceedsoftware/wpftoolkit/issues/1357">#1357</a>
- <a href="https://github.com/Dirkster99/AvalonDock/issues/6">#6</a>
- <a href="https://github.com/Dirkster99/AvalonDock/commit/86edf83d4232638eb0f5827f4b554dca7990f914">@Dirkster99</a>

### Crash Loading (Deserialized) Floating Tool Window Layout

Fixed Crash while loading (Deserialize) a layout from FLOATING tool window.
NullReferenceException in LayoutAnchorableFloatingWindowControl.OnInitialized

- <a href="https://github.com/xceedsoftware/wpftoolkit/issues/1442">#1442</a>
- <a href="https://github.com/Dirkster99/AvalonDock/commit/4e3b39470228fd8d36cb069eccdc71256ff8b359">@Dirkster99</a>

### LayoutRoot doesn't notify change for Children or ChildrenCount #1313
- <a href="https://github.com/xceedsoftware/wpftoolkit/issues/1313">#1313</a>
- <a href="https://github.com/Dirkster99/AvalonDock/commit/d65e9a9019867b1019810329727c154cc20b4c90">@Dirkster99</a>

### Fixed height of titles of floating windows #1203
- <a href="https://github.com/xceedsoftware/wpftoolkit/issues/1203">#1203</a>
- <a href="https://github.com/Dirkster99/AvalonDock/commit/5c22b1ea18a7b5b25b6a9cb328a1b4210474cbea">@Dirkster99</a>

### Initial AvalonDock Version 3.5
- <a href="https://github.com/xceedsoftware/wpftoolkit/tree/73b2e988dea6ea2e64bb11e8783b6bde66219ee1">Taken from this commit</a>
- <a href="https://github.com/Dirkster99/AvalonDock/commit/89823735ab60ab90a84c6cae7c594e9ee7fe858c">@Dirkster99</a>

## Patch History for AvalonDock Version 3.4

- <a href="https://github.com/xceedsoftware/wpftoolkit/commit/24ecc09cf35e7095ad02d0beecfd5bafd83d0658">Commit from xceedsoftware/wpftoolkit for Version 3.4</a>
- <a href="https://github.com/Dirkster99/AvalonDock/commit/b6b8248cad226bc264855cdbd7e27b9c34303d71">Local Commit in master for Version 3.4</a>

### Reapplied or New Fixes

<ul>
<li><a href="https://github.com/Dirkster99/AvalonDock/issues/11">DockingManager: ModelChange event happens before new LayoutDocumentItem is added</a>&nbsp;<a href="https://github.com/Dirkster99/AvalonDock/commit/e00b6ca88b0dbe8a7ab1803d0d700fde8ee51f8a">-&gt;</a></li>

<li><a href="https://github.com/xceedsoftware/wpftoolkit/issues/1424">Added resource file for AvalonDock for nl-BE</a>&nbsp;<a href="https://github.com/Dirkster99/AvalonDock/commit/6a796caa7270589cff9248e84cd8f60a03836f54">-&gt;</a></li>

<li><a href="https://github.com/Dirkster99/AvalonDock/issues/9">Fixed Auto Hide Layoutanchorabeles to left, right, top, or bottom of DockingManager layout</a>&nbsp;<a href="https://github.com/Dirkster99/AvalonDock/commit/ece04af23ed04a5ad4d8f2277e96bc7c41042b2c">-&gt;</a></li>
<li><a href="https://github.com/xceedsoftware/wpftoolkit/pull/1396">Corrected localization in zh-Hans in Version 3.4.0.6</a><br/>
<a href="https://github.com/xceedsoftware/wpftoolkit/pull/1383">Added Czech localization in Version 3.4.0.5</a><br/>
<a href="https://github.com/Dirkster99/AvalonDock/commit/ef4d5c9e11470025ff7d5577cac1e32ef67c39bf">-&gt;</a><br/>
</li>
<li><b><a href="https://github.com/xceedsoftware/wpftoolkit/issues/1379">Fix for issue #1379 AvalonDockCrash in LayoutGridControl after Resize - ArgumentException as suggested by RecursiveNerd</a>&nbsp;<a href="https://github.com/Dirkster99/AvalonDock/commit/79f3416ce520e74bba0371a0fb82c848fd3176c7">-&gt;</a></b></li>
</ul>

<b><a href="https://github.com/xceedsoftware/wpftoolkit/issues/1203">Fixed height of titles of floating windows #1203</a></b>

- Turn Height into MinHeight <a href="https://github.com/Dirkster99/AvalonDock/commit/1c2163e03178ed1a0cc3fafc57d0119b3a238d4a">-&gt;</a>
- Fix on UseLayoutRounding="True" <a href="https://github.com/Dirkster99/AvalonDock/commit/517ba40a63c7eaf080e1d95d65d28dd718fe4822">-&gt;</a>
<a href="https://github.com/Dirkster99/AvalonDock/commit/b567265c48b54a3abe6352d4f29b8ca6289c3e54">Commit</a>

<b><a href="https://github.com/xceedsoftware/wpftoolkit/pull/1311">System.InvalidOperationException when window docking #1311</a></b>
   <a href="https://github.com/xceedsoftware/wpftoolkit/issues/1310">Check before Close in InternalClose #1310</a>
<a href="https://github.com/Dirkster99/AvalonDock/commit/ca7b02b76a430bb4b911d94f88c9136090f7690e">Commit</a>

<b><a href="https://github.com/xceedsoftware/wpftoolkit/issues/1313">LayoutRoot doesn't notify change for Children or ChildrenCount #1313</a></b>
<a href="https://github.com/Dirkster99/AvalonDock/commit/75b8b54a300c01dad731771e15cbce24d30b9012">Commit</a>

- descendants of LayoutElement notify when their Children and ChildrenCount properties change

- Fix for Issue <a href="https://github.com/Dirkster99/AvalonDock/issues/6">#6 Drag and Drop of Document or ToolWindow content does not always work on (scaled) 4K display</a>

- Fix for Issue <a href="https://github.com/xceedsoftware/wpftoolkit/issues/1360">#1360 Clear Bindings</a>

## Patch History for AvalonDock Version 3.3

- (FIXED in 3.4) AvalonDock Bug after reloading layout <a href="https://github.com/Dirkster99/AvalonDock/wiki/(Fixed)-AvalonDock-Bug-after-reloading-layout">-&gt;</a>

- (FIXED in 3.4) Save Tool Window to Bottom and Reload does not work <a href="https://github.com/Dirkster99/AvalonDock/wiki/AvalonDock-Bug2">-&gt;</a>

## Patch History for AvalonDock Version 3.2

- Fixing an old styling issue on the Document Context Menu which appears with
  white background when styling for dark <a href="https://github.com/Dirkster99/AvalonDock/commit/105e9d9519b2a711500245f0d6d6c517fce98109">-&gt;</a> 

- Applied fix to LayoutAnchorable drawn behind LayoutDocument since v3.1 <a href="https://github.com/Dirkster99/AvalonDock/commit/5a26edaef16063f01ca2d6a353a239ee84fa3c63">-&gt;</a>

- Fixed Close Button Position in Generic Theme <a href="https://github.com/Dirkster99/AvalonDock/commit/fa81b3f9f7e043a3dca3fa1007c23b8d4f229675">-&gt;</a>

- Turned Height in MinHeight <a href="https://github.com/Dirkster99/AvalonDock/commit/1c2163e03178ed1a0cc3fafc57d0119b3a238d4a">-&gt;</a>

- Added fix on UseLayoutRounding="True" <a href="https://github.com/Dirkster99/AvalonDock/commit/517ba40a63c7eaf080e1d95d65d28dd718fe4822">-&gt;</a>

- Correct Build Configs for Any CPU, x86, x64 <a href="https://github.com/Dirkster99/AvalonDock/commit/282fdf296745ad4ccbd1986d938ffea9efe0aa9c">-&gt;</a>

- Fixed DocManager.Layout Properties null after Layout load:
  LeftSide, TopSide, BottomSide, RightSide null bug after loading layout <a href="https://github.com/Dirkster99/AvalonDock/commit/06b9682c1d5cfb9e895f63fdbc2ae6e9532dfc53">-&gt;</a>

# Initial Commit
https://github.com/Dirkster99/AvalonDock/commit/4bcb07f45e89e416257e668a209b238a26e5f414

in this repository was taken from the last Edi build: 131594
<a href="http://edi.codeplex.com/releases/view/131594">from CodePlex</a>
