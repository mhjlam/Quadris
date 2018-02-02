using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Quadris
{
	public class Constants
	{
		public const int SCREEN_WIDTH = 640;
		public const int SCREEN_HEIGHT = 480;

		public const int PIECE_TILES = 5;		            // Number of horizontal and vertical tiles of a matrix piece
		public const int TILE_SIZE = 16;		            // Width and height of each tile of a piece

		public const int GRID_WIDTH = 10;		            // Board width in tiles
		public const int GRID_HEIGHT = 20;		            // Board height in tiles
		public const int BOARD_CENTER_X = SCREEN_WIDTH / 2;	// Center position of the board from the left of the screen
		public const int BOARD_CENTER_Y = SCREEN_HEIGHT/2;  // Center position of the board from the top of the screen

		public const int BOARD_LEFT = BOARD_CENTER_X - (GRID_WIDTH * TILE_SIZE / 2);
		public const int BOARD_RIGHT = BOARD_CENTER_X + (GRID_WIDTH * TILE_SIZE / 2);
		public const int BOARD_TOP = BOARD_CENTER_Y - (GRID_HEIGHT * TILE_SIZE / 2);
		public const int BOARD_BOTTOM = BOARD_CENTER_Y + (GRID_HEIGHT * TILE_SIZE / 2);

		public const int BOARD_LINE_WIDTH = 6;              // Width in pixels of the board boundaries
	}

	public class Quadris : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		KeyboardState oldState;
		Texture2D surface;

		Grid grid;
		Tetromino thisPiece;
		Tetromino nextPiece;

		Random random;
		TimeSpan gravityTime;
		TimeSpan elapsedTime;


		public Quadris()
		{
			Content.RootDirectory = "Content";

			graphics = new GraphicsDeviceManager(this)
			{
				IsFullScreen = false,
				PreferredBackBufferWidth = Constants.SCREEN_WIDTH,
				PreferredBackBufferHeight = Constants.SCREEN_HEIGHT
			};

			oldState = Keyboard.GetState();

			grid = new Grid();
			thisPiece = new Tetromino();
			nextPiece = new Tetromino();

			random = new Random();
			gravityTime = TimeSpan.FromSeconds(1.0);
			elapsedTime = TimeSpan.Zero;
		}

		protected override void Initialize()
		{
			Window.AllowUserResizing = false;

			SpawnPiece(true);

			base.Initialize();
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			surface = new Texture2D(graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color); // empty 1x1 color surface
		}

		protected override void UnloadContent()
		{
			surface.Dispose();

			base.UnloadContent();
		}

		protected override void Update(GameTime gameTime)
		{
			UpdateInput(gameTime);
			UpdatePiece(gameTime);

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);
			DrawScene();

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
			else if (newState.IsKeyDown(Keys.Left) && !oldState.IsKeyDown(Keys.Left))
			{
				if (!grid.Collision(thisPiece.posx - 1, thisPiece.posy, thisPiece.type, thisPiece.rotation))
				{
					thisPiece.posx--;
				}
			}
			else if (newState.IsKeyDown(Keys.Right) && !oldState.IsKeyDown(Keys.Right))
			{
				if (!grid.Collision(thisPiece.posx + 1, thisPiece.posy, thisPiece.type, thisPiece.rotation))
				{
					thisPiece.posx++;
				}
			}
			else if (newState.IsKeyDown(Keys.Down) && !oldState.IsKeyDown(Keys.Down))
			{
				if (!grid.Collision(thisPiece.posx, thisPiece.posy + 1, thisPiece.type, thisPiece.rotation))
				{
					// Reset glide timer
					elapsedTime = TimeSpan.Zero;
					thisPiece.posy++;
				}
			}

			if (newState.IsKeyDown(Keys.Z) && !oldState.IsKeyDown(Keys.Z))
			{
				if (!grid.Collision(thisPiece.posx, thisPiece.posy, thisPiece.type, (thisPiece.rotation+1) % 4))
					thisPiece.rotation = (thisPiece.rotation + 1) % 4; // modulo ensure it rolls over to 0 again
			}

			if (newState.IsKeyDown(Keys.X) && !oldState.IsKeyDown(Keys.X))
			{
				// Drop piece
				while (!grid.Collision(thisPiece.posx, thisPiece.posy, thisPiece.type, thisPiece.rotation))
				{
					thisPiece.posy++;
				}
				
				grid.StorePiece(thisPiece.posx, thisPiece.posy - 1, thisPiece.type, thisPiece.rotation);
				grid.ClearLines();

				elapsedTime = TimeSpan.Zero;
				SpawnPiece();
			}

			// Update keyboard state
			oldState = newState;
		}

		private void UpdatePiece(GameTime gameTime)
		{
			elapsedTime += gameTime.ElapsedGameTime;

			if (elapsedTime > gravityTime)
			{
				elapsedTime = TimeSpan.Zero;

				if (!grid.Collision(thisPiece.posx, thisPiece.posy + 1, thisPiece.type, thisPiece.rotation))
				{
					thisPiece.posy++;
				}
				else
				{
					grid.StorePiece(thisPiece.posx, thisPiece.posy, thisPiece.type, thisPiece.rotation);
					grid.ClearLines();
					
					SpawnPiece();
				}
			}
		}

		private void SpawnPiece(bool first = false)
		{
			thisPiece.type = (first) ? (Tetromino.Type)random.Next(0, 6) : nextPiece.type;
			thisPiece.rotation = (first) ? random.Next(0, 3) : nextPiece.rotation;
			thisPiece.posx = (Constants.GRID_WIDTH / 2) + Pieces.OriginX(thisPiece.type, thisPiece.rotation);
			thisPiece.posy = Pieces.OriginY(thisPiece.type, thisPiece.rotation);

			nextPiece.type = (Tetromino.Type)random.Next(0, 6);
			nextPiece.rotation = random.Next(0, 3);
			nextPiece.posx = Constants.GRID_WIDTH + 5;
			nextPiece.posy = 5;

			if (grid.Collision(thisPiece.posx, thisPiece.posy, thisPiece.type, thisPiece.rotation))
				Exit();
		}

		private void DrawScene()
		{
			DrawBoard();
			DrawPiece(thisPiece);
			DrawPiece(nextPiece);
		}

		private void DrawBoard()
		{
			Color color = Color.Red;

			// Determine the bounds of the board (left, right, top, bottom)
			int left = Constants.BOARD_LEFT - 1;
			int right = Constants.BOARD_RIGHT;

			int top = Constants.BOARD_TOP - 1;
			int bottom = Constants.BOARD_BOTTOM;

			// Draw board boundaries
			DrawBorder(left, top, right, bottom, 1, Color.White);

			left = left + 1;
			
			// Draw filled board tiles
			for (int i = 0; i < Constants.GRID_WIDTH; i++)
			{
				for (int j = 0; j < Constants.GRID_HEIGHT; j++)
				{
					// Draw rectangle if tile is filled
					if (!grid.TileFree(i, j))
						DrawTile((left + i * Constants.TILE_SIZE), (top + j * Constants.TILE_SIZE), grid.TileColor(i, j));
				}
			}
		}

		private void DrawPiece(Tetromino piece)
		{
			// Position of the tile to draw
			int x = (Constants.BOARD_CENTER_X - (Constants.TILE_SIZE * (Constants.GRID_WIDTH  / 2))) + (piece.posx * Constants.TILE_SIZE);
			int y = (Constants.BOARD_CENTER_Y - (Constants.TILE_SIZE * (Constants.GRID_HEIGHT / 2))) + (piece.posy * Constants.TILE_SIZE);
			
			// Draw filled tiles
			for (int i = 0; i < Constants.PIECE_TILES; i++)
			{
				for (int j = 0; j < Constants.PIECE_TILES; j++)
				{
					//// Determine the color of the tile
					//switch (Pieces.TileType(piece.type, piece.rotation, j, i))
					//{
					//    case 1: color = Color.Lime; break;
					//    case 2: color = Color.Blue; break; // pivot
					//}

					if (Pieces.TileType(piece.type, piece.rotation, j, i) != 0)
					{
						DrawTile((x + i * Constants.TILE_SIZE), (y + j * Constants.TILE_SIZE), Tetromino.GetColor(piece.type));
					}
				}
			}
		}

		private void DrawTile(int left, int top, Color color)
		{
			DrawRectangle(left, top, left + Constants.TILE_SIZE - 1, top + Constants.TILE_SIZE - 1, color);
		}

		private void DrawRectangle(int left, int top, int right, int bottom, Color color)
		{
			surface.SetData(new Color[] { color });

			Rectangle rectangle = new Rectangle(left, top, right - left, bottom - top);

			spriteBatch.Begin();
			spriteBatch.Draw(surface, rectangle, color);
			spriteBatch.End();
		}

		private void DrawBorder(int left, int top, int right, int bottom, int thickness, Color color)
		{
			surface.SetData(new Color[] { color });

			Rectangle rectangle = new Rectangle(left, top, right - left, bottom - top);

			spriteBatch.Begin();
			spriteBatch.Draw(surface, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
			spriteBatch.Draw(surface, new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
			spriteBatch.Draw(surface, new Rectangle((rectangle.X + rectangle.Width - thickness), rectangle.Y, thickness, rectangle.Height), color);
			spriteBatch.Draw(surface, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - thickness, rectangle.Width, thickness), color);
			spriteBatch.End();
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
