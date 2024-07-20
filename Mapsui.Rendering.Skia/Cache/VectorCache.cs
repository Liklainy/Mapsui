﻿using System;
using Mapsui.Cache;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class VectorCache(IRenderService renderService, int capacity) : IDisposable
{
    private readonly LruCache<object, ICacheHolder> _paintCache = new(Math.Min(capacity, 1));
    private readonly LruCache<object, ICacheHolder> _pathParamCache = new(Math.Min(capacity, 1));

    public bool Enabled { get; set; } = true;

    public CacheTracker<TPaint> GetOrCreatePaint<TParam, TPaint>(TParam param, Func<TParam, TPaint> toPaint)
        where TParam : notnull
        where TPaint : class
    {
        if (Enabled == false)
            return new CacheTracker<TPaint>(toPaint(param));

#pragma warning disable IDISP001 // Dispose created is cached so I cannot dispose it here
        var holder = _paintCache.GetOrCreateValue(param, f =>
        {
            var paint = toPaint(f);
            return new CacheHolder<TPaint>(paint);
        });
#pragma warning restore IDISP001

        return holder?.Get<TPaint>() ?? new CacheTracker<TPaint>(toPaint(param));
    }

    public CacheTracker<TPaint> GetOrCreatePaint<TParam, TPaint>(TParam param, Func<TParam, IRenderService, TPaint> toPaint)
        where TParam : notnull
        where TPaint : class
    {
        if (Enabled == false)
            return new CacheTracker<TPaint>(toPaint(param, renderService));

#pragma warning disable IDISP001 // Dispose created is cached so I cannot dispose it here        
        var holder = _paintCache.GetOrCreateValue(param, f =>
        {
            var paint = toPaint(f, renderService);
            return new CacheHolder<TPaint>(paint);
        });
#pragma warning restore IDISP001

        return holder?.Get<TPaint>() ?? new CacheTracker<TPaint>(toPaint(param, renderService));
    }

    public CacheTracker<TPath> GetOrCreatePath<TParam, TPath>(TParam param, Func<TParam, TPath> toPath)
        where TParam : notnull
        where TPath : class
    {
        if (Enabled == false)
            return new CacheTracker<TPath>(toPath(param));

#pragma warning disable IDISP001 // Dispose created is cached so I cannot dispose it here        
        var holder = _pathParamCache.GetOrCreateValue(param, f =>
        {
            var path = toPath(f);
            return new CacheHolder<TPath>(path);
        });
#pragma warning restore IDSIP001

        return holder?.Get<TPath>() ?? new CacheTracker<TPath>(toPath(param));
    }

    public void Dispose()
    {
        _pathParamCache.Clear();
        _paintCache.Clear();
    }
}
