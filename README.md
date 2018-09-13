## Master Branch
[![Build status](https://ci.appveyor.com/api/projects/status/kq2wyupx5hm7fok2/branch/master?svg=true)](https://ci.appveyor.com/project/Dirkster99/avalondock/branch/master)[![Release](https://img.shields.io/github/release/Dirkster99/avalondock.svg)](https://github.com/Dirkster99/avalondock/releases/latest)&nbsp;[Continues Integration](https://ci.appveyor.com/project/Dirkster99/AvalonDock/build/artifacts)


## Log4Net Branch
[![Build status](https://ci.appveyor.com/api/projects/status/bci7gpfvas2w32pr?svg=true)](https://ci.appveyor.com/project/Dirkster99/avalondock-63n50)&nbsp;[Continues Integration](https://ci.appveyor.com/project/Dirkster99/avalondock-63n50/build/artifacts)

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

My editor [Edi](https://github.com/Dirkster99/Edi) is based on AvlonDock.

This repository contains a feature added fork from:
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

# Mile Stone History

## Patch History for AvalonDock Version 3.4

- <a href="https://github.com/xceedsoftware/wpftoolkit/commit/24ecc09cf35e7095ad02d0beecfd5bafd83d0658">Commit from xceedsoftware/wpftoolkit for Version 3.4</a>
- <a href="https://github.com/Dirkster99/AvalonDock/commit/b6b8248cad226bc264855cdbd7e27b9c34303d71">Local Commit in master for Version 3.4</a>

### Reapplied Fixes

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
