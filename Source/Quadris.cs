using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Quadris.Source;

public enum GameState
{
	Menu,
	Highscores,
	Game,
	Pause,
	GameOver
}

public enum MenuButton
{
	Play = 0,
	Highscores = 1,
	Exit = 2
}

public class Quadris : Game
{
	private readonly Input _input = new();
	private Renderer? _renderer;
	private IScreen? _currentScreen;

	private readonly Highscore _highscore = new();
	private readonly Session _session = new();
	private Menu _menu;

	// Animation state for line clear
	private bool _isClearingLines = false;
	private TimeSpan _lineClearAnimTime = TimeSpan.Zero;
	private readonly TimeSpan _lineClearAnimDuration = TimeSpan.FromMilliseconds(200);
	private List<int> _clearingLines = [];

	public Quadris()
	{
		Content.RootDirectory = "Content";
		var _ = new GraphicsDeviceManager(this)
		{
			IsFullScreen = false,
			PreferredBackBufferWidth = Constants.ScreenWidth,
			PreferredBackBufferHeight = Constants.ScreenHeight
		};
		_menu = CreateMenu();
	}

	public Input Input => _input;
	public Renderer? Renderer => _renderer;
	public Highscore Highscore => _highscore;
	public Session Session => _session;
	public Menu Menu => _menu;

	protected override void Initialize()
	{
		Window.AllowUserResizing = true;
		Window.ClientSizeChanged += (sender, args) =>
		{
			_renderer?.UpdateLayout();
		};

		Window.Position = new Point(
			(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - Constants.ScreenWidth) / 2 - 30,
			(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - Constants.ScreenHeight) / 2
		);

		base.Initialize();
	}

	protected override void LoadContent()
	{
		_renderer = new Renderer(GraphicsDevice, Content);
		ShowMenu();
	}

	protected override void Update(GameTime gameTime)
	{
		_renderer?.Update(gameTime, _session);

		var keyState = Keyboard.GetState();

		// Handle line clear animation
		if (_isClearingLines)
		{
			_lineClearAnimTime += gameTime.ElapsedGameTime;
			if (_lineClearAnimTime >= _lineClearAnimDuration)
			{
				_session.Well.Clear(_clearingLines);
				_isClearingLines = false;
				_clearingLines.Clear();
				SpawnPiece();
			}
			_input.UpdatePrevState(keyState);
			base.Update(gameTime);
			return;
		}

		// Handle input commands
		_input.HandleInput();

		if (_session.GameState == GameState.Game)
		{
			UpdatePiece(gameTime);
		}

		// Delegate update to the current screen
		_currentScreen?.Update(gameTime);

		_input.UpdatePrevState(keyState);
		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.Black);
		_renderer?.Begin();
		_renderer?.SetLineClearAnimation(_isClearingLines, _clearingLines, _lineClearAnimTime, _lineClearAnimDuration);
		_currentScreen?.Draw(gameTime, _renderer);
		_renderer?.End();
		base.Draw(gameTime);
	}

	// --- Centralized Screen Management ---

	public void ShowMenu()
	{
		_session.GameState = GameState.Menu;
		_menu = CreateMenu();
		SwitchScreen(new MenuScreen(this, _menu));
		SetMenuInputHandlers();
	}

	public void ShowGame()
	{
		_session.Reset();
		_session.GameState = GameState.Game;
		_session.Preview = GeneratePiece();
		SpawnPiece(first: true);
		SwitchScreen(new GameScreen(this));
		SetGameInputHandlers();
	}

	public void ShowGameOver()
	{
		_session.GameState = GameState.GameOver;
		SwitchScreen(new GameOverScreen(this));
		SetGameOverInputHandlers();
	}

	public void EscapeToMenu() => ShowMenu();

	private Menu CreateMenu()
	{
		return new Menu(_session, _highscore,
			onPlay: ShowGame,
			onExit: Exit
		);
	}

	public void SwitchScreen(IScreen newScreen)
	{
		_currentScreen?.Exit();
		_currentScreen = newScreen;
		_currentScreen.Enter();
	}

	public void ShowPause()
	{
		_session.GameState = GameState.Pause;
		SwitchScreen(new PauseScreen(this));
		SetPauseInputHandlers();
	}

	public void ResumeGame()
	{
		_session.GameState = GameState.Game;
		SwitchScreen(new GameScreen(this));
		SetGameInputHandlers();
	}

	// --- Game Logic ---

	public void UpdatePiece(GameTime gameTime)
	{
		if (_isClearingLines)
			return;

		if (_session.GravityTime >= _session.GravityTimer)
		{
			_session.GravityTime = TimeSpan.Zero;

			if (!_session.Well.Collision(_session.Piece, 0, 1))
			{
				_session.Piece.Y++;
			}
			else
			{
				_session.ClearTime = TimeSpan.Zero;
				_session.GravityTime = TimeSpan.Zero;

				_session.Well.Land(_session.Piece);
				_session.OnPieceLanded();
				_session.LinesCleared = _session.Well.LinesCleared();

				int lines = _session.LinesCleared.Count;
				if (lines > 0)
				{
					_session.AddScoreAndLevel(lines);
					foreach (var line in _session.LinesCleared)
					{
						_session.Well.MarkRow(line);
					}
					_isClearingLines = true;
					_lineClearAnimTime = TimeSpan.Zero;
					_clearingLines = [.. _session.LinesCleared];
				}
				else
				{
					SpawnPiece();
				}
			}
		}

		_session.GravityTime += gameTime.ElapsedGameTime;
	}

	private void LandPiece()
	{
		_session.ClearTime = TimeSpan.Zero;
		_session.GravityTime = TimeSpan.Zero;

		_session.Well.Land(_session.Piece);
		_session.OnPieceLanded();
		_session.LinesCleared = _session.Well.LinesCleared();

		int lines = _session.LinesCleared.Count;
		if (lines > 0)
		{
			_session.AddScoreAndLevel(lines);
			foreach (var line in _session.LinesCleared)
			{
				_session.Well.MarkRow(line);
			}
			_isClearingLines = true;
			_lineClearAnimTime = TimeSpan.Zero;
			_clearingLines = [.. _session.LinesCleared];
		}
		else
		{
			SpawnPiece();
		}
	}

	public void TranslatePiece(int dx, int dy)
	{
		if (_isClearingLines)
			return;

		if (_session.GameState != GameState.Game)
			return;

		if (dy < 0)
			return;

		if (!_session.Well.Collision(_session.Piece, dx, dy))
		{
			_session.Piece.X += dx;
			_session.Piece.Y += dy;
		}
		else if (dy == 1)
		{
			LandPiece();
		}
	}

	public void RotatePiece()
	{
		if (_isClearingLines)
			return;

		if (_session.GameState != GameState.Game)
			return;

		var rotated = _session.Piece.Copy();
		var rotatedTiles = _session.Piece.Rotate();
		if (rotatedTiles is null)
			return;
		rotated.Tiles = rotatedTiles;

		if (!_session.Well.Collision(rotated))
		{
			_session.Piece.Tiles = rotatedTiles;
		}
		else if (!_session.Well.Collision(rotated, 1, 0))
		{
			_session.Piece.X++;
			_session.Piece.Tiles = rotatedTiles;
		}
		else if (!_session.Well.Collision(rotated, -1, 0))
		{
			_session.Piece.X--;
			_session.Piece.Tiles = rotatedTiles;
		}
	}

	public void HardDrop()
	{
		if (_isClearingLines)
			return;

		if (_session.GameState != GameState.Game)
			return;

		while (!_session.Well.Collision(_session.Piece))
		{
			_session.Piece.Y++;
		}
		_session.Piece.Y--;
		_session.GravityTime = _session.GravityTimer;
	}

	private void SetMenuInputHandlers()
	{
		_input.SetActiveCommandMap(new Dictionary<Keys, IGameCommand>
		{
			{ Keys.Up, new MenuUpCommand(this) },
			{ Keys.Down, new MenuDownCommand(this) },
			{ Keys.Enter, new MenuSelectCommand(this) },
			{ Keys.Escape, new MenuBackCommand(this) }
		});
	}

	private void SetGameInputHandlers()
	{
		_input.SetActiveCommandMap(new Dictionary<Keys, IGameCommand>
		{
			{ Keys.Left, new MoveLeftCommand(this) },
			{ Keys.Right, new MoveRightCommand(this) },
			{ Keys.Down, new MoveDownCommand(this) },
			{ Keys.Z, new RotateCommand(this) },
			{ Keys.X, new DropCommand(this) },
			{ Keys.Escape, new PauseCommand(this) }
		});
	}

	private void SetPauseInputHandlers()
	{
		_input.SetActiveCommandMap(new Dictionary<Keys, IGameCommand>
		{
			{ Keys.Enter, new ResumeCommand(this) },
			{ Keys.Escape, new EscapeCommand(this) }
		});
	}

	private void SetGameOverInputHandlers()
	{
		_input.SetActiveCommandMap(new Dictionary<Keys, IGameCommand>
		{
			{ Keys.Enter, new EscapeCommand(this) },
			{ Keys.Escape, new EscapeCommand(this) }
		});
	}

	private Tetromino GeneratePiece()
	{
		Tetromino tetromino = _session.Random.Next(0, 7) switch
		{
			0 => new I(),
			1 => new J(),
			2 => new L(),
			3 => new O(),
			4 => new S(),
			5 => new T(),
			6 => new Z(),
			_ => new Tetromino()
		};

		for (int i = 0; i < _session.Random.Next(0, 4); ++i)
		{
			var rotatedTiles = tetromino.Rotate();
			if (rotatedTiles != null)
			{
				tetromino.Tiles = rotatedTiles;
			}
		}

		return tetromino;
	}

	private void SpawnPiece(bool first = false)
	{
		_input.IsNewPiece = true;
		_input.SetKeyDownTime(Keys.Left, 0);
		_input.SetKeyDownTime(Keys.Right, 0);
		_input.SetKeyDownTime(Keys.Down, 0);

		// Use preview as the current piece, or generate if not set
		_session.Piece = first ? _session.Preview : _session.Preview ?? GeneratePiece();
		_session.Piece.X = Constants.WellWidth / 2;
		_session.Piece.Y = 0;

		// Adjust Y to topmost non-empty row
		for (int y = 0; y <= Constants.PieceTiles / 2; ++y)
		{
			int sum = 0;
			for (int x = 0; x < Constants.PieceTiles; ++x)
			{
				sum += _session.Piece.Tiles[y, x];
				if (sum > 0) break;
			}
			if (sum > 0)
			{
				_session.Piece.Y += Math.Abs(Constants.PieceTiles / 2 - y);
				break;
			}
		}

		// Check for game over
		if (_session.Well.Collision(_session.Piece, 0, 0))
		{
			_session.GameState = GameState.GameOver;
			_highscore.Add(_session.Score);
			_session.OnGameOver();
			ShowGameOver();
			return;
		}

		// Always generate a new preview and set its position
		_session.Preview = GeneratePiece();
		_session.Preview.X = Constants.WellWidth + 3;
		_session.Preview.Y = 2;
	}
}
