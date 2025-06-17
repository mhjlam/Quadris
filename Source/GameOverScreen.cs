using Microsoft.Xna.Framework;

namespace Quadris.Source;

public class GameOverScreen : IScreen
{
	private readonly Quadris _game;

	public GameOverScreen(Quadris game)
	{
		_game = game;
	}

	public void Enter()
	{
		// No-op: game over state is already set by Quadris.ShowGameOver()
	}

	public void Exit()
	{
		// No-op for now, but could be used for cleanup if needed
	}

	public void Update(GameTime gameTime)
	{
		// Game over input/logic is handled by Quadris and commands
	}

	public void Draw(GameTime gameTime, Renderer? renderer)
	{
		var session = _game.Session;
		var highscores = _game.Highscore.Scores;
		renderer?.Draw(GameState.GameOver, session, highscores);
	}
}
