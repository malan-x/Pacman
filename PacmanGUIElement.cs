using UnityEngine;
using System.Collections;

public class PacmanGUIElement : MonoBehaviour {

	public enum TypeElement {
		Clear=0, 			// Очистка
		Wall,				// Стена
		PacmanSpawn,		// Место респауна игрока
		Point, 				// Точки
		Energy, 			// Энергетик
		Fruit, 				// Фрукты
		BunkerCenter,		// Бункер с привидениями
		Blinky, 
		Pinky, 
		Inky, 
		Clyde
	}
	public TypeElement ElementType;

	void OnClick ()
	{
		switch (ElementType)
		{
		case TypeElement.Clear:
			prm.CurrentPen = LevelEditor.PenType.Clear;
			break;
		case TypeElement.Wall:
			prm.CurrentPen = LevelEditor.PenType.Wall;
			break;
		case TypeElement.PacmanSpawn:
			prm.CurrentPen = LevelEditor.PenType.PacmanSpawn;
			break;
		case TypeElement.Point:
			prm.CurrentPen = LevelEditor.PenType.Point;
			break;
		case TypeElement.Energy:
			prm.CurrentPen = LevelEditor.PenType.Energy;
			break;
		case TypeElement.Fruit:
			prm.CurrentPen = LevelEditor.PenType.Fruit;
			break;
		case TypeElement.BunkerCenter:
			prm.CurrentPen = LevelEditor.PenType.BunkerCenter;
			break;
		case TypeElement.Blinky:
			prm.CurrentMap.Blinky = this.GetComponent<UIToggle>().value;
			MainController.Instance._LevelEditor.Cells[(int) (prm.CurrentMap.Ghosts.x), (int) (prm.CurrentMap.Ghosts.y)-3].UpdateCell(prm.CurrentMap.Blinky ? LevelEditor.PenType.BlinkySpawn : LevelEditor.PenType.Clear);
			break;
		case TypeElement.Pinky:
			prm.CurrentMap.Pinky = this.GetComponent<UIToggle>().value;
			MainController.Instance._LevelEditor.Cells[(int) (prm.CurrentMap.Ghosts.x), (int) (prm.CurrentMap.Ghosts.y)-1].UpdateCell(prm.CurrentMap.Pinky ? LevelEditor.PenType.PinkySpawn : LevelEditor.PenType.Clear);
			break;
		case TypeElement.Inky:
			prm.CurrentMap.Inky = this.GetComponent<UIToggle>().value;
			MainController.Instance._LevelEditor.Cells[(int) (prm.CurrentMap.Ghosts.x)-1, (int) (prm.CurrentMap.Ghosts.y)-1].UpdateCell(prm.CurrentMap.Inky ? LevelEditor.PenType.InkySpawn : LevelEditor.PenType.Clear);
			break;
		case TypeElement.Clyde:
			prm.CurrentMap.Clyde = this.GetComponent<UIToggle>().value;
			MainController.Instance._LevelEditor.Cells[(int) (prm.CurrentMap.Ghosts.x)+1, (int) (prm.CurrentMap.Ghosts.y)-1].UpdateCell(prm.CurrentMap.Clyde ? LevelEditor.PenType.ClydeSpawn : LevelEditor.PenType.Clear);
			break;
		}
	}
}
