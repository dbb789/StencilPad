> [!IMPORTANT]
> **ALPHA STATE** This project is mostly functionally complete, but still in early development. It's stable enough for experimentation, but expect bugs. This project will eventually be released in packages when it becomes more mature.

![alt text](https://github.com/dbb789/StencilPad/blob/main/Docs/Screenshot.png)

# StencilPad

StencilPad is a 2D CAD/drawing application that sits somewhere between Inkscape, Notepad and a CAD application.
It's designed to provide an easy to use, minimal UI that maximises screen space and generally keeps out of your way while giving you the ability to design real-world objects and/or plans with the precision of a CAD tool.
If you've found yourself looking for something that'll let you quickly draw diagrams and templates without the complexity and general friction that can come with more sophisticated graphics or CAD applications, this might be for you.

## Features

- Completely open source and free to use. No advertising or telemetry.
- Minimal and unobstructive UI, particularly suitable for devices with smaller screens.
- Full-blown CAD backend that can design and print real-world designs and templates with high precision.
- Lines, bezier curves, shapes and text.
- PNG/JPG image import and embedding.
- Marker point tool for producing holes or stitching guides, particularly useful for leatherwork.
- Ruler objects that update on-the-fly.
- Select, group and edit multiple objects simultaneously, including multiple vertex selection across diffent objects simultaneously.
- Grid and vertex snapping.
- Support for both metric and imperial units.
- Multiple sheets across different tabs, all saved into the same .spad file.
- PNG and SVG export.

## Prerequisites
- .NET 10.0 SDK
- Visual Studio 2026

## Keyboard Shortcuts

### File

| Shortcut | Action |
|---|---|
| `Ctrl+N` | New Project |
| `Ctrl+O` | Open Project |
| `Ctrl+S` | Save |
| `Ctrl+Shift+S` | Save As |
| `Ctrl+P` | Print |

### Edit

| Shortcut | Action |
|---|---|
| `Ctrl+Z` | Undo |
| `Ctrl+Y` | Redo |
| `Ctrl+X` | Cut |
| `Ctrl+C` | Copy |
| `Ctrl+V` | Paste |
| `Delete` | Delete |
| `Ctrl+A` | Select All |

### Sheet

| Shortcut | Action |
|---|---|
| `Ctrl+T` | Add Sheet |

### Canvas / View

| Shortcut | Action |
|---|---|
| `Escape` | Cancel current operation |
| `Tab` | Toggle between Select and Edit tool |
| `F1` | Toggle grid visibility |
| `F2` | Toggle grid lock (snap to grid) |
| `F3` | Toggle point lock (snap to points) |

### Selection Tool (objects selected)

| Shortcut | Action |
|---|---|
| `Ctrl+P` | Open properties for the selected element (shape, text, ruler, image, etc.) |
| `Ctrl+Shift+C` | Combine Shapes |
| `Ctrl+G` | Group |
| `Ctrl+U` | Ungroup |
| `Ctrl+Shift+H` | Flip Horizontal |
| `Ctrl+Shift+V` | Flip Vertical |
| `Ctrl+F` | Bring to Front |
| `Ctrl+B` | Send to Back |

#### Modifier keys while dragging (Selection Tool)

| Key held | Effect |
|---|---|
| `Shift` | Constrains drag movement to the nearest axis |
| `Shift` | Constrains resize to the original aspect ratio |
| `Shift` | Snaps rotation to the configured angle increment |

### Edit Tool (point editing)

| Shortcut | Action |
|---|---|
| `Ctrl+P` | Corner Properties |
| `Ctrl+I` | Insert Point |
| `Delete` | Delete selected point(s) |
| `Ctrl+Shift+O` | Open Path |
| `Ctrl+Shift+C` | Close Path |
| `Ctrl+Shift+S` | Set selected segment as Straight |
| `Ctrl+Shift+U` | Set selected segment as Curve |
