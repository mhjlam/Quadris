using Microsoft.Xna.Framework;

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
			switch (Tile(x, y))
			{
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

					int wellX = (piece.X + offsetX - Constants.PieceTiles / 2) + px;
					int wellY = (piece.Y + offsetY - Constants.PieceTiles / 2) + py;

					// Check if the piece is inside horizontal bounds of the well
					if (wellX < 0 || wellX > Constants.WellWidth - 1)
					{
						return true;
					}

					// Check if the piece is inside vertical bounds of the well
					if (wellY < 0 || wellY > Constants.WellHeight - 1)
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

		public void StorePiece(int pieceX, int pieceY, Tetromino piece)
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

					int wellX = (pieceX - Constants.PieceTiles / 2) + px;
					int wellY = (pieceY - Constants.PieceTiles / 2) + py;

					wellTiles[wellY * Constants.WellWidth + wellX] = t;
				}
			}
		}

		public void ClearLines()
		{
			// Remove all lines that are completely filled
			for (int y = 0; y < Constants.WellHeight; y++)
			{
				int x = 0;
				while (x < Constants.WellWidth)
				{
					if (wellTiles[y * Constants.WellWidth + x] == 0) break;
					x++;
				}

				// Move all lines above one row down
				if (x == Constants.WellWidth)
				{
					for (int k = y; k > 0; k--)
					{
						for (int h = 0; h < Constants.WellWidth; h++)
						{
							wellTiles[k * Constants.WellWidth + h] = wellTiles[(k - 1) * Constants.WellWidth + h];
						}
					}
				}
			}
		}
	}
}
