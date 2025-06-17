using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Quadris.Source;

public class Well(int width = Constants.WellWidth, int height = Constants.WellHeight)
{
    private readonly int _width = width;
    private readonly int _height = height;
    private readonly int[] _wellTiles = new int[width * height];

	public int Tile(int x, int y) => _wellTiles[y * _width + x];

    public Color TileColor(int x, int y) => TileColorFromValue(_wellTiles[y * _width + x]);

    private static Color TileColorFromValue(int value) => value switch
    {
        -1 => new(0xFFFFFF), // white
         0 => new(0x000000), // black
         1 => new(0xFFFF00), // cyan
         2 => new(0xFF0000), // blue
         3 => new(0x00A5FF), // orange
         4 => new(0x00FFFF), // yellow
         5 => new(0x00FF00), // green
         6 => new(0xFF00FF), // magenta
         7 => new(0x0000FF), // red
        _  => new(0x000000), // black
    };

    public bool Collision(Tetromino piece, int offsetX = 0, int offsetY = 0)
    {
        for (int py = 0; py < Constants.PieceTiles; ++py)
        {
            for (int px = 0; px < Constants.PieceTiles; ++px)
            {
                if (piece.Tiles[py, px] == 0) continue;

                int wellX = (int)piece.X + offsetX - Constants.PieceTiles / 2 + px;
                int wellY = (int)piece.Y + offsetY - Constants.PieceTiles / 2 + py;

                if (wellX < 0 || wellX > _width - 1)
                    return true;

                if (wellY > _height - 1)
                    return true;

                if (wellY >= 0 && _wellTiles[wellY * _width + wellX] != 0)
                    return true;
            }
        }
        return false;
    }

    public void Land(Tetromino piece)
    {
        int t = piece switch
        {
            I => 1,
            J => 2,
            L => 3,
            O => 4,
            S => 5,
            T => 6,
            Z => 7,
            _ => 0
        };

        for (int py = 0; py < Constants.PieceTiles; ++py)
        {
            for (int px = 0; px < Constants.PieceTiles; ++px)
            {
                if (piece.Tiles[py, px] == 0) continue;

                int wellX = (int)piece.X - Constants.PieceTiles / 2 + px;
                int wellY = (int)piece.Y - Constants.PieceTiles / 2 + py;

                // Prevent out of bounds error in rare cases
				if (wellX < 0 || wellX >= _width || wellY < 0 || wellY >= _height)
					continue;

				_wellTiles[wellY * _width + wellX] = t;
            }
        }
    }

    public List<int> LinesCleared()
    {
        var filledLines = new List<int>();
        for (int y = 0; y < _height; y++)
        {
            int x = 0;
            while (x < _width)
            {
                if (_wellTiles[y * _width + x] == 0) break;
                x++;
            }
            if (x == _width)
                filledLines.Add(y);
        }
        return filledLines;
    }

    public void MarkRow(int y)
    {
        for (int x = 0; x < _width; ++x)
            _wellTiles[y * _width + x] = -1;
    }

	public void Clear(List<int> lines)
	{
		// Sort lines to clear from lowest to highest
		lines.Sort();
		foreach (var line in lines)
		{
			for (int y = line; y > 0; y--)
			{
				for (int x = 0; x < _width; x++)
					_wellTiles[y * _width + x] = _wellTiles[(y - 1) * _width + x];
			}
			// Clear the top row
			for (int x = 0; x < _width; x++)
				_wellTiles[x] = 0;
		}
	}
}
