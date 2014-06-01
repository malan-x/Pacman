using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
/// <summary>
/// Клетка в редакторе уровней
/// </summary>
[RequireComponent (typeof (BoxCollider))]
[RequireComponent (typeof (UIImageButton))]
[RequireComponent (typeof (UISprite))]
public class Cell : MonoBehaviour {

	public int Xpos;						// Позиция клетки по горизонтали
	public int Ypos;						// Позиция клетки по вертикали
	public LevelEditor.PenType CellType;	// Тип клетки
	[HideInInspector] public UIImageButton ImageBtn;

	/// <summary>
	/// Возвращает название спрайта соответствующее типу клетки
	/// </summary>
	string GetSpriteName (LevelEditor.PenType _penType)
	{
		switch (_penType)
		{
		case LevelEditor.PenType.BunkerBricks:
			return "item_clear";
		case LevelEditor.PenType.BunkerCenter:
			return "item_bunker";
		case LevelEditor.PenType.BunkerClear:
			return "item_clear";
		case LevelEditor.PenType.BunkerDoor:
			return "item_clear";
		case LevelEditor.PenType.Clear:
			return "item_clear";
		case LevelEditor.PenType.Energy:
			return "item_powerup";
		case LevelEditor.PenType.Fruit:
			return "item_fruit";
		case LevelEditor.PenType.PacmanSpawn:
			return "item_pacmanspawn";
		case LevelEditor.PenType.Point:
			return "item_point";
		case LevelEditor.PenType.Wall:
			return "item_brick";
		case LevelEditor.PenType.BlinkySpawn:
			return "item_blinky";
		case LevelEditor.PenType.PinkySpawn:
			return "item_pinky";
		case LevelEditor.PenType.InkySpawn:
			return "item_inky";
		case LevelEditor.PenType.ClydeSpawn:
			return "item_clyde";
		default:
			return "item_clear";
		}

	}

	/// <summary>
	/// Обновляет клетку в зависимости от типа выбранного пера
	/// </summary>
	public void UpdatePen ()
	{
		ImageBtn.hoverSprite = GetSpriteName (prm.CurrentPen);
		ImageBtn.pressedSprite = GetSpriteName (prm.CurrentPen);
	}

	/// <summary>
	/// Обновляет положение и типа клетки
	/// </summary>
	public void UpdateCell()
	{
		transform.localPosition = new Vector3 (13+Xpos*25, -13-Ypos*25, 0);

		ImageBtn.target.spriteName = GetSpriteName (CellType);
		ImageBtn.normalSprite = GetSpriteName (CellType);
		ImageBtn.disabledSprite = GetSpriteName (CellType);
		UpdatePen ();
		ImageBtn.target.MakePixelPerfect();
	}

	/// <summary>
	/// Обновляет положение и типа клетки
	/// </summary>
	public void UpdateCell(LevelEditor.PenType _cellType)
	{
		CellType = _cellType;
		UpdateCell ();
	}

	/// <summary>
	/// Проверяет попадает ли новая клетка в бункер с привидениями
	/// </summary>
	bool CheckInBunker (LevelEditor.PenType _cellType)
	{
		int cx = (int) (prm.CurrentMap.Ghosts.x);
		int cy = (int) (prm.CurrentMap.Ghosts.y);
		return ((_cellType != LevelEditor.PenType.BunkerCenter)&&((cx-2<=Xpos)&&(cx+2>=Xpos)&&(cy-3<=Ypos)&&(cy>=Ypos)));
	}

	/// <summary>
	/// Устанавливает выбранный тип клетки
	/// </summary>
	public void SetUpCell (LevelEditor.PenType _cellType)
	{
		if (!CheckInBunker (_cellType))
		{
			switch (_cellType)
			{
			case LevelEditor.PenType.PacmanSpawn:	// Устанавливает клетку с пакманом, а предыдущее место затирает
				MainController.Instance._LevelEditor.Cells[(int) (prm.CurrentMap.PacmanSpawn.x), (int) (prm.CurrentMap.PacmanSpawn.y)].UpdateCell(LevelEditor.PenType.Clear);
				prm.CurrentMap.PacmanSpawn = new Vector2(Xpos, Ypos);
				UpdateCell(_cellType);
				break;
			case LevelEditor.PenType.BunkerCenter:	// Устанавливает клетку с бункером, а предыдущее место затирает
				int px = (int) (prm.CurrentMap.PacmanSpawn.x);
				int py = (int) (prm.CurrentMap.PacmanSpawn.y);
				if (((Xpos+2<prm.CurrentMap.width)&&(Xpos-2>=0)&&(Ypos<prm.CurrentMap.height)&&(Ypos-3>=0))&&	//входит ли в границы карты
				    ((Xpos+2<px)||(Xpos-2>=px)||(Ypos<py)||(Ypos-3>=py)))		//не пересекается ли с местом респауна игрока
				{
					int cx = (int) (prm.CurrentMap.Ghosts.x);
					int cy = (int) (prm.CurrentMap.Ghosts.y);

					for (int x = cx-2; x <= cx+2; x++)
					{
						for (int y = cy-3; y <= cy; y++)
						{
							MainController.Instance._LevelEditor.Cells[x, y].UpdateCell(LevelEditor.PenType.Clear);
						}
					}

					prm.CurrentMap.Ghosts = new Vector2(Xpos, Ypos);
					for (int x = Xpos-2; x <= Xpos+2; x++)
					{
						for (int y = Ypos-3; y <= Ypos; y++)
						{
							MainController.Instance._LevelEditor.Cells[x, y].UpdateCell(LevelEditor.PenType.Clear);
						}
					}				
					MainController.Instance._LevelEditor.Cells[Xpos, Ypos-3].UpdateCell(prm.CurrentMap.Blinky ? LevelEditor.PenType.BlinkySpawn : LevelEditor.PenType.Clear);
					MainController.Instance._LevelEditor.Cells[Xpos, Ypos-2].UpdateCell(LevelEditor.PenType.BunkerDoor);
					MainController.Instance._LevelEditor.Cells[Xpos, Ypos-1].UpdateCell(prm.CurrentMap.Pinky ? LevelEditor.PenType.PinkySpawn : LevelEditor.PenType.Clear);
					MainController.Instance._LevelEditor.Cells[Xpos-1, Ypos-1].UpdateCell(prm.CurrentMap.Inky ? LevelEditor.PenType.InkySpawn : LevelEditor.PenType.Clear);
					MainController.Instance._LevelEditor.Cells[Xpos+1, Ypos-1].UpdateCell(prm.CurrentMap.Clyde ? LevelEditor.PenType.ClydeSpawn : LevelEditor.PenType.Clear);
					MainController.Instance._LevelEditor.Cells[Xpos, Ypos].UpdateCell(LevelEditor.PenType.BunkerCenter);

					MainController.Instance._LevelEditor.Cells[Xpos-2, Ypos-2].UpdateCell(LevelEditor.PenType.BunkerBricks);
					MainController.Instance._LevelEditor.Cells[Xpos-1, Ypos-2].UpdateCell(LevelEditor.PenType.BunkerBricks);
					MainController.Instance._LevelEditor.Cells[Xpos+1, Ypos-2].UpdateCell(LevelEditor.PenType.BunkerBricks);
					MainController.Instance._LevelEditor.Cells[Xpos+2, Ypos-2].UpdateCell(LevelEditor.PenType.BunkerBricks);
					MainController.Instance._LevelEditor.Cells[Xpos-2, Ypos-1].UpdateCell(LevelEditor.PenType.BunkerBricks);
					MainController.Instance._LevelEditor.Cells[Xpos+2, Ypos-1].UpdateCell(LevelEditor.PenType.BunkerBricks);
					MainController.Instance._LevelEditor.Cells[Xpos-2, Ypos].UpdateCell(LevelEditor.PenType.BunkerBricks);
					MainController.Instance._LevelEditor.Cells[Xpos-1, Ypos].UpdateCell(LevelEditor.PenType.BunkerBricks);
					MainController.Instance._LevelEditor.Cells[Xpos+1, Ypos].UpdateCell(LevelEditor.PenType.BunkerBricks);
					MainController.Instance._LevelEditor.Cells[Xpos+2, Ypos].UpdateCell(LevelEditor.PenType.BunkerBricks);
					UpdateCell(_cellType);
				}
				break;
			default:
				if (CellType != LevelEditor.PenType.PacmanSpawn) {UpdateCell(_cellType);}
				break;
			}
			

		}
	}

	void OnClick ()
	{
		SetUpCell (prm.CurrentPen);
	}

	void OnHover (bool isOver)
	{
		UpdatePen ();
		if (isOver) 
		{
			ImageBtn.target.spriteName = GetSpriteName (prm.CurrentPen);
			if (prm.CurrentPen == LevelEditor.PenType.BunkerCenter)
			{
				ImageBtn.target.depth = 4;
			}
			ImageBtn.target.MakePixelPerfect();
		}
		else
		{
			ImageBtn.target.depth = 3;
		}
	}
	// Update is called once per frame
	void Update () {
		if (!Application.isPlaying) {UpdateCell();}
	}
}
