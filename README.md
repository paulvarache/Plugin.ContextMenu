# .NET MAUI Context Menu plugin

The .NET MAUI Context menu plugin adds the ability to add context menus to your application.

At the moment, this plugin only supports iOS and Android

## Getting Started

In order to use the plugint you need to call the extension method in your `MauiProgram.cs` file as follows:

```csharp
using Plugin.ContextMenu;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		
		// Initialise the toolkit
		builder.UseMauiApp<App>().UseContextMenu();

		// the rest of your logic...
	}
}
```

### XAML usage

In order to make use of the plugin within XAML you can use this namespace:

```xml
xmlns:cm="clr-namespace:Plugin.ContextMenu;assembly=Plugin.ContextMenu"
```

Then you can add a context menu to any control:

```xml
<ContentView HeightRequest="200" WidthRequest="200" Background="GreenYellow">
    <cm:ContextMenu.Menu>
        <DataTemplate>
            <cm:Menu>
                <cm:Action Title="Upload documents" />
                <cm:Action Title="Copy" />
                <cm:Action Title="Cut" />
                <cm:Action Title="Paste" />
            </cm:Menu>
        </DataTemplate>
    </cm:ContextMenu.Menu>
</ContentView>
```

### Defining the menu

The available elements for the menu are:

 - `<cm:Menu>`: The top level menu, can be nested for submenus
 - `<cm:Action>`: An action users can select from the menu
 - `<cm:Group>`: A group of `Action`s or `Menu`s. `Group`s cannot be nested

The `<cm:Menu>` element supports the following parameters:

| Property  | Type    | Description                                                    | iOS | Android |
|-----------|---------|:--------------------------------------------------------------|---|-------|
| `Title`   | `string`    |  A string title to be displayed at the top of the context menu | ✅ | ❌      |

The `<cm:Group>` element supports the following parameters:

| Property  | Type    | Description                                                    | iOS | Android |
|-----------|---------|:--------------------------------------------------------------|---|-------|
| `Title`   | `string`    |  A string title to be displayed at the top of the group        | ✅ | ❌      |


The `<cm:Action>` element supports the following parameters:

| Property  | Type    | Description                                                    | iOS | Android |
|-----------|---------|:--------------------------------------------------------------|---|-------|
| `Title`   | `string`|  The text for the action        | ✅ | ✅      |
| `Command`   | `ICommand`    |  A command to execute when the user selectes this action        | ✅ | ✅      |
| `CommandParameter`   | `object`    |  A parameter that will be passed to the `Command`        | ✅ | ✅      |
| `Icon`   | `ImageSource`    |  An icon to be displayed next to the action text        | ✅ | ✅      |
| `SystemIcon`   | `string`    |  A string reference to a system icon to use instead of the `Icon`        | ✅* | ❌**      |
| `IsEnabled`   | `bool`    |  Enales/Disables the action in the menu        | ✅ |    ✅   |
| `IsVisible`   | `bool`    |  Hides/Displays the action in the menu        | ✅ |    ✅   |
| `IsDestructive`   | `bool`    |  Displays the action as destructive       | ✅ |    ✅   |

 - \* SystemIcons on iOS use the SFSymbols collection of icons: https://developer.apple.com/sf-symbols/
 - \*\* Android does not recommend using system icons in your apps

Here is what a more complex menu would look like using most properties available:

```xml
<cm:Menu Title="More options">
    <cm:Action Title="Copy" SystemIcon="doc.on.clipboard" />
    <cm:Action Title="Upload documents" Icon="dotnet_bot.png" />
    <cm:Group Title="Lifecycle">
        <cm:Action Title="Start" Command="{Binding StartCommand, Source={x:Reference this}}" CommandParameter="foo" />
        <cm:Action Title="Stop" IsDestructive="True" />
    </cm:Group>
    <cm:Menu Title="Clipboard">
        <cm:Action Title="Copy" SystemIcon="doc.on.clipboard" IsEnabled="False" />
        <cm:Action Title="Paste" IsVisible="False" />
    </cm:Menu>
</cm:Menu>
```

### Customising the preview

When an item is selected for its context menu to open, a highlighted preview can be displayed. This preview by default is a snapshot of the view itself. This can be customised using the `Preview` property.

| Property  | Type    | Description                                                    | iOS | Android |
|-----------|---------|:--------------------------------------------------------------|---|-------|
| `PreviewTemplate`   | `DataTemplate`    |  Provide a different view to render the preview        | ✅ | ✅      |
| `VisiblePath`   | `IShape`|  Customise the path used to clip the preview        | ✅* | ✅      |
| `BackgroundColor`   | `Color`    |  The background color for the hightlight preview        | ✅ | ✅      |
| `Padding`   | `Thickness`    |  The padding of the VisiblePath within the highlight preview        | ✅ | ✅      |

 - \* By default the Visible path on iOS has a corner radius set

Example:

```xml
<cm:ContextMenu.Preview>
    <cm:Preview BackgroundColor="Red" Padding="16">
        <cm:Preview.PreviewTemplate>
            <DataTemplate>
                <ContentView WidthRequest="100" HeightRequest="100" Background="Green" />
            </DataTemplate>
        </cm:Preview.PreviewTemplate>
        <cm:Preview.VisiblePath>
            <RoundRectangle CornerRadius="10, 20, 30, 40" />
        </cm:Preview.VisiblePath>
    </cm:Preview>
</cm:ContextMenu.Preview>
```


### Standard click

Because of how context menus ar attached to view on each different platforms, the `TapGestureRecognizer` might not work in combination with the context menu. This is why this plugin also offers a `ClickCommand` property to ensure your command isa called on every platform.

```xml
<ContentView 
        cm:ContextMenu.ClickCommand="{Binding ClickCommand}"
        cm:ContextMenu.ClickCommandParameter="Hello">
    <cm:ContextMenu.Menu>
        <DataTemplate>
            <!-- ... -->
        </DataTemplate>
    </cm:ContextMenu.Menu>
</ContentView>
```

### Show menu on click

Sometimes you want to show the context menu on a click instead of the default long press/right click. Setting `ShowMenuOnClick` to true will simply do that. The highlight preview will however not be shown in this mode.

On iOS it is not possible to open a context menu on click. This plugin uses the UIKit `showsMenuAsPrimaryAction` property to support this feature. This is however only supported on UIKit.UIbutton, meaning `ShowMenuOnClick` will only work on `Button` on iOS

### TableView, ListView, CollectionView

Due to the way context menus are supported for those elements on iOS, you must use this plugin's version of the TableView/ListView/CollectionView. These controls are direct extensions of their .NET MAUI counterparts, so no feature is changed.

To add a contextmenu to all items of a CollectionView for example:

```xml
<cm:CollectionView>
    <cm:ContextMenu.Menu>
        <DataTemplate>
            <cm:Menu>
                <cm:Action Title="Upload" />
                <cm:Action Title="Delete" />
            </cm:Menu>
        </DataTemplate>
    </cm:ContextMenu.Menu>
    <!-- ... -->
</cm:CollectionView>
```

This will add a contextmenu to each item of a CollectionView. However all the menus will be identical. If you need to have the menu change based on the item of the CollectionView selected, you can do so by using bindings. By default, the BindingContext of the `<cm:Menu>` is the item the user opened the context menu from. 

This can also be useful to pass the item to the command of an action.

For example, if the Items of the CollectionView are:

```cs

public class UserDocuments {
    public string Name { get; set; }
    public bool CanUpload { get; set; }
}

```

You can configure the context menu for these items like so:

```xml
<cm:CollectionView>
    <cm:ContextMenu.Menu>
        <DataTemplate>
            <cm:Menu>
                <cm:Action
                    Title="Upload"
                    IsVisible="{Binding CanUpload}"
                    Command="{Binding UploadDocument, Source={x:Reference this}}"
                    CommandParameter="{Binding .}"
                />
            </cm:Menu>
        </DataTemplate>
    </cm:ContextMenu.Menu>
    <!-- ... -->
</cm:CollectionView>
```

## Implementation details

This plugins aims to use the underlying platform's features as much as possible, however when needed the platform was extended to offer the best user experience.

### iOS

iOS provides a native and comprehensive context menu feature. This was used to support this plugin.

The [UIContextMenuInteraction](https://developer.apple.com/documentation/uikit/uicontextmenuinteraction) was used to support standalone controls. For items views, [contextMenuConfigurationForItemsAt](https://developer.apple.com/documentation/uikit/uicollectionviewdelegate/4002186-collectionview) or an equivalent method was used


### Android

Android offers a native context menu API. However this API basically just opens a [PopupMenu](https://developer.android.com/reference/android/widget/PopupMenu) anchored to the target view.

This plugin implementes a more complete context menu experience by adding a highlighted preview and multiple other features to match the featureset provided by iOS.

A [MenuBuilder](https://developer.android.com/reference/android/view/Menu) is still used to conform the custom implementation to all other Android menus.

## TODO:

 - Allows for the specification of the icon color. This woul let developer choose whether their icons will match the text color or use the icon's colors.
