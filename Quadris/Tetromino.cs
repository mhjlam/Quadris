using Microsoft.Xna.Framework;

namespace Quadris
{
	public class Tetromino
	{
		public int X;
		public int Y;

		public Color Color;
		public int[,] Tiles;

		public Tetromino()
		{
			Color = new Color(0xFFFFFF);
		}

		public Tetromino Copy()
		{
			return new Tetromino
			{
				X = X,
				Y = Y,
				Color = Color,
				Tiles = Tiles
			};
		}

		public virtual int[,] Rotate()
		{
			if (Tiles == null || Tiles.Length == 0) return Tiles;

			int rows = Tiles.GetLength(0);
			int cols = Tiles.GetLength(1);

			if (rows != cols) return Tiles;

			// r1 = rows-1 - c0
			// c1 = r0
			int[,] rotatedTiles = new int[rows, cols];
			for (int c = 0; c < cols; ++c)
			{
				for (int r = 0; r < rows; ++r)
				{
					rotatedTiles[rows - 1 - c, r] = Tiles[r, c];
				}
			}

			return rotatedTiles;
		}

		public virtual int[,] Rotate2() // rotate clockwise (unused)
		{
			if (Tiles == null || Tiles.Length == 0) return Tiles;

			int rows = Tiles.GetLength(0);
			int cols = Tiles.GetLength(1);

			if (rows != cols) return Tiles;

			// r1 = c0
			// c1 = cols-1 - r0
			int[,] rotatedTiles = new int[rows, cols];
			for (int c = 0; c < cols; ++c)
			{
				for (int r = 0; r < rows; ++r)
				{
					rotatedTiles[c, cols - 1 - r] = Tiles[r, c];
				}
			}

			return rotatedTiles;
		}
	}

	public class I : Tetromino
	{
		public I()
		{
			Color = new Color(0xFFFF00);

			Tiles = new int[5, 5]
			{
				{ 0, 0, 0, 0, 0 },
				{ 0, 0, 0, 0, 0 },
				{ 0, 1, 1, 1, 1 },
				{ 0, 0, 0, 0, 0 },
				{ 0, 0, 0, 0, 0 }
			};

			// optimization: store coordinates of blocks instead of using a mask
			// rotations must be stored this way too
			// 0x21, 0x22, 0x23, 0x24
		}
	}

	public class J : Tetromino
	{
		public J()
		{
			Color = new Color(0xFF0000);

			Tiles = new int[5, 5]
			{
				{ 0, 0, 0, 0, 0 },
				{ 0, 0, 1, 0, 0 },
				{ 0, 0, 1, 0, 0 },
				{ 0, 1, 1, 0, 0 },
				{ 0, 0, 0, 0, 0 }
			};

			// 0x12, 0x22, 0x31, 0x32
		}
	}

	public class L : Tetromino
	{
		public L()
		{
			Color = new Color(0x00A5FF);

			Tiles = new int[5, 5]
			{
				{ 0, 0, 0, 0, 0 },
				{ 0, 0, 1, 0, 0 },
				{ 0, 0, 1, 0, 0 },
				{ 0, 0, 1, 1, 0 },
				{ 0, 0, 0, 0, 0 }
			};

			// 0x12, 0x22, 0x32, 0x33
		}
	}

	public class O : Tetromino
	{
		public O()
		{
			Color = new Color(0x00FFFF);

			Tiles = new int[5, 5]
			{
				{ 0, 0, 0, 0, 0 },
				{ 0, 0, 0, 0, 0 },
				{ 0, 0, 1, 1, 0 },
				{ 0, 0, 1, 1, 0 },
				{ 0, 0, 0, 0, 0 }
			};

			// 0x22, 0x23, 0x32, 0x33
		}

		// disable rotating for the O piece
		public override int[,] Rotate()
		{
			return Tiles;
		}

		public override int[,] Rotate2()
		{
			return Tiles;
		}
	}

	public class S : Tetromino
	{
		public S()
		{
			Color = new Color(0x00FF00);

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
			Color = new Color(0xFF00FF);

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
			Color = new Color(0x0000FF);

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
}
