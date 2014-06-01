using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour {

	public Camera GameCamera;
	public GameObject PrefabClear;
	public GameObject PrefabWall;
	public GameObject PrefabPoint;
	public GameObject PrefabPowerUp;
	public GameObject PrefabFruit;
	public GameObject PrefabDoor;

	public GameObject PrefabPathPoint;
	public List<PathPoint> PathPoints;

	public GameObject Pacman;
	public GameObject Blinky;
	public GameObject Pinky;
	public GameObject Inky;
	public GameObject Clyde;


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

	// Начинается новая игра, загружается карта, расставляются персонажи
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

	// Устанавливает камера над центром карты
	void SetUpCamera ()
	{
		GameCamera.transform.position = new Vector3 (-0.5f+(float)prm.CurrentMap.width/2f, 20f, 0.5f -(float)prm.CurrentMap.height/2f);
		GameCamera.fieldOfView = prm.CurrentMap.height*3 ;
	}

	// Сбрасываем характеристики персонажей
	void ResetPlayers ()
	{
		foreach (Move m in FindObjectsOfType<Move>())
		{
			m.CurrentPathPoint = null;
			m.CurrentMoveStateUnit = Move.MoveState.stay;
			m.NextMoveStateUnit = Move.MoveState.stay;
			m.CanChangeDirection = true;
			m.MarkedPoint = 0;
			m.PathList.Clear();
		}
	}

	// Рестарт уровня, из-за смерти персонажа или по собственному желанию. -1 жизнь
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
		/*---------- З А Г Л У Ш К А ----------*/
	}
	
	// Продожает ранее сохранённую игру
	public void ResumeGame ()
	{
		Time.timeScale = 1f;
		/*---------- З А Г Л У Ш К А ----------*/
	}
	
	// Ставит на паузу игру
	public void Pause ()
	{
		Time.timeScale = 0f;
		/*---------- З А Г Л У Ш К А ----------*/
	}

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

	// Возвращает принадлежность к неподвижным объектам
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



	//Создаёт нужный префаб в нужной клетке
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

			/*---------- З А Г Л У Ш К А ----------*/

		}
		
	}

	// Очищает список с объектами
	void ClearListObjects (List<GameObject> listGO)
	{
		if (listGO.Count>0)
		{
			listGO.ForEach (go => DestroyObject(go));
			listGO.Clear();
		}
	}

	// Очищает список с ключевыми точками
	void ClearPathPoints ()
	{
		if (PathPoints.Count>0)
		{
			PathPoints.ForEach (go => DestroyObject(go.gameObject));
			PathPoints.Clear();
		}
	}

	public List<GameObject> Blocks;	// Список клеток карты



	// Загружает выранный уровень
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
		ConnectPathPoints (prm.CurrentMap.map);
		SetUpCamera ();
		StaticBatchingUtility.Combine(gameObject);
		/*---------- З А Г Л У Ш К А ----------*/
	}

	// Можно ли двигаться по этой клетке
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

	// Возвращает тип ключевой точки
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



	// Процедура проверяет является ли точка ключевой и в зависимости от результата создаёт объект
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

	// Находит ближайшую доступную ключевую точку
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

	// Обновляет количество жизней
	void UpdateLives ()
	{
		LivesSprite.width = prm.LivesCount * 44;
	}
	
	// Добавляет очки
	public void AddScore (int score)
	{
		prm.Score += score;
		ScoreLabel.text = prm.Score.ToString();
	}

	// Обновляет название карты и её порядковый номер
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

	// Переход на следующий уровень
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
		/*---------- З А Г Л У Ш К А ----------*/
	}
	
	// Показывает конец игры
	void EndGame ()
	{
		Time.timeScale = 0f;
		ScoreEndLabel.text = prm.Score.ToString();
		StageEndLabel.text = prm.CurrentLevel.ToString()+"/"+prm.MaxLevel.ToString();
		/*---------- З А Г Л У Ш К А ----------*/
	}

	// Показывает выбранное игровое сообщение
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

	// Закрывает текущее информационное окно и выполняет соответствующие действия
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

	/*XmlAttribute GetXmlAttr (XmlDocument doc, string TitleAttr)
	{
		XmlAttribute TmpAttr = doc.CreateAttribute(TitleAttr);
		return
	}*/

	// Сохраняем игру в файл
	/*public void SaveGame ()
	{



			// Создаём файловое хранилище
			XmlDocument xmlDoc = new XmlDocument ();
			XmlNode rootNode = null;
			rootNode = xmlDoc.CreateElement ("data");
			xmlDoc.AppendChild (rootNode);
			
			//XmlNodeList levelList = xmlDoc.GetElementsByTagName ("level");
			
			foreach (Map m in Maps)
			{
				// Запись в файл
				XmlElement elmNew = xmlDoc.CreateElement ("level");
				XmlAttribute GUID = xmlDoc.CreateAttribute ("GUID");
				XmlAttribute TITLE = xmlDoc.CreateAttribute ("TITLE");
				XmlAttribute WIDTH = xmlDoc.CreateAttribute ("WIDTH");
				XmlAttribute HEIGHT = xmlDoc.CreateAttribute ("HEIGHT");
				XmlAttribute MAP = xmlDoc.CreateAttribute ("MAP");
				
				XmlAttribute PacmanSpawnX = xmlDoc.CreateAttribute ("PacmanSpawnX");
				XmlAttribute PacmanSpawnY = xmlDoc.CreateAttribute ("PacmanSpawnY");
				XmlAttribute GhostsX = xmlDoc.CreateAttribute ("GhostsX");
				XmlAttribute GhostsY = xmlDoc.CreateAttribute ("GhostsY");
				XmlAttribute Blinky = xmlDoc.CreateAttribute ("Blinky");
				XmlAttribute Pinky = xmlDoc.CreateAttribute ("Pinky");
				XmlAttribute Inky = xmlDoc.CreateAttribute ("Inky");
				XmlAttribute Clyde = xmlDoc.CreateAttribute ("Clyde");
				
				GUID.Value = m.GUID;
				TITLE.Value = m.title;
				WIDTH.Value = m.width.ToString();
				HEIGHT.Value = m.height.ToString();
				MAP.Value = string.Join("/", m.map);
				
				PacmanSpawnX.Value = ((int) (m.PacmanSpawn.x)).ToString();
				PacmanSpawnY.Value = ((int) (m.PacmanSpawn.y)).ToString();
				GhostsX.Value = ((int) (m.Ghosts.x)).ToString();
				GhostsY.Value = ((int) (m.Ghosts.y)).ToString();
				Blinky.Value = m.Blinky.ToString();
				Pinky.Value = m.Pinky.ToString();
				Inky.Value = m.Inky.ToString();
				Clyde.Value = m.Clyde.ToString();
				
				elmNew.SetAttributeNode (GUID);
				elmNew.SetAttributeNode (TITLE);
				elmNew.SetAttributeNode (WIDTH);
				elmNew.SetAttributeNode (HEIGHT);
				elmNew.SetAttributeNode (MAP);
				
				elmNew.SetAttributeNode (PacmanSpawnX);
				elmNew.SetAttributeNode (PacmanSpawnY);
				elmNew.SetAttributeNode (GhostsX);
				elmNew.SetAttributeNode (GhostsY);
				elmNew.SetAttributeNode (Blinky);
				elmNew.SetAttributeNode (Pinky);
				elmNew.SetAttributeNode (Inky);
				elmNew.SetAttributeNode (Clyde);			
				
				rootNode.AppendChild (elmNew);
			} 
			
			xmlDoc.Save (Path.Combine(Application.dataPath, "game.sav"));
		
	}*/

	// Загружаем игру из файла
	public void LoadGame ()
	{
		
	}


	// Use this for initialization
	void Start () {
	
	}


	// Update is called once per frame
	void Update () {

	
	}
}
