﻿using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific;

namespace Plugin.ContextMenu;

public class CollectionViewHandler : ReorderableItemsViewHandler<ReorderableItemsView>
{
#if IOS
    public CollectionViewHandler(): base()
    {
        ReorderableItemsViewMapper.ModifyMapping(SelectableItemsView.SelectionModeProperty.PropertyName, MapSelectionMode);
    }

    private void MapSelectionMode(ReorderableItemsViewHandler<ReorderableItemsView> handler, ReorderableItemsView view, Action<IElementHandler, IElement> action)
    {
        var ctrl = (handler.ViewController as SelectableItemsViewController<ReorderableItemsView>);
        if (ctrl == null)
        {
            return;
        }
        ctrl.CollectionView.AllowsSelection = true;
        ctrl.CollectionView.AllowsMultipleSelection = false;
    }

    protected override CollectionViewController CreateController(ReorderableItemsView itemsView, ItemsViewLayout layout)
             => new CollectionViewController(itemsView, layout);
#endif
}
