using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class LevelEditor : MonoBehaviour {

	// Типы кистей в редакторе уровней
	public enum PenType {
		Clear=0, 			// Очистка
		Wall,				// Стена
		PacmanSpawn,		// Место респауна игрока
		Point, 				// Точки
		Energy, 			// Энергетик
		Fruit, 				// Фрукты
		BunkerCenter,		// Бункер с привидениями (Центр)
		BunkerBricks,		// Бункер с привидениями (Стены)
		BunkerClear,		// Бункер с привидениями (Проход)
		BunkerDoor,			// Бункер с привидениями (Дверь)
		BlinkySpawn,		// Место респауна Блинки
		PinkySpawn,			// Место респауна Пинки
		InkySpawn,			// Место респауна Инки
		ClydeSpawn			// Место респауна Клайд
		
		
	}

	public Map CurrentMap; 	// Карта которая выбирается из списка карт

	public UISprite GridSprite;

	public GameObject CellPrefab;
	public Cell [,] Cells = new Cell[8,8];

	public GameObject MapPrefab;
	public List<Map> Maps;

	public UIInput InputMapTitle;
	public UIPopupList WidthPopup;
	public UIPopupList HeigthPopup;

	public PacmanGUIElement BlinkyButton;
	public PacmanGUIElement PinkyButton;
	public PacmanGUIElement InkyButton;
	public PacmanGUIElement ClydeButton;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}


	// Генерирует пустую карту
	public void GenerateClearMap ()
	{
		if (CurrentMap)
		{
			GridSprite.width = CurrentMap.width*25;
			GridSprite.height = CurrentMap.height*25;

			foreach (Cell c in Cells) {if (c) DestroyObject(c.gameObject);}
			Cells = new Cell[CurrentMap.width, CurrentMap.height];
			//Заполняем поле пустыми клетками
			for (int x = 0; x<CurrentMap.width; x++)
			{
				for (int y = 0; y<CurrentMap.height; y++)
				{
					GameObject tempGO = Instantiate (CellPrefab) as GameObject;
					Cells[x,y] = tempGO.GetComponent<Cell>();
					tempGO.transform.parent = CellPrefab.transform.parent;
					tempGO.transform.localScale = Vector3.one;
					tempGO.SetActive(true);
					tempGO.name = "Cell "+x.ToString()+" "+y.ToString();
					Cells[x,y].Xpos = x;
					Cells[x,y].Ypos = y;
					Cells[x,y].UpdateCell(PenType.Clear);
				}
			}

			//Добавляем бункер и место респауна игрока
			Cells[(int) (CurrentMap.PacmanSpawn.x), (int) (CurrentMap.PacmanSpawn.y)].SetUpCell(PenType.PacmanSpawn);
			Cells[(int) (CurrentMap.Ghosts.x), (int) (CurrentMap.Ghosts.y)].SetUpCell(PenType.BunkerCenter);

		}
	}

	// Возвращает новый объект в списке карт
	Map GetNewMapItem (string _title, string _guid, int _width, int _height)
	{
		GameObject tempMap = Instantiate (MapPrefab) as GameObject;
		CurrentMap = tempMap.GetComponent<Map>();
		//prm.CurrentMap = CurrentMap;
		Maps.Add (CurrentMap);
		tempMap.transform.parent = MapPrefab.transform.parent;
		tempMap.transform.localScale = Vector3.one;
		MapPrefab.transform.parent.GetComponent<UIGrid>().repositionNow = true;
		tempMap.SetActive(true);
		tempMap.name = "Map "+(Maps.Count).ToString();
		CurrentMap.title = _title;
		CurrentMap.GUID = _guid;
		CurrentMap.width = _width;
		CurrentMap.height = _height;
		return tempMap.GetComponent<Map>();
	}
	
	// Создать новую карту
	public void CreateMap ()
	{
		try
		{
			//Создаём объект в списке карт
			prm.CurrentMap = GetNewMapItem (InputMapTitle.value, System.Guid.NewGuid().ToString(), int.Parse(WidthPopup.value), int.Parse(HeigthPopup.value));

			//Генерируем пустую карту с расположенными бункером и местом респауна игрока
			GenerateClearMap();
			SaveMap ();
		}
		catch (UnityException ex)
		{
			NGUIDebug.print(ex.Message);
		}


	}
	
	// Сохранить карту
	// Записываем редактируемую карту в выбранный Map-скрипт
	public void SaveMap ()
	{
		if (CurrentMap)
		{
			prm.CurrentMap = CurrentMap;
			CurrentMap.width = Cells.GetUpperBound(0)+1;
			CurrentMap.height = Cells.GetUpperBound(1)+1;

			CurrentMap.map = new string[CurrentMap.height];

			//Заполняем массив строк значениями клеток
			for (int y = 0; y<CurrentMap.height; y++)
			{
				CurrentMap.map[y] = "";
				for (int x = 0; x<CurrentMap.width; x++)
				{
					CurrentMap.map[y] += CellToChar(Cells[x,y].CellType);
				}
			}
		}
		SaveMapsToXML("maps.xml");
	}

	// Функция возвращает символ клетки
	public char CellToChar (PenType _CellType)
	{
		switch (_CellType)
		{
		case PenType.BunkerBricks:
			return 'B';
		case PenType.BunkerCenter:
			return 'C';
		case PenType.BunkerClear:
			return '-';
		case PenType.BunkerDoor:
			return '=';
		case PenType.Clear:
			return '.';
		case PenType.Energy:
			return '+';
		case PenType.Fruit:
			return 'f';
		case PenType.PacmanSpawn:
			return 'P';
		case PenType.Point:
			return '*';
		case PenType.Wall:
			return 'W';
		case PenType.BlinkySpawn:
			return 'b';
		case PenType.PinkySpawn:
			return 'p';
		case PenType.InkySpawn:
			return 'i';
		case PenType.ClydeSpawn:
			return 'c';
		default:
			return '.';
		}
	}

	// Функция возвращает тип клетки соответствующий символу
	public PenType CharToCell (char _CellChar)
	{
		switch (_CellChar)
		{
		case 'B':
			return PenType.BunkerBricks;
		case 'C':
			return PenType.BunkerCenter;
		case '-':
			return PenType.BunkerClear;
		case '=':
			return PenType.BunkerDoor;
		case '.':
			return PenType.Clear;
		case '+':
			return PenType.Energy;
		case 'f':
			return PenType.Fruit;
		case 'P':
			return PenType.PacmanSpawn;
		case '*':
			return PenType.Point;
		case 'W':
			return PenType.Wall;
		case 'b':
			return PenType.BlinkySpawn;
		case 'p':
			return PenType.PinkySpawn;
		case 'i':
			return PenType.InkySpawn;
		case 'c':
			return PenType.ClydeSpawn;
		default:
			return PenType.Clear;
		}
	}

	// Загрузить карту в редактор
	public void LoadMap ()
	{
		if (CurrentMap)
		{
			prm.CurrentMap = CurrentMap;
			GridSprite.width = CurrentMap.width*25;
			GridSprite.height = CurrentMap.height*25;
			
			foreach (Cell c in Cells) {if (c) DestroyObject(c.gameObject);}
			Cells = new Cell[CurrentMap.width, CurrentMap.height];
			//Заполняем поле пустыми клетками
			for (int x = 0; x<CurrentMap.width; x++)
			{
				for (int y = 0; y<CurrentMap.height; y++)
				{
					GameObject tempGO = Instantiate (CellPrefab) as GameObject;
					Cells[x,y] = tempGO.GetComponent<Cell>();
					tempGO.transform.parent = CellPrefab.transform.parent;
					tempGO.transform.localScale = Vector3.one;
					tempGO.SetActive(true);
					tempGO.name = "Cell "+x.ToString()+" "+y.ToString();
					Cells[x,y].Xpos = x;
					Cells[x,y].Ypos = y;
					Cells[x,y].UpdateCell(CharToCell(CurrentMap.map[y][x]));
				}
			}

			//Обновляем состояние кнопок
			BlinkyButton.GetComponent<UIToggle>().value = prm.CurrentMap.Blinky;
			PinkyButton.GetComponent<UIToggle>().value = prm.CurrentMap.Pinky;
			InkyButton.GetComponent<UIToggle>().value = prm.CurrentMap.Inky;
			ClydeButton.GetComponent<UIToggle>().value = prm.CurrentMap.Clyde;
		}
	}
	
	// Удалить карту
	public void DeleteMap ()
	{
		if (CurrentMap) 
		{
			Maps.Remove(CurrentMap);
			DestroyObject(CurrentMap.gameObject);
			CurrentMap = null;
			MapPrefab.transform.parent.GetComponent<UIGrid>().repositionNow = true;
			SaveMapsToXML("maps.xml");
		}

	}

	// Записываем карты в файл
	void SaveMapsToXML(string storage)
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
		
		xmlDoc.Save (Path.Combine(Application.dataPath, storage));
	}


	// Чтение карт из файла
	public void ReadMapsFromXML(string storage)
	{
		if (File.Exists(Path.Combine(Application.dataPath, storage)))
		{
			XmlDocument xmlDoc = new XmlDocument ();     
			xmlDoc.Load(Path.Combine(Application.dataPath, storage));
			XmlNodeList levelList = xmlDoc.GetElementsByTagName ("level");
			
			if (Maps.Count>0) {foreach (Map m in Maps) {if (m) DestroyObject(m.gameObject);}}
			Maps.Clear();
			
			
			for (int i = 0; i < levelList.Count; i++)
			{
				string _guid = levelList[i].Attributes[0].Value;
				string _title = levelList[i].Attributes[1].Value;
				int _width = XmlConvert.ToInt32(levelList[i].Attributes[2].Value);
				int _height = XmlConvert.ToInt32 (levelList[i].Attributes[3].Value);
				CurrentMap = GetNewMapItem (_title, _guid, _width, _height);
				CurrentMap.map = levelList[i].Attributes[4].Value.Split ('/');
				CurrentMap.PacmanSpawn = new Vector2(XmlConvert.ToInt32(levelList[i].Attributes[5].Value), XmlConvert.ToInt32(levelList[i].Attributes[6].Value));
				CurrentMap.Ghosts = new Vector2(XmlConvert.ToInt32(levelList[i].Attributes[7].Value), XmlConvert.ToInt32(levelList[i].Attributes[8].Value));
				CurrentMap.Blinky = levelList[i].Attributes[9].Value == "True";
				CurrentMap.Pinky = levelList[i].Attributes[10].Value == "True";
				CurrentMap.Inky = levelList[i].Attributes[11].Value == "True";
				CurrentMap.Clyde = levelList[i].Attributes[12].Value == "True";
			}
		}
		
		
	}
	
	void OnEnable ()
	{
		ReadMapsFromXML("maps.xml");
	}
}
