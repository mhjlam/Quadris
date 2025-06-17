using Microsoft.Xna.Framework;

namespace Quadris.Source;

public class Tetromino
{
    public float X { get; set; }
    public float Y { get; set; }
    public Color Color { get; set; } = new(0xFFFFFF);
    public Color Color2 { get; set; }
    public int[,] Tiles { get; set; } = new int[5, 5];

    public Tetromino() { }

    public Tetromino(float x, float y, Color color, Color color2, int[,] tiles)
        => (X, Y, Color, Color2, Tiles) = (x, y, color, color2, tiles);

	public Tetromino Copy() => new()
	{
		X = X,
		Y = Y,
		Color = Color,
		Color2 = Color2,
		Tiles = (int[,])Tiles.Clone()
	};

	public virtual int[,]? Rotate()
    {
        if (Tiles is null || Tiles.Length == 0) return Tiles;

        int rows = Tiles.GetLength(0), cols = Tiles.GetLength(1);
        if (rows != cols) return Tiles;

        var rotatedTiles = new int[rows, cols];
        for (int c = 0; c < cols; ++c)
            for (int r = 0; r < rows; ++r)
                rotatedTiles[rows - 1 - c, r] = Tiles[r, c];
        return rotatedTiles;
    }

    public virtual int[,]? Rotate2()
    {
        if (Tiles is null || Tiles.Length == 0) return Tiles;

        int rows = Tiles.GetLength(0), cols = Tiles.GetLength(1);
        if (rows != cols) return Tiles;

        var rotatedTiles = new int[rows, cols];
        for (int c = 0; c < cols; ++c)
            for (int r = 0; r < rows; ++r)
                rotatedTiles[c, cols - 1 - r] = Tiles[r, c];
        return rotatedTiles;
    }
}

public class I : Tetromino
{
    public I()
    {
        Color = new(0xFFFF00);
        Color2 = new(0xFFA500);
        Tiles = new int[5, 5]
        {
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 },
            { 0, 1, 1, 1, 1 },
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 }
        };
    }
}

public class J : Tetromino
{
    public J()
    {
        Color = new(0xFF0000);
        Color2 = new(0xFFA500);
        Tiles = new int[5, 5]
        {
            { 0, 0, 0, 0, 0 },
            { 0, 0, 1, 0, 0 },
            { 0, 0, 1, 0, 0 },
            { 0, 1, 1, 0, 0 },
            { 0, 0, 0, 0, 0 }
        };
    }
}

public class L : Tetromino
{
    public L()
    {
        Color = new(0x00A5FF);
        Color2 = new(0x0080FF);
        Tiles = new int[5, 5]
        {
            { 0, 0, 0, 0, 0 },
            { 0, 0, 1, 0, 0 },
            { 0, 0, 1, 0, 0 },
            { 0, 0, 1, 1, 0 },
            { 0, 0, 0, 0, 0 }
        };
    }
}

public class O : Tetromino
{
    public O()
    {
        Color = new(0x00FFFF);
        Color2 = new(0xA5FFFF);
        Tiles = new int[5, 5]
        {
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 },
            { 0, 0, 1, 1, 0 },
            { 0, 0, 1, 1, 0 },
            { 0, 0, 0, 0, 0 }
        };
    }

    public override int[,] Rotate() => Tiles;
    public override int[,] Rotate2() => Tiles;
}

public class S : Tetromino
{
    public S()
    {
        Color = new(0x00FF00);
        Color2 = new(0x00FFA5);
        Tiles = new int[5, 5]
        {
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 },
            { 0, 0, 1, 1, 0 },
            { 0, 1, 1, 0, 0 },
            { 0, 0, 0, 0, 0 }
        };
    }
}

public class T : Tetromino
{
    public T()
    {
        Color = new(0xFF00FF);
        Color2 = new(0xFFA5FF);
        Tiles = new int[5, 5]
        {
            { 0, 0, 0, 0, 0 },
            { 0, 0, 1, 0, 0 },
            { 0, 1, 1, 1, 0 },
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 }
        };
    }
}

public class Z : Tetromino
{
    public Z()
    {
        Color = new(0x0000FF);
        Color2 = new(0xD2D2FF);
        Tiles = new int[5, 5]
        {
            { 0, 0, 0, 0, 0 },
            { 0, 1, 1, 0, 0 },
            { 0, 0, 1, 1, 0 },
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 }
        };
    }
}
