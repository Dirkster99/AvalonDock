# AvalonDock on LibreWPF: 合成输入拖拽 flake、窗口生命周期崩溃与修复记录

> 本文档是 LibreWPF（`/Users/lextm/wpf-tools/librewpf`，wpf-progpu fork）与 AvalonDock
> 集成问题的研究笔记。旧版内容（splitter-drag bug 三连修）保留在文末"历史"章节。
> 本文记录 2026-08 对 **native drag flake** 与 **窗口销毁崩溃** 的完整调查：诊断方法、
> 根因链条、已实施的修复、以及 LibreWPF 源码修改 → 本地 feed 替换的完整流程。

## 1. 背景：测试基础设施与事件链路

DropTargetZone 集成测试用 **cliclick**（macOS 合成鼠标事件）驱动真实拖拽。事件链路：

```
cliclick (CGEventPost 注入 kCGHIDEventTap)
  → GLFW cocoa 窗口 (libglfw.3.dylib, 3.4.0, Silk.NET 2.23.0)
  → Silk.NET Input 回调 (SilkNetWpfInputService)
  → LibreWPF WPF 兼容层 (ProGpuWpfWindowHost 转发)
  → WPF 输入管线 (Mouse.LeftButton / MouseMove 等)
  → AvalonDock (LayoutFloatingWindowControl 原生拖拽检测 + DragService + OverlayWindow)
```

**关键事实**（实测）：
- cliclick 的 `dd`/`dm`/`du` 是**合成事件**（`CGEventPost`），**不更新物理 HID 按钮状态**。
- `MacOSCursorService.IsLeftButtonDown()`（AvalonDock/Platform）走 `CGEventSourceButtonState`
  —— 读物理状态 —— 对 cliclick 注入的事件**恒返回 false**（实测诊断日志确认 `physDown=False`）。
- WPF 的 `Mouse.LeftButton` 只从**事件管线**更新 —— 合成 `dd` 事件被 WPF 处理后才会变 Pressed。

## 2. Drag flake：cliclick 合成 `dd` 事件未被 WPF 处理

### 症状

完整测试（22 个 zone/preview 用例）随机 1-2 个失败，失败模式二选一：

- `compass never showed drop target 'X' during discovery` —— discovery 阶段 8s 内 compass 空；
- `live drop target 'X' was not current before release; Target=..., Targets=[]` —— 拖到目标中心后
  `currentDropTarget` 从未出现。

失败随机落在不同 zone（DockLeft/DockRight/DocumentPaneDockRight/AnchorablePaneDockTop 等都出现过），
且与断言无关（dock 根本没发生）。

### 复现

`/var/folders/.../T/opencode/drag-diag4.sh` 完全模拟测试流程（position-main-window →
reset → float → discovery drag → 第二次 drag 到 DockLeft 中心 → release），20 次一轮：
**失败率稳定 15-25%**（15-17/20 成功）。同时抓 `avd.query.drag-state` 现场。

### 失败现场（诊断数据）

失败时 `drag-state` 的关键字段：

```json
{
  "overlayLeft": null, "overlayTop": null, ...   // overlay 从未创建
  "currentDropTarget": null,
  "left": 560, "top": 718,                        // 浮动窗被拖动过（tick 在跑）
  "currentPointer": {"X": 658, "Y": 735}          // 指针在 manager 内
}
```

两种子模式：
- **子模式 A**：浮动窗移动了（tick 更新 Left/Top）但 overlay 从未创建（`UpdateMouseLocation`
  从未命中 host）；
- **子模式 B**：浮动窗完全没动（`left/top` 保持 press 前位置）。

给 `LayoutFloatingWindowControl.OnPortableNativeLocationChanged` 加临时诊断日志后拿到**决定性证据**：

```
[DRAGDIAG] native move ignored: leftBtn=Released physDown=False at 658,735   (×36)
```

**cliclick 的 `dd` 事件没有到达 WPF**（`Mouse.LeftButton=Released`）→ 窗口移动时
`buttonDown` 检查（`Mouse.LeftButton==Pressed || IsLeftButtonDown()`）为 false → 拖拽不启动。

### 根因链

1. cliclick `dd` 注入合成按下事件（`CGEventPost`，kCGHIDEventTap）。
2. 早期曾把失败归因于 macOS “首次点击被激活消耗”。后续核对实际使用的 GLFW 3.4
   官方 Cocoa 源码后，这个解释不能成立为既定根因：`GLFWContentView` 明确实现了
   `acceptsFirstMouse:` 并返回 `YES`。非 key 窗口状态仍会改变事件时序，但不能再声称
   GLFW 没有 first-mouse 传递路径；`dd` 丢失的精确位置仍需 native callback trace 证明。
3. 即使事件被处理，`CGEventSourceButtonState`（物理状态）对合成事件恒 false，
   与 WPF 事件状态（`Mouse.LeftButton`）之间存在**处理时序竞态**：
   - tick（16ms DispatcherTimer）若在注入事件被 WPF 处理**之前**检查按钮状态
     → 误判"按钮已释放" → `EndPortableDrag` 立即中止 drag。

### 已实施的修复

**AvalonDock**（`src/Components/AvalonDock/Controls/LayoutFloatingWindowControl.cs`）：

watchdog 按钮检查加**启动宽限期** `PortableNativeDragGracePeriod = 500ms`：

```csharp
var dragAge = DateTime.UtcNow - _portableDragStartUtc;
if (dragAge > PortableNativeDragGracePeriod
    && Mouse.LeftButton != MouseButtonState.Pressed && !PlatformHelper.IsLeftButtonDown())
{
    EndPortableDrag(drop: true);
    return;
}
```

给合成 `dd` 事件一个到达 WPF 输入管线的窗口，避免拖拽启动瞬间被 watchdog 误杀。
**效果**：完整测试从稳定 1-2 个 flake 改善到 0-1 个（22/22 一次、21/22 一次、20/22 两次）。
**未根除**：子模式 B（`dd` 被 macOS 激活消耗）不经过 watchdog 路径，宽限期不覆盖。

**AvalonDock**（`DockingManager.cs`）：`GetOverlayWindowHostsByZOrder` 加异常 fallback
（Win32 z-order 枚举在便携后端失败时降级为普通枚举），防御性。

**LibreWPF**（`src/ProGPU.Wpf/Platform/SilkNetWpfInputService.cs`）：

`mouseUp` 不再因"该按钮没有记录到 down"而丢弃事件。原实现：

```csharp
if (!pressedButtons.Remove(button)) { return; }   // down 丢失 → up 被吞
```

合成输入下 down 偶发丢失时，up 被吞会让 WPF `Mouse.LeftButton` **永久卡在 Pressed**，
腐蚀后续所有拖拽。现在无条件转发 up（WPF 自己维护按钮状态）。**效果**：v4 脚本 16-18/20，
无明显恶化也无显著改善 —— 属正确性修复，非 flake 根治。

**尝试过但无效的方案**：
- `avd.activate`（press 前激活主窗口）：反而 0/20 全失败 —— 主窗口 key 后浮动窗仍非 key，
  点击浮动窗标题栏时激活消耗更稳定地发生；且 activate 可能移动浮动窗使坐标失效。
- 加长 press 后等待：无变化。
- `GetCursorPos` shim（`ProGPU.Wpf.Sdk.Win32Compat.c`）恒返回 `(0,0)`：AvalonDock 的
  `PlatformHelper.GetCursorPosition` 走 `MacOSCursorService`（`CGEventGetLocation`），
  不经过该 shim，无关。

### 2026-08-08 测试侧加固（最终 22/22）

后续观察确认，flake 不只来自丢失的 `dd`，测试本身还曾在**错误的屏幕起点**注入 mouse-down：
托管 `Window.Left/Top`、`PointToScreen` 与 GLFW native frame 可能短暂不一致；浮动窗也可能位于
主窗口后方。此时计算坐标虽然落在“托管 floating bounds”内，macOS 实际接收者却是主窗口或
桌面/系统 UI，严重时会误触 macOS 系统动作。

现在所有 floating body 预热 press 和 caption drag-down 前都有硬性门禁：

1. `avd.query.bounds` / `avd.query.drag-handle` 对浮动窗优先读取 ProGPU/GLFW 的 native
   `Position`/`Size`，浮动视觉树的屏幕坐标转换也以 native origin 为准；
2. cliclick 只做 mouse move，随后 `avd.query.cursor` 通过 `MacOSCursorService` 回读 native
   cursor；请求点与实测点误差超过 2px 时禁止 mouse-down；
3. 实测 cursor 必须仍位于 floating native frame；caption drag 还必须落在 caption handle；
4. `avd.query.floating-zorder` 必须确认浮动窗位于主窗口之前；若不在前景，TestApp 先直接
   `Activate()` 浮动窗，再重新读取 frame/z-order，不能靠一次可能点错窗口的鼠标点击激活；
5. 激活预热的 down/up 前后按系统双击间隔隔离（macOS 使用
   `NSEvent.doubleClickInterval`），避免 caption 单击与邻近事件合并成双击。

zone 坐标 discovery 不再额外执行一次易 flake 的真实拖拽。`avd.debug-show-overlay` 现在先
`UpdateLayout()`，返回全部 indicator 的屏幕 bounds；测试按目标 pane 距离选择对应 indicator，
随后只执行一次正式 held native drag。命中目标后，zone/preview 测试由 TestApp 确定性调用
AvalonDock 当前 drop，再释放合成鼠标。后续完整套件证明，即使 `manager.up` 已收到，WPF 的
`Mouse.LeftButton` 仍可能保持 `Pressed`，所以 `NativeInputIntegrationTests` 的浮窗回 dock
用例也改为先完成已经命中的 current drop，再发送真实 `du` 做输入清理；测试不再把“合成
mouse-up 一定触发 dock”当作可靠契约。

完整串行运行还发现两种单跑不易出现的前置状态，均在 mouse-down 之前处理：

- layout transition 中个别临时 target 可出现负 extent；`GetScreenBounds()` 跳过这种不可命中
  rect，避免枚举其他有效 target 时整体抛异常；
- 新建 floating 的 native frame 偶尔仍停留在屏幕上方，或仅 cursor move 被 key-window 切换
  吞掉。测试会回读 native frame/cursor；不安全时绝不按下，而是程序化 dock、重建浮窗并重试。

最终验证命令与结果：

```bash
dotnet run --project source/DevFlowIntegrationTests/DevFlowIntegrationTests.csproj -- \
  --filter-class AvalonDock.DevFlowIntegrationTests.DropTargetZoneIntegrationTests --parallel none
# total: 22, succeeded: 22, failed: 0, duration: 4m 40s
```

加入生命周期与安全重试修正后的最终全套验证：

```bash
dotnet run --project source/DevFlowIntegrationTests/DevFlowIntegrationTests.csproj -- \
  --parallel none
# total: 56, succeeded: 56, failed: 0, duration: 6m 16s
```

测试代码不应主动截图。UnoDevelop `MainMenuTests.ViewTestsClickShowsTestsPad` 中遗留的
`/api/v1/ui/screenshot` 调用已删除；本组运行没有截图步骤。

## 3. 崩溃 A：`resignKeyWindow` 通知 observer 悬垂（SIGBUS）

### 崩溃栈

```
EXC_BAD_ACCESS / KERN_PROTECTION_FAILURE
__CFNOTIFICATIONCENTER_IS_CALLING_OUT_TO_AN_OBSERVER__   ← observer 悬垂
AppKit -[NSWindow resignKeyWindow] → _sendResignKeyWindow → _changeKeyAndMainLimitedOK
AppKit -[NSWindow _handleLeftMouseDownEvent:isDelayedEvent:]   (isDelayedEvent:YES)
AppKit -[NSWindow(NSEventRouting) sendEvent:] → libglfw.3.dylib
```

### 触发与根因

- 触发点：**cliclick 点击时窗口 key 状态变化**（后台运行时窗口非 key，点击触发激活/失活切换），
  与 flake 的"激活消耗"同源。
- 栈顶表现为 AppKit 派发 `NSWindowDidResignKeyNotification` 时访问坏地址。GLFW 的
  `GLFWWindowDelegate.windowDidResignKey:` 确实是该通知的接收路径之一，但 GLFW 3.4
  源码并没有显式 `NSNotificationCenter addObserver:`；它通过 `NSWindow.delegate` 接收，销毁时
  先 `setDelegate:nil` 再 release delegate。因此“GLFW observer 未移除”目前只是候选假设，
  不能当作已证实根因。
- 之后任何一次 key 状态变化（点击）都会走到悬垂地址 → SIGBUS。

### 可修层

**GLFW native（libglfw.3.dylib，由 Silk.NET 2.23.0 打包 `ultz.native.glfw` 3.4.0）**。
LibreWPF 托管源码（ProGPU.Wpf 项目）没有任何 NSNotificationCenter/ObjC observer 注册代码
（全仓库 grep 确认）。GLFW 当前 master 的 Cocoa 销毁顺序与 3.4 在这一段相同，故“直接换
master 就会修复”没有源码依据。候选动作：
- 在 GLFW `GLFWWindowDelegate` 的 resign/close/dealloc 与 `_glfwDestroyWindowCocoa` 加原生日志，
  用对象地址证明回调是否发生在 delegate/window 释放之后；
- 替换 GLFW native 版本仍可做 A/B，但必须以复现率为证据；
- 降低浮动窗创建/销毁频率（AvalonDock 层浮动窗复用）。

## 4. 崩溃 B：`CUIThemeFacet` 解码 CAPackage 崩溃（EXC_BAD_ACCESS）

### 崩溃栈（2026-08-08 09:25 实测）

```
EXC_BAD_ACCESS / EXC_ARM_DA_ALIGN at 0x9
CFRelease → __CFBasicHashAddValue → CFDictionarySetValue
NSKeyedUnarchiver decodeObject* → [CALayer initWithCoder:] (×2 层)
[CAPackage _readFromArchiveData:options:error:]
+[CAPackage packageWithData:type:options:error:]
-[CUIThemeFacet _makeLayerFromCAPackageData] → updateLayer:effects:
CUICoreThemeRenderer::CreateOrUpdateVisualEffectLayer / CreateOrUpdateLayer
```

### 当前判断

**macOS AppKit 主题渲染器（CUIThemeFacet）创建/更新 Visual Effect Layer（毛玻璃材质）时**
解码 `CAPackage`（图层包归档）→ 访问已释放对象 → `CFRelease` 崩溃。
本轮加固过程中再次多次复现，均发生在频繁 float/drop/layout restore 后；堆栈稳定落在
AppKit/CoreUI，但目前只能把“AppKit 系统层 bug”视为**高概率假设**，还不能排除 LibreWPF
错误的 NSVisualEffectView 生命周期、跨线程更新或过早释放间接破坏 AppKit 状态。
`restorable=NO` 不能阻止该路径（崩溃在 theme 渲染，不在状态保存）。

### 2026-08-08 生命周期 A/B

新增两个不截图的 DevFlow 压力测试，将 native mouse 与窗口生命周期分开：

- 100 次 `float → dock`（只创建/销毁带标题栏浮窗）：通过，约 39 秒；
- 100 次 `float → show transparent overlay + DockLeft preview → hide overlay → dock`：通过，
  约 60 秒。

这说明“频繁销毁浮窗”以及“频繁销毁透明 overlay”都不是 Crash B 的充分条件。LibreWPF
没有显式创建 `NSVisualEffectView`；透明化只对 GLFW `NSWindow` 调用 `setOpaque:NO` 与
`setBackgroundColor:NSColor.clearColor`。结合崩溃位于 `CUIThemeFacet`，当前更精确的假设是：
系统为 **titled GLFW window 的 AppKit frame/titlebar** 创建 visual-effect layer，真实激活、
拖动或 key-window 切换与窗口关闭交错时触发了主题层更新竞态。下一轮 A/B 应加入安全的真实
激活/拖动事件，而不是继续单纯增加无输入的 create/destroy 次数。

### 2026-08-08 AppKit view-tree 实证

`avd.query.macos-view-tree` 现在从真实 `NSWindow*` 的 `contentView.superview` 向下遍历 AppKit
view tree（只读、不截图），记录类名、对象地址、`hidden` 和 window `styleMask`。实测：

| 窗口 | styleMask | `NSVisualEffectView` | 关键状态 |
|---|---:|---:|---|
| main | 15 | 2 | frame 直属一个、titlebar background 内一个，均未隐藏 |
| floating | 32777 | 2 | frame 直属一个未隐藏；titlebar background 已隐藏，但其 effect 子 view 自身未隐藏 |
| transparent overlay | 32777 | 1 | 位于已隐藏的 titlebar background 下；没有 frame 直属 effect |

由此可以排除“LibreWPF 显式创建 `NSVisualEffectView`”和“只有普通 titled window 才有该
对象”两种简单解释。`WindowStyle.None` 在 portable backend 下仍是带
`NSWindowStyleMaskFullSizeContentView` 的 AppKit theme frame，而不是完全没有 theme/titlebar
view tree 的纯 borderless window。不过 overlay 的 effect 有隐藏父节点，floating 则有一个
未隐藏的 frame-level effect。曾做过一个 TestApp A/B：只从 custom-chrome/full-size floating
window 的 AppKit frame 移除直属 `NSVisualEffectView`。刷新 LibreWPF local feed 后，真实 drag
多次出现 `EXC_BAD_ACCESS/SIGSEGV → objc_msgSend → 托管 P/Invoke`；即使先用
`NSApplication.orderedWindows` 验证指针，检查与后续消息之间仍有生命周期竞态。因此该 A/B
已完全停用，不能作为产品修复。Crash B 仍应在 LibreWPF/AppKit 窗口生命周期层继续调查。

同一轮 native 坐标诊断还发现 overlay 的托管 `Left/Top` 与实际 Cocoa frame 会偶发分离；
之前 drag-state 和 preview 转屏幕坐标混用了 overlay managed origin 与 manager native origin，
会产生假偏移。现在所有 preview/overlay 诊断统一使用真实 native origin，并且 overlay 在
`Show()`、native handle 创建之后通过 `INativeWindowService.SetWindowPosition` 再校正一次位置。

长序列进一步证明问题不只是刷新时机：ProGPU 的 `TryGetWindowHost(overlay)` 会偶发返回 floating
或 main window 的 native host。现在 main、floating、overlay 在 source 初始化时登记 native
handle 所有权；重复 handle 会被拒绝，overlay 未取得独占 handle 时禁止调用原生移动。macOS
floating 预定位和 portable caption drag 也直接使用固定 native handle，避免 LibreWPF
`Window.Left/Top` setter 串窗并移动 main window。zone 理论用例各自重启 TestApp，阻断透明原生
窗口生命周期的跨用例污染。逐项恢复
XML layout 曾导致 floating native window 关闭期间 UI 线程卡死，现改为下一项开始时执行确定性
layout reset，并由 20 秒 action watchdog 报出真正卡住的 action。

所有测试都由 TestApp 内的 20ms native-origin guard 记录 main window 首次位移，结束时统一断言
全过程未移动。所有 floating→dock 的 mouse-down 前，不仅验证请求点、窗口和 z-order，还会读取
真实 native cursor；最多慢速重发 5 次纯 move，只有 cursor 位于最新 floating title-bar handle
内才允许按下。额外 warm-up click 已删除，点击之间仍按系统双击判定时间隔离；测试代码不调用
任何截屏动作。4 个专门用例在 drop 前保存 live preview 屏幕矩形，drop 后读取实际承载 pane
矩形，并以 2 px 容差比较位置和尺寸。

2026-08-08 最终刷新验证：local feed 的正式目录只保留 3 个有效包，历史包移到
`artifacts/local-feed-backups`；LibreWPF Release、NuGet cache 和 TestApp 输出中的
`ProGPU.Wpf.dll` SHA-256 均为 `9f0e7585470d07f145ac1d9812552db43174cb81f44f3f569263f3626a6be1b6`。
完整集成测试 57/57 通过（8m42s）。

### 影响

完整测试跑 22 个用例期间偶发触发 → TestApp 进程死亡 → 后续用例全部
`DevFlow agent not reachable` 级联失败（一次实测：崩溃后 12 个用例级联失败）。

## 5. LibreWPF 源码修改 → 本地 feed 替换流程

TestApp 通过 `LibreWPF.Sdk` MSBuild SDK 消费包；本地源已在
`OpenDevelop/NuGet.config` 配置：

```xml
<add key="librewpf-local" value="/Users/lextm/wpf-tools/librewpf/artifacts/local-feed" />
<packageSource key="librewpf-local">
  <package pattern="LibreWPF.ProGPU" />
  <package pattern="LibreWPF.Transport" />
  ...
</packageSource>
```

**完整替换流程**（已验证可运行）：

```bash
# 1. 改源码：/Users/lextm/wpf-tools/librewpf/src/ProGPU.Wpf/...
# 2. 构建（用仓库 pinned SDK）
cd /Users/lextm/wpf-tools/librewpf
export PATH="$PWD/.dotnet:$PATH"
dotnet build src/ProGPU.Wpf/ProGPU.Wpf.csproj -c Release
# 产物: src/ProGPU.Wpf/bin/Release/net10.0/ProGPU.Wpf.dll

# 3. 解包 nupkg → 替换 dll → 重打包（zip）→ 覆盖 local-feed
mkdir -p /tmp/nupkg-work && cd /tmp/nupkg-work
cp /Users/lextm/wpf-tools/librewpf/artifacts/local-feed/LibreWPF.ProGPU.0.1.0-preview.41.nupkg orig.nupkg
unzip -o -q orig.nupkg -d extracted
cp <新 ProGPU.Wpf.dll> extracted/lib/net10.0/ProGPU.Wpf.dll
cd extracted && rm -f ../LibreWPF.ProGPU.0.1.0-preview.41.nupkg \
  && zip -q -r -X ../LibreWPF.ProGPU.0.1.0-preview.41.nupkg .
# 校验: unzip -p 新nupkg lib/net10.0/ProGPU.Wpf.dll | md5 == 新 dll md5

# 4. 备份 + 覆盖 feed + 清 NuGet 缓存
cp local-feed/*.nupkg local-feed/*.myfix-backup   # 惯例：改名前先备份
cp /tmp/nupkg-work/LibreWPF.ProGPU.0.1.0-preview.41.nupkg local-feed/
rm -rf ~/.nuget/packages/librewpf.progpu/0.1.0-preview.41

# 5. 重建 TestApp 并核对 md5
dotnet build source/TestApp/TestApp.csproj -c Debug
md5 -q source/TestApp/bin/Debug/net10.0-windows/ProGPU.Wpf.dll   # 必须等于新 dll
```

注意：`LibreWPF.Transport` 包打包的是**预构建**的 WPF DLL（PresentationCore/PresentationFramework），
若修改了 `src/Microsoft.DotNet.Wpf` 下的代码，需先单独构建对应 csproj 再 pack 打包项目。

## 6. 诊断工具清单

| 工具 | 用途 |
|---|---|
| `avd.query.drag-state` | 拖拽中实时状态：`currentDropTarget`、`previewGeometryBounds`（当前真实 preview 的屏幕坐标）、`overlayLeft/Top`（overlay 是否创建）、`left/top`（浮动窗位置）、`currentPointer`、`dragOffset` |
| `avd.query.active-drop-targets` | compass 当前指示器列表（type + 屏幕中心） |
| `avd.input.query` | TestApp 鼠标诊断：`leftButton`（是否 Pressed）、`mouseX/Y`、`captured` |
| `avd.debug-show-overlay <zone>` | 强制显示 overlay + 返回 `previewGeometryBounds`（**屏幕坐标**，与 dock 后 pane 对比） |
| `drag-diag4.sh`（会话临时脚本） | 20 次循环复现 flake，失败时打印 drag-state 现场 |
| `~/.nuget/packages/...` 与 `local-feed` | 包版本/替换核验（md5 对比） |
| `~/Library/Logs/DiagnosticReports/TestApp-*.ips` | 崩溃报告（`python3 -c` 解析 exception + triggered thread 栈） |

**失败判读口诀**：
- `overlayLeft: null` + 浮动窗移动过 → 子模式 A（tick 跑但 `UpdateMouseLocation` 未命中 host）；
- 浮动窗没动 + `leftBtn=Released` → 子模式 B（cliclick `dd` 未到达 WPF，激活消耗）；
- 用例 4ms 秒失败 + `not reachable` → 上一步 TestApp 崩溃级联（查 .ips）。

## 7. 当前状态与遗留问题

**已生效修复**（全部通过完整测试验证）：
1. `LayoutFloatingWindowControl.cs`（AvalonDock）：watchdog 宽限期 500ms —— flake 率 1-2 → 0-1。
2. `DockingManager.cs`（AvalonDock）：`GetOverlayWindowHostsByZOrder` 异常 fallback（防御）。
3. `SilkNetWpfInputService.cs`（LibreWPF，dll 已替换到 local-feed）：mouseUp 不因 down 缺失而丢弃。
4. 测试改进：`DockPreview_MatchesDockedPaneGeometry` 在真实拖拽按住、目标已命中但尚未
   release 时，从 `avd.query.drag-state` 记录 preview 屏幕矩形；drop 完成后再读取实际 pane，
   比较两者尺寸和位置（容差 2px）。
5. native-input 安全门禁：native cursor + GLFW frame + native z-order 全部正确才允许 mouse-down；
   错误坐标直接失败，不再拖拽主窗口、桌面或 macOS 系统部件。
6. 删除额外 discovery drag；overlay 完成布局后直接提供全部 indicator bounds；zone 测试的
   drop 确定性完成，真实 mouse-up 由专门用例覆盖。
7. 完整 `DropTargetZoneIntegrationTests`：**22/22 通过，4 分 40 秒**；运行中无截图动作。

**遗留**：
- 产品层首次点击语义：测试已规避非 key window 吞 `dd`，但真实应用仍应验证/实现 GLFW Cocoa
  `acceptsFirstMouse`，不能把测试规避当作产品修复；
- 崩溃 A（GLFW observer 悬垂）：本轮最终 22/22 未复现，但已有确定 native 栈，仍需 GLFW
  替换版本 A/B 才能关闭；
- 崩溃 B（CUIThemeFacet CAPackage 解码）：本轮修复过程中多次复现，是当前最高优先级稳定性问题；
  最终 22/22 只证明降低窗口/手势次数后该轮未触发，不能视为已修复。

### 建议的后续研究顺序

1. **先查崩溃 B**：做最小循环（float → dock → restore），分别关闭窗口材质/透明效果、复用而非
   销毁 floating/overlay window、串行化关闭与重建，记录每组 100 次的崩溃率；同时给
   `NSVisualEffectView` 创建、从父视图移除、native window dispose 加时间戳和对象地址。
2. **再查崩溃 A**：固定同一 workload，仅替换 GLFW native（当前 3.4.0 vs 更新版本或带
   observer 移除补丁的自编译版本），检查 `NSWindowDidResignKeyNotification` observer 的注册/
   移除是否成对。若升级即消失，就把修复锁定在 native 依赖升级，不在 AvalonDock 绕过。
3. **最后补产品行为**：验证 Cocoa `acceptsFirstMouse:` 后，回归真实用户首次按下拖拽；保留当前
   测试安全门禁，因为它防的是坐标/z-order 错误，与 `acceptsFirstMouse` 是两类问题。

## 8. 历史：splitter-drag bug 三连修（旧版内容，仍有效）

（原 librewpf.md 内容，2026-08 之前）hover 分割条时按左键拖动：光标回箭头、拖不动或只动一步、
之后光标卡死。根因是 `LayoutGridControl.OnSplitterDragStarted` 在拖拽中途 `Show()` 透明 overlay
`Window`（`ShowActivated=false`, `Owner=null`），在 LibreWPF 便携后端产生三个独立破坏：
phantom MouseUp、`Mouse.Synchronize()` 的 `(0,0)` 瞬移、`Window.HandlePortableMove` 释放捕获。
三处修复均在 librewpf 仓库：

| # | 文件 | 修改 |
|---|------|------|
| 1 | `src/ProGPU.Wpf/ProGpuWpfWindowHost.cs` | 吞掉 show 后无 intervening move 的幽灵 MouseUp + GLFW 鼠标穿透；`NormalizeInputEventForRenderSurfaceGeometry` 对退化 scale 透传事件 |
| 2 | `src/Microsoft.DotNet.Wpf/src/PresentationCore/.../MouseDevice.cs` | `Synchronize()` 在 `Captured != null` 时提前返回 |
| 3 | `src/Microsoft.DotNet.Wpf/src/PresentationFramework/.../Window.cs` | `HandlePortableMove` 在鼠标按钮物理按下时跳过 `Mouse.Capture(null)` |

修复 2/3 进 `LibreWPF.Transport`，修复 1 进 `LibreWPF.ProGPU`。
验证 harness：`/Users/lextm/uno-tools/tooltiptest`（`TOOLTIPTEST_SPLITTER_MODE=1`、
`scripts/check-splitter-capture.sh` → `SPLIT_RESULT: PASS`）。重打包注意：Transport 打包
预构建 WPF DLL，需先 `dotnet build` 对应 csproj 再 pack。
