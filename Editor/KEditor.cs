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
    enum KEditorLayers : int
    {
        EDITOR_LAYER,
        LINE_LAYER,
        LAYER_COUNT,
    }

    enum KTileLayers : int
    {
        TILE_LAYER_0,
        TILE_LAYER_1,
        TILE_LAYER_2,
        TILE_LAYER_3,
        TILE_LAYER_4,
        LAYER_COUNT
    }

    public const int TILE_SIZE = 4;
        
    public KButton Button;
    public KTexturePalette Palette;
    public KInputManager InputManager;

    public Vector2i TileUnit => new(TILE_SIZE, TILE_SIZE);

    public KEditor(KInputManager inputManager)
    {
        //_currentTool = KEditorTool.CURSOR;

        Button = new KButton(new KSprite
        {
            Rotation = 0.0f,
            Color = Color.White,
            Rotocenter = (0, 0),
            Bounds = new((2, 2), (8, 4)),
            TextureBounds = new(),
            Frames = []
        }, string.Empty);

        Palette = 
            new(new VertexBuffer(60_000, PrimitiveType.Triangles, VertexBuffer.UsageSpecifier.Stream),
            )
        {
            
        };

        //Palette = new();
        InputManager = inputManager;
    }

    public void Init(KRenderManager renderer, KTextureAtlas atlas)
    {
        Palette.Init(atlas.Texture);
    }

    public void Update(uint currentFrame)
    {
        //value to downscale mouse/screen coords to layer's coords.
        var downScale = 1 / KProgram.DrawLayers[(int)KEditorLayers.EDITOR_LAYER].GetScaleXRelativeTo(KProgram.Window.Size.X);

        if (InputManager.IsKeyPressed(Keyboard.Key.Q)) Palette.Enabled = !Palette.Enabled; 
        
        if (!Palette.Enabled) Button.Update(InputManager, InputManager.GetMousePosition(downScale));
    }

    public void FrameUpdate(uint currentFrame, KRenderManager renderer)
    {
        if (Palette.Enabled) Palette.FrameUpdate(renderer, InputManager, 
            (int)KEditorLayers.EDITOR_LAYER, 
            (int)KEditorLayers.LINE_LAYER);

        if (!Palette.Enabled) 
        {
            Button.FrameUpdate(renderer, (int)KEditorLayers.EDITOR_LAYER);
        }
    }
}