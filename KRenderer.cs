using SFML.Graphics;
using SFML.System;
using SFML.Window;

public class KRenderer
{
    public const int SCREEN_LAYER = -1;

    private View _view;
    private Vertex[] _drawBuffer;

    public KBufferRegion ScreenRegion;
    public RenderWindow Window;
    public VertexBuffer VertexBuffer;
    public KDrawLayer[] DrawLayers;

    public float aspect => Window.Size.Y / Window.Size.X;
    public Vector2u ScreenSize => Window.Size;
    public Vector2u Center => Window.Size / 2;

    public KRenderer(RenderWindow window, VertexBuffer buffer)
    {
        _view = window.DefaultView;
        _drawBuffer = new Vertex[6];

        Window = window;
        VertexBuffer =  buffer;
        DrawLayers = [];

        window.Resized += ResizeView;
    }

    public void Init(KBufferRegion screenRegion, KDrawLayer[] drawLayers)
    {
        ScreenRegion = screenRegion;
        DrawLayers = drawLayers;
    }

    public void FrameUpdate()
    {     
        for (int i = 0; i < DrawLayers.Length; i++)
        {
            ref var l = ref DrawLayers[i];
            var renderStates = l.States;

            if (VertexBuffer.PrimitiveType != l.Primitive) 
                VertexBuffer.PrimitiveType = l.Primitive;

            if (l.Upscale)
                renderStates.Transform
                    .Scale(((float)Window.Size.X / l.Resolution.X, 
                            (float)Window.Size.X / l.Resolution.X));

            VertexBuffer.Draw(Window, l.Region.Offset, l.Region.Count, renderStates);

            if (!l.IsStatic) l.Region.Count = 0;
        }
    }

    public void DrawBuffer(Vertex[] vertices, uint vCount, int layer = SCREEN_LAYER)
    {
        //Unholy.
        
        if (layer < 0)
        {
            if (ScreenRegion.Count + vCount > ScreenRegion.Capacity) vCount = ScreenRegion.Capacity - ScreenRegion.Count;

            VertexBuffer.Update(vertices, vCount, ScreenRegion.Offset);
            ScreenRegion.Count += vCount;
        }
        else
        {
            ref var region = ref DrawLayers[layer].Region;
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