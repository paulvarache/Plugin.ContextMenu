using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Handlers.Items;
using UIKit;

namespace Plugin.ContextMenu;

public class CollectionViewDelegator : ReorderableItemsViewDelegator<ReorderableItemsView, CollectionViewController>
{
    public CollectionViewDelegator(ItemsViewLayout itemsViewLayout, CollectionViewController itemsViewController) : base(itemsViewLayout, itemsViewController) { }

    public override UIContextMenuConfiguration GetContextMenuConfiguration(UICollectionView collectionView, NSIndexPath indexPath, CGPoint point)
    {
        var menuTemplate = ContextMenu.GetMenu(ViewController.ItemsView);

        if (menuTemplate == null)
        {
            return null;
        }

        var content = menuTemplate.CreateContent();

        if (content is Menu menu)
        {
            var item = ViewController.ItemsSource[indexPath];
            BindableObject.SetInheritedBindingContext(menu, item);
            return UIContextMenuConfiguration.Create(null, null, action =>
            {
                return ContextMenu.CreateMenu(menu);
            });
        }
        return null;
    }

    public override void WillDisplayCell(UICollectionView collectionView, UICollectionViewCell cell, NSIndexPath indexPath)
    {
        cell.ContentView.UserInteractionEnabled = false;
    }

    public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
    {
        var item = ViewController.ItemsSource[indexPath];
        ContextMenu.ExecuteClickCommand(ViewController.ItemsView, item);
    }
}

