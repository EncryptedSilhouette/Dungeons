using SFML.Graphics;
using SFML.System;

public delegate void PaletteHandler(KTexturePalette palette);

public class KTexturePalette
{
    public enum KPaletteTools
    {
        NONE, 
        EYE_DROPPER,
        PAINT,
    }

    public bool Enabled;
    public bool ShowPalette;
    public bool ShowGrid;
    public int ActiveTileMap;
    public KPaletteTools CurrentTool;
    public Color BackgroundColor;
    public Vector2f Position;
    public FloatRect SelectedTile;
    public Texture Texture;
    public RenderTexture RenderTexture;
    public KTileMap[] TileLayers;

    public event PaletteHandler? PaletteUpdated;

    public KTexturePalette(KTileMap[] layers)
    {
        TileLayers = layers;

        ShowPalette = Enabled = false;
        ActiveTileMap = 0;
        CurrentTool = KPaletteTools.NONE;
        BackgroundColor = new(100, 100, 100);
        Position = new();
        SelectedTile = new();
        RenderTexture = new((640, 480));
        Texture = RenderTexture.Texture;
    
        PaletteUpdated += (p) => p.Texture = p.RenderTexture.Texture;
    }

    public void Init(Texture atlas)
    {
        Texture = atlas;
        RenderTexture = new(atlas.Size);
        RenderTexture.Draw(new Sprite(atlas));
        RenderTexture.Display();
    }

    public void Update(KInputManager input)
    {
        //Enables/disables parts of the editor
        if (input.IsKeyPressed(SFML.Window.Keyboard.Key.Q)) ShowPalette = !ShowPalette;
        if (input.IsKeyPressed(SFML.Window.Keyboard.Key.G)) ShowGrid = !ShowGrid; 
    }

    public void FrameUpdate(KRenderManager renderer,  KInputManager input, int layer, int lineLayer)
    {
        ref var l = ref renderer.DrawLayers[layer];
        ref var tmap = ref TileLayers[ActiveTileMap];
        var downScale = 1 / l.GetScaleXRelativeTo(KProgram.Window.Size.X);
        tmap.Grid.CoordsToIndex(input.GetMousePosition(downScale), out int index);

        if (tmap.Enabled && input.IsMouseDown(KMouseStates.M1_DOWN))
        {   
            switch (CurrentTool)
            {
                case KPaletteTools.EYE_DROPPER:
                    SelectedTile = new(tmap.Grid.IndexToCoords(index), tmap.Grid.CellSize);
                    break;

                case KPaletteTools.PAINT: 
                    var pos = tmap.Grid.IndexToCoords(index);
                    var bounds = SelectedTile;
                    bounds.Position -= Position;

                    tmap.Buffer[index * 6] = 
                        new(pos, bounds.Position);

                    tmap.Buffer[index * 6 + 1] = 
                        new((pos.X + tmap.Grid.CellWidth, pos.Y), 
                            (bounds.Left + bounds.Width, bounds.Top));

                    tmap.Buffer[index * 6 + 2] = 
                        new((pos.X, pos.Y + tmap.Grid.CellHeight), 
                            (bounds.Left, bounds.Top + bounds.Height));

                    tmap.Buffer[index * 6 + 3] = 
                        new((pos.X + tmap.Grid.CellWidth, pos.Y), 
                            (bounds.Left + bounds.Width, bounds.Top));

                    tmap.Buffer[index * 6 + 4] = 
                        new(pos + tmap.Grid.CellSize, 
                            bounds.Position + bounds.Size);

                    tmap.Buffer[index * 6 + 5] = 
                        new((pos.X, pos.Y + tmap.Grid.CellHeight), 
                            (bounds.Left, bounds.Top + SelectedTile.Height));
                    break;
            }
        }

        if (ShowPalette)
        {    
            renderer.DrawRect(new FloatRect(Position, (Vector2f)Texture.Size), BackgroundColor, layer);
            renderer.DrawRect(
                new FloatRect(Position, (Vector2f)Texture.Size), 
                new FloatRect((0, 0), (Vector2f)Texture.Size), Color.White, layer);
        }

        if (ShowGrid) tmap.Grid.FrameUpdate(renderer, lineLayer);

        renderer.DrawRect(
            new FloatRect(tmap.Grid.IndexToCoords(index), tmap.Grid.CellSize), 
            new(255, 255, 0, 150), layer);
    }

    public void DrawBuffer(Vertex[] vertices, uint vCount, PrimitiveType primitive, RenderStates states)
    {
        RenderTexture.Draw(vertices, 0, vCount, primitive, states);
        PaletteUpdated?.Invoke(this);
    }

    public void DrawBuffer(Vertex[] vertices, uint vCount, PrimitiveType primitive) => 
        DrawBuffer(vertices, vCount, primitive, new(Texture)); 

    public void Clear() => RenderTexture.Clear(BackgroundColor);
}