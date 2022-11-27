#if IOS
using UIKit;
#endif

namespace Plugin.ContextMenu;

public interface IContextMenuPreviewProvider
{
#if IOS
    public abstract UITargetedPreview GetPreviewForHighlightingMenu(UIContextMenuInteraction interaction, UIContextMenuConfiguration configuration);
#endif
}
