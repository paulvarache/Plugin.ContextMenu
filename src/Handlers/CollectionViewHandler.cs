using Microsoft.Maui.Controls.Handlers.Items;

namespace Plugin.ContextMenu;

public class CollectionViewHandler : ReorderableItemsViewHandler<ReorderableItemsView>
{
#if IOS
    protected override CollectionViewController CreateController(ReorderableItemsView itemsView, ItemsViewLayout layout)
             => new CollectionViewController(itemsView, layout);
#endif
}

