using System.Buffers;
using SFML.Graphics;
using SFML.System;

public struct KTextLayer
{
    public byte FontSize; 
    public Font Font;
    //public KBufferRegion StaticRegion;
    public KDrawLayer DrawLayer;

    public KTextLayer(Vector2f size, Font font, byte fontSize, bool isStatic = false)
    {
        FontSize = fontSize;
        Font = font;
        //StaticRegion = staticRegion;
        DrawLayer = new()
        {
            IsStatic = isStatic,
            Upscale = false,
            Size = size,
            States = RenderStates.Default,
            Region = new(),
            Atlas = new(),
        };
    }

    public void StaticDrawText()
    {
        
    }

    public void DrawText()
    {
        
    }

    public Glyph GetGlyph(byte ch, bool bold, bool updateTexture = false)
    {
        var glyph = Font.GetGlyph(ch, FontSize, bold, 0);
        
        if (updateTexture) DrawLayer.States.Texture = Font.GetTexture(FontSize);
        
        return glyph;
    }

    public Glyph GetGlyph(KGlyphHandle glyphHandle, bool updateTexture = false)
    {
        var glyph = Font.GetGlyph(glyphHandle.Chr, glyphHandle.Size, glyphHandle.Bold, 0);
        
        if (updateTexture) DrawLayer.States.Texture = Font.GetTexture(FontSize);
        
        return glyph;
    }
}

public struct KText
{
    public Color Color;
    public string TextStr;
    public nint FontHandle;
    public byte Size;
    public bool Bold;

    public KText()
    {
        TextStr = string.Empty;
        FontHandle = 0;
        Size = 12;
        Bold = false;
    }

    public KText(string str, Color color, nint fontHandle, byte size = 12, bool bold = false)
    {
        TextStr = str;
        Color = color;
        FontHandle = fontHandle;
        Size = size;
        Bold = bold;
    }
}

public struct KTextBox
{
    public FloatRect Bounds; 
    public KGlyphHandle[] Glyphs;

    public KTextBox(FloatRect bounds, KGlyphHandle[] glyphs)
    {
        Bounds = bounds;
        Glyphs = glyphs;
    }
}

public struct KGlyphHandle 
{
    public bool Bold;
    public byte FontID;
    public byte Size; 
    public char Chr; 

    public KGlyphHandle(byte fontID, char chr, byte size, bool bold)
    {
        FontID = fontID ;
        Chr = chr;
        Size = size;
        Bold = bold;
    }
}

public class KTextHandler
{
    private KRenderManager _renderer;
    private Dictionary<KGlyphHandle, Glyph> _glyphCache;
    public KTextLayer[] TextLayers;

    public KTextHandler(KRenderManager renderer)
    {
        _renderer = renderer;
        _glyphCache = new(256);

        TextLayers = [];
    }

    public void Init(KTextLayer[] textLayers)
    {
        TextLayers = textLayers;
    }

    public void Update()
    {
        
    }

    public void FrameUpdate(KRenderManager renderer)
    {
        for (int i = 0; i < TextLayers.Length; i++)
        {
            renderer.DrawLayer(ref TextLayers[i].DrawLayer);
        }
    }

    public void DrawBuffer(Vertex[] vertices, uint vCount, int layer)
    {
        ref var region = ref TextLayers[layer].DrawLayer.Region;
        if (region.Count + vCount > region.Capacity) vCount = region.Capacity - region.Count;

        _renderer.VertexBuffer.Update(vertices, vCount, region.Offset + region.Count);
        region.Count += vCount;
    }

    public void DrawText(string text, Vector2f pos, int textLayer, Color color, out FloatRect bounds, byte lnSpacing = 0, bool bold = false, uint wrapThres = 0)
    {
        var buffer = ArrayPool<Vertex>.Shared.Rent(text.Length * 6); 
        ref var l = ref TextLayers[textLayer];
        bounds = new FloatRect(pos, (0,0));
        
        pos.Y += l.FontSize;
        
        for (int i = 0; i < text.Length; i++)
        {
            var handle = new KGlyphHandle((byte)textLayer, text[i], l.FontSize, bold);

            if (text[i] == '\n')
            {
                pos.X = bounds.Position.X;
                pos.Y += l.FontSize + lnSpacing;
                
                buffer[i * 6] = default;
                buffer[i * 6 + 1] = default;
                buffer[i * 6 + 2] = default;
                buffer[i * 6 + 3] = default;
                buffer[i * 6 + 4] = default;
                buffer[i * 6 + 5] = default;
                continue;    
            }

            if (!_glyphCache.TryGetValue(handle, out Glyph glyph))
            {
                glyph = l.GetGlyph(handle, true);
                _glyphCache.Add(handle, glyph);
            }
            
            buffer[i * 6] = new Vertex 
            { 
                Position = pos + glyph.Bounds.Position,
                Color = color,
                TexCoords = (Vector2f)glyph.TextureRect.Position,
            };
            buffer[i * 6 + 1] = new Vertex 
            { 
                Position = (pos.X + glyph.Bounds.Left + glyph.Bounds.Width, pos.Y + glyph.Bounds.Top),
                Color = color,
                TexCoords = (glyph.TextureRect.Left + glyph.TextureRect.Width, glyph.TextureRect.Top),
            };
            buffer[i * 6 + 2] = new Vertex 
            { 
                Position = (pos.X + glyph.Bounds.Left, pos.Y + glyph.Bounds.Top + glyph.Bounds.Height),
                Color = color,
                TexCoords = (glyph.TextureRect.Left, glyph.TextureRect.Top + glyph.TextureRect.Height),
            };

            buffer[i * 6 + 3] = new Vertex 
            { 
                Position = (pos.X + glyph.Bounds.Left + glyph.Bounds.Width, pos.Y + glyph.Bounds.Top),
                Color = color,
                TexCoords = (glyph.TextureRect.Left + glyph.TextureRect.Width, glyph.TextureRect.Top),
            };
            buffer[i * 6 + 4] = new Vertex 
            { 
                Position = pos + glyph.Bounds.Position + glyph.Bounds.Size,
                Color = color,
                TexCoords = (Vector2f)(glyph.TextureRect.Position + glyph.TextureRect.Size),
            };
            buffer[i * 6 + 5] = new Vertex 
            { 
                Position = (pos.X + glyph.Bounds.Left, pos.Y + glyph.Bounds.Top + glyph.Bounds.Height),
                Color = color,
                TexCoords = (glyph.TextureRect.Left, glyph.TextureRect.Top + glyph.TextureRect.Height),
            };
        
            pos.X += glyph.Advance;

            if (wrapThres > 0 && bounds.Size.X + glyph.Advance > wrapThres)
            {
                pos.X = bounds.Position.X;
                pos.Y += l.FontSize + lnSpacing;
            }

            if (pos.X - bounds.Position.X > bounds.Width)
            {
                bounds.Size.X = pos.X - bounds.Position.X;
            }
        }

        bounds.Size.Y = pos.Y - bounds.Position.Y;
        pos.Y -= l.FontSize;

        DrawBuffer(buffer, (uint)text.Length * 6, 0);

        ArrayPool<Vertex>.Shared.Return(buffer);
    }

    public void DrawText(in KText text, Vector2f pos, int textLayer, out FloatRect bounds, uint wrapThres = 0)
    {
        DrawText(text.TextStr, pos, textLayer, text.Color, out FloatRect b);
        bounds = b;
    }
}

    
//public class KTextHandler
//{
//    #region static

//        private static void UpdateTexture(KTextHandler handler, KGlyphHandle glyph)
//        {
//            for (int i = 0; i < handler.TextLayers.Length; i++)
//            {
//                if ((glyph.FontID, glyph.Size) == (handler.TextLayers[i].FontID, handler.TextLayers[i].FontSize))
//                {
//                    handler.TextLayers[i].RenderStates.Texture = handler.Fonts[glyph.FontID].GetTexture(glyph.FontID);    
//                    return;
//                }
//            }
//#if DEBUG
//            KProgram.LogManager.DebugLog($"failed to locate and update font texture: id:{glyph.FontID} size:{glyph.Size}.");
//#endif
//        }

//        #endregion

//        private KRenderManager _renderer;
//        private Dictionary<KGlyphHandle, Glyph> _glyphCache;

//        public Font[] Fonts;
//        public KTextLayer[] TextLayers;
//        public event Action<KTextHandler, KGlyphHandle>? GlyphCacheUpdated;

//        public KTextHandler(KRenderManager renderer)
//        {
//            _renderer = renderer;
//            _glyphCache = new(52);
//            TextLayers = [];
//            Fonts = [];
//        }

//        public void Init(Font[] fonts, KTextLayer[] layers)
//        {
//            Fonts = fonts;
//            TextLayers = layers;
//        }

//        public void FrameUpdate(KRenderManager renderer)
//        {
//            if (Fonts.Length < 1 || TextLayers.Length < 1) return;
                
//            for (int i = 0; i < TextLayers.Length; i++)
//            {
//                ref var region = ref TextLayers[i].BufferRegion;
                
//            }
//        }

//        public void DrawText(Vector2f pos, string text, byte fontID, byte fontSize, bool bold, Color color,
//            byte lnSpacing = 0,
//            byte wrapThreshold = 0)
//        {
//            var chars = text.AsSpan();

            
//            var buffer = ArrayPool<Vertex>.Shared.Rent(chars.Length * 6); 

//            for (int i = 0; i < chars.Length; i++)
//            {
//                var handle = new KGlyphHandle(fontID, chars[fontID], fontSize, bold);
//                var bounds = new FloatRect(pos, (0,0));

//                if (chars[i] == '\n')
//                {
//                    bounds.Position.X = 0;
//                    bounds.Position.Y -= fontSize + lnSpacing;
//                    buffer[i] = default;
//                    continue;    
//                }

//                var glyph = GetGlyphFromCache(handle);

//                buffer[i * 6] = new Vertex 
//                { 
//                    Position = pos + glyph.Bounds.Position,
//                    Color = color,
//                    TexCoords = (Vector2f)glyph.TextureRect.Position,
//                };
//                buffer[i * 6 + 1] = new Vertex 
//                { 
//                    Position = (pos.X + glyph.Bounds.Left + glyph.Bounds.Width, pos.Y + glyph.Bounds.Top),
//                    Color = color,
//                    TexCoords = (glyph.TextureRect.Left + glyph.TextureRect.Width, glyph.TextureRect.Top),
//                };
//                buffer[i * 6 + 2] = new Vertex 
//                { 
//                    Position = (pos.X + glyph.Bounds.Left, pos.Y + glyph.Bounds.Top + glyph.Bounds.Height),
//                    Color = color,
//                    TexCoords = (glyph.TextureRect.Left, glyph.TextureRect.Top + glyph.TextureRect.Height),
//                };
//                buffer[i * 6 + 3] = new Vertex 
//                { 
//                    Position = (pos.X + glyph.Bounds.Left + glyph.Bounds.Width, pos.Y + glyph.Bounds.Top + glyph.Bounds.Height),
//                    Color = color,
//                    TexCoords = (glyph.TextureRect.Left + glyph.TextureRect.Width, glyph.TextureRect.Top + glyph.TextureRect.Height),
//                };

//                if (wrapThreshold > 0 && bounds.Size.X + glyph.Advance > wrapThreshold)
//                {
//                    bounds.Position.X = 0;
//                    bounds.Position.Y -= fontSize + lnSpacing;
//                }
//            }

//            //_renderer.DrawBufferToLayer(buffer, (uint)chars.Length * 6);

//            ArrayPool<Vertex>.Shared.Return(buffer);
//        }

//        public KTextBox CreateTextBox(Vector2f position, string text, Color color, byte fontID, byte fontSize, 
//            bool bold = false, 
//            byte lnSpacing = 4,
//            int wrapThreshold = 0)
//        {
//            FloatRect bounds = new FloatRect(position, (0,0));

//            if (string.IsNullOrEmpty(text)) return new KTextBox(bounds, []);

//            var chars = text.AsSpan();
//            var buffer = new KGlyphHandle[chars.Length];

//            for (int i = 0; i < chars.Length; i++)
//            {
//                var handle = new KGlyphHandle(fontID, chars[fontID], fontSize, bold);

//                if (chars[i] == '\n')
//                {
//                    bounds.Position.X = 0;
//                    bounds.Position.Y -= fontSize + lnSpacing;
//                    buffer[i] = new KGlyphHandle(fontID, chars[i], fontSize, bold);
//                    continue;    
//                }

//                var glyph = GetGlyphFromCache(handle);

//                if (wrapThreshold > 0 && bounds.Size.X + glyph.Advance > wrapThreshold)
//                {
//                    bounds.Position.X = 0;
//                    bounds.Position.Y -= fontSize + lnSpacing;
//                }
//            }
//            return new KTextBox(new FloatRect(position, bounds.Size), buffer);
//        }

//        public Glyph GetGlyphFromCache(KGlyphHandle handle)
//        {
//            if (!_glyphCache.TryGetValue(handle, out Glyph glyph))
//            {
//                glyph = Fonts[handle.FontID].GetGlyph(handle.Chr, handle.Size, handle.Bold, 0);
//                _glyphCache.Add(handle, glyph);
//#if DEBUG
//                KProgram.LogManager.DebugLog($"Glyph cached: fontID: {handle.FontID}, char: {handle.Chr}, bold: {handle.Bold}");

//#endif
//                GlyphCacheUpdated?.Invoke(this, handle);
//            }
//            return glyph;
//        }
//    }
