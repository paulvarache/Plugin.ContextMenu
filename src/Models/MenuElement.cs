using Maui.BindableProperty.Generator.Core;

namespace Plugin.ContextMenu;

public partial class MenuElement : Element
{
    [AutoBindable]
    readonly string title;
}
