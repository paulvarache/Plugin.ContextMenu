using System;
using Maui.BindableProperty.Generator.Core;
using Microsoft.Maui.Controls.Shapes;

namespace Plugin.ContextMenu;

public partial class Preview : Element
{
	[AutoBindable]
	DataTemplate previewTemplate;

    [AutoBindable]
    IShape visiblePath;

    [AutoBindable]
    Color backgroundColor;

    [AutoBindable]
    Thickness padding = new Thickness();
}

