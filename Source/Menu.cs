using System;

namespace Quadris.Source;

public enum MenuView
{
	Main,
	Highscores
}

public class Menu(Session session, Highscore highscore, Action? onPlay, Action? onExit)
{
	private readonly Session _session = session;
	private readonly Highscore _highscore = highscore;
	private readonly Action? _onPlay = onPlay;
	private readonly Action? _onExit = onExit;

	public GameState State { get; set; } = GameState.Menu;
	public MenuButton SelectedButton { get; set; } = MenuButton.Play;
	public MenuView CurrentView { get; private set; } = MenuView.Main;


	public void MoveUp()
	{
		if (SelectedButton > MenuButton.Play)
			SelectedButton--;
	}

	public void MoveDown()
	{
		if (SelectedButton < MenuButton.Exit)
			SelectedButton++;
	}

	public void Select()
	{
		if (CurrentView == MenuView.Main)
		{
			switch (SelectedButton)
			{
				case MenuButton.Play:
					_onPlay?.Invoke();
					break;

				case MenuButton.Highscores:
					ShowHighscores();
					break;

				case MenuButton.Exit:
					_onExit?.Invoke();
					break;
			}
		}
		else if (CurrentView == MenuView.Highscores)
		{
			ShowMainMenu();
		}
	}

	public void Back()
	{
		if (CurrentView == MenuView.Highscores)
		{
			ShowMainMenu();
		}
	}

	public void SetState(GameState state)
	{
		State = state;
		_session.GameState = state;
	}

	public void ShowHighscores()
	{
		CurrentView = MenuView.Highscores;
	}

	public void ShowMainMenu()
	{
		CurrentView = MenuView.Main;
	}
}
