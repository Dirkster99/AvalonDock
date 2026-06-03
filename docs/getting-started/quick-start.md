---
title: Quick Start
layout: default
parent: Getting Started
nav_order: 2
description: "Build your first AvalonDock layout in 5 minutes."
---

# Quick Start

This guide walks you through creating a basic docking layout with documents and tool windows — all in under 5 minutes.

---

## Step 1: Create a WPF Project

Create a new WPF application targeting .NET 9 or later:

```bash
dotnet new wpf -n MyDockingApp --framework net9.0-windows
cd MyDockingApp
```

## Step 2: Install AvalonDock

```bash
dotnet add package Dirkster.AvalonDock
dotnet add package Dirkster.AvalonDock.Themes.Arc
```

## Step 3: Create the Layout

Replace the contents of `MainWindow.xaml` with:

```xml
<Window x:Class="MyDockingApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:avalonDock="https://github.com/Dirkster99/AvalonDock"
        xmlns:themes="clr-namespace:AvalonDock.Themes;assembly=AvalonDock.Themes.Arc"
        Title="My Docking App" Height="600" Width="900">

    <avalonDock:DockingManager>
        <!-- Apply the Arc Dark theme -->
        <avalonDock:DockingManager.Theme>
            <themes:ArcDarkTheme />
        </avalonDock:DockingManager.Theme>

        <avalonDock:LayoutRoot>
            <avalonDock:LayoutPanel Orientation="Horizontal">

                <!-- Left: Tool Window -->
                <avalonDock:LayoutAnchorablePane DockWidth="220">
                    <avalonDock:LayoutAnchorable Title="Explorer"
                                                  ContentId="explorer"
                                                  CanClose="False">
                        <TreeView>
                            <TreeViewItem Header="Solution" IsExpanded="True">
                                <TreeViewItem Header="Project">
                                    <TreeViewItem Header="MainWindow.xaml" />
                                    <TreeViewItem Header="App.xaml" />
                                </TreeViewItem>
                            </TreeViewItem>
                        </TreeView>
                    </avalonDock:LayoutAnchorable>
                </avalonDock:LayoutAnchorablePane>

                <!-- Center: Documents -->
                <avalonDock:LayoutDocumentPane>
                    <avalonDock:LayoutDocument Title="Welcome"
                                               ContentId="welcome">
                        <TextBlock Text="Welcome to AvalonDock!"
                                   FontSize="24"
                                   HorizontalAlignment="Center"
                                   VerticalAlignment="Center" />
                    </avalonDock:LayoutDocument>
                    <avalonDock:LayoutDocument Title="Document1.txt"
                                               ContentId="doc1">
                        <TextBox AcceptsReturn="True"
                                 Text="Type your content here..."
                                 FontFamily="Consolas" />
                    </avalonDock:LayoutDocument>
                </avalonDock:LayoutDocumentPane>

                <!-- Right: Properties -->
                <avalonDock:LayoutAnchorablePane DockWidth="200">
                    <avalonDock:LayoutAnchorable Title="Properties"
                                                  ContentId="properties"
                                                  CanClose="False">
                        <StackPanel Margin="8">
                            <TextBlock Text="Properties Panel"
                                       FontWeight="Bold" Margin="0,0,0,8" />
                            <TextBlock Text="Name:" />
                            <TextBox Margin="0,2,0,8" />
                            <TextBlock Text="Value:" />
                            <TextBox Margin="0,2,0,8" />
                        </StackPanel>
                    </avalonDock:LayoutAnchorable>
                </avalonDock:LayoutAnchorablePane>

            </avalonDock:LayoutPanel>

            <!-- Bottom: Output Panel (auto-hidden) -->
            <avalonDock:LayoutRoot.BottomSide>
                <avalonDock:LayoutAnchorSide>
                    <avalonDock:LayoutAnchorGroup>
                        <avalonDock:LayoutAnchorable Title="Output"
                                                      ContentId="output">
                            <TextBox AcceptsReturn="True"
                                     IsReadOnly="True"
                                     Text="Build started...&#x0a;Build succeeded."
                                     FontFamily="Consolas" />
                        </avalonDock:LayoutAnchorable>
                    </avalonDock:LayoutAnchorGroup>
                </avalonDock:LayoutAnchorSide>
            </avalonDock:LayoutRoot.BottomSide>
        </avalonDock:LayoutRoot>
    </avalonDock:DockingManager>
</Window>
```

## Step 4: Run

```bash
dotnet run
```

You should see a Visual Studio-like layout with:
- A **Solution Explorer** tool window on the left
- **Tabbed documents** in the center
- A **Properties** panel on the right
- An **auto-hidden Output** panel at the bottom

Try dragging panels around, floating them, and docking them to different positions!

---

## What's Next?

- Learn about the [Layout Model]({% link concepts/layout-model.md %}) to understand how AvalonDock organizes content.
- Explore [MVVM Integration]({% link guides/mvvm.md %}) to bind docking layouts to view models.
- Browse the [Themes]({% link themes/index.md %}) to find the right look for your app.
