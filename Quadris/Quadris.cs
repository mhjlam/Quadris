using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Quadris
{
	public class Constants
	{
		public const int ScreenWidth = 640;
		public const int ScreenHeight = 480;

		public const int TileSize = 16;
		public const int PieceTiles = 5;

		public const int WellWidth = 10;
		public const int WellHeight = 20;

		public const int WellCenterX = ScreenWidth / 2;
		public const int WellCenterY = ScreenHeight / 2;

		public const int WellTop = WellCenterY - (WellHeight * TileSize / 2);
		public const int WellLeft = WellCenterX - (WellWidth * TileSize / 2);
		public const int WellRight = WellCenterX + (WellWidth * TileSize / 2);
		public const int WellBottom = WellCenterY + (WellHeight * TileSize / 2);
	}

	public class Quadris : Game
	{
		GraphicsDeviceManager graphicsDevice;
		SpriteBatch spriteBatch;
		Texture2D tileSurface;

		KeyboardState prevKeyState = Keyboard.GetState();
		Dictionary<Keys, int> keyDownTime = new Dictionary<Keys, int>();

		Random random = new Random();
		TimeSpan gravityTimer = TimeSpan.FromSeconds(1.0);
		TimeSpan gravityTime = TimeSpan.Zero;

		List<int> FilledLines = new List<int>();
		TimeSpan clearTimer = TimeSpan.FromSeconds(0.2);
		TimeSpan clearTime = TimeSpan.Zero;

		Well well = new Well();
		Tetromino piece = new Tetromino();
		Tetromino preview = new Tetromino();

		public Quadris()
		{
			Content.RootDirectory = "Content";

			graphicsDevice = new GraphicsDeviceManager(this)
			{
				IsFullScreen = false,
				PreferredBackBufferWidth = Constants.ScreenWidth,
				PreferredBackBufferHeight = Constants.ScreenHeight
			};

			Window.AllowUserResizing = false;
			Window.Position = new Point((GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - graphicsDevice.PreferredBackBufferWidth) / 2 - 30, 
										(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - graphicsDevice.PreferredBackBufferHeight) / 2);

			keyDownTime[Keys.Left] = 0;
			keyDownTime[Keys.Right] = 0;
			keyDownTime[Keys.Down] = 0;
		}

		protected override void Initialize()
		{
			SpawnPiece(true);
			base.Initialize();
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			tileSurface = new Texture2D(graphicsDevice.GraphicsDevice, 1, 1, false, SurfaceFormat.Color); // empty 1x1 color surface
		}

		protected override void UnloadContent()
		{
			tileSurface.Dispose();
			base.UnloadContent();
		}

		protected override void Update(GameTime gameTime)
		{
			if (FilledLines.Count > 0)
			{
				clearTime += gameTime.ElapsedGameTime;

				if (clearTime >= clearTimer)
				{
					well.Clear(FilledLines);

					clearTime = TimeSpan.Zero;
					FilledLines.Clear();

					SpawnPiece();
				}
			}
			else
			{
				UpdateInput(gameTime);
				UpdatePiece(gameTime);
			}

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			DrawWell();
			DrawTetromino(piece);
			DrawPreview();

			base.Draw(gameTime);
		}


		private void UpdateInput(GameTime gameTime)
		{
			KeyboardState newState = Keyboard.GetState();

			// Exit when escape key is pressed
			if (newState.IsKeyDown(Keys.Escape))
			{
				Exit();
			}
			
			if (newState.IsKeyDown(Keys.Left))
			{
				if (!prevKeyState.IsKeyDown(Keys.Left))
				{
					keyDownTime[Keys.Left] = 0;
					keyDownTime[Keys.Right] = 0;

					if (!well.Collision(piece, -1, 0))
					{
						piece.X--;
					}
				}
				else // key was down on last tick as well
				{
					if (++keyDownTime[Keys.Left] % Math.Max(4, 20 - keyDownTime[Keys.Left] / 2) == 0)
					{
						if (!well.Collision(piece, -1, 0))
						{
							piece.X--;
						}
					}
				}
			}
			else if (newState.IsKeyDown(Keys.Right))
			{
				if (!prevKeyState.IsKeyDown(Keys.Right))
				{
					keyDownTime[Keys.Left] = 0;
					keyDownTime[Keys.Right] = 0;

					if (!well.Collision(piece, 1, 0))
					{
						piece.X++;
					}
				}
				else
				{
					if (++keyDownTime[Keys.Right] % Math.Max(4, 20 - keyDownTime[Keys.Right] / 2) == 0)
					{
						if (!well.Collision(piece, 1, 0))
						{
							piece.X++;
						}
					}
				}
			}

			// Soft drop
			else if (newState.IsKeyDown(Keys.Down))
			{
				if (!prevKeyState.IsKeyDown(Keys.Down))
				{
					keyDownTime[Keys.Down] = 0;

					if (!well.Collision(piece, 0, 1))
					{
						piece.Y++;
					}
				}
				else
				{
					if (++keyDownTime[Keys.Down] % Math.Max(4, 20 - keyDownTime[Keys.Down] / 2) == 0)
					{
						if (!well.Collision(piece, 0, 1))
						{
							piece.Y++;
						}
					}
				}
			}

			if (newState.IsKeyDown(Keys.Z) && !prevKeyState.IsKeyDown(Keys.Z))
			{
				Tetromino rotated = piece.Copy();
				rotated.Tiles = piece.Rotate();

				// attempt rotation as-is
				if (!well.Collision(rotated))
				{
					piece.Tiles = rotated.Tiles;
				}
				// attempt wall-kick-right before rotation
				else if (!well.Collision(rotated, 1, 0))
				{
					piece.X++;
					piece.Tiles = rotated.Tiles;
				}
				// attempt wall-kick-left before rotation
				else if (!well.Collision(rotated, -1, 0))
				{
					piece.X--;
					piece.Tiles = rotated.Tiles;
				}
				// specific test for newly spawned piece
				else if ((int)rotated.Y < Constants.PieceTiles)
				{
					for (int y = (int)rotated.Y; y < Constants.PieceTiles; ++y)
					{
						rotated.Y = y;
						if (!well.Collision(rotated))
						{
							piece.Y = rotated.Y;
							piece.Tiles = rotated.Tiles;
							break;
						}
					}
				}
			}

			// Hard drop
			if (newState.IsKeyDown(Keys.X) && !prevKeyState.IsKeyDown(Keys.X))
			{
				while (!well.Collision(piece))
				{
					piece.Y++;
				}

				piece.Y--;
			}

			// Update keyboard state
			prevKeyState = newState;
		}

		private void UpdatePiece(GameTime gameTime)
		{
			// Drop
			if (well.Collision(piece, 0, 1))
			{
				clearTime = TimeSpan.Zero;
				gravityTime = TimeSpan.Zero;

				well.Land(piece);
				FilledLines = well.FilledLines();

				if (FilledLines.Count == 0)
				{
					SpawnPiece();
				}
				else
				{
					piece.Color = piece.Color2;
					piece.Color.A = 255;
				}
			}
			else
			{
				// Gravity
				gravityTime += gameTime.ElapsedGameTime;

				if (gravityTime >= gravityTimer)
				{
					gravityTime = TimeSpan.Zero;

					if (!well.Collision(piece, 0, 1))
					{
						piece.Y++;
					}
				}
			}
		}

		private Tetromino GenerateRandomPiece()
		{
			Tetromino tetromino = new Tetromino();

			switch (random.Next(0, 7))
			{
				case 0: tetromino = new I(); break;
				case 1: tetromino = new J(); break;
				case 2: tetromino = new L(); break;
				case 3: tetromino = new O(); break;
				case 4: tetromino = new S(); break;
				case 5: tetromino = new T(); break;
				case 6: tetromino = new Z(); break;
			}

			for (int i = 0; i < random.Next(0, 4); ++i)
			{
				tetromino.Tiles = tetromino.Rotate();
			}

			return tetromino;
		}

		private void SpawnPiece(bool first = false)
		{
			if (first)
			{
				// Generate random piece type
				piece = GenerateRandomPiece();
			}
			else
			{
				piece = preview;
			}

			piece.X = Constants.WellWidth / 2;
			piece.Y = 0;

			// Calculate vertical clearance space
			for (int y = 0; y <= Constants.PieceTiles / 2; ++y)
			{
				// if sum of row > 0
				int sum = 0;
				for (int x = 0; x < Constants.PieceTiles; ++x)
				{
					sum += piece.Tiles[y, x];
					if (sum > 0) break;
				}

				if (sum > 0)
				{
					piece.Y += Math.Abs((Constants.PieceTiles / 2) - y);
					break;
				}
			}

			// Fail state occurs when there is no space to spawn next piece
			if (well.Collision(piece, 0, 0))
			{
				Exit();
			}

			// Generate random piece type
			preview = GenerateRandomPiece();

			preview.X = Constants.WellWidth + 5;
			preview.Y = 5;
		}

		private void DrawWell()
		{
			// Draw board boundaries
			DrawRectangle(Constants.WellLeft - 2, Constants.WellTop - 1, Constants.WellRight, Constants.WellBottom, Color.White, false);
			
			// Draw filled board tiles
			for (int i = 0; i < Constants.WellWidth; i++)
			{
				for (int j = 0; j < Constants.WellHeight; j++)
				{
					// Draw rectangle if tile is filled
					if (well.Tile(i, j) != 0)
					{
						DrawTile((Constants.WellLeft - 1 + i * Constants.TileSize), (Constants.WellTop - 1 + j * Constants.TileSize), well.TileColor(i, j));
					}
				}
			}
		}

		private void DrawPreview()
		{
			int padding = 10;
			int offset = (Constants.TileSize * Constants.PieceTiles / 2) - padding;

			int x = -1 + Constants.WellCenterX - (Constants.TileSize * (Constants.WellWidth / 2)) + ((int)preview.X - Constants.PieceTiles / 2) * Constants.TileSize;
			int y = -1 + Constants.WellCenterY - (Constants.TileSize * (Constants.WellHeight / 2)) + ((int)preview.Y - Constants.PieceTiles / 2) * Constants.TileSize;

			DrawRectangle(x, y, x + Constants.PieceTiles * Constants.TileSize, y + Constants.PieceTiles * Constants.TileSize, Color.White, false);
			DrawTetromino(preview);
		}

		private void DrawTetromino(Tetromino tetromino)
		{
			// Position of the tile to draw
			int x = -1 + Constants.WellCenterX - (Constants.TileSize * (Constants.WellWidth / 2)) + ((int)tetromino.X - Constants.PieceTiles / 2) * Constants.TileSize;
			int y = -1 + Constants.WellCenterY - (Constants.TileSize * (Constants.WellHeight / 2)) + ((int)tetromino.Y - Constants.PieceTiles / 2) * Constants.TileSize;
			
			// Draw filled tiles
			for (int py = 0; py < Constants.PieceTiles; py++)
			{
				for (int px = 0; px < Constants.PieceTiles; px++)
				{
					if (tetromino.Tiles[px, py] == 0) continue;
					DrawTile((x + py * Constants.TileSize), (y + px * Constants.TileSize), tetromino.Color);
				}
			}
		}

		private void DrawTile(int left, int top, Color color)
		{
			DrawRectangle(left, top, left + Constants.TileSize - 1, top + Constants.TileSize - 1, color);
		}

		private void DrawRectangle(int left, int top, int right, int bottom, Color color, bool solid = true)
		{
			tileSurface.SetData(new Color[] { color });

			Rectangle rectangle = new Rectangle(left, top, right - left, bottom - top);

			if (solid)
			{
				spriteBatch.Begin();
				spriteBatch.Draw(tileSurface, rectangle, color);
				spriteBatch.End();
			}
			else
			{
				spriteBatch.Begin();
				spriteBatch.Draw(tileSurface, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, 1), color);
				spriteBatch.Draw(tileSurface, new Rectangle(rectangle.X, rectangle.Y, 1, rectangle.Height), color);
				spriteBatch.Draw(tileSurface, new Rectangle(rectangle.X + rectangle.Width - 1, rectangle.Y, 1, rectangle.Height), color);
				spriteBatch.Draw(tileSurface, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - 1, rectangle.Width, 1), color);
				spriteBatch.End();
			}
		}
	}


	static class Program
	{
		static void Main(string[] args)
		{
			using (Quadris quadris = new Quadris())
			{
				quadris.Run();
			}
		}
	}
}
