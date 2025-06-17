using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quadris.Source;

public class Renderer
{
	private readonly GraphicsDevice _graphicsDevice;
	private readonly SpriteBatch _spriteBatch;
	private readonly SpriteFont _spriteFont16;
	private readonly SpriteFont _spriteFont32;
	private readonly Texture2D _tileSurface;

	// Dynamic layout fields
	private int _tileSize;
	private int _wellLeft, _wellTop, _wellRight, _wellBottom, _wellCenterX, _wellCenterY;
	private const int Margin = 40;

	// GameOver animation state
	private int _gameOverRedLine = -1;
	private bool _showGameOverOverlay = false;
	private double _gameOverAnimTimer = 0;
	private const double GameOverLineAnimDelay = 0.05;  // seconds per line
	private const double GameOverOverlayDelay = 0.50;   // seconds after last line

	// Line clear animation state (set by Quadris)
	private bool _isClearingLines = false;
	private List<int> _clearingLines = [];
	private TimeSpan _lineClearAnimTime = TimeSpan.Zero;
	private TimeSpan _lineClearAnimDuration = TimeSpan.Zero;

	public Renderer(GraphicsDevice device, ContentManager content)
	{
		_graphicsDevice = device;
		_spriteBatch = new SpriteBatch(_graphicsDevice);
		_spriteFont16 = content.Load<SpriteFont>("RedOctober16");
		_spriteFont32 = content.Load<SpriteFont>("RedOctober48");
		_tileSurface = new Texture2D(_graphicsDevice, 1, 1, false, SurfaceFormat.Color);
		_tileSurface.SetData([Color.White]);

		UpdateLayout();
	}

	// Call this whenever the window size changes
	public void UpdateLayout()
	{
		int windowWidth = _graphicsDevice.PresentationParameters.BackBufferWidth;
		int windowHeight = _graphicsDevice.PresentationParameters.BackBufferHeight;

		// Fit by height, but also check width
		int tileSizeByHeight = (windowHeight - 2 * Margin) / Constants.WellHeight;
		int tileSizeByWidth = (windowWidth - 2 * Margin) / Constants.WellWidth;

		_tileSize = Math.Min(tileSizeByHeight, tileSizeByWidth);

		int wellPixelWidth = _tileSize * Constants.WellWidth;
		int wellPixelHeight = _tileSize * Constants.WellHeight;

		_wellLeft = (windowWidth - wellPixelWidth) / 2;
		_wellTop = (windowHeight - wellPixelHeight) / 2;
		_wellRight = _wellLeft + wellPixelWidth;
		_wellBottom = _wellTop + wellPixelHeight;
		_wellCenterX = _wellLeft + wellPixelWidth / 2;
		_wellCenterY = _wellTop + wellPixelHeight / 2;
	}

	public void Update(GameTime gameTime, Session session)
	{
		if (session.GameState != GameState.GameOver)
		{
			_gameOverRedLine = -1;
			_gameOverAnimTimer = 0;
			_showGameOverOverlay = false;
			return;
		}

		if (_showGameOverOverlay)
			return;

		_gameOverAnimTimer += gameTime.ElapsedGameTime.TotalSeconds;

		int linesToShow = (int)(_gameOverAnimTimer / GameOverLineAnimDelay);
		if (linesToShow >= Constants.WellHeight)
		{
			if (_gameOverAnimTimer >= Constants.WellHeight * GameOverLineAnimDelay + GameOverOverlayDelay)
			{
				_showGameOverOverlay = true;
			}
			_gameOverRedLine = Constants.WellHeight - 1;
		}
		else
		{
			_gameOverRedLine = linesToShow;
		}
	}

	public void Begin()
	{
		_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
	}

	public void End()
	{
		_spriteBatch.End();
	}

	public void SetLineClearAnimation(bool isClearing, List<int> clearingLines, TimeSpan animTime, TimeSpan animDuration)
	{
		_isClearingLines = isClearing;
		_clearingLines = clearingLines ?? [];
		_lineClearAnimTime = animTime;
		_lineClearAnimDuration = animDuration;
	}

	public void Draw(GameState state, Session session, IReadOnlyList<Score> highscores)
	{
		switch (state)
		{
			case GameState.Menu:
				DrawMenu((int)session.SelectedMenuButton);
				break;

			case GameState.Highscores:
				DrawHighscores(highscores);
				break;

			case GameState.Game:
				DrawWell(session.Well);
				DrawPiece(session.Piece);
				DrawPreview(session.Preview);
				DrawOverlay(session.Score.Points, session.Score.Level, session.Score.Lines, session.Preview);
				break;

			case GameState.GameOver:
				DrawWellGameOver(session.Well, session.Piece);
				DrawPiece(session.Piece, Color.Red * 0.75f);
				DrawPreview(session.Preview, Color.Red);
				DrawOverlay(session.Score.Points, session.Score.Level, session.Score.Lines, session.Preview, Color.Red);
				if (_showGameOverOverlay)
				{
					DrawGameOverOverlay();
				}
				break;
		}
	}

	// Draw the well with animated red lines and deeper red for tetromino tiles
	private void DrawWellGameOver(Well well, Tetromino lastPiece)
	{
		DrawRectangle(_wellLeft - 2, _wellTop - 1, _wellRight, _wellBottom, Color.Red, 1);

		// Build a lookup for tetromino tiles
		bool[,] pieceMask = new bool[Constants.WellWidth, Constants.WellHeight];
		for (int py = 0; py < Constants.PieceTiles; py++)
		{
			for (int px = 0; px < Constants.PieceTiles; px++)
			{
				if (lastPiece.Tiles[py, px] == 0) continue;
				int wellX = (int)lastPiece.X - Constants.PieceTiles / 2 + px;
				int wellY = (int)lastPiece.Y - Constants.PieceTiles / 2 + py;
				if (wellX >= 0 && wellX < Constants.WellWidth && wellY >= 0 && wellY < Constants.WellHeight)
					pieceMask[wellX, wellY] = true;
			}
		}

		for (int j = 0; j < Constants.WellHeight; j++)
		{
			for (int i = 0; i < Constants.WellWidth; i++)
			{
				if (well.Tile(i, j) != 0)
				{
					Color color;
					if (_gameOverRedLine >= j)
					{
						color = pieceMask[i, j]
							? new Color(120, 0, 0) // Deeper red for tetromino
							: Color.Red;
					}
					else
					{
						color = well.TileColor(i, j);
					}
					DrawTile(_wellLeft - 1 + i * _tileSize, _wellTop - 1 + j * _tileSize, color);
				}
			}
		}
	}

	private void DrawGameOverOverlay()
	{
		string message = "You didn't fail -- the plan was flawed.";
		var textSize = _spriteFont16.MeasureString(message);
		int boxWidth = (int)textSize.X + 60;
		int boxHeight = (int)textSize.Y + 40;
		int boxX = _wellCenterX - boxWidth / 2;
		int boxY = _wellCenterY - boxHeight / 2;

		// Draw semi-transparent black box with white border
		DrawRectangle(boxX, boxY, boxX + boxWidth, boxY + boxHeight, Color.White, 3);
		_spriteBatch.Draw(_tileSurface, new Rectangle(boxX + 3, boxY + 3, boxWidth - 6, boxHeight - 6), Color.Black * 0.85f);
		_spriteBatch.DrawString(
			_spriteFont16,
			message,
			new Vector2(
				(int)(_wellCenterX - textSize.X / 2),
				(int)(_wellCenterY - textSize.Y / 2)
			),
			Color.White
		);
	}

	public void DrawWell(Well well, Color? borderColor = null)
	{
		var color = borderColor ?? Color.White;
		DrawRectangle(_wellLeft - 2, _wellTop - 1, _wellRight, _wellBottom, color, 1);

		for (int j = 0; j < Constants.WellHeight; j++)
		{
			for (int i = 0; i < Constants.WellWidth; i++)
			{
				if (well.Tile(i, j) != 0)
				{
					Color tileColor = (_isClearingLines && _clearingLines.Contains(j)) ? Color.White : well.TileColor(i, j);
					DrawTile(_wellLeft - 1 + i * _tileSize, _wellTop - 1 + j * _tileSize, tileColor);
				}
			}
		}
	}

	public void DrawPiece(Tetromino piece, Color? pieceColor = null)
	{
		int x = -1 + _wellCenterX - _tileSize * (Constants.WellWidth / 2) + ((int)piece.X - Constants.PieceTiles / 2) * _tileSize;
		int y = -1 + _wellCenterY - _tileSize * (Constants.WellHeight / 2) + ((int)piece.Y - Constants.PieceTiles / 2) * _tileSize;
		var color = pieceColor ?? piece.Color;

		for (int py = 0; py < Constants.PieceTiles; py++)
		{
			for (int px = 0; px < Constants.PieceTiles; px++)
			{
				if (piece.Tiles[py, px] == 0) continue;
				if (piece.Y - Constants.PieceTiles / 2 + py < 0) continue;
				DrawTile(x + px * _tileSize, y + py * _tileSize, color);
			}
		}
	}

	public void DrawPreview(Tetromino preview, Color? borderColor = null)
	{
		// Draw preview at a fixed offset from the well, always relative to the well
		int previewBoxX = _wellRight + 40;
		int previewBoxY = _wellTop + 40;
		var color = borderColor ?? Color.White;

		DrawRectangle(previewBoxX, previewBoxY, previewBoxX + Constants.PieceTiles * _tileSize, previewBoxY + Constants.PieceTiles * _tileSize, color, 1);

		for (int py = 0; py < Constants.PieceTiles; py++)
		{
			for (int px = 0; px < Constants.PieceTiles; px++)
			{
				if (preview.Tiles[py, px] == 0) continue;
				DrawTile(previewBoxX + px * _tileSize, previewBoxY + py * _tileSize, preview.Color);
			}
		}
	}

	public void DrawOverlay(int score, int level, int lines, Tetromino preview, Color? textColor = null)
	{
		// Draw overlay below the preview box
		int previewBoxX = _wellRight + 40;
		int previewBoxY = _wellTop + 40 + Constants.PieceTiles * _tileSize + 10;
		var color = textColor ?? Color.White;

		_spriteBatch.DrawString(_spriteFont16, $"Score: {score}", new Vector2(previewBoxX, previewBoxY), color);
		_spriteBatch.DrawString(_spriteFont16, $"Level: {level}", new Vector2(previewBoxX, previewBoxY + 20), color);
		_spriteBatch.DrawString(_spriteFont16, $"Lines: {lines}", new Vector2(previewBoxX, previewBoxY + 40), color);
	}

	public void DrawMenu(int selectedIndex)
	{
		var quadrisTextSize = _spriteFont32.MeasureString("Quadris");
		var playTextSize = _spriteFont16.MeasureString("Play");
		var highscoresTextSize = _spriteFont16.MeasureString("Highscores");
		var exitTextSize = _spriteFont16.MeasureString("Exit");

		var buttonData = new (string Label, MenuButton Enum, Vector2 Size)[]
		{
			("Play", MenuButton.Play, playTextSize),
			("Highscores", MenuButton.Highscores, highscoresTextSize),
			("Exit", MenuButton.Exit, exitTextSize)
		};

		int buttonMargin = 24;
		float maxButtonWidth = Math.Max(playTextSize.X, Math.Max(highscoresTextSize.X, exitTextSize.X));
		float maxButtonHeight = Math.Max(playTextSize.Y, Math.Max(highscoresTextSize.Y, exitTextSize.Y));
		float totalButtonsHeight = buttonData.Length * maxButtonHeight + (buttonData.Length - 1) * buttonMargin;
		int windowHeight = _graphicsDevice.PresentationParameters.BackBufferHeight;
		float startY = windowHeight / 2f - totalButtonsHeight / 2f;
		int centerX = _wellCenterX;

		int quadrisY = windowHeight / 4 - (int)quadrisTextSize.Y / 2;
		_spriteBatch.DrawString(_spriteFont32, "Quadris", new Vector2((int)(centerX - quadrisTextSize.X / 2), quadrisY), Color.Red, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 0);

		for (int i = 0; i < buttonData.Length; i++)
		{
			float y = startY + i * (maxButtonHeight + buttonMargin);
			int rectLeft = (int)(centerX - maxButtonWidth / 2) - 16;
			int rectTop = (int)y - 8;
			int rectRight = (int)(centerX + maxButtonWidth / 2) + 16;
			int rectBottom = (int)(y + maxButtonHeight) + 8;
			bool isSelected = (i == selectedIndex);

			DrawButton(rectLeft, rectTop, rectRight, rectBottom, buttonData[i].Label, isSelected);
		}
	}

	public void DrawHighscores(IReadOnlyList<Score> highscores)
	{
		var highscoresTextSize = _spriteFont16.MeasureString("Highscores");
		var backTextSize = _spriteFont16.MeasureString("Back");

		// Prepare headers
		string[] headers = ["Score", "Level", "Date"];
		int minRows = 5;
		int rowCount = minRows;
		int rowHeight = (int)_spriteFont16.MeasureString("A").Y + 8;
		int headerHeight = (int)_spriteFont16.MeasureString("A").Y + 12;
		int tablePadding = 20;
		int colSpacing = 32;

		// Calculate max width for each column
		float[] colWidths = [
			_spriteFont16.MeasureString(headers[0]).X,
			_spriteFont16.MeasureString(headers[1]).X,
			_spriteFont16.MeasureString(headers[2]).X,
		];

		// Find the widest date string (e.g., "9999/12/31")
		float maxDateWidth = _spriteFont16.MeasureString("9999/12/31").X;
		colWidths[2] = Math.Max(colWidths[2], maxDateWidth);

		foreach (var hs in highscores)
		{
			colWidths[0] = Math.Max(colWidths[0], _spriteFont16.MeasureString(hs.Points.ToString()).X);
			colWidths[1] = Math.Max(colWidths[1], _spriteFont16.MeasureString(hs.Level.ToString()).X);
			colWidths[2] = Math.Max(colWidths[2], _spriteFont16.MeasureString(hs.Date.ToString("yyyy/MM/dd")).X);
		}

		var today = DateTime.Today;
		var mockData = new List<Score> {
			new(12000, 8, 54, today.AddDays(-2)),
			new(9500, 7, 48, today.AddDays(-9)),
			new(7000, 5, 36, today.AddDays(-3)),
			new(4200, 3, 22, today.AddDays(-77)),
			new(1800, 1, 8, today.AddDays(-14)),
		};

		// Merge and sort highscores with mock data
		var displayScores = new List<Score>(highscores);

		// If fewer than 5 real scores, add mock data and sort
		if (displayScores.Count < minRows)
		{
			// Add enough mock data to reach 5
			int needed = minRows - displayScores.Count;
			displayScores.AddRange(mockData.Take(needed));
		}

		// Sort: real scores always before mock scores if tied
		displayScores = [.. displayScores
			.OrderByDescending(s => s.Points)
			.ThenBy(s => highscores.Contains(s) ? 0 : 1) // real before mock if tied
			.ThenByDescending(s => s.Date)
			.Take(minRows)];

		// Make the table wider to fit dates comfortably
		int tableWidth = (int)(colWidths[0] + colWidths[1] + colWidths[2] + colSpacing * 2 + tablePadding * 2 + 40);
		int tableHeight = headerHeight + rowCount * rowHeight + tablePadding * 2;
		int tableX = _wellCenterX - tableWidth / 2;
		int tableY = _wellCenterY - tableHeight / 2;

		int col0X = tableX + tablePadding;
		int col1X = col0X + (int)colWidths[0] + colSpacing;
		int col2X = col1X + (int)colWidths[1] + colSpacing;

		// Draw table background and border
		_spriteBatch.DrawString(_spriteFont16, "Highscores", new Vector2((int)(_wellCenterX - highscoresTextSize.X / 2), (int)(tableY - highscoresTextSize.Y - 16)), Color.Red);

		// Draw background first
		_spriteBatch.Draw(_tileSurface, new Rectangle(tableX + 2, tableY + 2, tableWidth - 4, tableHeight - 4), Color.Black * 0.85f);

		// Draw border on top
		DrawRectangle(tableX, tableY, tableX + tableWidth, tableY + tableHeight, Color.White, 2);

		// Draw headers
		_spriteBatch.DrawString(_spriteFont16, headers[0], new Vector2(col0X, tableY + tablePadding), Color.White);
		_spriteBatch.DrawString(_spriteFont16, headers[1], new Vector2(col1X, tableY + tablePadding), Color.White);
		_spriteBatch.DrawString(_spriteFont16, headers[2], new Vector2(col2X, tableY + tablePadding), Color.White);

		// Draw rows
		for (int i = 0; i < rowCount; i++)
		{
			int y = tableY + tablePadding + headerHeight + i * rowHeight;
			if (i < displayScores.Count)
			{
				var hs = displayScores[i];
				Color color = highscores.Contains(hs) ? Color.White : Color.White * 0.5f; // dim mock data
				_spriteBatch.DrawString(_spriteFont16, hs.Points.ToString(), new Vector2(col0X, y), color);
				_spriteBatch.DrawString(_spriteFont16, hs.Level.ToString(), new Vector2(col1X, y), color);
				_spriteBatch.DrawString(_spriteFont16, hs.Date.ToString("yyyy/MM/dd"), new Vector2(col2X, y), color);
			}
			else
			{
				// Should not happen, but for safety
				_spriteBatch.DrawString(_spriteFont16, "-", new Vector2(col0X, y), Color.White * 0.3f);
				_spriteBatch.DrawString(_spriteFont16, "-", new Vector2(col1X, y), Color.White * 0.3f);
				_spriteBatch.DrawString(_spriteFont16, "-", new Vector2(col2X, y), Color.White * 0.3f);
			}
		}

		// Draw "Back" button with solid background
		int backY = tableY + tableHeight + 24;
		int backX = _wellCenterX - (int)backTextSize.X / 2;
		int backLeft = backX - 12;
		int backTop = backY - 8;
		int backRight = backX + (int)backTextSize.X + 12;
		int backBottom = backY + (int)backTextSize.Y + 8;

		// Always selected in highscores menu
		DrawButton(backLeft, backTop, backRight, backBottom, "Back", true);
	}

	public void DrawPauseOverlay()
	{
		string header = "Paused";
		string instr1 = "Press Enter to continue";
		string instr2 = "Press Escape to return to menu";

		var headerSize = _spriteFont16.MeasureString(header);
		var instr1Size = _spriteFont16.MeasureString(instr1);
		var instr2Size = _spriteFont16.MeasureString(instr2);

		int centerX = _wellCenterX, centerY = _wellCenterY;
		float maxWidth = MathF.Max(headerSize.X, MathF.Max(instr1Size.X, instr2Size.X));
		int padX = 40, padY = 24, spacing = 16;
		int boxW = (int)maxWidth + padX * 2;
		int boxH = (int)headerSize.Y + (int)instr1Size.Y + (int)instr2Size.Y + spacing * 2 + padY * 2;
		int boxX = centerX - boxW / 2, boxY = centerY - boxH / 2;

		DrawRectangle(boxX, boxY, boxX + boxW, boxY + boxH, Color.White, 3);
		_spriteBatch.Draw(_tileSurface, new Rectangle(boxX + 3, boxY + 3, boxW - 6, boxH - 6), Color.Black * 0.85f);

		int y = boxY + padY;
		_spriteBatch.DrawString(_spriteFont16, header, new Vector2(centerX - headerSize.X / 2, y), Color.Red);
		y += (int)headerSize.Y + spacing;

		void DrawKeyLine(string text, string key, Vector2 size)
		{
			int idx = text.IndexOf(key, StringComparison.Ordinal);
			float x = centerX - size.X / 2;
			if (idx < 0)
			{
				_spriteBatch.DrawString(_spriteFont16, text, new Vector2(x, y), Color.White);
			}
			else
			{
				string before = text[..idx], after = text[(idx + key.Length)..];
				_spriteBatch.DrawString(_spriteFont16, before, new Vector2(x, y), Color.White);
				x += _spriteFont16.MeasureString(before).X;
				_spriteBatch.DrawString(_spriteFont16, key, new Vector2(x, y), Color.Red);
				x += _spriteFont16.MeasureString(key).X;
				_spriteBatch.DrawString(_spriteFont16, after, new Vector2(x, y), Color.White);
			}
		}

		DrawKeyLine(instr1, "Enter", instr1Size);
		y += (int)instr1Size.Y + spacing;
		DrawKeyLine(instr2, "Escape", instr2Size);
	}

	private void DrawButton(int left, int top, int right, int bottom, string text, bool selected)
	{
		// Use a subtle background for unselected, solid red for selected
		Color bgColor = selected ? Color.Red : Color.White;
		DrawRectangle(left, top, right, bottom, bgColor, 1);

		// Draw centered text (always white)
		var textSize = _spriteFont16.MeasureString(text);
		int centerX = (left + right) / 2;
		int centerY = (top + bottom) / 2;
		_spriteBatch.DrawString(_spriteFont16, text, new Vector2((int)(centerX - textSize.X / 2), (int)(centerY - textSize.Y / 2)), Color.White);
	}

	private void DrawTile(int left, int top, Color color)
		=> DrawRectangle(left, top, left + _tileSize - 1, top + _tileSize - 1, color);

	private void DrawRectangle(int left, int top, int right, int bottom, Color color, int thickness = 0)
	{
		var rectangle = new Rectangle(left, top, right - left, bottom - top);

		if (thickness == 0)
		{
			_spriteBatch.Draw(_tileSurface, rectangle, color); // tint with color
		}
		else
		{
			_spriteBatch.Draw(_tileSurface, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
			_spriteBatch.Draw(_tileSurface, new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
			_spriteBatch.Draw(_tileSurface, new Rectangle(rectangle.X + rectangle.Width - thickness, rectangle.Y, thickness, rectangle.Height), color);
			_spriteBatch.Draw(_tileSurface, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - thickness, rectangle.Width, thickness), color);
		}
	}
}
