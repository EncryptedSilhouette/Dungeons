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

    public KTexturePalette(Texture atlas) =>
        Init(atlas);

    public void Init(Texture atlas)
    {
        Enabled = false;
        BackgroundColor = new(100, 100, 100);
        Position = (0, 0);

        RenderTexture = new(atlas.Size);
        RenderTexture.Clear(BackgroundColor);
        RenderTexture.Draw(new RectangleShape((Vector2f)atlas.Size), new(atlas));
        RenderTexture.Display();
        
        Texture = RenderTexture.Texture;

        PaletteUpdated += (ref pal) => pal.Texture = pal.RenderTexture.Texture;
    }

    public void FrameUpdate(KRenderer renderer, int layer)
    {
        renderer.DrawRect(new FloatRect(Position, (Vector2f)Texture.Size), BackgroundColor, layer);
        renderer.DrawRect(
            new FloatRect(Position, (Vector2f)Texture.Size), 
            new FloatRect(Position, (Vector2f)Texture.Size), Color.White, layer);
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

    public KTileMap(in KGrid grid, FloatRect[] tileSet, int layer)
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
            
            if (i < TileSet.Length)
            {
                color = Color.White;
                tRect = TileSet[Grid.Cells[i]];
            }
            else
            {
                color = Color.Magenta;
                tRect = new FloatRect();
            }

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
    public const int LINE_LAYER = 0;

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
        TileMap = new(Grid, [], EDITOR_LAYER);
        Palette = new();
        _mousePos = (0, 0);
        _currentTool = KEditorTool.CURSOR;
    }

    public void Init(RenderWindow window, KRenderer renderer)
    {
        const int DEFAULT_ATLAS = 0;

        Palette.Init(KProgram.Atlases[DEFAULT_ATLAS].Texture);

        window.SetKeyRepeatEnabled(false);
        window.MouseMoved += HandleMouseInput;
        window.MouseButtonPressed += HandleMouseInput;
        window.KeyPressed += HandleKeyInput;
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

        if (Grid.Enabled)
        {
            if (Palette.Enabled) Palette.FrameUpdate(renderer, EDITOR_LAYER);

            Grid.Draw(renderer, 1);

            var index = Grid.CoordsToIndex(
                (_mousePos.X, _mousePos.Y), 
                1 / ((float)renderer.Window.Size.X / renderer.DrawLayers[0].Resolution.X));

            renderer.DrawRect(new FloatRect(Grid.IndexToCoords(index), (TILE_SIZE, TILE_SIZE)), new(255, 255, 0, 150), EDITOR_LAYER);
        }
    }

    public void HandleMouseInput(object? obj, MouseMoveEventArgs args)
    {
        //not a fan of closure classes.
        _mousePos = args.Position;
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
            
            default:
                break;
        }
    }
}