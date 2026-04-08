using SFML.Graphics;
using SFML.System;
using SFML.Window;

public struct KSprite
{
    public int Layer;
    public float Rotation;
    public Color Color;
    public Vector2f Rotocenter;
    public FloatRect Bounds;
    public FloatRect TextureBounds;
    public FloatRect[] Frames;

    public KSprite()
    {
        Rotation = 0.0f;
        Rotocenter = (0, 0);
        Bounds = new();
        TextureBounds = new();
        Frames = [];
    }

    public Vector2f Center => Bounds.Position + Bounds.Size / 2;
    public Vertex VertexA
    {
        get
        {
            Vector2f pos = Rotocenter - Bounds.Size / 2;
            if (Rotation % 360 == 0)
            {
                pos.X = pos.X * MathF.Cos(Rotation) - pos.Y * MathF.Sin(Rotation);
                pos.Y = pos.X * MathF.Sin(Rotation) + pos.Y * MathF.Cos(Rotation);
            }
            return new(Center + pos, Color, TextureBounds.Position);
        }
    }

    public Vertex VertexB
    {
        get
        {
            Vector2f pos = Rotocenter + (Bounds.Width / 2, Bounds.Height / -2);
            if (Rotation % 360 == 0)
            {
                pos.X = pos.X * MathF.Cos(Rotation) - pos.Y * MathF.Sin(Rotation);
                pos.Y = pos.X * MathF.Sin(Rotation) + pos.Y * MathF.Cos(Rotation);
            }
            return new(Center + pos, Color, TextureBounds.Position);
        }
    }

    public Vertex VertexC
    {
        get
        {
            Vector2f pos = Rotocenter + Bounds.Size / 2;
            if (Rotation % 360 == 0)
            {
                pos.X = pos.X * MathF.Cos(Rotation) - pos.Y * MathF.Sin(Rotation);
                pos.Y = pos.X * MathF.Sin(Rotation) + pos.Y * MathF.Cos(Rotation);
            }
            return new(Center + pos, Color, TextureBounds.Position);
        }
    }

    public Vertex VertexD
    {
        get
        {
            Vector2f pos = Rotocenter + (Bounds.Width / -2, + Bounds.Height / 2);
            if (Rotation % 360 == 0)
            {
                pos.X = pos.X * MathF.Cos(Rotation) - pos.Y * MathF.Sin(Rotation);
                pos.Y = pos.X * MathF.Sin(Rotation) + pos.Y * MathF.Cos(Rotation);
            }
            return new(Center + pos, Color, TextureBounds.Position);
        }
    }
}

public struct KRenderLayer
{
    public int AtlasHandle;
    public FloatRect Bounds;
    public PrimitiveType Primitive;
    public RenderStates States;
    public KBufferRegion Region;
    public RenderTexture RenderTexture;
    public Color BackgroundColor;
    public bool Upscale;
    public Texture Texture => RenderTexture.Texture;

    public KRenderLayer(RenderTexture renderTexture, FloatRect bounds, int atlasHandle)
    {
        RenderTexture = renderTexture;
        Bounds = bounds;
        AtlasHandle = atlasHandle;
    }

    public Vector2f GetScaleRelativeTo(Vector2f otherSize) => 
        new(otherSize.X / Bounds.Size.X, otherSize.Y / Bounds.Size.Y);
    public float GetScaleXRelativeTo(float width) => width / Bounds.Size.X;
    public float GetScaleYRelativeTo(float height) => height / Bounds.Size.Y;
}

public class KRenderManager
{
    public const int SCREEN_LAYER = -1;

    private View _view;
    private Vertex[] _drawBuffer;

    public KBufferRegion ScreenRegion;
    public RenderStates States;
    public RenderWindow Window;
    public VertexBuffer VertexBuffer;
    public KTextHandler TextHandler;
    public KRenderLayer[] RenderLayers;

    //public float aspect => Window.Size.Y / Window.Size.X;
    public Vector2u ScreenSize => Window.Size;
    public Vector2u Center => Window.Size / 2;

    public KRenderManager(RenderWindow window, VertexBuffer buffer)
    {
        _view = window.DefaultView;
        _drawBuffer = new Vertex[128];

        States = RenderStates.Default;
        Window = window;
        TextHandler = new(this);
        VertexBuffer = buffer;
        RenderLayers = [];
    }

    public void Init(KBufferRegion screenRegion, KRenderLayer[] drawLayers, KTextLayer[] textLayers)
    {
        ScreenRegion = screenRegion;
        RenderLayers = drawLayers;
        TextHandler.Init(textLayers);
        Window.Resized += ResizeView;
    }

    public void FrameUpdate()
    {     
        for (int i = 0; i < RenderLayers.Length; i++)
        {
            ref var layer = ref RenderLayers[i];
            layer.RenderTexture.Clear(layer.BackgroundColor);

            if (VertexBuffer.PrimitiveType != layer.Primitive) 
                VertexBuffer.PrimitiveType = layer.Primitive;
            
            VertexBuffer.Draw(layer.RenderTexture, layer.Region.Offset, layer.Region.Count, layer.States);
        }

        TextHandler.FrameUpdate(this);

        for (int i = 0; i < RenderLayers.Length; i++)
        {
            ref var layer = ref RenderLayers[i];

            layer.RenderTexture.Display();

            _drawBuffer[0] = new Vertex
            {
                Position = (layer.Bounds.Left, layer.Bounds.Top),
                Color = Color.White,  
                TexCoords = (0, 0),
            };
            _drawBuffer[1] = new Vertex
            {
                Position = (layer.Bounds.Left + layer.Bounds.Width, layer.Bounds.Top),
                Color = Color.White,  
                TexCoords = (layer.RenderTexture.Size.X, 0),
            };
            _drawBuffer[2] = new Vertex
            {
                Position = (layer.Bounds.Left, layer.Bounds.Top + layer.Bounds.Height),
                Color = Color.White,  
                TexCoords = (0, layer.RenderTexture.Size.Y),
            };

            _drawBuffer[3] = new Vertex
            {
                Position = (layer.Bounds.Left + layer.Bounds.Width, layer.Bounds.Top),
                Color = Color.White,  
                TexCoords = (layer.RenderTexture.Size.X, 0),
            };
            _drawBuffer[4] = new Vertex
            {
                Position = (layer.Bounds.Left + layer.Bounds.Width, layer.Bounds.Top + layer.Bounds.Height),
                Color = Color.White,  
                TexCoords = (layer.RenderTexture.Size.X, layer.RenderTexture.Size.Y),
            };
            _drawBuffer[5] = new Vertex
            {
                Position = (layer.Bounds.Left, layer.Bounds.Top + layer.Bounds.Height),
                Color = Color.White,  
                TexCoords = (0, layer.RenderTexture.Size.Y),
            };
            Window.Draw(_drawBuffer, 0, 6, PrimitiveType.Triangles);

            layer.Region.Count = 0;
        }
    }

    public void DrawBuffer(Vertex[] vertices, uint vCount, int layer = SCREEN_LAYER)
    {
        if (layer < 0)
        {
            if (ScreenRegion.Count + vCount > ScreenRegion.Capacity) vCount = ScreenRegion.Capacity - ScreenRegion.Count;

            VertexBuffer.Update(vertices, vCount, ScreenRegion.Offset + ScreenRegion.Count);
            ScreenRegion.Count += vCount;
        }
        else
        {
            ref var region = ref RenderLayers[layer].Region;
            if (region.Count + vCount > region.Capacity) vCount = region.Capacity - region.Count;

            VertexBuffer.Update(vertices, vCount, region.Offset + region.Count);
            region.Count += vCount;
        }
    }

    public void DrawLine(Vector2f pointA, Vector2f pointB, Color color, int lineLayer = SCREEN_LAYER)
    {
        _drawBuffer[0] = new(pointA, color);
        _drawBuffer[1] = new(pointB, color);

        DrawBuffer(_drawBuffer, 2, lineLayer);
    }

    public void DrawRect(float x, float y, float width, float height, Color color, int layer = SCREEN_LAYER)
    {
        _drawBuffer[0] = new((x, y), color, (0, 0));
        _drawBuffer[1] = new((x + width, y), color, (0, 0));   
        _drawBuffer[2] = new((x, y + height), color, (0, 0));

        _drawBuffer[3] = new((x + width, y), color, (0, 0));   
        _drawBuffer[4] = new((x + width, y + height), color, (0, 0));   
        _drawBuffer[5] = new((x, y + height), color, (0, 0));   

        DrawBuffer(_drawBuffer, 6, layer);
    }

    public void DrawRect(in FloatRect rect, Color color, int layer = SCREEN_LAYER)
    {
        _drawBuffer[0] = new((rect.Left, rect.Top), color, (0, 0));
        _drawBuffer[1] = new((rect.Left + rect.Width, rect.Top), color, (0, 0));   
        _drawBuffer[2] = new((rect.Left, rect.Top + rect.Height), color, (0, 0));

        _drawBuffer[3] = new((rect.Left + rect.Width, rect.Top), color, (0, 0));   
        _drawBuffer[4] = new((rect.Left + rect.Width, rect.Top + rect.Height), color, (0, 0));   
        _drawBuffer[5] = new((rect.Left, rect.Top + rect.Height), color, (0, 0));   

        DrawBuffer(_drawBuffer, 6, layer);
    }

    public void DrawRect(in FloatRect rect, in FloatRect textureRect, Color color, int layer = SCREEN_LAYER)
    {
        _drawBuffer[0] = new(rect.Position, color, textureRect.Position);
        _drawBuffer[1] = new((rect.Left + rect.Width, rect.Top), color, (textureRect.Left + textureRect.Width, textureRect.Top));   
        _drawBuffer[2] = new((rect.Left, rect.Top + rect.Height), color, (textureRect.Left, textureRect.Top + textureRect.Height));

        _drawBuffer[3] = new((rect.Left + rect.Width, rect.Top), color, (textureRect.Left + textureRect.Width, textureRect.Top));   
        _drawBuffer[4] = new(rect.Position + rect.Size, color, textureRect.Position + textureRect.Size);   
        _drawBuffer[5] = new((rect.Left, rect.Top + rect.Height), color, (textureRect.Left, textureRect.Top + textureRect.Height));   

        DrawBuffer(_drawBuffer, 6, layer);
    }

    public void DrawRect(Vector2f position, Vector2f size, Color color, int layer = SCREEN_LAYER) => 
        DrawRect(new FloatRect(position, size), color, layer);

    public void DrawSprite(in KSprite sprite, Color color, int layer = SCREEN_LAYER)
    {
        _drawBuffer[0] = sprite.VertexA;
        _drawBuffer[0].Color = color;
        
        _drawBuffer[1] = sprite.VertexB;
        _drawBuffer[1].Color = color;

        _drawBuffer[2] = sprite.VertexD;
        _drawBuffer[2].Color = color;

        _drawBuffer[3] = sprite.VertexB;
        _drawBuffer[3].Color = color;

        _drawBuffer[4] = sprite.VertexC;
        _drawBuffer[4].Color = color;

        _drawBuffer[5] = sprite.VertexD;
        _drawBuffer[5].Color = color;
        
        DrawBuffer(_drawBuffer, 6, layer);
    }

    public void DrawSprite(KSprite sprite, int layer = SCREEN_LAYER)
    {
        _drawBuffer[0] = sprite.VertexA;
        _drawBuffer[1] = sprite.VertexB;
        _drawBuffer[2] = sprite.VertexD;

        _drawBuffer[3] = sprite.VertexB;
        _drawBuffer[4] = sprite.VertexC;
        _drawBuffer[5] = sprite.VertexD;

        DrawBuffer(_drawBuffer, 6, layer);
    }

    public void DrawSprite(in KSprite sprite) =>
        DrawSprite(sprite, sprite.Layer);

    public VertexBuffer ResizeBuffer(uint size, PrimitiveType primitive = PrimitiveType.Points)
    {
        VertexBuffer newBuffer = new(size, primitive, VertexBuffer.UsageSpecifier.Stream);
        newBuffer.Update(VertexBuffer);

        VertexBuffer.Dispose();
        return VertexBuffer = newBuffer;
    }

    private void ResizeView(object? _, SizeEventArgs e)
    {
        _view.Size = (Vector2f)e.Size;
        _view.Center = _view.Size / 2;
        Window.SetView(_view);
    }
} 