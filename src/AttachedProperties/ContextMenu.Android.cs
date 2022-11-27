﻿using Android.Views;
using Android.Widget;
using static Android.Views.View;
using AView = Android.Views.View;
using AndroidX.Core.View;
using Microsoft.Maui.Platform;
using Android.Graphics.Drawables;
using Android.Graphics;
using Android.Util;

namespace Plugin.ContextMenu;

public static partial class ContextMenu
{
    public static readonly BindableProperty ChildElementsProperty = BindableProperty.CreateAttached("ChildElements", typeof(List<VisualElement>), typeof(VisualElement), new List<VisualElement>());
    public static void RegisterChildElement(BindableObject bindable, VisualElement element)
    {
        var views = (List<VisualElement>)bindable.GetValue(ChildElementsProperty);
        views.Add(element);
        if (bindable is CollectionView collectionView)
        {
            if (collectionView.GetValue(MenuProperty) != null)
            {
                AttachMenuToView(element, collectionView);
            }
            if (collectionView.GetValue(ClickCommandProperty) != null)
            {
                AttachClickToView(element, collectionView);
            }
        }
    }
    public static void UnregisterChildElement(BindableObject bindable, VisualElement element)
    {
        var views = (List<VisualElement>)bindable.GetValue(ChildElementsProperty);
        views.Remove(element);
        DetachMenuFromView(element);
        DetachClickFromView(element);
    }
    static void ForEachElement(BindableObject bindable, Action<VisualElement> action)
    {
        var elements = (List<VisualElement>)bindable.GetValue(ChildElementsProperty);
        foreach (var element in elements)
        {
            action(element);
        }
    }
    static partial void SetupMenu(BindableObject bindable)
    {
        if (bindable is CollectionView collectionView)
        {
            ForEachElement(collectionView, element => AttachMenuToView(element, collectionView));
        }
        else if (bindable is VisualElement visualElement)
        {
            AttachMenuToView(visualElement, visualElement);
        }
    }

    public static void AttachMenuToView(VisualElement visualElement, BindableObject propertySource)
    {
        var aview = (AView)visualElement.Handler.PlatformView;
        var showOnClick = GetShowMenuOnClick(propertySource);
        if (showOnClick)
        {
            aview.Clickable = true;
            aview.SetOnClickListener(new MenuActionListener(propertySource, visualElement));
        }
        else
        {
            aview.LongClickable = true;
            aview.SetOnLongClickListener(new MenuActionListener(propertySource, visualElement));
        }
    }
    public static void DetachMenuFromView(VisualElement visualElement)
    {
        var aview = (AView)visualElement.Handler.PlatformView;
        aview.LongClickable = false;
        aview.SetOnLongClickListener(null);
    }

    public static void AttachClickToView(VisualElement visualElement, BindableObject propertySource)
    {
        var aview = (AView)visualElement.Handler.PlatformView;
        aview.Clickable = true;
        aview.SetOnClickListener(new OnClickListener(propertySource, visualElement));
    }
    public static void DetachClickFromView(VisualElement visualElement)
    {
        var aview = (AView)visualElement.Handler.PlatformView;
        aview.Clickable = false;
        aview.SetOnClickListener(null);
    }

    static partial void DisposeMenu(BindableObject bindable)
    {
        if (bindable is CollectionView collectionView)
        {
            ForEachElement(collectionView, DetachMenuFromView);
        }
        else if (bindable is VisualElement visualElement)
        {
            DetachMenuFromView(visualElement);
        }
    }

    static partial void SetupClickCommand(BindableObject bindable)
    {
        if (bindable is CollectionView collectionView)
        {
            ForEachElement(collectionView, element => AttachClickToView(element, collectionView));
        }
        else if (bindable is VisualElement visualElement)
        {
            AttachClickToView(visualElement, visualElement);
        }
    }

    static partial void DisposeClickCommand(BindableObject bindable)
    {
        if (bindable is CollectionView collectionView)
        {
            ForEachElement(collectionView, DetachClickFromView);
        }
        else if (bindable is VisualElement visualElement)
        {
            DetachClickFromView(visualElement);
        }
    }

    public static void AddRootMenuItem(MenuElement item, IMenu amenu)
    {
        var id = 0;
        if (item is Action action)
        {
            AddAction(action, amenu);
        }
        else if (item is Group group)
        {
            AddGroup(group, amenu, ++id);
        }
        else if (item is Menu menu)
        {
            AddSubmenu(menu, amenu, ++id);
        }
    }
    static float DpToPixel(float dp)
    {
        return dp * ((float)Platform.CurrentActivity.Resources.DisplayMetrics.DensityDpi / (float)DisplayMetricsDensity.Default);
    }
    static Bitmap ScaleBitmap(Bitmap targetBmp, int reqHeightInPixels, int reqWidthInPixels)
    {
        Matrix matrix = new Matrix();
        matrix.SetRectToRect(new Android.Graphics.RectF(0, 0, targetBmp.Width, targetBmp.Height), new Android.Graphics.RectF(0, 0, reqWidthInPixels, reqHeightInPixels), Matrix.ScaleToFit.Center);
        Bitmap scaledBitmap = Bitmap.CreateBitmap(targetBmp, 0, 0, targetBmp.Width, targetBmp.Height, matrix, true);
        return scaledBitmap;
    }
    public static void SetActionIcon(IMenuItem item, Action action)
    {
        //if (action.SystemIcon != null)
        //{
        //    //var id = Android.Content.Res.Resources.System.GetIdentifier("ic_menu_add", "drawable", Platform.CurrentActivity.PackageName);
        //    var icon = Platform.CurrentActivity.GetDrawable(Android.Resource.Drawable.IcDelete);
        //    item.SetIcon(icon);
        //}
        //else
        if (action.Icon != null)
        {
            var id = Platform.CurrentActivity.GetDrawableId(((IFileImageSource)action.Icon).File);
            var drawable = Platform.CurrentActivity.GetDrawable(id);
            var size = (int)DpToPixel(32f);
            var bitmap = ScaleBitmap(((BitmapDrawable)drawable).Bitmap, size, size);
            var r = new BitmapDrawable(Platform.CurrentActivity.Resources, bitmap);
            item.SetIcon(r);
        }
    }
    public static void AddAction(Action action, IMenu amenu)
    {
        var item = amenu.Add(action.Title);
        item.SetVisible(action.IsVisible);
        item.SetEnabled(action.IsEnabled);
        SetActionIcon(item, action);
        MenuItemCompat.SetContentDescription(item, action.Title);
        item.SetOnMenuItemClickListener(new MenuItemClickListener(action));
    }
    public static void AddGroup(Group group, IMenu amenu, int groupId)
    {
        var itemId = 0;
        foreach (var item in group.Children)
        {
            AddGroupItem(item, amenu, groupId, ++itemId);
        }
    }
    public static void AddSubmenu(Menu menu, IMenu amenu, int itemId)
    {
        var submenu = amenu.AddSubMenu(menu.Title);
        foreach (var item in menu.Children)
        {
            AddRootMenuItem(item, submenu);
        }
    }
    public static void AddGroupItem(MenuElement item, IMenu amenu, int groupId, int itemId)
    {
        if (item is Action action)
        {
            AddGroupAction(action, amenu, groupId, itemId);
        }
        else if (item is Group group)
        {
            throw new InvalidOperationException("Nested ContextMenu groups is not supported");
        }
        else if (item is Menu menu)
        {
            AddGroupMenu(menu, amenu, groupId, itemId);
        }
    }
    public static void AddGroupAction(Action action, IMenu amenu, int groupId, int itemId)
    {
        var item = amenu.Add(groupId, itemId, itemId, action.Title);
        item.SetEnabled(action.IsEnabled);
        item.SetVisible(action.IsVisible);
        SetActionIcon(item, action);
    }
    public static void AddGroupMenu(Menu menu, IMenu amenu, int groupId, int itemId)
    {
        var submenu = amenu.AddSubMenu(groupId, itemId, itemId, menu.Title);
        foreach (var item in menu.Children)
        {
            AddRootMenuItem(item, submenu);
        }
    }
}


public class ContextMenuListener : Java.Lang.Object, IOnCreateContextMenuListener
{
    readonly VisualElement _visualElement;

    public ContextMenuListener(VisualElement visualElement) : base()
    {
        _visualElement = visualElement;
    }

    public void OnCreateContextMenu(Android.Views.IContextMenu amenu, AView v, Android.Views.IContextMenuContextMenuInfo menuInfo)
    {
        var menuTemplate = ContextMenu.GetMenu(_visualElement);

        var content = menuTemplate.CreateContent();

        if (content is Menu menu)
        {
            BindableObject.SetInheritedBindingContext(menu, _visualElement.BindingContext);

            if (!string.IsNullOrEmpty(menu.Title))
            {
                amenu.SetHeaderTitle(menu.Title);
            }

            foreach (var item in menu.Children)
            {
                ContextMenu.AddRootMenuItem(item, amenu);
            }
#if ANDROID28_0_OR_GREATER
            amenu.SetGroupDividerEnabled(true);
#endif
        }
        else
        {
            throw new NotSupportedException("Only Menus can be used in a MenuTemplate");
        }

    }
}

public class MenuItemClickListener : Java.Lang.Object, IMenuItemOnMenuItemClickListener
{
    readonly Action _action;

    public MenuItemClickListener(Action action)
    {
        _action = action;
    }

    public bool OnMenuItemClick(Android.Views.IMenuItem item)
    {
        _action.Command?.Execute(_action.CommandParameter);

        return true;
    }
}

public class OnClickListener : Java.Lang.Object, IOnClickListener
{
    private BindableObject _propertyOwner;
    private BindableObject _contextOwner;


    public OnClickListener(BindableObject propertyOnwer, BindableObject contextOwner) : base()
    {
        _propertyOwner = propertyOnwer;
        _contextOwner = contextOwner;
    }
    public void OnClick(AView v)
    {
        ContextMenu.ExecuteClickCommand(_propertyOwner, _contextOwner.BindingContext);
    }
}

public class MenuActionListener : Java.Lang.Object, IOnLongClickListener, IOnClickListener
{
    private BindableObject _propertyOwner;
    private BindableObject _contextOwner;


    public MenuActionListener(BindableObject propertyOnwer, BindableObject contextOwner) : base()
    {
        _propertyOwner = propertyOnwer;
        _contextOwner = contextOwner;
    }

    public void OnClick(AView v)
    {
        ShowMenu(v);
    }

    public bool OnLongClick(AView v)
    {
        ShowMenu(v);
        return true;
    }

    public void ShowMenu(AView aview)
    {
        var amenu = new PopupMenu(Platform.CurrentActivity, aview);
        amenu.SetForceShowIcon(true);

        var menuTemplate = ContextMenu.GetMenu(_propertyOwner);

        var content = menuTemplate.CreateContent();

        if (content is Menu menu)
        {
            BindableObject.SetInheritedBindingContext(menu, _contextOwner.BindingContext);

            if (!string.IsNullOrEmpty(menu.Title))
            {
                //amenu.Menu.SetHeaderTitle(menu.Title);
            }

            foreach (var item in menu.Children)
            {
                ContextMenu.AddRootMenuItem(item, amenu.Menu);
            }
#if ANDROID28_0_OR_GREATER
            amenu.Menu.SetGroupDividerEnabled(true);
#endif
        }

        amenu.Show();
        //var anim = new Android.Views.Animations.ScaleAnimation(
        //    1f, 1.3f, // Start and end values for the X axis scaling
        //    1f, 1.3f, // Start and end values for the Y axis scaling
        //    Android.Views.Animations.Dimension.RelativeToSelf, .5f, // Pivot point of X scaling
        //    Android.Views.Animations.Dimension.RelativeToSelf, .5f);
        //anim.FillAfter = true; // Needed to keep the result of the animation
        //anim.Duration = 1000;
        //aview.BringToFront();
        //aview.Elevation = 2;

        //aview.StartAnimation(anim);
        //aview.Animation.AnimationEnd += (s, e) =>
        //{
        //    amenu.Show();
        //    void onDismiss(object sender, EventArgs e)
        //    {
        //        amenu.DismissEvent -= onDismiss;
        //        aview.ClearAnimation();
        //    }
        //    amenu.DismissEvent += onDismiss;
        //};
    }
}
