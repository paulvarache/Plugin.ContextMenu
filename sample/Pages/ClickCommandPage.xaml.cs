using CommunityToolkit.Mvvm.Input;

namespace Plugin.ContextMenu.Sample.Pages;

public partial class ClickCommandPage : ContentPage
{
	public ClickCommandPage()
	{
		InitializeComponent();
	}

	[RelayCommand]
	void OnClick(string message)
	{
		DisplayAlert("From click command", message, "Ok");
	}
}