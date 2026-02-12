using SFML.Graphics;
using SFML.System;
using SFML.Window;

public delegate void PaletteHandler(ref KTexturePalette palette);

public struct KTexturePalette
{
    public bool Enabled;
    public Color BackgroundColor;
    public Vector2f Position;
    public Texture Texture;
    public RenderTexture RenderTexture;

    public event PaletteHandler? PaletteUpdated;

    public KTexturePalette(Texture atlas)
    {
        Enabled = false;
        BackgroundColor = new(100, 100, 100);
        Position = (0, 0);

        Texture = atlas;
        RenderTexture = new(atlas.Size);
        RenderTexture.Clear(BackgroundColor);
        RenderTexture.Draw(new Sprite(atlas));
        RenderTexture.Display();
        
        PaletteUpdated += (ref pal) => pal.Texture = pal.RenderTexture.Texture;
    }

    public void Init(Texture atlas)
    {
        Enabled = false;
        BackgroundColor = new(100, 100, 100);
        Position = (0, 0);

        Texture = atlas;
        RenderTexture = new(atlas.Size);
        RenderTexture.Draw(new Sprite(atlas));
        RenderTexture.Display();
    }

    public void FrameUpdate(KRenderer renderer, int layer)
    {
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

public enum KEditorTool
{
    CURSOR = 0,
    TILE_BRUSH = 1
}

public struct KTileMap
{
    public bool Enabled;
    public KGrid Grid;
    public FloatRect[] TileSet;
    public Vertex[] Buffer;

    public KTileMap(in KGrid grid, FloatRect[] tileSet)
    {
        Grid = grid;
        TileSet = tileSet;   
        Buffer = new Vertex[grid.CellCount * 6];
    }

    public Vertex[] BakeVertices()
    {
        for (int i = 0; i < Grid.CellCount; i++)
        { 
            Color color;
            FloatRect tRect;
            var pos = Grid.IndexToCoords(i);
            
            color = Color.White;
            tRect = TileSet[Grid.Cells[i]];

            Buffer[i * 6] = new(pos, color, tRect.Position);
            Buffer[i * 6 + 1] = new((pos.X + Grid.CellSize.X, pos.Y), color, (tRect.Left + tRect.Width, tRect.Top));   
            Buffer[i * 6 + 2] = new((pos.X, pos.Y + Grid.CellSize.Y), color, (tRect.Left, tRect.Top + tRect.Height));

            Buffer[i * 6 + 3] = new((pos.X + Grid.CellSize.X, pos.Y), color, (tRect.Left + tRect.Width, tRect.Top));   
            Buffer[i * 6 + 4] = new(pos + (Vector2f)Grid.CellSize, color, tRect.Position + tRect.Size);   
            Buffer[i * 6 + 5] = new((pos.X, pos.Y + Grid.CellSize.Y), color, (tRect.Left, tRect.Top + tRect.Height));   
        }
        return Buffer;
    }

    public void FrameUpdate(KRenderer renderer, int layer)
    {
        if (renderer.DrawLayers.Length <= layer) return;

        renderer.DrawBuffer(Buffer, (uint)Buffer.Length, layer);
    }

    // public void BakeTexture(in KTexturePalette texturePalette)
    // {
    //     var buffer = BakeVertices();
    //     texturePalette.Clear();
    //     texturePalette.DrawBuffer(buffer, (uint)buffer.Length, PrimitiveType.Triangles);
    // }
}

public class KEditor
{
    public const int EDITOR_LAYER = 0;
    public const int LINE_LAYER = 1;

    public const int TILE_SIZE = 4;

    private KEditorTool _currentTool;

    private Vector2i _mousePos;
    private Vector2f _selectedTile;
    
    public KGrid Grid;
    public KTileMap TileMap;
    public KTexturePalette Palette;

    public KEditor()
    {
        Grid = new()
        {
            Enabled = true,
            Rows = 240 / 4,
            Columns = 320 / 4,
            LineColor = Color.Green,
            Position = (0, 0),
            CellSize = (TILE_SIZE, TILE_SIZE),
        };  

        TileMap = new(Grid, []);
        Palette = new();
        _mousePos = (0, 0);
        _currentTool = KEditorTool.CURSOR;
    }

    public void Init(RenderWindow window, KRenderer renderer, KTextureAtlas atlas)
    {
        window.SetKeyRepeatEnabled(false);
        window.MouseMoved += HandleMouseInput;
        window.MouseButtonPressed += HandleMouseInput;
        window.KeyPressed += HandleKeyInput;

        Palette.Init(atlas.Texture);
        
        var coords = atlas.Coordinates;

        TileMap = new(Grid,
        [
            coords["floor"],
            coords["pit"],
            coords["void"],

            coords["wall_tl"],
            coords["wall_t"],
            coords["wall_tr"],
            coords["wall_l"],
            coords["stairs"],
            coords["wall_r"],
            coords["wall_bl"],
            coords["wall_b"],
            coords["wall_br"],
            
            coords["cliff_tl"],
            coords["cliff_t"],
            coords["cliff_tr"],
            coords["cliff_l"],
            coords["seal"],
            coords["cliff_r"],
            coords["cliff_bl"],
            coords["cliff_b"],
            coords["cliff_br"],

            coords["ice_tl"],
            coords["ice_t"],
            coords["ice_tr"],
            coords["ice_l"],
            coords["ice"],
            coords["ice_r"],
            coords["ice_bl"],
            coords["ice_b"],
            coords["ice_br"],
        ]);
        TileMap.BakeVertices();
    }

    public void Deinit(RenderWindow window)
    {
        window.MouseMoved -= HandleMouseInput;
        window.MouseButtonPressed -= HandleMouseInput;
        window.KeyPressed -= HandleKeyInput;
    }

    public void Update(uint currentFrame)
    {
        
    }

    public void FrameUpdate(uint currentFrame, KRenderer renderer)
    {
        ref var layer = ref renderer.DrawLayers[EDITOR_LAYER];

        if (TileMap.Enabled) TileMap.FrameUpdate(renderer, EDITOR_LAYER);
        if (Palette.Enabled) Palette.FrameUpdate(renderer, EDITOR_LAYER);
        if (Grid.Enabled) Grid.Draw(renderer, LINE_LAYER);

        var index = Grid.CoordsToIndex(
            (_mousePos.X, _mousePos.Y), 
            1 / ((float)renderer.Window.Size.X / renderer.DrawLayers[0].Resolution.X));

        renderer.DrawRect(new FloatRect(Grid.IndexToCoords(index), (TILE_SIZE, TILE_SIZE)), new(255, 255, 0, 150), EDITOR_LAYER);
    }

    public void HandleMouseInput(object? obj, MouseMoveEventArgs args)
    {
        KProgram.Editor._mousePos = args.Position;
    }

    public void HandleMouseInput(object? obj, MouseButtonEventArgs args)
    {
        if (args.Button == Mouse.Button.Left && Palette.Enabled)
        {
            
        }
    }

    public void HandleKeyInput(object? obj, KeyEventArgs args)
    {
        switch (args.Code)
        {
            case Keyboard.Key.Q:
                Palette.Enabled = !Palette.Enabled;
                break;

            case Keyboard.Key.E:
                TileMap.Enabled = !TileMap.Enabled; 
                break;

            case Keyboard.Key.G:
                Grid.Enabled = !Grid.Enabled; 
                break;
            
            default:
                break;
        }
    }
}