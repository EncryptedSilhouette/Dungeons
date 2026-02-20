using System.Diagnostics;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

public struct KTextureAtlas
{
    public Texture Texture;
    public Dictionary<string, FloatRect> Coordinates;
}

public struct KBufferRegion
{
    public uint Offset;
    public uint Count;
    public uint Capacity;
}

public struct KDrawLayer
{
    public bool IsStatic;
    public bool Upscale;
    public Vector2f Size;
    public PrimitiveType Primitive;
    public RenderStates States;
    public KBufferRegion Region;
    public KTextureAtlas Atlas;
    
    public Vector2f GetScaleRelativeTo(Vector2f otherSize) => 
        new(otherSize.X / Size.X, otherSize.Y / Size.Y);
    public float GetScaleXRelativeTo(float width) => width / Size.X;
    public float GetScaleYRelativeTo(float height) => height / Size.Y;
}

public class KProgram
{
    public enum KProgramState
    {
        NORMAL = 0
    }

    public const uint FRAME_RATE = 60;
    public static readonly string ATLAS_FILEPATH = "Assets/atlas.csv";
    
    public static bool Running;
    public static KProgramState State;
    public static RenderWindow Window;
    public static KConsole DebugConsole;
    public static KRenderManager Renderer;
    public static KEditor Editor;
    public static KInputManager InputManager;
    public static KGameManager GameManager;
    public static VertexBuffer Buffer;
    public static KBufferRegion[] BufferRegions;
    public static KTextureAtlas[] Atlases;
    public static KDrawLayer[] DrawLayers;

    static KProgram()
    {
        Running = false;
        Window = new(VideoMode.DesktopMode, "Dungeons");
        Window.SetFramerateLimit(FRAME_RATE);
        Window.Closed += (_, _) => Running = false;

        Atlases = [];
        DrawLayers = [];
        BufferRegions = CreateBufferRegions([60_000, 60_000, 60_000]);  
        Buffer = new(180_000, PrimitiveType.Points, VertexBuffer.UsageSpecifier.Dynamic);
        
        Renderer = new(Window, Buffer);
        InputManager = new(Window);
        Editor = new(Renderer, InputManager);
        DebugConsole = new();
        GameManager = new();
    }

    public static void Main()
    {
        DebugConsole.Start();
        
        LoadAndInit();
        Start();

        DebugConsole.Stop();
    }

    public static void LoadAndInit()
    {
        Atlases = 
        [
            LoadTextureAtlas(ATLAS_FILEPATH)
        ];

        DrawLayers = 
        [
            new() //Background Layer
            {
                IsStatic = false,
                Upscale = true,
                Size = (320, 240),
                Primitive = PrimitiveType.Triangles,
                States = new(Atlases[0].Texture),
                Region = BufferRegions[0],
                Atlas = Atlases[0]
            },
            new()
            {
                IsStatic = false,
                Upscale = true,
                Size = (320, 240),
                Primitive = PrimitiveType.Lines,
                States = RenderStates.Default,
                Region = BufferRegions[1],
            },
            new()
            {
                IsStatic = false,
                Upscale = false,
                Size = (Vector2f)Window.Size,
                Primitive = PrimitiveType.Triangles,
                States = RenderStates.Default,
                Region = BufferRegions[2],
            },
        ];

        Renderer.Init(BufferRegions[2], DrawLayers);
        Editor.Init(Renderer, Atlases[0]);
    }

    public static void Start()
    {
        if (Running) return;

        uint currentFrame = 0;
        Running = true;
        
        GC.Collect();

        while(Running)
        {
            Update(currentFrame);

            Window.Clear();

            FrameUpdate(currentFrame);
            currentFrame++;

            Window.Display();
            Window.DispatchEvents();
        }
    }

    private static void Update(uint currentFrame)
    {
        Editor.Update(currentFrame);
    }

    private static void FrameUpdate(uint currentFrame)
    {
        Editor.FrameUpdate(currentFrame, Renderer);
        Renderer.FrameUpdate();
        InputManager.Update(); //Call last for proper input.
    }

    public static KTextureAtlas LoadTextureAtlas(string filePath)
    {
        KTextureAtlas atlas = new()
        {
            Coordinates = new(128)
        };

        var atlasData = File.ReadAllLines(filePath);

        foreach (var line in atlasData)
        {
            var values = line.Split(',');

            switch(values[0])
            {
                case "atlas":
                    atlas.Texture = new Texture(values[1]);
                    Console.WriteLine($"Loaded Texture, {values[1]}");
                    break;

                case "sprite":
                    atlas.Coordinates.Add(values[1], new()
                    {
                        Position = (int.Parse(values[2]), int.Parse(values[3])),
                        Size = (int.Parse(values[4]), int.Parse(values[5])),   
                    });
                    Console.WriteLine($"Loaded Sprite: {values[1]}");

                    break;

                default:
                    break;
            }
            if (atlas.Texture is null) atlas.Texture = CreateErrorTexture(640, 480);
        }
        return atlas;
    }

    public static KBufferRegion[] CreateBufferRegions(uint[] bufferSizes)
    {
        uint offset = 0;
        var regions = new KBufferRegion[bufferSizes.Length];

        for (int i = 0; i < regions.Length; i++)
        {
            regions[i] = new()
            {
                Offset = offset,
                Count = 0,
                Capacity = bufferSizes[i]
            };
            offset += bufferSizes[i];
        }
        return regions;
    } 
    
    public static Texture CreateErrorTexture(uint width, uint height)
    {
        Image img = new((width, height), Color.Magenta);
        return new(img);
    }

    public static bool CheckPointCircleCollision(Vector2f posA, Vector2f posB, float r) =>
        (posB - posA).Length <= r; 

    public static bool CheckPointRectCollision(Vector2f pos, FloatRect rect) =>
        rect.Contains(pos);

    public static bool CheckCircleCircleCollison(Vector2f posA, Vector2f posB, float ra, float rb) =>
        (posB - posA).Length <= ra + rb; 

    //WIP
    public static bool CheckCircleRectCollision(Vector2f pos, float r, FloatRect rect)
    {
        return true;
    }

    public static bool CheckRectRectCollision()
    {
        return true;
    }    
}