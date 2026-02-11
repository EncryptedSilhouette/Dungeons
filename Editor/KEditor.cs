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

    public event PaletteHandler? PaletteUpdated;

    public KTexturePalette(Texture atlas)
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
            new FloatRect(Position, (Vector2f)Texture.Size), Color.White, 1);
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
        Console.WriteLine(Grid.CellCount);
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
    public const int EDITOR_LAYER = 1;
    public const int TILE_SIZE = 4;

    private KEditorTool _currentTool;

    private Vector2i _mousePos;
    private Vector2f _selectedTile;
    
    public KGrid Grid;
    public KTileMap TileMap;
    public KTexturePalette Palette;

    public KEditor()
    {
        Grid = new(); 
        TileMap = new();
        Palette = new();
        _mousePos = (0, 0);
        _currentTool = KEditorTool.CURSOR;
    }

    public void Init(KTextureAtlas atlas, RenderWindow window, KRenderer renderer)
    {
        Palette = new()
        {
            Enabled = false,
        };

        window.SetKeyRepeatEnabled(false);
        window.MouseMoved += (_, e) => _mousePos = e.Position;
        window.MouseButtonPressed += (_, e) => 
        {
            var index = Grid.CoordsToIndex(
                (_mousePos.X, _mousePos.Y), 
                1 / ((float)renderer.Window.Size.X / renderer.DrawLayers[1].Resolution.X));

            Console.WriteLine(index);
        };
        window.KeyPressed += (_, e) =>
        {
            if (e.Code == SFML.Window.Keyboard.Key.Q)
            {
                Palette.Enabled = !Palette.Enabled;
            }  
        };

        var bufferRegions = KRenderer.CreateBufferRegions([60000, 60000, 60000]);

        KDrawLayer[] drawLayers =
        [
            new() 
            {
                IsStatic = false,
                Upscale = true,
                Resolution = (320, 240),
                Primitive = PrimitiveType.Lines,
                States = RenderStates.Default,
                Region = bufferRegions[0],    
            },
            new() 
            {
                IsStatic = false,
                Upscale = true,
                Resolution = (320, 240),
                Primitive = PrimitiveType.Triangles,
                States = new(atlas.Texture),
                Region = bufferRegions[1],    
            }
        ];

        renderer.Init(bufferRegions[1], drawLayers);

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
        TileMap.BakeVertices();
    }

    public void UnInit()
    {
        
    }

    public void Update()
    {
        
    }

    public void FrameUpdate(KRenderer renderer, uint currentFrame)
    {
        ref var layer = ref renderer.DrawLayers[EDITOR_LAYER];

        TileMap.FrameUpdate(renderer, EDITOR_LAYER);

        if (Grid.Enabled)
        {
            Grid.Draw(renderer, 0);

            var index = Grid.CoordsToIndex(
                (_mousePos.X, _mousePos.Y), 
                1 / ((float)renderer.Window.Size.X / renderer.DrawLayers[1].Resolution.X));

            renderer.DrawRect(new FloatRect(Grid.IndexToCoords(index), (TILE_SIZE, TILE_SIZE)), new(255, 255, 0, 150), EDITOR_LAYER);
        }
    }
}