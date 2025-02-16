﻿using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.Styles;
using System;
using Mapsui.Widgets.BoxWidgets;
using System.Collections.Generic;
using Mapsui.Manipulations;

namespace Mapsui.Widgets.InfoWidgets;

public class RulerWidget(Map map) : BaseWidget
{
    private readonly Map _map = map;

    public Color Color { get; set; } = new Color(192, 30, 20, 255);
    public Color ColorOfBeginAndEndDots { get; set; } = new Color(192, 30, 20, 128);
    public MPoint? StartPosition { get; internal set; }
    public MPoint? CurrentPosition { get; internal set; }
    public double? DistanceInKilometers { get; internal set; }
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the info should be shown next to the ruler.
    /// When set to false you can use the RulerWidget.InfoBox fields to position it like any
    /// other widget.
    /// </summary>
    public bool ShowInfoNextToRuler { get; set; } = true;

    public TextBoxWidget InfoBox { get; set; } = new TextBoxWidget
    {
        VerticalAlignment = VerticalAlignment.Bottom,
        HorizontalAlignment = HorizontalAlignment.Left,
        BackColor = new Color(108, 117, 125),
        TextColor = Color.White,
        Padding = new MRect(8, 6),
        CornerRadius = 4,
        Margin = new(4),
    };

    public event EventHandler<RulerWidgetUpdatedEventArgs>? DistanceUpdated = null;

    public override bool OnPointerPressed(Navigator navigator, WidgetEventArgs e)
    {
        CurrentPosition = null;
        StartPosition = _map.Navigator.Viewport.ScreenToWorld(e.ScreenPosition);
        _map.RefreshGraphics();
        return true;
    }

    public override bool OnPointerMoved(Navigator navigator, WidgetEventArgs e)
    {
        if (e.TapType == TapType.Hover)
            return false; // Not dragging.

        CurrentPosition = _map.Navigator.Viewport.ScreenToWorld(e.ScreenPosition);
        DistanceInKilometers = GetDistance(StartPosition, CurrentPosition);
        DistanceUpdated?.Invoke(this, new RulerWidgetUpdatedEventArgs(TapType.Drag));
        _map.RefreshGraphics();
        return true;
    }

    public override bool OnPointerReleased(Navigator navigator, WidgetEventArgs e)
    {
        DistanceUpdated?.Invoke(this, new RulerWidgetUpdatedEventArgs(TapType.Release));
        _map.RefreshGraphics();
        return true;
    }

    public override bool OnTapped(Navigator navigator, WidgetEventArgs e)
    {
        if (e.TapType == TapType.SingleTap)
        {
            StartPosition = _map.Navigator.Viewport.ScreenToWorld(e.ScreenPosition);
            CurrentPosition = null;
            DistanceUpdated?.Invoke(this, new RulerWidgetUpdatedEventArgs(TapType.SingleTap));
            _map.RefreshGraphics();
        }
        return true;
    }

    public void Reset()
    {
        StartPosition = null;
        CurrentPosition = null;
    }

    private static double? GetDistance(MPoint? fromPosition, MPoint? toPosition)
    {
        if (fromPosition == null || toPosition == null)
            return null;

        var fromLatLon = SphericalMercator.ToLonLat(fromPosition);
        var toLatLon = SphericalMercator.ToLonLat(toPosition);

        return Utilities.Haversine.Distance(fromLatLon.X, fromLatLon.Y, toLatLon.X, toLatLon.Y);
    }

    public (IFeature? startFeature, IFeature? currentFeature) SnapToFeature(Func<MPoint?, IFeature?> getFeaturesToSnapTo)
    {
        var result = new List<IFeature>();

        var startFeature = getFeaturesToSnapTo(StartPosition);
        if (startFeature != null)
        {
            result.Add(startFeature);
            StartPosition = startFeature?.Extent?.Centroid;
        }

        var currentFeature = getFeaturesToSnapTo(CurrentPosition);
        if (currentFeature != null)
        {
            result.Add(currentFeature);
            CurrentPosition = currentFeature?.Extent?.Centroid ?? CurrentPosition;
        }

        return (startFeature, currentFeature);
    }

    public class RulerWidgetUpdatedEventArgs(TapType tapType) : EventArgs
    {
        public TapType TapType { get; } = tapType;
    }
}
