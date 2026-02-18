using System.Buffers;
using SFML.Graphics;
using SFML.System;

public struct KGrid
{
    public int Columns;
    public int Rows;
    public int CellWidth;
    public int CellHeight;
    public Color LineColor;
    public Vector2f Position;
    public uint[] Cells;

    public int CellCount => Columns * Rows;
    public Vector2f CellSize => new(CellWidth, CellHeight);
    
    public KGrid()
    {
        Rows = Columns = CellWidth = CellHeight = 0;
        LineColor = Color.White;
        Cells = [];
    }

    public KGrid(int columns, int rows, int x, int y, int cellWidth, int cellHeight)
    {
        Columns = columns;
        Rows = rows;
        LineColor = Color.White;
        CellWidth = cellWidth;
        CellHeight = cellHeight;
        Position = (x, y);
        Cells = new uint[Rows * Columns];
        Array.Fill<uint>(Cells, 0);
    }

    public void FrameUpdate(KRenderManager renderer, int lineLayer)
    {
        uint vCount = 0;
        var buffer = ArrayPool<Vertex>.Shared.Rent(Columns * 2 + Rows * 2);

        for (int i = 0; i < Rows; i++)
        {
            buffer[vCount] = new((Position.X, Position.Y + i * CellWidth), LineColor);
            buffer[vCount + 1] = new((Position.X + Columns * CellWidth, Position.Y + i * CellHeight), LineColor);
            vCount += 2;
        }
        for (int i = 0; i < Columns; i++)
        {
            buffer[vCount] = new((Position.X + i * CellWidth, Position.Y), LineColor);
            buffer[vCount + 1] = new((Position.X + i * CellWidth, Position.Y + Rows * CellHeight), LineColor);
            vCount += 2;
        }

        renderer.DrawBuffer(buffer, vCount, lineLayer);

        ArrayPool<Vertex>.Shared.Return(buffer);
    }

    public int PositionToIndex(int column, int row) => column + row * Columns;
    public (int, int) IndexToPosition(int index) => (index % Columns, index / Columns);

    public int CoordsToIndex(Vector2f pos, float scale = 1)
    {
        pos -= Position * scale;
        int column = (int)(pos.X / CellWidth * scale);
        int row = (int)(pos.Y / CellHeight * scale);

        return PositionToIndex(column, row);   
    }

    public Vector2f IndexToCoords(int index, float scale = 1)
    {
        return new()
        {
            X = Position.X + (int)(index % Columns) * CellWidth * scale,
            Y = Position.Y + (int)(index / Columns) * CellHeight * scale,
        };
    }
}