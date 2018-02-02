using Microsoft.Xna.Framework;

namespace Quadris
{
	class Grid
	{
		private int[] grid;
		private Pieces pieces = new Pieces();

		public Grid()
		{
			grid = new int[Constants.GRID_WIDTH * Constants.GRID_HEIGHT];
		}

		public bool TileFree(int x, int y)
		{
			return grid[y * Constants.GRID_WIDTH + x] == 0;
		}

		public Color TileColor(int x, int y)
		{
			Tetromino.Type type = (Tetromino.Type)(grid[y * Constants.GRID_WIDTH + x] - 1);
			return Tetromino.GetColor(type);
		}

		public bool Collision(int x, int y, Tetromino.Type type, int rotation)
		{
			// Loop over the 5x5 tiles of a piece on the board (i1,j1 = position on board; i2,j2 = position in piece tiles)
			for (int i1 = x, i2 = 0; i1 < x + Constants.PIECE_TILES; i1++, i2++)
			{
				for (int j1 = y, j2 = 0; j1 < y + Constants.PIECE_TILES; j1++, j2++)
				{
					// Check if the piece is outside the board bounds
					if (i1 < 0 || i1 > Constants.GRID_WIDTH-1 || j1 > Constants.GRID_HEIGHT-1)
					{
						if (Pieces.TileType(type, rotation, j2, i2) != 0)
							return true;
					}

					// Check if the piece has collided with a tile that was already stored
					if (j1 >= 0)
					{
						if ((Pieces.TileType(type, rotation, j2, i2) != 0) && (!TileFree(i1, j1)))
							return true;
					}
				}
			}

			// no collision
			return false;
		}

		public void StorePiece(int x, int y, Tetromino.Type type, int rotation)
		{
			for (int i1 = x, i2 = 0; i1 < x + Constants.PIECE_TILES; i1++, i2++)
				for (int j1 = y, j2 = 0; j1 < y + Constants.PIECE_TILES; j1++, j2++)
					if (Pieces.TileType(type, rotation, j2, i2) != 0)
					{
						grid[j1 * Constants.GRID_WIDTH + i1] = (int)type + 1;
					}
		}

		public void ClearLines()
		{
			// Remove all lines that are completely filled
			for (int j = 0; j < Constants.GRID_HEIGHT; j++)
			{
				int i = 0;
				while (i < Constants.GRID_WIDTH)
				{
					if (grid[j * Constants.GRID_WIDTH + i] == 0) break;
					i++;
				}

				// Move all lines above one row down
				if (i == Constants.GRID_WIDTH)
				{
					for (int k = j; k > 0; k--)
						for (int h = 0; h < Constants.GRID_WIDTH; h++)
							grid[k * Constants.GRID_WIDTH + h] = grid[(k-1) * Constants.GRID_WIDTH + h];
				}
			}
		}
	}
}
