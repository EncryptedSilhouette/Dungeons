using SFML.Graphics;
using SFML.System;
using SFML.Window;

public class KTextureAtlas
{
    public Texture Texture;
    public Dictionary<string, FloatRect> Coordinates;

    public KTextureAtlas(Texture texture, Dictionary<string, FloatRect> coords)
    {
        Texture = texture;
        Coordinates = coords;
    }
}

public struct KBufferRegion
{
    public uint Offset;
    public uint Count;
    public uint Capacity;

    public KBufferRegion(uint offset, uint capacity)
    {
        Offset = offset;
        Count = 0;
        Capacity = capacity;
    }
}


public class KProgram
{
    public enum KProgramState
    {
        NORMAL = 0
    }

    public enum KLayers : int
    {
        GROUND0,
        GROUND1,
        WALL,
        CEILING,
        OVERLAY,
        LINE,
        LAYER_COUNT,
    }

    public const uint FRAME_RATE = 60;
    public static readonly string ATLAS_FILEPATH = "Assets/atlas.csv";
    
    public static bool Running;
    public static KProgramState State;
    public static RenderWindow Window;
    public static KRenderManager Renderer;
    //public static KEditor Editor;
    public static KInputManager InputManager;
    public static KGameManager GameManager;
    public static VertexBuffer Buffer;
    public static Font[] Fonts;
    public static KBufferRegion[] BufferRegions;
    public static KTextureAtlas[] Atlases;
    public static KRenderLayer[] DrawLayers;

    static KProgram()
    {
        Running = false;
        Window = new(VideoMode.DesktopMode, "Dungeons");
        Window.SetFramerateLimit(FRAME_RATE);
        Window.Closed += (_, _) => Running = false;

        Fonts = [];
        Atlases = [];
        DrawLayers = [];
        BufferRegions = new KBufferRegion[(int)KLayers.LAYER_COUNT]
        {
            new(600_000 * (int)KLayers.GROUND0, 600_000),
            new(600_000 * (int)KLayers.GROUND1, 600_000),
            new(600_000 * (int)KLayers.WALL, 600_000),
            new(600_000 * (int)KLayers.CEILING, 600_000),
            new(600_000 * (int)KLayers.OVERLAY, 600_000),
            new(600_000 * (int)KLayers.LINE, 600_000),
        };
        Buffer = new(600_000 * (int)KLayers.LAYER_COUNT, PrimitiveType.Points, VertexBuffer.UsageSpecifier.Dynamic);
        
        Renderer = new(Window, Buffer);
        InputManager = new(Window);
        //Editor = new(Renderer, InputManager);
        GameManager = new(Renderer, InputManager);
    }

    public static void Main()
    {
        LoadAndInit();
        Start();
    }

    public static void LoadAndInit()
    {
        foreach (var filePath in Directory.EnumerateFiles("Assets"))
        {
            if (filePath is null) continue;
            Console.WriteLine($"[init] found: {filePath}");

            //switch (Path.GetExtension(filePath))
            //{
            //    case ".tff":
            //        break;

            //    case ".csv":
            //        break;
            //}
        }

        Fonts = [ new("Assets/Roboto-Black.ttf") ];

        Atlases = 
        [
            LoadTextureAtlas(ATLAS_FILEPATH)
        ];

        DrawLayers = 
        [
            new() //GROUND0
            {
                Upscale = true,
                Bounds = new((0, 0), (Vector2f)Window.Size),
                Primitive = PrimitiveType.Triangles,
                States = RenderStates.Default,
                Region = BufferRegions[(int)KLayers.GROUND0],
            },
            new() //GROUND1
            {
                Upscale = true,
                Bounds = new((0, 0), (Vector2f)Window.Size),
                Primitive = PrimitiveType.Triangles,
                States = RenderStates.Default,
                Region = BufferRegions[(int)KLayers.GROUND1],
            },
            new() //WALL
            {
                Upscale = true,
                Bounds = new((0, 0), (Vector2f)Window.Size),
                Primitive = PrimitiveType.Triangles,
                States = RenderStates.Default,
                Region = BufferRegions[(int)KLayers.WALL],
            },
            new() //CEILING
            {
                Upscale = true,
                Bounds = new((0, 0), (Vector2f)Window.Size),
                Primitive = PrimitiveType.Triangles,
                States = RenderStates.Default,
                Region = BufferRegions[(int)KLayers.CEILING],
            },
            new() //OVERLAY
            {
                Upscale = true,
                Bounds = new((0, 0), (Vector2f)Window.Size),
                Primitive = PrimitiveType.Triangles,
                States = RenderStates.Default,
                Region = BufferRegions[(int)KLayers.OVERLAY],
            },
            new() //LINE
            {
                Upscale = true,
                Bounds = new((0, 0), (Vector2f)Window.Size),
                Primitive = PrimitiveType.Lines,
                States = RenderStates.Default,
                Region = BufferRegions[(int)KLayers.LINE],
            },
        ];

        KTextLayer[] TextLayers =
        [
            new()
            {
                FontSize = 32,
                Font = Fonts[0],
                DrawLayer = new()
                {
                    Upscale = false,
                    Bounds = new((0, 0), (Vector2f)Window.Size),
                    Primitive = PrimitiveType.Triangles,
                    States = new(Fonts[0].GetTexture(14)),
                    Region = BufferRegions[3],
                },
            }
        ];

        Renderer.Init(BufferRegions[2], DrawLayers, TextLayers);
        //Editor.Init(Renderer, Atlases[0]);
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
        //Editor.Update(currentFrame);
    }

    private static void FrameUpdate(uint currentFrame)
    {
        //Editor.FrameUpdate(currentFrame, Renderer);
        Renderer.FrameUpdate();
        InputManager.Update(); //Call last for proper input.
    }

    public static KTextureAtlas LoadTextureAtlas(string filePath)
    {
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