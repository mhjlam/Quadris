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

		public static readonly int[] Gravity =
		{
			// frames/cell					// level
			48,								// 0
			43,								// 1
			38,								// 2
			33,								// 3
			28,								// 4
			23,								// 5
			18,								// 6
			13,								// 7
			8,								// 8
			6,								// 9
			5, 5, 5,						// 10-12
			4, 4, 4,						// 13-15
			3, 3, 3,						// 16-18
			2, 2, 2, 2, 2, 2, 2, 2, 2, 2,	// 19-28
			1								// 29+
		};
	}

	public enum GameState
	{
		Menu,
		Game,
		Pause,
		GameOver
	}

	public enum MenuButton
	{
		Play,
		Exit
	}

	public class Quadris : Game
	{
		GraphicsDeviceManager graphicsDevice;
		SpriteBatch spriteBatch;
		SpriteFont spriteFont;
		Texture2D tileSurface;

		KeyboardState prevKeyState = Keyboard.GetState();
		Dictionary<Keys, int> keyDownTime = new Dictionary<Keys, int>();

		int score = 0;
		int level = 0;
		int lines = 0;
		bool newpiece = true;

		Random random = new Random();
		List<int> LinesCleared = new List<int>();
		TimeSpan gravityTimer = TimeSpan.FromSeconds(0.8);
		TimeSpan gravityTime = TimeSpan.Zero;
		TimeSpan clearTimer = TimeSpan.FromSeconds(0.2);
		TimeSpan clearTime = TimeSpan.Zero;

		Well well = new Well();
		Tetromino piece = new Tetromino();
		Tetromino preview = new Tetromino();

		GameState gameState = GameState.Menu;
		MenuButton menuButton = MenuButton.Play;

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
		}

		public void Reset()
		{
			prevKeyState = Keyboard.GetState();
			keyDownTime = new Dictionary<Keys, int>();
			keyDownTime[Keys.Left] = 0;
			keyDownTime[Keys.Right] = 0;
			keyDownTime[Keys.Down] = 0;

			score = 0;
			level = 0;
			lines = 0;
			newpiece = true;

			random = new Random();
			LinesCleared = new List<int>();
			gravityTimer = TimeSpan.FromSeconds(0.8);
			gravityTime = TimeSpan.Zero;
			clearTimer = TimeSpan.FromSeconds(0.2);
			clearTime = TimeSpan.Zero;

			well = new Well();
			piece = new Tetromino();
			preview = new Tetromino();

			gameState = GameState.Menu;
			menuButton = MenuButton.Play;

			SpawnPiece(true);
		}

		protected override void Initialize()
		{
			Reset();
			base.Initialize();
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			spriteFont = Content.Load<SpriteFont>("RedOctober");
			tileSurface = new Texture2D(graphicsDevice.GraphicsDevice, 1, 1, false, SurfaceFormat.Color); // empty 1x1 color surface
		}

		protected override void UnloadContent()
		{
			tileSurface.Dispose();
			base.UnloadContent();
		}

		protected override void Update(GameTime gameTime)
		{
			KeyboardState keyState = Keyboard.GetState();

			// Exit when escape key is pressed
			if (keyState.IsKeyDown(Keys.Escape))
			{
				Exit();
			}

			switch (gameState)
			{
				case GameState.Menu:
					{
						if (keyState.IsKeyDown(Keys.Down) && !prevKeyState.IsKeyDown(Keys.Down))
						{
							menuButton = ((int)menuButton + 1 > Enum.GetNames(typeof(MenuButton)).Length) ? menuButton = 0 : menuButton + 1;
						}
						else if (keyState.IsKeyDown(Keys.Up) && !prevKeyState.IsKeyDown(Keys.Up))
						{
							menuButton = ((int)menuButton + 1 < 0) ? menuButton = (MenuButton)Enum.GetNames(typeof(MenuButton)).Length - 1 : menuButton - 1;
						}
						else if (keyState.IsKeyDown(Keys.Enter) && !prevKeyState.IsKeyDown(Keys.Enter))
						{
							switch (menuButton)
							{
								case MenuButton.Play:
									gameState = GameState.Game;
									break;
								case MenuButton.Exit:
									Exit();
									break;
							}
						}
						break;
					}
				case GameState.Game:
					{
						if (LinesCleared.Count > 0)
						{
							clearTime += gameTime.ElapsedGameTime;

							if (clearTime >= clearTimer)
							{
								well.Clear(LinesCleared);

								clearTime = TimeSpan.Zero;
								int prevLines = lines;
								lines += LinesCleared.Count;

								// Update level
								if (lines / 10 > prevLines / 10)
								{
									level = Math.Min(level + 1, 29);
									gravityTimer = TimeSpan.FromSeconds(Constants.Gravity[level] / 60.0);
								}

								// Update score
								int multiplier = 40;
								switch (LinesCleared.Count)
								{
									case 2: multiplier = 100; break;
									case 3: multiplier = 300; break;
									case 4: multiplier = 1200; break;
								}
								score += multiplier * (level + 1);

								LinesCleared.Clear();
								SpawnPiece();
							}
						}
						else
						{
							UpdateInput(keyState, gameTime);
							UpdatePiece(gameTime);
						}

						break;
					}
			}

			// Update keyboard state
			prevKeyState = keyState;

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			switch (gameState)
			{
				case GameState.Menu:
					{
						DrawMenu();
						break;
					}
				case GameState.Game:
					{
						DrawWell();
						DrawPiece();
						DrawPreview();
						DrawOverlay();
						break;
					}
			}

			base.Draw(gameTime);
		}


		private void UpdateInput(KeyboardState keyboardState, GameTime gameTime)
		{
			if (keyboardState.IsKeyDown(Keys.Left))
			{
				if (!prevKeyState.IsKeyDown(Keys.Left))
				{
					newpiece = false;
					keyDownTime[Keys.Left] = 0;
					keyDownTime[Keys.Right] = 0;

					if (!well.Collision(piece, -1, 0))
					{
						piece.X--;
					}
				}
				else if (!newpiece)
				{
					// key was down on last tick as well
					if (++keyDownTime[Keys.Left] % Math.Max(4, 20 - keyDownTime[Keys.Left] / 2) == 0)
					{
						if (!well.Collision(piece, -1, 0))
						{
							piece.X--;
						}
					}
				}
			}
			else if (keyboardState.IsKeyDown(Keys.Right))
			{
				if (!prevKeyState.IsKeyDown(Keys.Right))
				{
					newpiece = false;
					keyDownTime[Keys.Left] = 0;
					keyDownTime[Keys.Right] = 0;

					if (!well.Collision(piece, 1, 0))
					{
						piece.X++;
					}
				}
				else if (!newpiece)
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
			else if (keyboardState.IsKeyDown(Keys.Down))
			{
				if (!prevKeyState.IsKeyDown(Keys.Down))
				{
					newpiece = false;
					keyDownTime[Keys.Down] = 0;

					if (!well.Collision(piece, 0, 1))
					{
						piece.Y++;
					}
					else
					{
						gravityTime = gravityTimer;
					}
				}
				else if (!newpiece)
				{
					if (++keyDownTime[Keys.Down] % Math.Max(4, 20 - keyDownTime[Keys.Down] / 2) == 0)
					{
						if (!well.Collision(piece, 0, 1))
						{
							piece.Y++;
						}
						else
						{
							gravityTime = gravityTimer;
						}
					}
				}
			}

			if (keyboardState.IsKeyDown(Keys.Z) && !prevKeyState.IsKeyDown(Keys.Z))
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
			}

			// Hard drop
			if (keyboardState.IsKeyDown(Keys.X) && !prevKeyState.IsKeyDown(Keys.X))
			{
				while (!well.Collision(piece))
				{
					piece.Y++;
				}

				piece.Y--;
				gravityTime = gravityTimer;
			}
		}

		private void UpdatePiece(GameTime gameTime)
		{
			// Gravity
			if (gravityTime >= gravityTimer)
			{
				gravityTime = TimeSpan.Zero;

				if (!well.Collision(piece, 0, 1))
				{
					piece.Y++;
				}
				else
				{
					clearTime = TimeSpan.Zero;
					gravityTime = TimeSpan.Zero;

					well.Land(piece);
					LinesCleared = well.LinesCleared();

					if (LinesCleared.Count == 0)
					{
						SpawnPiece();
					}
					else
					{
						for (int i = 0; i < LinesCleared.Count; ++i)
						{
							well.MarkRow(LinesCleared[i]);
						}
					}
				}
			}

			gravityTime += gameTime.ElapsedGameTime;
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
			// Reset keys
			newpiece = true;
			keyDownTime[Keys.Left] = 0;
			keyDownTime[Keys.Right] = 0;
			keyDownTime[Keys.Down] = 0;

			// Set current piece
			piece = (first) ? GenerateRandomPiece() : preview;
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
				gameState = GameState.Menu;
				Reset();
				//Exit();
			}

			// Generate next piece
			preview = GenerateRandomPiece();
			preview.X = Constants.WellWidth + 5;
			preview.Y = 5;
		}

		private void DrawWell()
		{
			// Draw board boundaries
			DrawRectangle(Constants.WellLeft - 2, Constants.WellTop - 1, Constants.WellRight, Constants.WellBottom, Color.White, 1);

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
			int x = -1 + Constants.WellCenterX - (Constants.TileSize * (Constants.WellWidth / 2)) + ((int)preview.X - Constants.PieceTiles / 2) * Constants.TileSize;
			int y = -1 + Constants.WellCenterY - (Constants.TileSize * (Constants.WellHeight / 2)) + ((int)preview.Y - Constants.PieceTiles / 2) * Constants.TileSize;

			DrawRectangle(x, y, x + Constants.PieceTiles * Constants.TileSize, y + Constants.PieceTiles * Constants.TileSize, Color.White, 1);

			// Draw filled tiles
			for (int py = 0; py < Constants.PieceTiles; py++)
			{
				for (int px = 0; px < Constants.PieceTiles; px++)
				{
					if (preview.Tiles[px, py] == 0) continue;
					DrawTile((x + py * Constants.TileSize), (y + px * Constants.TileSize), preview.Color);
				}
			}
		}

		private void DrawOverlay()
		{
			int x = -1 + Constants.WellCenterX - (Constants.TileSize * (Constants.WellWidth / 2)) + ((int)preview.X - Constants.PieceTiles / 2) * Constants.TileSize;
			int y = -1 + Constants.WellCenterY - (Constants.TileSize * (Constants.WellHeight / 2)) + ((int)preview.Y - Constants.PieceTiles / 2) * Constants.TileSize;
			y += Constants.PieceTiles * Constants.TileSize + 10;

			spriteBatch.Begin();
			spriteBatch.DrawString(spriteFont, "Score: " + score, new Vector2(x, y), Color.White);
			spriteBatch.DrawString(spriteFont, "Level: " + level, new Vector2(x, y + 20), Color.White);
			spriteBatch.DrawString(spriteFont, "Lines: " + lines, new Vector2(x, y + 40), Color.White);
			spriteBatch.End();
		}

		private void DrawPiece()
		{
			// Position of the tile to draw
			int x = -1 + Constants.WellCenterX - (Constants.TileSize * (Constants.WellWidth / 2)) + ((int)piece.X - Constants.PieceTiles / 2) * Constants.TileSize;
			int y = -1 + Constants.WellCenterY - (Constants.TileSize * (Constants.WellHeight / 2)) + ((int)piece.Y - Constants.PieceTiles / 2) * Constants.TileSize;

			// Draw filled tiles
			for (int py = 0; py < Constants.PieceTiles; py++)
			{
				for (int px = 0; px < Constants.PieceTiles; px++)
				{
					if (piece.Tiles[py, px] == 0) continue;
					if (piece.Y - (Constants.PieceTiles / 2) + py < 0) continue;
					DrawTile((x + px * Constants.TileSize), (y + py * Constants.TileSize), piece.Color);
				}
			}
		}

		private void DrawMenu()
		{
			Vector2 playTextSize = spriteFont.MeasureString("Play");
			Vector2 exitTextSize = spriteFont.MeasureString("Exit");

			DrawRectangle(Constants.WellCenterX - 10 - (int)playTextSize.X / 2,
						  Constants.WellCenterY - 10 - (int)playTextSize.Y - 10,
						  Constants.WellCenterX + 10 + (int)playTextSize.X / 2,
						  Constants.WellCenterY + 10 - (int)playTextSize.Y / 2 - 10,
						  (menuButton == MenuButton.Play) ? Color.Red : Color.White, 2);

			DrawRectangle(Constants.WellCenterX - 10 - (int)playTextSize.X / 2,
						  Constants.WellCenterY - 10 + (int)playTextSize.Y / 2 + 10,
						  Constants.WellCenterX + 10 + (int)playTextSize.X / 2,
						  Constants.WellCenterY + 10 + (int)playTextSize.Y + 10,
						  (menuButton == MenuButton.Exit) ? Color.Red : Color.White, 2);

			spriteBatch.Begin();
			spriteBatch.DrawString(spriteFont, "Play", new Vector2(Constants.WellCenterX - (int)playTextSize.X / 2, Constants.WellCenterY - (int)playTextSize.Y - 10), Color.White);
			spriteBatch.DrawString(spriteFont, "Exit", new Vector2(Constants.WellCenterX - (int)exitTextSize.X / 2, Constants.WellCenterY + (int)playTextSize.Y / 2 + 10), Color.White);
			spriteBatch.End();
		}

		private void DrawTile(int left, int top, Color color)
		{
			DrawRectangle(left, top, left + Constants.TileSize - 1, top + Constants.TileSize - 1, color);
		}

		private void DrawRectangle(int left, int top, int right, int bottom, Color color, int thickness = 0)
		{
			tileSurface.SetData(new Color[] { color });

			Rectangle rectangle = new Rectangle(left, top, right - left, bottom - top);

			if (thickness == 0) // solid
			{
				spriteBatch.Begin();
				spriteBatch.Draw(tileSurface, rectangle, color);
				spriteBatch.End();
			}
			else
			{
				spriteBatch.Begin();
				spriteBatch.Draw(tileSurface, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
				spriteBatch.Draw(tileSurface, new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
				spriteBatch.Draw(tileSurface, new Rectangle(rectangle.X + rectangle.Width - thickness, rectangle.Y, thickness, rectangle.Height), color);
				spriteBatch.Draw(tileSurface, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - thickness, rectangle.Width, thickness), color);
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
