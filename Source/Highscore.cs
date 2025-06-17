using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Quadris.Source;

public class Highscore
{
	private const int MaxHighscores = 3;
	private const string HighscoreFilePath = "highscores";

    public List<Score> Scores { get; private set; } = [];


	public Highscore()
    {
        Load();
    }

	public void Load()
	{
		try
		{
			if (File.Exists(HighscoreFilePath))
			{
				var contents = File.ReadAllText(HighscoreFilePath);
				var scores = JsonSerializer.Deserialize<List<Score>>(contents);

				if (scores is not null)
				{
					Scores = scores;
					Sort();
				}
			}
		}
		catch
		{
			Scores = [];
		}
	}

	public void Save()
	{
		try
		{
			var contents = JsonSerializer.Serialize(Scores);
			File.WriteAllText(HighscoreFilePath, contents);
		}
		catch { }
	}

	public void Add(Score score)
    {
        Scores.Add(score);
		Sort();
		Save();
    }

	private void Sort()
	{
		Scores.Sort((x, y) => y.Points.CompareTo(x.Points));
		if (Scores.Count > MaxHighscores)
		{
			Scores.RemoveRange(MaxHighscores, Scores.Count - MaxHighscores);
		}
	}
}