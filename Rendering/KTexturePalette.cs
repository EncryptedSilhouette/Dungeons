using SFML.Graphics;
using SFML.System;

public delegate void PaletteHandler(ref KTexturePalette palette);

public struct KTexturePalette
{
    public bool Enabled;
    public Color BackgroundColor;
    public Vector2f Position;
    public Texture Texture;
    public RenderTexture RenderTexture;
    public KTileMap TileMap;

    public event PaletteHandler? PaletteUpdated;

    public KTexturePalette(Texture atlas, KTileMap tileMap)
    {
        Enabled = false;
        BackgroundColor = new(100, 100, 100);
        Position = (0, 0);

        Texture = atlas;
        TileMap = tileMap;
        
        RenderTexture = new(atlas.Size);
        RenderTexture.Clear(BackgroundColor);
        RenderTexture.Draw(new Sprite(atlas));
        RenderTexture.Display();
        
        PaletteUpdated += (ref pal) => pal.Texture = pal.RenderTexture.Texture;
    }

    public void Init(Texture atlas)
    {
        Texture = atlas;
        RenderTexture = new(atlas.Size);
        RenderTexture.Draw(new Sprite(atlas));
        RenderTexture.Display();
    }

    public void FrameUpdate(KRenderManager renderer, int layer)
    {
        //if (TileMap.Enabled) TileMap.FrameUpdate(renderer, layer);

        renderer.DrawRect(new FloatRect(Position, (Vector2f)Texture.Size), BackgroundColor, layer);
        renderer.DrawRect(
            new FloatRect(Position, (Vector2f)Texture.Size), 
            new FloatRect((0, 0), (Vector2f)Texture.Size), Color.White, layer);
    }

    public void DrawBuffer(Vertex[] vertices, uint vCount, PrimitiveType primitive, RenderStates states)
    {
        RenderTexture.Draw(vertices, 0, vCount, primitive, states);
        PaletteUpdated?.Invoke(ref this);
    }

    public void DrawBuffer(Vertex[] vertices, uint vCount, PrimitiveType primitive) => 
        DrawBuffer(vertices, vCount, primitive, new(Texture)); 

    public void Clear() => RenderTexture.Clear(BackgroundColor);
}