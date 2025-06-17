using System;
using System.Collections.Generic;

namespace Quadris.Source;

public class Score(int points, int level, int lines, DateTime date)
{
	public int Points { get; set; } = points;
	public int Level { get; set; } = level;
	public int Lines { get; set; } = lines;
	public DateTime Date { get; set; } = date;
}

public class Session
{
	public GameState GameState { get; set; } = GameState.Menu;
	public Score Score { get; set; } = new(0, 0, 0, DateTime.Now);
	public List<int> LinesCleared { get; set; } = [];
	public TimeSpan GravityTimer { get; set; } = GetGravityForLevel(0);
	public TimeSpan GravityTime { get; set; } = TimeSpan.Zero;
	public TimeSpan ClearTimer { get; set; } = TimeSpan.FromSeconds(0.2);
	public TimeSpan ClearTime { get; set; } = TimeSpan.Zero;
	public Well Well { get; set; } = new();
	public Tetromino Piece { get; set; } = new();
	public Tetromino Preview { get; set; } = new();
	public Random Random { get; set; } = new();
	public MenuButton SelectedMenuButton { get; set; } = MenuButton.Play;

	// Events (optional)
	public event Action<int>? LineCleared;
	public event Action<int>? ScoreChanged;
	public event Action<int>? LevelChanged;
	public event Action? GameOver;
	public event Action? PieceLanded;

	public void OnLineCleared(int lines) => LineCleared?.Invoke(lines);
	public void OnScoreChanged(int score) => ScoreChanged?.Invoke(score);
	public void OnLevelChanged(int level) => LevelChanged?.Invoke(level);
	public void OnGameOver() => GameOver?.Invoke();
	public void OnPieceLanded() => PieceLanded?.Invoke();

	public void AddScoreAndLevel(int linesCleared)
	{
		// NES Tetris scoring
		int[] nesPoints = [0, 40, 100, 300, 1200];
		int level = Score.Level;
		if (linesCleared >= 1 && linesCleared <= 4)
		{
			Score.Points += nesPoints[linesCleared] * (level + 1);
			Score.Lines += linesCleared;

			// Level up every 10 lines
			int newLevel = Score.Lines / 10;
			if (newLevel > Score.Level)
			{
				Score.Level = newLevel;
				GravityTimer = GetGravityForLevel(newLevel);
				OnLevelChanged(newLevel);
			}
			OnScoreChanged(Score.Points);
			OnLineCleared(linesCleared);
		}
	}

	public static TimeSpan GetGravityForLevel(int level)
	{
		// NES Tetris gravity table (seconds per drop)
		double[] gravityTable = [
			0.8, 0.7167, 0.6333, 0.55, 0.4667, 0.3833, 0.3, 0.2167, 0.1333, 0.1,
			0.0833, 0.0833, 0.0833, 0.0666, 0.0666, 0.0666, 0.05, 0.05, 0.05, 0.0333
		];

		if (level < gravityTable.Length)
		{
			return TimeSpan.FromSeconds(gravityTable[level]);
		}
		return TimeSpan.FromSeconds(0.0167); // Fastest
	}

	public void Reset()
	{
		GameState = GameState.Menu;
		Score = new Score(0, 0, 0, DateTime.Now);
		LinesCleared.Clear();
		GravityTimer = GetGravityForLevel(0);
		GravityTime = TimeSpan.Zero;
		ClearTimer = TimeSpan.FromSeconds(0.2);
		ClearTime = TimeSpan.Zero;
		Well = new Well();
		Piece = new Tetromino();
		Preview = new Tetromino();
		SelectedMenuButton = MenuButton.Play;
	}
}
