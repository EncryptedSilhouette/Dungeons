using SFML.Graphics;
using SFML.System;

public struct KEditorPalette
{
    public bool Enabled;
    public KTextureAtlas TextureAtlas;
    public FloatRect SelectedTile;
}

public class KEditor
{
    public const int EDITOR_LAYER = 1;
    public const int TILE_SIZE = 4;

    private Vector2i _mousePos;
    
    public KGrid Grid;
    public KEditorPalette Palette;

    public KEditor()
    {
        Grid = new();
        Palette = new();
        _mousePos = (0, 0);
    }

    public void Init(KTextureAtlas atlas, RenderWindow window, KRenderer renderer)
    {
        Palette = new()
        {
            Enabled = false,
            TextureAtlas = atlas,    
        };

        window.SetKeyRepeatEnabled(false);
        window.MouseMoved += (_, e) => _mousePos = e.Position;
        window.MouseButtonPressed += (_, e) => 
        {
            if (Palette.Enabled)
            {
                var scale = (float)renderer.Window.Size.X / renderer.DrawLayers[1].Resolution.X;
                var pos = Grid.PointToCell((_mousePos.X / (float)scale, _mousePos.Y / (float)scale));
                Palette.SelectedTile = new((Vector2f)pos, (TILE_SIZE, TILE_SIZE));   
            }
        };
        window.KeyPressed += (_, e) =>
        {
            if (e.Code == SFML.Window.Keyboard.Key.Q)
            {
                Palette.Enabled = !Palette.Enabled;
            }  
        };

        var bufferRegions = KRenderer.CreateBufferRegions([6000, 6000, 6000]);

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

        if (Grid.Enabled)
        {
            Grid.Draw(renderer, 0);

            if (Palette.Enabled && Palette.TextureAtlas.Texture != null)
            {
                var t = Palette.TextureAtlas.Texture;
                renderer.DrawRect(new FloatRect((0, 0), (Vector2f)t.Size), new(100, 100, 100), EDITOR_LAYER);
                renderer.DrawRect(
                    new FloatRect((0, 0), (Vector2f)t.Size), 
                    new FloatRect((0, 0), (Vector2f)t.Size), Color.White, 1);
            }

            var scale = (float)renderer.Window.Size.X / renderer.DrawLayers[1].Resolution.X;
            var pos = Grid.PointToCell((_mousePos.X / (float)scale, _mousePos.Y / (float)scale));

            renderer.DrawRect(new FloatRect((Vector2f)pos, (TILE_SIZE, TILE_SIZE)), new(255, 255, 0, 150), EDITOR_LAYER);
        }
    }
}