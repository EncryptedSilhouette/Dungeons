using SFML.Graphics;
using SFML.System;
using SFML.Window;

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
    private FloatRect _selectedTile;
    
    public KButton Button;
    public KTexturePalette Palette;
    public KInputManager InputManager;

    public Vector2i TileUnit => new(TILE_SIZE, TILE_SIZE);

    public KEditor(KInputManager inputManager)
    {
        _currentTool = KEditorTool.CURSOR;
        _selectedTile = new();
        
        Button = new KButton(new KSprite
        {
            Rotation = 0.0f,
            Color = Color.White,
            Rotocenter = (0, 0),
            Bounds = new((2, 2), (8, 4)),
            TextureBounds = new(),
            Frames = []
        }, string.Empty);

        Palette = new();
        InputManager = inputManager;
    }

    public void Init(KRenderManager renderer, KTextureAtlas atlas)
    {
        KGrid grid = new(320 / TILE_SIZE, 240 / TILE_SIZE, 0, 0, TILE_SIZE, TILE_SIZE)
        {
            Enabled = false
        };

        Palette = new(atlas.Texture, new(grid))
        {
            Position = (8,8)
        };

        Palette.Init(atlas.Texture);
    }

    public void Update(uint currentFrame)
    {
        ref var tileMap = ref Palette.TileMap;
        var downScale = 1 / ((float)KProgram.Window.Size.X / KProgram.DrawLayers[0].Resolution.X);

        if (InputManager.IsKeyPressed(Keyboard.Key.Q))
        {
            Palette.Enabled = !Palette.Enabled;  
            if (Palette.Enabled) 
            {
                tileMap.Enabled = true;          
            }
        }
        if (InputManager.IsKeyPressed(Keyboard.Key.E))
        {
            tileMap.Enabled = !tileMap.Enabled; 
            Palette.Enabled = tileMap.Enabled;
            tileMap.Grid.Enabled = tileMap.Enabled;
        }
        if (InputManager.IsKeyPressed(Keyboard.Key.G))
        {
            tileMap.Grid.Enabled = !tileMap.Grid.Enabled; 
        }
        
        if (!Palette.Enabled && !tileMap.Enabled) 
        {
            Button.Update(InputManager, InputManager.GetMousePosition(downScale));
        }
    }

    public void FrameUpdate(uint currentFrame, KRenderManager renderer)
    {
        ref var layer = ref renderer.DrawLayers[EDITOR_LAYER];
        ref var tileMap = ref Palette.TileMap;
        var downScale = 1 / ((float)KProgram.Window.Size.X / KProgram.DrawLayers[0].Resolution.X);
        var index = tileMap.Grid.CoordsToIndex(InputManager.GetMousePosition(downScale));

        if (tileMap.Enabled && InputManager.IsMouseDown(KMouseStates.M1_DOWN))
        {
            if (Palette.Enabled)
            {
                _selectedTile = new(tileMap.Grid.IndexToCoords(index), (Vector2f)TileUnit);
            }
            else
            {
                var pos = tileMap.Grid.IndexToCoords(index);
                var bounds = _selectedTile;
                bounds.Position -= Palette.Position;

                tileMap.Buffer[index * 6] = new(pos, bounds.Position);
                tileMap.Buffer[index * 6 + 1] = new((pos.X + TILE_SIZE, pos.Y), (bounds.Left + bounds.Width, bounds.Top));
                tileMap.Buffer[index * 6 + 2] = new((pos.X, pos.Y + TILE_SIZE), (bounds.Left, bounds.Top + bounds.Height));

                tileMap.Buffer[index * 6 + 3] = new((pos.X + TILE_SIZE, pos.Y), (bounds.Left + bounds.Width, bounds.Top));
                tileMap.Buffer[index * 6 + 4] = new(pos + (Vector2f)TileUnit, bounds.Position + bounds.Size);
                tileMap.Buffer[index * 6 + 5] = new((pos.X, pos.Y + TILE_SIZE), (bounds.Left, bounds.Top + _selectedTile.Height));
            }
        }

        if (tileMap.Enabled) tileMap.FrameUpdate(renderer, EDITOR_LAYER);
        if (Palette.Enabled) Palette.FrameUpdate(renderer, EDITOR_LAYER);
        if (tileMap.Grid.Enabled) tileMap.Grid.FrameUpdate(renderer, LINE_LAYER);

        renderer.DrawRect(
            new FloatRect(tileMap.Grid.IndexToCoords(index), (Vector2f)TileUnit), 
            new(255, 255, 0, 150), EDITOR_LAYER);

        if (!Palette.Enabled && !tileMap.Enabled) 
        {
            Button.FrameUpdate(renderer, EDITOR_LAYER);
        }
    }
}