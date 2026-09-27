# XTower

Godot 4 + Avalonia starter from **[Ouse.Estragonia.Templates](https://www.nuget.org/packages/Ouse.Estragonia.Templates/)**.

- Library: [Ouse.Estragonia](https://www.nuget.org/packages/Ouse.Estragonia/)
- Docs / source: [0use-TE/Estragonia](https://github.com/0use-TE/Estragonia)

## Open in Godot

1. Install [.NET 10 SDK](https://dotnet.microsoft.com/download) and **Godot 4.7.2+ (.NET)**.
2. Run `dotnet restore` in this folder (the solution root).
3. Open **`XTower/project.godot`** with Godot (not the `.godot/` cache directory).

Autoload `AvaloniaLoader` and the default `UserInterface` (`UiHost`) are already configured.

## Layout

| Path | Role |
|------|------|
| `XTower.sln` / `XTower.slnx` | Solution (same folder as `XTower`) |
| `XTower/project.godot` | Open this in Godot |
| `XTower/XTower.csproj` | Single Godot + Avalonia assembly |
| `XTower/UI/` | Avalonia UI + `Estragonia/` host scripts |
| `Directory.Packages.props` | NuGet versions (solution-level) |
| `global.json` | .NET SDK pin |

Edit `XTower/UI/Views/MainView.axaml` and `XTower/UI/ViewModels/MainViewModel.cs` to build your UI.

Runtime is Godot via `XTower/project.godot` — do not start the C# project as a console app.
