namespace Plugin.ContextMenu;

public class CollectionView : ReorderableItemsView
{
#if ANDROID
    protected override void OnChildAdded(Element child)
    {
        base.OnChildAdded(child);
        ContextMenu.RegisterChildElement(this, (VisualElement)child);
    }

    protected override void OnChildRemoved(Element child, int oldLogicalIndex)
    {
        base.OnChildRemoved(child, oldLogicalIndex);
        ContextMenu.UnregisterChildElement(this, (VisualElement)child);
    }
#endif
}

