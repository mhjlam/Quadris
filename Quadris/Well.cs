using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Quadris
{
	class Well
	{
		private int width;
		private int height;
		private int[] wellTiles;

		public Well(int width = 10, int height = 20)
		{
			this.width = width;
			this.height = height;
			wellTiles = new int[Constants.WellWidth * Constants.WellHeight];
		}

		public int Tile(int x, int y)
		{
			return wellTiles[y * Constants.WellWidth + x];
		}

		public Color TileColor(int x, int y)
		{
			switch (wellTiles[y * Constants.WellWidth + x])
			{
				case -1: return new Color(0xFFFFFF); // white
				case 0:  return new Color(0x000000); // black
				case 1:  return new Color(0xFFFF00); // cyan
				case 2:  return new Color(0xFF0000); // blue
				case 3:  return new Color(0x00A5FF); // orange
				case 4:  return new Color(0x00FFFF); // yellow
				case 5:  return new Color(0x00FF00); // green
				case 6:  return new Color(0xFF00FF); // magenta
				case 7:  return new Color(0x0000FF); // red
				default: return new Color(0x000000); // black
			}
		}

		public bool Collision(Tetromino piece, int offsetX = 0, int offsetY = 0)
		{
			// Loop over the 5x5 tiles of a piece on the board (i1,j1 = position in well; i2,j2 = position in piece tile grid)
			for (int py = 0; py < Constants.PieceTiles; ++py)
			{
				for (int px = 0; px < Constants.PieceTiles; ++px)
				{
					if (piece.Tiles[py, px] == 0) continue;

					int wellX = ((int)piece.X + offsetX - Constants.PieceTiles / 2) + px;
					int wellY = ((int)piece.Y + offsetY - Constants.PieceTiles / 2) + py;

					// Check if the piece is inside horizontal bounds of the well
					if (wellX < 0 || wellX > Constants.WellWidth - 1)
					{
						return true;
					}

					// Check if the piece is inside vertical bounds of the well
					if (wellY > Constants.WellHeight - 1)
					{
						return true;
					}

					// Check if piece has collided with a previously stored tile
					if (wellY >= 0 && wellTiles[wellY * Constants.WellWidth + wellX] != 0)
					{
						return true;
					}
				}
			}

			// No collision
			return false;
		}

		public void Land(Tetromino piece)
		{
			int t = 0;
			if (piece is I) t = 1;
			if (piece is J) t = 2;
			if (piece is L) t = 3;
			if (piece is O) t = 4;
			if (piece is S) t = 5;
			if (piece is T) t = 6;
			if (piece is Z) t = 7;

			for (int py = 0; py < Constants.PieceTiles; ++py)
			{
				for (int px = 0; px < Constants.PieceTiles; ++px)
				{
					if (piece.Tiles[py, px] == 0) continue;

					int wellX = ((int)piece.X - Constants.PieceTiles / 2) + px;
					int wellY = ((int)piece.Y - Constants.PieceTiles / 2) + py;

					wellTiles[wellY * Constants.WellWidth + wellX] = t;
				}
			}
		}

		public List<int> LinesCleared()
		{
			List<int> filledLines = new List<int>();

			for (int y = 0; y < Constants.WellHeight; y++)
			{
				int x = 0;
				while (x < Constants.WellWidth)
				{
					if (wellTiles[y * Constants.WellWidth + x] == 0) break;
					x++;
				}

				if (x == Constants.WellWidth)
				{
					filledLines.Add(y);
				}
			}

			return filledLines;
		}

		public void MarkRow(int y)
		{
			for (int x = 0; x < Constants.WellWidth; ++x)
			{
				wellTiles[y * Constants.WellWidth + x] = -1;
			}
		}

		public void Clear(List<int> lines)
		{
			for (int i = 0; i < lines.Count; ++i)
			{
				for (int y = lines[i]; y > 0; y--)
				{
					for (int x = 0; x < Constants.WellWidth; x++)
					{
						wellTiles[y * Constants.WellWidth + x] = wellTiles[(y - 1) * Constants.WellWidth + x];
					}
				}
			}
		}
	}
}
