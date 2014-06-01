using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainController : Singleton<MainController> {

	// Список Экранов
	public enum TypeScreens {
		StartMenu=0, 		//1.0
		Game,				//2.0
		PauseGameMenu,		//2.1
		Options,			//3.0
		LevelEditor, 		//4.0
		LevelEditorMenu, 	//4.1
		NewLevelMenu, 		//4.2
		EndGameMenu			//2.2
	}

	// Текущий экран
	public static TypeScreens CurrentScreen;

	public List<GameObject> StartMenuList;
	public List<GameObject> GameList;
	public List<GameObject> PauseGameMenuList;
	public List<GameObject> OptionsList;
	public List<GameObject> LevelEditorList;
	public List<GameObject> LevelEditorMenuList;
	public List<GameObject> NewLevelMenuList;
	public List<GameObject> EndGameMenuList;

	
	// Список основных системных событий
	public enum TypeSystemActions {
		ShowStartMenu, 		// Показать стартовое меню
		StartGame,			// Начать новую игру
		LoadGame,			// Загрузить игру
		ShowGameScreen, 	// Показать игровой экран
		SoundTurn, 			// Включить/выключить звук
		SaveGame, 			// Сохранить текущую игру
		Close, 				// Закрыть окно
		BackToStartMenu, 	// Вернуться в стартовое меню
		ShowOptions, 		// Показать меню настроек
		ShowLevelEditor, 	// Показать экран редактора уровней
		ShowLevelEditorMenu,// Показать меню управлению картами на экране редактора
		ShowNewMapMenu, 	// Показать меню созданию новго уровня
		Restart,			// Рестарт
		CreateMap,			// Создать новую карту
		SaveMap,			// Сохранить карту
		LoadMap,			// Загрузить карту
		DeleteMap,			// Удалить карту
		PauseGame,			// Остановить игру и показать экран паузы
		ResumeGame,			// Продолжить игру
		ShowEndGame,		// Показать меню окончания игры
		None
	}

	// Переключатели привидений
	public UIToggle BlinkyToggle;
	public UIToggle PinkyToggle;
	public UIToggle InkyToggle;
	public UIToggle ClydeToggle;

	public UISlider SoundSlider;
	public UISlider MusicSlider;
	public UISlider SpeedSlider;
	public UISlider TimeSlider;


	//Список всех действий
	public List<TypeSystemActions> ListActions = new List<TypeSystemActions>();

	// Список активных элементов GUI
	public List<GameObject> ListActiveGUIGO = new List<GameObject>();

	[HideInInspector] public LevelEditor _LevelEditor;

	// Use this for initialization
	void Start () {
		MainControl(TypeSystemActions.ShowStartMenu);
		_LevelEditor.ReadMapsFromXML("maps.xml");

		// Устанавливаем значения настроек
		SetOptionsValues();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// Устанавливаем сохранённые настройки
	void SetOptionsValues ()
	{
		BlinkyToggle.value = prm.ShowBlinky = PlayerPrefs.GetInt("ShowBlinky", 1)==1;
		PinkyToggle.value = prm.ShowPinky = PlayerPrefs.GetInt("ShowPinky", 1)==1;
		InkyToggle.value = prm.ShowInky = PlayerPrefs.GetInt("ShowInky", 1)==1;
		ClydeToggle.value = prm.ShowClyde = PlayerPrefs.GetInt("ShowClyde", 1)==1;

		SoundSlider.value = prm.SoundVolume = PlayerPrefs.GetFloat("SoundVolume", 1f);
		MusicSlider.value = prm.MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
		TimeSlider.value = prm.PacmanSpeed = PlayerPrefs.GetFloat("PacmanSpeed", 2f)-1f;
		SpeedSlider.value = prm.GameSpeed = PlayerPrefs.GetFloat("GameSpeed", 1f);
		 
	}

	// Обновляем настройки
	void GetOptionsValues ()
	{
		PlayerPrefs.SetInt("ShowBlinky", (prm.ShowBlinky = BlinkyToggle.value)?1:0);
		PlayerPrefs.SetInt("ShowPinky", (prm.ShowPinky = PinkyToggle.value)?1:0);
		PlayerPrefs.SetInt("ShowInky", (prm.ShowInky = InkyToggle.value)?1:0);
		PlayerPrefs.SetInt("ShowClyde", (prm.ShowClyde = ClydeToggle.value)?1:0);

		PlayerPrefs.SetFloat("SoundVolume", prm.SoundVolume = SoundSlider.value);
		PlayerPrefs.SetFloat("MusicVolume", prm.MusicVolume = MusicSlider.value);
		PlayerPrefs.SetFloat("PacmanSpeed", prm.PacmanSpeed = TimeSlider.value + 1f);
		PlayerPrefs.SetFloat("GameSpeed", prm.GameSpeed = SpeedSlider.value);
	}

	// Процедура возвращающая событие, из заданного набора, которое произошло раньше остальных
	public TypeSystemActions FirstPreviousTSA(TypeSystemActions[] TSAr)
	{
		for (int i = ListActions.Count-1; i>=0; i--) 
		{
			for (int n=0; n<TSAr.Length; n++) {if (ListActions[i]==TSAr[n]) {return TSAr[n];}}
		}
		return TypeSystemActions.None;
	}

	// Главная управляющая процедура
	public void MainControl(TypeSystemActions TSA)
	{
		// Список действий пользователя
		ListActions.Add (TSA);
		switch (TSA) 
		{
		case TypeSystemActions.ShowStartMenu:
			SetScreen(TypeScreens.StartMenu);
			break;
		case TypeSystemActions.StartGame:
			SetScreen(TypeScreens.Game);
			this.GetComponent<Game>().StartNewGame();
			/*---------- З А Г Л У Ш К А ----------*/
			break;
		case TypeSystemActions.LoadGame:
			SetScreen(TypeScreens.Game);
			/*---------- З А Г Л У Ш К А ----------*/
			break;
		case TypeSystemActions.ShowGameScreen:
			SetScreen(TypeScreens.Game);
			break;
		case TypeSystemActions.SoundTurn:
			/*---------- З А Г Л У Ш К А ----------*/
			break;
		case TypeSystemActions.SaveGame:
			/*---------- З А Г Л У Ш К А ----------*/
			break;
		case TypeSystemActions.Close:
			/*---------- З А Г Л У Ш К А ----------*/
			switch (CurrentScreen)
			{
			case TypeScreens.LevelEditor:

				break;
			}
			break;
		case TypeSystemActions.BackToStartMenu:
			if (ListActions[ListActions.Count-2] == TypeSystemActions.ShowOptions) {GetOptionsValues();}
			SetScreen(TypeScreens.StartMenu);
			/*---------- З А Г Л У Ш К А ----------*/
			break;
		case TypeSystemActions.ShowOptions:
			SetScreen(TypeScreens.Options);
			break;
		case TypeSystemActions.ShowLevelEditor:
			SetScreen(TypeScreens.LevelEditor);
			break;
		case TypeSystemActions.ShowLevelEditorMenu:
			SetScreen(TypeScreens.LevelEditorMenu);
			break;
		case TypeSystemActions.ShowNewMapMenu:
			SetScreen(TypeScreens.NewLevelMenu);
			break;
		case TypeSystemActions.Restart:

			SetScreen(TypeScreens.Game);
			this.GetComponent<Game>().Restart();
			/*---------- З А Г Л У Ш К А ----------*/
			break;		
		case TypeSystemActions.PauseGame:
			if (CurrentScreen == TypeScreens.Game)
			{
				SetScreen(TypeScreens.PauseGameMenu);
				this.GetComponent<Game>().Pause();
			}
			else
			{
				SetScreen(TypeScreens.Game);
				this.GetComponent<Game>().ResumeGame();
			}
			/*---------- З А Г Л У Ш К А ----------*/
			break;
		case TypeSystemActions.ResumeGame:
			SetScreen(TypeScreens.Game);
			/*---------- З А Г Л У Ш К А ----------*/
			break;
		case TypeSystemActions.CreateMap:
			_LevelEditor.CreateMap();
			SetScreen(TypeScreens.LevelEditor);
			break;
		case TypeSystemActions.SaveMap:
			_LevelEditor.SaveMap();
			break;
		case TypeSystemActions.LoadMap:
			_LevelEditor.LoadMap();
			SetScreen(TypeScreens.LevelEditor);
		break;
		case TypeSystemActions.DeleteMap:
			_LevelEditor.DeleteMap();
			break;
		case TypeSystemActions.ShowEndGame:
			SetScreen(TypeScreens.EndGameMenu);
			break;

		}
	}

	// Установка выбранного экрана
	public void SetScreen(TypeScreens s)
	{
		CurrentScreen = s;
		switch (s) 
		{
		case TypeScreens.StartMenu: 			//1.0
			ChangeScreen(StartMenuList, true);
			break;
		case TypeScreens.Game: 					//2.0
			ChangeScreen(GameList, true);
			break;
		case TypeScreens.PauseGameMenu: 		//2.1
			ChangeScreen(PauseGameMenuList, false);
			break;
		case TypeScreens.EndGameMenu: 			//2.2
			ChangeScreen(EndGameMenuList, true);
			break;
		case TypeScreens.Options: 				//3.0
			ChangeScreen(OptionsList, true);
			break;
		case TypeScreens.LevelEditor: 			//4.0
			ChangeScreen(LevelEditorList, true);
			// Показываем карту только если она выбрана
			_LevelEditor.GridSprite.gameObject.SetActive(prm.CurrentMap);
			break;
		case TypeScreens.LevelEditorMenu: 		//4.1
			ChangeScreen(LevelEditorMenuList, false);
			NewLevelMenuList.ForEach(go => go.SetActive(false));
			break;
		case TypeScreens.NewLevelMenu: 			//4.2
			ChangeScreen(NewLevelMenuList, false);
			LevelEditorMenuList.ForEach(go => go.SetActive(false));
			break;
		}
	}

	// Отключает активные объекты GUI (при clear == true) и активирует объекты из списка templist
	void ChangeScreen (List<GameObject> templist, bool clear)
	{
		if (clear)
		{
			ListActiveGUIGO.ForEach(go => go.SetActive(false));
			ListActiveGUIGO.Clear();
		}
		ListActiveGUIGO.AddRange(templist);
		ListActiveGUIGO.ForEach(go => go.SetActive(true));
	}

}
