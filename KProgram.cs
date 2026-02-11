using SFML.Graphics;
using SFML.System;
using SFML.Window;

public record struct KTileMapping(string Tile, int ID);
public record struct KTileSet(KTileMapping[] Mappings);

public struct KTextureAtlas
{
    public Texture Texture;
    public KTileSet[] TileSets; 
    public Dictionary<string, FloatRect> Coordinates;
}

public struct KBufferRegion
{
    public uint Offset;
    public uint Count;
    public uint Capacity;
}

public class KDrawLayer
{
    public bool IsStatic;
    public bool Upscale;
    public Vector2u Resolution;
    public PrimitiveType Primitive;
    public RenderStates States;
    public KBufferRegion Region;
    public KTextureAtlas TextureAtlas;
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
    public static KConsole Console;
    public static KRenderer Renderer;
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

        Console = new();
        BufferRegions = CreateBufferRegions([60_000, 60_000, 60_000]);  
        Buffer = new(60_000 * 3, PrimitiveType.Points, VertexBuffer.UsageSpecifier.Dynamic);
        Renderer = new(Window, Buffer);
        GameManager = new();
        Atlases = [];
        DrawLayers = [];
    }

    public static void Main()
    {
        Console.Start();
        
        LoadAndInit();
        Start();

        Console.Stop();
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
                IsStatic = true,
                Upscale = true,
                Resolution = (320, 240),
                Primitive = PrimitiveType.Triangles,
                States = RenderStates.Default,
                Region = BufferRegions[0],
            },
            new()
            {
                IsStatic = false,
                Upscale = true,
                Resolution = (320, 240),
                Primitive = PrimitiveType.Lines,
                States = RenderStates.Default,
                Region = BufferRegions[1],
            },
        ];

        Renderer.Init(BufferRegions[0], DrawLayers);
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
    }

    private static void FrameUpdate(uint currentFrame)
    {
        Renderer.FrameUpdate();
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
                    break;

                case "tilesets":
                    atlas.TileSets = new KTileSet[values.Length];
                    for (int i = 1; i < values.Length; i++)
                    {
                        atlas.TileSets[i] = LoadTileSet(values[i]);
                    }
                    break;

                case "sprite":
                    atlas.Coordinates.Add(values[1], new()
                    {
                        Position = (int.Parse(values[2]), int.Parse(values[3])),
                        Size = (int.Parse(values[4]), int.Parse(values[5])),   
                    });
                    break;

                default:
                    break;
            }

            if (atlas.Texture is null) atlas.Texture = CreateErrorTexture(640, 480);
        }
        return atlas;
    }

    public static KTileSet LoadTileSet(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        KTileMapping[] tileSet = new KTileMapping[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            var values = lines[i].Split(',');
            if (values.Length < 2) continue;

            tileSet[i] = new(values[0], int.Parse(values[1]));
        }
        return new(tileSet);
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
        Color color = new(0,0,0);
        Image img = new((width, height));

        for (uint i = 0; i < height; i++)
        {
            for (uint j = 0; j < width; j++)
            {
                if (color.R + 1 > byte.MaxValue)
                {
                    color.R = 0;

                    if (color.G + 1 > byte.MaxValue)
                    {
                        color.G = 0;

                        if (color.B + 1 > byte.MaxValue)
                        {
                            color.B = 0;
                        }
                        else color.B++;
                    }
                    else color.G++;
                }
                else color.R++;

                img.SetPixel(new Vector2u(i, j), color);
            }
        }

        img.SetPixel(new Vector2u(0, 0), Color.White);
        return new(img);
    }
}