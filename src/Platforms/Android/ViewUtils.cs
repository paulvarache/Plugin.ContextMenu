﻿using Android.Content.Res;
using Android.OS;
using AndroidX.Core.View;
using AView = Android.Views.View;

namespace Plugin.ContextMenu;

internal class ViewUtils
{
    public static int DpToPx(int dp)
    {
        return (int)Math.Round(dp * Resources.System.DisplayMetrics.Density);
    }
}
