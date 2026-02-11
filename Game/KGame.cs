
using System.Buffers;
using SFML.Graphics;
using SFML.System;

public struct KGrid
{
    public bool Enabled;
    public int Rows;
    public int Columns;
    public Color LineColor;
    public Vector2f Position;
    public Vector2i CellSize;
    public uint[] Cells;

    public int CellCount => Rows * Columns;

    public KGrid()
    {
        Enabled = true;
        Rows = Columns = CellSize.X = CellSize.Y = 0;
        LineColor = Color.White;
        Cells = [];
    }

    public KGrid(int rows, int columns, int x, int y, int cellWidth, int cellHeight)
    {
        Enabled = true;
        Rows = rows;
        Columns = columns;
        CellSize.X = cellWidth;
        CellSize.Y = cellHeight;
        Position = (x, y);
        Cells = new uint[Rows * Columns];
    }

    public void Draw(KRenderer renderer, int lineLayer)
    {
        ref var l = ref renderer.DrawLayers[lineLayer];
        
        uint vCount = 0;
        int rows = (int) l.Resolution.Y / 4;
        int columns = (int) l.Resolution.X / 4;
        var buffer = ArrayPool<Vertex>.Shared.Rent(rows * 2 + columns * 2);

        for (int i = 0; i < rows; i++)
        {
            buffer[vCount] = new((Position.X, Position.Y + i * CellSize.X), LineColor);
            buffer[vCount + 1] = new((Position.X + columns * CellSize.X, Position.Y + i * CellSize.Y), LineColor);
            vCount += 2;
        }
        for (int i = 0; i < columns; i++)
        {
            buffer[vCount] = new((Position.X + i * CellSize.X, Position.Y), LineColor);
            buffer[vCount + 1] = new((Position.X + i * CellSize.X, Position.Y + rows * CellSize.Y), LineColor);
            vCount += 2;
        }

        renderer.DrawBuffer(buffer, vCount, 0);

        ArrayPool<Vertex>.Shared.Return(buffer);
    }

    public int PositionToIndex(int column, int row) => column + row * Columns;
    public (int, int) IndexToPosition(int index) => (index % Columns, index / Columns);

    public int CoordsToIndex(Vector2f pos, float scale = 1)
    {
        pos -= Position * scale;
        int column = (int)(pos.X / CellSize.X * scale);
        int row = (int)(pos.Y / CellSize.Y * scale);

        return PositionToIndex(column, row);   
    }

    public Vector2f IndexToCoords(int index, float scale = 1)
    {
        return new()
        {
            X = Position.X + (int)(index % Columns) * CellSize.X * scale,
            Y = Position.Y + (int)(index / Columns) * CellSize.Y * scale,
        };
    }
}

public struct KGameMap
{
    public KGrid Grid;
}


public class KGame
{
    public KGameMap GameMap;

    public KGame()
    {
        GameMap = new()
        {
            Grid = new()
            {
                
            }
        };
    }

    public void Update(uint currentFrame)
    {
        
    }

    public void FrameUpdate(uint currentFrame)
    {
        
    }

    public void GenerateMap()
    {
        
    }
}

