using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Основной скрипт содержащий игровую логику, функции начала/окончания/загрузки/сохранения игры
/// </summary>
public class Game : MonoBehaviour {

	public Camera GameCamera;

	// Префабы игровых клеток
	public GameObject PrefabClear;
	public GameObject PrefabWall;
	public GameObject PrefabPoint;
	public GameObject PrefabPowerUp;
	public GameObject PrefabFruit;
	public GameObject PrefabDoor;

	// Префабы ключевой точки
	public GameObject PrefabPathPoint;
	public List<PathPoint> PathPoints;

	// Модели игрока и ботов
	public GameObject Pacman;
	public GameObject Blinky;
	public GameObject Pinky;
	public GameObject Inky;
	public GameObject Clyde;

	// Элементы интерфейса
	public UISprite LivesSprite;
	public UILabel TitleLabel;
	public UILabel ScoreLabel;
	public UILabel StageLabel;
	public UILabel TipsLabel;
	public UILabel ScoreEndLabel;
	public UILabel StageEndLabel;
	public UILabel GameMessageLabel;

	public enum MessageState {none=0, startgame, nextlevel, kill}	//Статусы сообщений
	public MessageState GameMessageState;

	/// <summary>
	/// Создаёт новую игру, в том числе загружает случайную карту и сбрасывает все игровые параметры
	/// </summary>
	public void StartNewGame ()
	{
		prm.MaxLevel = 10;
		prm.CurrentLevel = 1;
		prm.CurrentMap = MainController.Instance._LevelEditor.Maps[UnityEngine.Random.Range(0, MainController.Instance._LevelEditor.Maps.Count)];
		UpdateLevelGUI();

		prm.LivesCount = 3;
		UpdateLives ();

		prm.Score = 0;
		AddScore (0);

		LoadLevel ();
		foreach (Move m in FindObjectsOfType<Move>()) {m.CheckStartParams();}
		ShowGameMessage(MessageState.startgame);
	}

	/// <summary>
	/// Устанавливает камеру над центром карты и регулирует угол обзора в зависимости от её высоты
	/// </summary>
	void SetUpCamera ()
	{
		GameCamera.transform.position = new Vector3 (-0.5f+(float)prm.CurrentMap.width/2f, 20f, 0.5f -(float)prm.CurrentMap.height/2f);
		GameCamera.fieldOfView = prm.CurrentMap.height*3 ;
	}

	/// <summary>
	/// Сбрасываем характеристики персонажей
	/// </summary>
	void ResetPlayers ()
	{
		foreach (Move m in FindObjectsOfType<Move>())
		{
			m.CurrentPathPoint = null;
			m.CurrentMoveStateUnit = Move.MoveState.stay;
			m.NextMoveStateUnit = Move.MoveState.stay;
			m.CanChangeDirection = true;
			m.MarkedPoints = 0;
			m.PathList.Clear();
		}
	}

	/// <summary>
	/// Рестарт уровня, из-за смерти персонажа или по собственному желанию. -1 жизнь
	/// </summary>
	public void Restart ()
	{
		if (0 < prm.LivesCount--)
		{
			UpdateLives();
			ResetPlayers();
			LoadLevel();
			Time.timeScale = 1f;
		}
		else
		{
			EndGame();
			MainController.Instance.MainControl(MainController.TypeSystemActions.ShowEndGame);
		}
	}
	
	/// <summary>
	/// Продожает игру поставленную на паузу
	/// </summary>
	public void ResumeGame ()
	{
		Time.timeScale = 1f;
	}
	
	/// <summary>
	/// Ставит на паузу игру
	/// </summary>
	public void Pause ()
	{
		Time.timeScale = 0f;
	}

	/// <summary>
	/// Возвращает  игровой префаб или существующую модель персонажа в зависимости от типа клетки
	/// </summary>
	GameObject CellToPrefabType (LevelEditor.PenType CellType)
	{
		switch (CellType)
		{
		case LevelEditor.PenType.Point:
			return PrefabPoint;
		case LevelEditor.PenType.Energy:
			return PrefabPowerUp;
		case LevelEditor.PenType.Fruit:
			return PrefabFruit;
		case LevelEditor.PenType.Wall:
			return PrefabWall;
		case LevelEditor.PenType.BunkerCenter:
			return PrefabWall;
		case LevelEditor.PenType.BunkerDoor:
			return PrefabDoor;
		case LevelEditor.PenType.BunkerBricks:
			return PrefabWall;
		default:
			return PrefabClear;
		}
	}

	/// <summary>
	/// Возвращает принадлежность к неподвижным объектам
	/// </summary>
	bool isRealStatic (LevelEditor.PenType CellType)
	{
		switch (CellType)
		{
		case LevelEditor.PenType.Clear:
		case LevelEditor.PenType.BlinkySpawn:
		case LevelEditor.PenType.InkySpawn:
		case LevelEditor.PenType.PinkySpawn:
		case LevelEditor.PenType.ClydeSpawn:
		case LevelEditor.PenType.PacmanSpawn:
		case LevelEditor.PenType.BunkerClear:
			return false;
		default:
			return true;
		}
	}



	/// <summary>
	/// Создаёт нужный префаб в нужной клетке
	/// </summary>
	GameObject SetCell (LevelEditor.PenType CellType, int posX, int posY)
	{
		if (isRealStatic (CellType))
		{
			if (CellType == LevelEditor.PenType.Point) {prm.PointsCount++;}
			GameObject tempGO = Instantiate (CellToPrefabType(CellType)) as GameObject;
			tempGO.transform.parent = transform;
			tempGO.transform.localScale = Vector3.one;
			tempGO.transform.localPosition = new Vector3(posX, 0, -posY);
			Blocks.Add (tempGO);
			return tempGO;
		}
		else
		{
			switch (CellType)
			{
			case LevelEditor.PenType.BlinkySpawn:
				Blinky.transform.position = new Vector3(posX, 0, -posY);
				Blinky.GetComponent<Move>().TargetPoint = new Vector3(posX, 0, -posY);
				Blinky.GetComponent<GhostAI>().RespawnPoint = new Vector3(posX, 0, -posY);
				Blinky.GetComponent<GhostAI>().ChangeMode(GhostAI.ModeBehavior.around);
				Blinky.SetActive(prm.ShowBlinky);
				return Blinky;				
			case LevelEditor.PenType.InkySpawn:
				Inky.transform.position = new Vector3(posX, 0, -posY);
				Inky.GetComponent<Move>().TargetPoint = new Vector3(posX, 0, -posY);
				Inky.GetComponent<GhostAI>().RespawnPoint = new Vector3(posX, 0, -posY);
				Inky.GetComponent<GhostAI>().ChangeMode(GhostAI.ModeBehavior.around);
				Inky.SetActive(prm.ShowInky);
				return Inky;				
			case LevelEditor.PenType.PinkySpawn:
				Pinky.transform.position = new Vector3(posX, 0, -posY);
				Pinky.GetComponent<Move>().TargetPoint = new Vector3(posX, 0, -posY);
				Pinky.GetComponent<GhostAI>().RespawnPoint = new Vector3(posX, 0, -posY);
				Pinky.GetComponent<GhostAI>().ChangeMode(GhostAI.ModeBehavior.around);
				Pinky.SetActive(prm.ShowPinky);
				return Pinky;				
			case LevelEditor.PenType.ClydeSpawn:
				Clyde.transform.position = new Vector3(posX, 0, -posY);
				Clyde.GetComponent<Move>().TargetPoint = new Vector3(posX, 0, -posY);
				Clyde.GetComponent<GhostAI>().RespawnPoint = new Vector3(posX, 0, -posY);
				Clyde.GetComponent<GhostAI>().ChangeMode(GhostAI.ModeBehavior.around);
				Clyde.SetActive(prm.ShowClyde);
				return Clyde;
			case LevelEditor.PenType.PacmanSpawn:
				Pacman.transform.position = new Vector3(posX, 0, -posY);
				Pacman.GetComponent<Move>().TargetPoint = new Vector3(posX, 0, -posY);
				Pacman.SetActive(true);
				return Pacman;				
			case LevelEditor.PenType.Clear:
			case LevelEditor.PenType.BunkerClear:
			default:
				GameObject tempGO = Instantiate (PrefabClear) as GameObject;
				tempGO.transform.parent = transform;
				tempGO.transform.localScale = Vector3.one;
				tempGO.transform.localPosition = new Vector3(posX, 0, -posY);
				return tempGO;
			}
		}
		
	}

	/// <summary>
	/// Очищает список с объектами
	/// </summary>
	void ClearListObjects (List<GameObject> listGO)
	{
		if (listGO.Count>0)
		{
			listGO.ForEach (go => DestroyObject(go));
			listGO.Clear();
		}
	}

	/// <summary>
	/// Очищает список с ключевыми точками
	/// </summary>
	void ClearPathPoints ()
	{
		if (PathPoints.Count>0)
		{
			PathPoints.ForEach (go => DestroyObject(go.gameObject));
			PathPoints.Clear();
		}
	}

	public List<GameObject> Blocks;	// Список клеток карты

	/// <summary>
	/// Загружает выбранный уровень
	/// </summary>
	void LoadLevel ()
	{
		ClearListObjects(Blocks);	// Очищает текущую карту
		ClearPathPoints();	
		prm.PointsCount = 0;

		for (int x = 0; x<prm.CurrentMap.width; x++)
		{
			for (int y = 0; y<prm.CurrentMap.height; y++)
			{
				SetCell (MainController.Instance._LevelEditor.CharToCell(prm.CurrentMap.map[y][x]), x, y);	//Создаём клетку
				CheckPathPoint (prm.CurrentMap.map, x, y);	// Проверяем является ли точка ключевой и добавляем её в список
			}
		}
		ConnectPathPoints (prm.CurrentMap.map);	//Рассчтиываем ключевые точки
		SetUpCamera ();
		StaticBatchingUtility.Combine(gameObject);
	}

	/// <summary>
	/// Можно ли двигаться по этой клетке
	/// </summary>
	bool isPassCell (char CharCell)
	{
		switch (MainController.Instance._LevelEditor.CharToCell(CharCell))
		{
		case LevelEditor.PenType.Wall:
		case LevelEditor.PenType.BunkerCenter:
		case LevelEditor.PenType.BunkerBricks:
			return false;
		default:
			return true;
		}
	}

	/// <summary>
	/// Возвращает тип ключевой точки
	/// </summary>
	PathPoint.PointType GetTypePathPoint (string[] _map, int x, int y)
	{
		int leftX = (x+_map[y].Length-1)%_map[y].Length;
		int rightX = (x+_map[y].Length+1)%_map[y].Length;
		int upY = (y+_map.Length-1)%_map.Length;
		int downY = (y+_map.Length+1)%_map.Length;
		switch ((isPassCell(_map[y][leftX])?1:0)|(isPassCell(_map[y][rightX])?2:0)|(isPassCell(_map[upY][x])?4:0)|(isPassCell(_map[downY][x])?8:0))
		{
		case 3:
		case 12:
			return PathPoint.PointType.pass;
		case 1:
		case 2:
		case 4:
		case 8:
			return PathPoint.PointType.deadlock;
		default:
			return PathPoint.PointType.fork;
		}

	}



	/// <summary>
	/// Процедура проверяет является ли точка ключевой и в зависимости от результата создаёт объект
	/// </summary>
	void CheckPathPoint (string[] _map, int x, int y)
	{
		if (isPassCell(_map[y][x]) && (GetTypePathPoint(_map, x, y) != PathPoint.PointType.pass))
		{
			GameObject tempGO = Instantiate (PrefabPathPoint) as GameObject;
			tempGO.transform.parent = PrefabPathPoint.transform.parent;
			tempGO.transform.localScale = Vector3.one*0.3f;
			tempGO.transform.localPosition = new Vector3(x, 0, -y);
			tempGO.SetActive(true);
			PathPoints.Add (tempGO.GetComponent<PathPoint>());
			PathPoints[PathPoints.Count-1].PathPointType = GetTypePathPoint(_map, x, y);
		}
	}

	/// <summary>
	/// Находит ближайшую доступную ключевую точку
	/// </summary>
	void FindClosePathPoint (char[,] _pointsMap, PathPoint pp, Move.MoveState direction)
	{
		int dx=0, dy=0;
		int cx = (int) pp.transform.localPosition.x;
		int cy = - (int) pp.transform.localPosition.z;
		switch (direction)
		{
		case Move.MoveState.right:
			dx = 1;
			break;
		case Move.MoveState.down:
			dy = 1;
			break;
		default:
			break;
		}

		bool blockray = false;
		bool findPP = false;
		int raylength = 0;
		while (!blockray)
		{
			switch (_pointsMap[cx = ((cx + dx) % prm.CurrentMap.width), cy = ((cy + dy) % prm.CurrentMap.height)])
			{
			case 'X':
				raylength++;
				findPP = true;
				blockray = true;
				break;
			case '0':
				blockray = true;
				break;
			default:
				raylength++;
				break;
			}
		}
		if (findPP) 
		{
			PathPoint tempPP = PathPoints.Find(p => p.transform.localPosition == new Vector3 (cx, 0, -cy));
			switch (direction)
			{
			case Move.MoveState.right:
				pp.RightPoint = tempPP;
				tempPP.LeftPoint = pp;
				pp.DistanceToRight = raylength;
				tempPP.DistanceToLeft = raylength;
				break;		
			case Move.MoveState.down:
				pp.DownPoint = tempPP;
				tempPP.UpPoint = pp;
				pp.DistanceToDown = raylength;
				tempPP.DistanceToUp = raylength;
				break;
			default:
				break;
			}
		}
	}

	public static char[,] pointsMap;

	/// <summary>
	/// Процедура устанавливает взаимосвязи между ключевыми точками, высчитывает растояния между ними
	/// Также создаётся массив прохождения карты pointsMap, в котором проходные клетки обозначаются '*', непроходимый как '0', а ключевые точки как 'X'
	/// </summary>
	/// <param name="_map">_map.</param>
	void ConnectPathPoints (string[] _map)
	{
		pointsMap = new char[prm.CurrentMap.width, prm.CurrentMap.height];

		for (int y = 0; y<prm.CurrentMap.height; y++)
		{
			for (int x = 0; x<prm.CurrentMap.width; x++)
			{
				// Заменяем непроходимые объекты на '0', а проходимые на '*'
				pointsMap[x, y] = ("BCW".Contains(_map[y][x].ToString()) ? '0' : '*');

			}
		}

		prm.MaxPathPoints = PathPoints.Count;
		// Расставляем ключевые точки на карте
		foreach (PathPoint pp in PathPoints)
		{
			pointsMap[(int) (pp.transform.localPosition.x), -(int) (pp.transform.localPosition.z)] = 'X';
		}
	
		// Находим взаимосвязи
		foreach (PathPoint pp in PathPoints)
		{
			if (pp.PathPointType != PathPoint.PointType.pass)
			{
				FindClosePathPoint (pointsMap, pp, Move.MoveState.right);
				FindClosePathPoint (pointsMap, pp, Move.MoveState.down);
			}
		}
	}

	/// <summary>
	/// Обновляет количество жизней
	/// </summary>
	void UpdateLives ()
	{
		LivesSprite.width = prm.LivesCount * 44;
	}
	
	/// <summary>
	/// Добавляет очки
	/// </summary>
	public void AddScore (int score)
	{
		prm.Score += score;
		ScoreLabel.text = prm.Score.ToString();
	}

	/// <summary>
	/// Обновляет название карты и её порядковый номер
	/// </summary>
	void UpdateLevelGUI ()
	{
		try
		{
			TitleLabel.text = prm.CurrentMap.title;
			StageLabel.text = prm.CurrentLevel+" / "+prm.MaxLevel;
		}
		catch (UnityException ex)
		{
			Debug.LogWarning(ex.Message);
		}
	}

	/// <summary>
	/// Переход на следующий уровень
	/// </summary>
	public void NextLevel ()
	{
		if (prm.CurrentLevel++ <= prm.MaxLevel)
		{
			ShowGameMessage(MessageState.nextlevel);
		}
		else
		{
			EndGame();
			MainController.Instance.MainControl(MainController.TypeSystemActions.ShowEndGame);
		}
	}
	
	/// <summary>
	/// Показывает конец игры
	/// </summary>
	void EndGame ()
	{
		Time.timeScale = 0f;
		ScoreEndLabel.text = prm.Score.ToString();
		StageEndLabel.text = prm.CurrentLevel.ToString()+"/"+prm.MaxLevel.ToString();
	}

	/// <summary>
	/// Показывает выбранное игровое сообщение
	/// </summary>
	public void ShowGameMessage (MessageState _GameMessageState)
	{
		GameMessageState = _GameMessageState;
		GameMessageLabel.gameObject.SetActive(true);
		Time.timeScale = 0f;
		switch (_GameMessageState)
		{
		case MessageState.startgame:
			GameMessageLabel.text = "Press to start";
			break;
		case MessageState.nextlevel:
			GameMessageLabel.text = "YOU WIN!!!\nGo to\nNext Level";
			break;
		case MessageState.kill:
			GameMessageLabel.text = "YOU DIED\nSUCKER";
			break;
		}
	}

	/// <summary>
	/// Закрывает текущее информационное окно и выполняет соответствующие действия
	/// </summary>
	public void CloseMessage ()
	{
		GameMessageLabel.gameObject.SetActive(false);
		switch (GameMessageState)
		{
		case MessageState.startgame:
			Time.timeScale = 1f;
			break;
		case MessageState.nextlevel:
			prm.CurrentMap = MainController.Instance._LevelEditor.Maps[UnityEngine.Random.Range(0, MainController.Instance._LevelEditor.Maps.Count)];
			UpdateLevelGUI();
			ResetPlayers();
			LoadLevel ();
			foreach (Move m in FindObjectsOfType<Move>()) {m.CheckStartParams();}
			Time.timeScale = 1f;
			break;
		case MessageState.kill:
			Restart();
			break;
		}
		GameMessageLabel.gameObject.SetActive(false);
		GameMessageState = MessageState.none;
	}

	/// <summary>
	/// Сохраняем игру в файл
	/// </summary>
	public void SaveGame ()
	{
		PlayerPrefs.SetString("LastGameGUID", prm.CurrentMap.GUID);
		PlayerPrefs.SetInt("MaxLevel", prm.MaxLevel);
		PlayerPrefs.SetInt("LastLevel", prm.CurrentLevel);
		PlayerPrefs.SetInt("LastScore", prm.Score);
		PlayerPrefs.SetInt("LastLives", prm.LivesCount);
	}

	/// <summary>
	/// Загружаем игру из файла
	/// </summary>
	public void LoadGame ()
	{

		prm.MaxLevel =PlayerPrefs.GetInt("MaxLevel", 10);
		prm.CurrentLevel = PlayerPrefs.GetInt("LastLevel", 1);
		prm.Score = PlayerPrefs.GetInt("LastScore", 0);
		prm.LivesCount = PlayerPrefs.GetInt("LastLives", 3);

		prm.CurrentMap = MainController.Instance._LevelEditor.Maps.Find(m => m.GUID == PlayerPrefs.GetString("LastGameGUID", MainController.Instance._LevelEditor.Maps[0].GUID));
		UpdateLevelGUI();
		UpdateLives ();
		AddScore (0);
		LoadLevel ();
		foreach (Move m in FindObjectsOfType<Move>()) {m.CheckStartParams();}
		ShowGameMessage(MessageState.startgame);
	}

}
