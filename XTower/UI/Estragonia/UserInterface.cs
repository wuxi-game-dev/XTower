using XTower.UI.ViewModels;
using XTower.UI.Views;
using JLeb.Estragonia;
using AvControl = Avalonia.Controls.Control;

namespace XTower;

/// <summary>Default Avalonia host. Swap the root view / view-model as needed.</summary>
public partial class UserInterface : UiHost {

	protected override AvControl CreateRoot()
		=> new MainView { DataContext = new MainViewModel() };

}
