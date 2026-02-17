using System.Buffers;
using SFML.Graphics;
using SFML.System;

public struct KTextLayer
{
    public byte FontSize; 
    public Font Font;
    public KBufferRegion StaticRegion;
    public KDrawLayer DrawLayer;
    public RenderTexture RenderTexture;

    public KTextLayer(RenderTexture renderTexture, KBufferRegion staticRegion, Vector2f size, Font font, byte fontSize, bool isStatic = false)
    {
        FontSize = fontSize;
        Font = font;
        StaticRegion = staticRegion;
        DrawLayer = new()
        {
            IsStatic = isStatic,
            Upscale = false,
            Size = size,
            States = RenderStates.Default,
            Region = new(),
            Atlas = new(),
        };
        RenderTexture = renderTexture;
    }

    public void StaticDrawText()
    {
        
    }

    public void DrawText()
    {
        
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

    public Font[] Fonts;
    public KTextLayer[] TextLayers;

    public KTextHandler(KRenderManager renderer)
    {
        _renderer = renderer;
        _glyphCache = new(256);

        Fonts = [];
        TextLayers = [];
    }

    public void Init(Font[] fonts, KTextLayer[] textLayers)
    {
        TextLayers = textLayers;
        Fonts = fonts;
    }

    public void Update()
    {
        
    }

    public void FrameUpdate(KRenderManager renderer)
    {
        
    }

    public void DrawText(string text, byte textLayer, bool bold)
    {
        var vCount = 0;
        var buffer = ArrayPool<Vertex>.Shared.Rent(text.Length * 6);

        var chars = text.AsSpan();
        ref var layer = ref TextLayers[textLayer];
        int xOffSet;

        for (int i = 0; i < chars.Length; i++)
        {

            var ch = chars[i];

            if (_glyphCache.TryGetValue(new KGlyphHandle(textLayer, ch, layer.FontSize, bold), out Glyph glyph))
            {
                var b = glyph.Bounds;
                var t = glyph.TextureRect; 
                //glyph.Advance;           

            }
        }
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
