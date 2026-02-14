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
    
    public KGrid Grid;
    public KButton Button;
    public KTexturePalette Palette;
    public KInputManager InputManager;

    public Vector2i TileUnit => new(TILE_SIZE, TILE_SIZE);

    public KEditor(KInputManager inputManager)
    {
        _currentTool = KEditorTool.CURSOR;
        _selectedTile = new();
        
        InputManager = inputManager;
        
        Grid = new(320 / TILE_SIZE, 240 / TILE_SIZE, 0, 0, TILE_SIZE, TILE_SIZE);
        Grid.Enabled = false;

        Palette = new();

        Button = new KButton(new KSprite
        {
            Rotation = 0.0f,
            Color = Color.White,
            Rotocenter = (0, 0),
            Bounds = new((2, 2), (8, 4)),
            TextureBounds = new(),
            Frames = []
        }, string.Empty);
    }

    public void Init(KRenderManager renderer, KTextureAtlas atlas)
    {
        var coords = atlas.Coordinates;

        Palette = new(atlas.Texture, new(Grid));
        Palette.Position = (8,8);
        Palette.Init(atlas.Texture);
    }

    public void Update(uint currentFrame)
    {
        if (InputManager.IsKeyPressed(Keyboard.Key.Q))
        {
            Palette.Enabled = !Palette.Enabled;  
            if (Palette.Enabled) Palette.TileMap.Enabled = true;          
        }
        if (InputManager.IsKeyPressed(Keyboard.Key.E))
        {
            Palette.TileMap.Enabled = !Palette.TileMap.Enabled; 
            Grid.Enabled = Palette.TileMap.Enabled;
        }
        if (InputManager.IsKeyPressed(Keyboard.Key.G))
        {
            Grid.Enabled = !Grid.Enabled; 
        }

        var downScale = 1 / ((float)KProgram.Window.Size.X / KProgram.DrawLayers[0].Resolution.X);

        if (InputManager.IsMouseDown(KMouseStates.M1_DOWN))
        {
            var index = Grid.CoordsToIndex(
                InputManager.GetMousePosition(downScale));

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
                    var bounds = _selectedTile;
                    bounds.Position -= Palette.Position;

                    Palette.TileMap.Buffer[index * 6] = new(pos, bounds.Position);
                    Palette.TileMap.Buffer[index * 6 + 1] = new((pos.X + TILE_SIZE, pos.Y), (bounds.Left + bounds.Width, bounds.Top));
                    Palette.TileMap.Buffer[index * 6 + 2] = new((pos.X, pos.Y + TILE_SIZE), (bounds.Left, bounds.Top + bounds.Height));

                    Palette.TileMap.Buffer[index * 6 + 3] = new((pos.X + TILE_SIZE, pos.Y), (bounds.Left + bounds.Width, bounds.Top));
                    Palette.TileMap.Buffer[index * 6 + 4] = new(pos + (Vector2f)TileUnit, bounds.Position + bounds.Size);
                    Palette.TileMap.Buffer[index * 6 + 5] = new((pos.X, pos.Y + TILE_SIZE), (bounds.Left, bounds.Top + _selectedTile.Height));
                }
            }
        }

        if (!Palette.Enabled && !Palette.TileMap.Enabled) 
        {
            Button.Update(InputManager, InputManager.GetMousePosition(downScale));
        }
    }

    public void FrameUpdate(uint currentFrame, KRenderManager renderer)
    {
        ref var layer = ref renderer.DrawLayers[EDITOR_LAYER];

        if (Palette.TileMap.Enabled) Palette.TileMap.FrameUpdate(renderer, EDITOR_LAYER);
        if (Palette.Enabled) Palette.FrameUpdate(renderer, EDITOR_LAYER);
        if (Grid.Enabled) Grid.FrameUpdate(renderer, LINE_LAYER);

        var index = Grid.CoordsToIndex(
            InputManager.GetMousePosition(),
            1 / ((float)renderer.Window.Size.X / renderer.DrawLayers[0].Resolution.X));

        renderer.DrawRect(
            new FloatRect(Grid.IndexToCoords(index), (Vector2f)TileUnit), 
            new(255, 255, 0, 150), EDITOR_LAYER);

        if (!Palette.Enabled && !Palette.TileMap.Enabled) 
        {
            Button.FrameUpdate(renderer, EDITOR_LAYER);
        }
    }
}