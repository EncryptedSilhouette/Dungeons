using SFML.Graphics;
using SFML.System;
using SFML.Window;

public delegate void PaletteHandler(ref KTexturePalette palette);

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

    public KTileMap(in KGrid grid) : this(grid, []) { }

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
            Buffer[i * 6 + 1] = new((pos.X + Grid.CellWidth, pos.Y), color, (tRect.Left + tRect.Width, tRect.Top));   
            Buffer[i * 6 + 2] = new((pos.X, pos.Y + Grid.CellHeight), color, (tRect.Left, tRect.Top + tRect.Height));

            Buffer[i * 6 + 3] = new((pos.X + Grid.CellWidth, pos.Y), color, (tRect.Left + tRect.Width, tRect.Top));   
            Buffer[i * 6 + 4] = new(pos + (Grid.CellWidth, Grid.CellHeight), color, tRect.Position + tRect.Size);   
            Buffer[i * 6 + 5] = new((pos.X, pos.Y + Grid.CellHeight), color, (tRect.Left, tRect.Top + tRect.Height));   
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

public enum KEditorTool
{
    CURSOR = 0,
    TILE_BRUSH = 1
}

public class KEditor
{
    public const int EDITOR_LAYER = 0;
    public const int LINE_LAYER = 1;

    public const int TILE_SIZE = 4;

    private KEditorTool _currentTool;
    private Vector2i _mousePos;
    private FloatRect _selectedTile;
    
    public KGrid Grid;
    public KTexturePalette Palette;

    public Vector2i TileUnit => new(TILE_SIZE, TILE_SIZE);

    public KEditor()
    {
        _currentTool = KEditorTool.CURSOR;
        _mousePos = (0, 0);
        _selectedTile = new();

        Grid = new(320 / TILE_SIZE, 240 / TILE_SIZE, 0, 0, TILE_SIZE, TILE_SIZE);
        Palette = new();
    }

    public void Init(RenderWindow window, KRenderer renderer, KTextureAtlas atlas)
    {
        window.SetKeyRepeatEnabled(false);
        window.MouseMoved += HandleMouseInput;
        window.MouseButtonPressed += HandleMouseInput;
        window.KeyPressed += HandleKeyInput;
        
        var coords = atlas.Coordinates;

        Palette = new(atlas.Texture, new(Grid));
        Palette.Init(atlas.Texture);
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

        if (Palette.TileMap.Enabled) Palette.TileMap.FrameUpdate(renderer, EDITOR_LAYER);
        if (Palette.Enabled) Palette.FrameUpdate(renderer, EDITOR_LAYER);
        if (Grid.Enabled) Grid.FrameUpdate(renderer, LINE_LAYER);

        var index = Grid.CoordsToIndex(
            (_mousePos.X, _mousePos.Y), 
            1 / ((float)renderer.Window.Size.X / renderer.DrawLayers[0].Resolution.X));

        renderer.DrawRect(
            new FloatRect(Grid.IndexToCoords(index), (Vector2f)TileUnit), 
            new(255, 255, 0, 150), EDITOR_LAYER);
    }

    public void HandleMouseInput(object? obj, MouseMoveEventArgs args)
    {
        KProgram.Editor._mousePos = args.Position;
    }

    public void HandleMouseInput(object? obj, MouseButtonEventArgs args)
    {
        if (args.Button == Mouse.Button.Left)
        {
            var index = Grid.CoordsToIndex(
                (Vector2f)args.Position, 
                1 / ((float)KProgram.Window.Size.X / KProgram.DrawLayers[0].Resolution.X));

            if (Palette.TileMap.Enabled)
            {
                if (Palette.Enabled)
                {
                    _selectedTile = new(Grid.IndexToCoords(index), (Vector2f)TileUnit);
                    Console.WriteLine($"selected: {index}, {_selectedTile.Position}");
                }
                else
                {
                    Console.WriteLine($"paint: {index}, {_selectedTile.Position}");

                    var pos = Grid.IndexToCoords(index);
                    Palette.TileMap.Buffer[index * 6] = new(pos, _selectedTile.Position);
                    Palette.TileMap.Buffer[index * 6 + 1] = new((pos.X + TILE_SIZE, pos.Y), (_selectedTile.Left + _selectedTile.Width, _selectedTile.Top));
                    Palette.TileMap.Buffer[index * 6 + 2] = new((pos.X, pos.Y + TILE_SIZE), (_selectedTile.Left, _selectedTile.Top + _selectedTile.Height));

                    Palette.TileMap.Buffer[index * 6 + 3] = new((pos.X + TILE_SIZE, pos.Y), (_selectedTile.Left + _selectedTile.Width, _selectedTile.Top));
                    Palette.TileMap.Buffer[index * 6 + 4] = new(pos + (Vector2f)TileUnit, _selectedTile.Position + _selectedTile.Size);
                    Palette.TileMap.Buffer[index * 6 + 5] = new((pos.X, pos.Y + TILE_SIZE), (_selectedTile.Left, _selectedTile.Top + _selectedTile.Height));
                }
            }
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
                Palette.TileMap.Enabled = !Palette.TileMap.Enabled; 
                break;

            case Keyboard.Key.G:
                Grid.Enabled = !Grid.Enabled; 
                break;
            
            default:
                break;
        }
    }
}