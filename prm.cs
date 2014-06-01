using UnityEngine;
using System.Collections;

// Статический класс для хранения переменных
public static class prm{

	public static Map CurrentMap;
	public static LevelEditor.PenType CurrentPen;

	public static int CurrentLevel;				// Текущий уровень
	public static int MaxLevel;					// Максимальный уровень
	public static int LivesCount;				// Количество жизней
	public static int Score;					// Очки
	public static int PointsCount;				// Число точек на уровне
	public static float GameSpeed = 1f;			// Скорость игры
	public static float PacmanSpeed = 1.5f;		// Скорость игрока
	public static float GhostSpeed = 1f;		// Скорость привидений
	public static float MaxTimeFear = 5f;		// Скорость привидений
	public static bool ShowBlinky = true;				// Показывать ли Блинки
	public static bool ShowPinky = true;				// Показывать ли Пинки
	public static bool ShowInky = true;				// Показывать ли Инки
	public static bool ShowClyde = true;				// Показывать ли Клайда
	public static float MusicVolume = 1f;
	public static float SoundVolume = 1f;
	public static int MaxPathPoints;			// Общее количество ключевых точек н текущей карте
}
