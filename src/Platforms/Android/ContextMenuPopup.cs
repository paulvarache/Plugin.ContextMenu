using Android.Content;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.View.Menu;
using AndroidX.RecyclerView.Widget;
using Java.Lang;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using static Android.Icu.Text.CaseMap;
using AView = Android.Views.View;

namespace Plugin.ContextMenu;

internal class SubMenuSelectedEventArgs : EventArgs
{
    public SubMenuSelectedEventArgs(AView view, SubMenuBuilder builder)
    {
        View = view;
        Builder = builder;
    }

    public AView View { get; }
    public SubMenuBuilder Builder { get; }
}

internal class ContextMenuItemView : LinearLayout
{
    Context _context;
    TextView _text;
    ImageView _image;
    AView _divider;

    public ContextMenuItemView(Context context) : base(context)
    {
        _context = context;
        Setup();
    }

    void Setup()
    {
        var rootLayout = new LinearLayout(_context)
        {
            Orientation = Orientation.Vertical
        };

        _divider = new AView(_context);
        _divider.Background = new ColorDrawable(Android.Graphics.Color.Black);
        _divider.Alpha = .4f;

        rootLayout.AddView(_divider, new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, 1));

        LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
        var outValue = new TypedValue();
        _context.Theme.ResolveAttribute(Resource.Attribute.selectableItemBackground, outValue, true);
        var layout = new LinearLayout(_context)
        {
            LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.MatchParent),
            Foreground = _context.GetDrawable(outValue.ResourceId),
            Orientation = Orientation.Horizontal,
        };
        layout.SetGravity(GravityFlags.CenterVertical);
        layout.SetPadding(ViewUtils.DpToPx(16), ViewUtils.DpToPx(13), ViewUtils.DpToPx(16), ViewUtils.DpToPx(13));


        _text = new TextView(_context)
        {
            LayoutParameters = new LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.WrapContent)
            {
                Weight = 1
            },
        };

        layout.AddView(_text);

        var p = new LinearLayout.LayoutParams(ViewUtils.DpToPx(24), ViewUtils.DpToPx(24))
        {
            MarginStart = ViewUtils.DpToPx(16),
            Gravity = GravityFlags.Right,
        };

        _image = new ImageView(_context)
        {
            LayoutParameters = p,
            ImportantForAccessibility = ImportantForAccessibility.No,
        };


        layout.AddView(_image);

        rootLayout.AddView(layout, new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent));

        AddView(rootLayout, new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent));
    }
    public override bool Enabled
    {
        get => base.Enabled;
        set
        {
            base.Enabled = value;
            Alpha = value ? 1f : .5f;
        }
    }

    public void SetTitle(ICharSequence title)
    {
        _text.SetText(title, TextView.BufferType.Normal);
    }

    public void SetIcon(Drawable icon)
    {
        _image.SetImageDrawable(icon);
    }

    public void SetGroupDividerEnabled(bool enabled)
    {
        _divider.Visibility = enabled ? ViewStates.Visible : ViewStates.Gone;
    }
}

internal class ContextMenuViewAdapter : RecyclerView.Adapter
{
    internal class ViewHolder : RecyclerView.ViewHolder, AView.IOnClickListener
    {
        public event EventHandler<int> Clicked;

        public ViewHolder(AView itemView) : base(itemView)
        {
            itemView.SetOnClickListener(this);
        }

        public void OnClick(AView v)
        {
            Clicked?.Invoke(this, BindingAdapterPosition);
        }
    }

    Context _context;
    MenuBuilder _menu;

    public event EventHandler ActionSelected;
    public event EventHandler<SubMenuSelectedEventArgs> SubMenuSelected;

    public override int ItemCount => _menu.Size();

    public ContextMenuViewAdapter(Context context, MenuBuilder menu)
    {
        _context = context;
        _menu = menu;
    }

    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
    {
        if (holder is ContextMenuViewAdapter.ViewHolder h && h.ItemView is ContextMenuItemView view)
        {
            var item = _menu.GetItem(position);
            view.SetTitle(item.TitleFormatted);
            view.Enabled = item.IsEnabled;
            if (item.HasSubMenu)
            {

            }
            else
            {
                view.SetIcon(item.Icon);
            }
            h.Clicked += OnItemClicked;

            var currGroupId = item.GroupId;
            var prevGroupId = position - 1 >= 0 ? _menu.GetItem(position - 1).GroupId : currGroupId;

            view.SetGroupDividerEnabled(currGroupId != prevGroupId);
        }
    }

    public override void OnViewRecycled(Java.Lang.Object holder)
    {
        if (holder is ContextMenuViewAdapter.ViewHolder h)
        {
            h.Clicked -= OnItemClicked;
        }

        base.OnViewRecycled(holder);
    }

    private void OnItemClicked(object sender, int position)
    {
        var item = _menu.GetItem(position);

        var itemImpl = (MenuItemImpl)item;

        if (itemImpl == null || !itemImpl.IsEnabled)
        {
            return;
        }
        var invoked = itemImpl.Invoke();

        if (itemImpl.HasSubMenu)
        {
            if (sender is ContextMenuViewAdapter.ViewHolder holder)
            {
                var subMenu = (SubMenuBuilder)itemImpl.SubMenu;
                SubMenuSelected?.Invoke(this, new SubMenuSelectedEventArgs(holder.ItemView, subMenu));
            }
        }
        else
        {
            ActionSelected?.Invoke(this, EventArgs.Empty);
        }

    }

    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
    {
        var item = new ContextMenuItemView(_context);

        return new ViewHolder(item);
    }
}

internal class ContextMenuPopup : PopupWindow
{
    AView _anchor;
    RecyclerView _recyclerView;
    ContextMenuViewAdapter _adapter;
    MenuBuilder _menu;
    ContextMenuPopup? _parent;

    public ContextMenuPopup(AView anchor, SubMenuBuilder subMenu, ContextMenuPopup parent) : base(anchor.Context)
    {
        _parent = parent;
        _parent.DismissEvent += OnParentClosed;
        Setup(anchor, subMenu);
    }

    private void OnParentClosed(object sender, EventArgs e)
    {
        Dismiss();
    }

    public ContextMenuPopup(AView anchor) : base(anchor.Context)
    {
        Setup(anchor, new MenuBuilder(anchor.Context));
    }

    void Setup(AView anchor, MenuBuilder menu)
    {
        _menu = menu;
        _anchor = anchor;
        _recyclerView = new RecyclerView(anchor.Context);
        _adapter = new ContextMenuViewAdapter(anchor.Context, _menu);
        _recyclerView.SetAdapter(_adapter);
        _recyclerView.SetLayoutManager(new LinearLayoutManager(anchor.Context));
        ContentView = _recyclerView;

        var s = new Android.Graphics.Drawables.ShapeDrawable(new Android.Graphics.Drawables.Shapes.RoundRectShape(new float[] {
            ViewUtils.DpToPx(18),
            ViewUtils.DpToPx(18),
            ViewUtils.DpToPx(18),
            ViewUtils.DpToPx(18),
            ViewUtils.DpToPx(18),
            ViewUtils.DpToPx(18),
            ViewUtils.DpToPx(18),
            ViewUtils.DpToPx(18)
        }, null, null));
        s.Paint.Color = Android.Graphics.Color.White;
        SetBackgroundDrawable(s);
        ContentView.Background = s;
        ContentView.ClipToOutline = true;
        OutsideTouchable = true;
        // TODO: increase to 9 if submenu, and add 1 for each level of submenu
        Elevation = ViewUtils.DpToPx(8);

        _adapter.ActionSelected += OnActionSelected;
        _adapter.SubMenuSelected += OnSubMenuSelected;
    }

    private void OnSubMenuSelected(object sender, SubMenuSelectedEventArgs e)
    {
        var p = new ContextMenuPopup(e.View, e.Builder, this);
        p.Show(0, 0);
    }

    public override void Dismiss()
    {
        base.Dismiss();
        _parent?.Dismiss();
    }

    private void OnActionSelected(object sender, EventArgs e)
    {
        Dismiss();
    }

    public IMenu Menu => _menu;
    public void Show(int offsetX, int offsetY)
    {
        _adapter.NotifyDataSetChanged();
        ShowAsDropDown(_anchor, offsetX, offsetY, GravityFlags.Top | GravityFlags.Start);
    }

    public int GetHeight()
    {
        ContentView.Measure((int)MeasureSpecMode.Unspecified, (int)MeasureSpecMode.Unspecified);
        return ContentView.MeasuredHeight;
    }
}
