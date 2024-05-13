﻿using System;
using Mapsui.Styles;

namespace Mapsui.Rendering;

public interface ISymbolCache : IDisposable
{
    Size? GetSize(SymbolStyle symbolStyle);
    IBitmapInfo GetOrCreate(SymbolStyle symbolStyle);
    Size? GetSize(int bitmapId);
    IBitmapInfo GetOrCreate(int bitmapId);
    IBitmapInfo GetOrCreate(string bitmapPath);
}
