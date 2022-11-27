using Foundation;
using Microsoft.Maui.Controls.Handlers.Items;
using UIKit;

namespace Plugin.ContextMenu;

public class CollectionViewController : ReorderableItemsViewController<ReorderableItemsView>
{
    public CollectionViewController(ReorderableItemsView reorderableItemsView, ItemsViewLayout layout) : base(reorderableItemsView, layout) { }

    protected override UICollectionViewDelegateFlowLayout CreateDelegator()
    {
        return new CollectionViewDelegator(ItemsViewLayout, this);
    }
}