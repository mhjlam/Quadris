namespace Quadris.Source;

public static class Constants
{
	public const int ScreenWidth = 1280;
	public const int ScreenHeight = 720;

	public const int TileSize = 16;
	public const int PieceTiles = 5;

	public const int WellWidth = 10;
	public const int WellHeight = 20;

	public const int WellCenterX = ScreenWidth / 2;
	public const int WellCenterY = ScreenHeight / 2;

	public const int WellTop = WellCenterY - WellHeight * TileSize / 2;
	public const int WellLeft = WellCenterX - WellWidth * TileSize / 2;
	public const int WellRight = WellCenterX + WellWidth * TileSize / 2;
	public const int WellBottom = WellCenterY + WellHeight * TileSize / 2;

	public static readonly int[] Gravity =
	[
		48, 43, 38, 33, 28, 23, 18, 13, 8, 6, 5, 5, 5, 4, 4, 4, 3, 3, 3, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1
	];
}
