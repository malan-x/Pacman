using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GhostAI : Move {

	public enum ModeBehavior {chase=0, around, outrun, backtobase}	// режимы привидения 

	public float TimeFear=0f;

	public ModeBehavior BehaviorMode = ModeBehavior.around;

	public Vector3 RespawnPoint;		// Ключевая точка, в которой персонаж возрождается
	public SkinnedMeshRenderer rend;

	public List<PathPoint> CurrentPath = new List<PathPoint>();


	// Use this for initialization
	void Start () {
		//StaticBatchingUtility.Combine(this.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		if (CanChangeDirection)
		{
			NextMoveStateUnit = GetDirection(TypeObject);
			TargetPoint = GetTarget(NextMoveStateUnit);
			if (Vector3.Distance(TargetPoint, transform.position) >= 0.5f) {CurrentMoveStateUnit = (NextMoveStateUnit == MoveState.stay) ? CurrentMoveStateUnit : NextMoveStateUnit;}
			CanChangeDirection = false;
		}

		// Проверка не кончился ли режим испуга
		if (TimeFear>0f)
		{
			if  ((TimeFear -= Time.deltaTime)<=0f) {ChangeMode(ModeBehavior.around);}
		}
		
		base.Update();

	}

	/// <summary>
	/// Возвращает все известные пути
	/// </summary>
	List<List<PathPoint>> GetAllPathes ()
	{
		List<List<PathPoint>> alp = new List<List<PathPoint>>();
		foreach (Move m in FindObjectsOfType<Move>())
		{
			alp.Add(m.PathList);
		}
		return alp;
	}

	/// <summary>
	/// Меняем режим поведения привидения
	/// </summary>
	public void ChangeMode (ModeBehavior NewMode)
	{
		BehaviorMode = NewMode;
		TimeFear = 0f;
		switch (NewMode)
		{
		case ModeBehavior.outrun:
			// цвет привидения меняется на испуганный
			TimeFear = prm.MaxTimeFear;
			rend.materials[0].color = new Color32 (30, 0, 255, 255);
			break;
		case ModeBehavior.backtobase:
			// привидение становится прозрачным и возвращается на базу
			rend.materials[0].shader = Shader.Find("Mobile/Particles/Additive");
			BuffSpeed = 2f;
			// Вычисляем путь до респауна
			CurrentPath = GetShortPath (GetAllPathes() , CurrentPathPoint, GetCloserPathPoint(RespawnPoint));

			break;
		default:
			// Цвет привидения становится нормальным
			rend.materials[0].shader = Shader.Find("Toon/Basic");
			rend.materials[0].color = new Color32 (128, 128, 128, 255);
			BuffSpeed = 1f;
			break;
		}
		/*---------- З А Г Л У Ш К А ----------*/
	}

	// Поиск конечной цели в зависимости от типа АИ
	// ДОПИСАТЬ
	/*PathPoint GetAim (ObjectType TypeAI)
	{
		switch (TypeAI)
		{
			
		default:
			return ;
		}
	}*/

	//Процедура поиска путей

	/// <summary>
	/// Возвращает направление до указанного вектора
	/// </summary>
	MoveState GetDirectionToPoint (Vector3 target)
	{

		int x = (int) transform.localPosition.x;
		int y = - (int) transform.localPosition.z;
		int dx = (int) target.x;
		int dy = - (int) target.z;

		if (transform.position.z == target.z)
		{
			// Проверяем встречаются ли преграды на пути сперва слева направо, потом наоборот
			for (int cx = (x + 1) % prm.CurrentMap.width ; cx != x; x = ((x + 1) % prm.CurrentMap.width)) 
			{
				if (Game.pointsMap[cx, y] == 'X') {cx = x;}
				else if (cx == dx) {return MoveState.right;}
			}
			for (int cx = (x + prm.CurrentMap.width - 1) % prm.CurrentMap.width ; cx != x; x = ((x + prm.CurrentMap.width - 1) % prm.CurrentMap.width)) 
			{
				if (Game.pointsMap[cx, y] == 'X') {cx = x;}
				else if (cx == dx) {return MoveState.left;}
			}
		}
		else if (transform.position.x == target.x)
		{
			// Проверяем встречаются ли преграды на пути сперва сверху вниз, потом наоборот
			for (int cy = (y + 1) % prm.CurrentMap.height ; cy != y; y = ((y + 1) % prm.CurrentMap.height)) 
			{
				if (Game.pointsMap[x, cy] == 'X') {cy = y;}
				else if (cy == dy) {return MoveState.down;}
			}
			for (int cy = (y + prm.CurrentMap.height - 1) % prm.CurrentMap.height ; cy != y; y = ((y + prm.CurrentMap.height - 1) % prm.CurrentMap.height)) 
			{
				if (Game.pointsMap[x, cy] == 'X') {cy = y;}
				else if (cy == dy) {return MoveState.up;}
			}
		}
		return MoveState.stay;
	}

	/// <summary>
	/// Возвращает направление с помощью которого можно попасть из одной ключевой точки в другую
	/// </summary>
	MoveState GetDirectionFrom2PathPoints (PathPoint fromPoint, PathPoint toPoint)
	{
		if (fromPoint.LeftPoint == toPoint) {return MoveState.left;}
		else if (fromPoint.RightPoint == toPoint) {return MoveState.right;}
		else if (fromPoint.UpPoint == toPoint) {return MoveState.up;}
		else if (fromPoint.DownPoint == toPoint) {return MoveState.down;}
		else return MoveState.stay;
	}


	/// <summary>
	/// Возвращает желаемое направление если оно ещё не было пройдено, либо возвращает другое не пройденное направление, либо случайное из возможных
	/// </summary>
	MoveState GetNotMarkedDirection (MoveState direction)
	{
		if (CurrentPathPoint)
		{
			List<MoveState> marked = new List<MoveState>();
			List<MoveState> unmarked = new List<MoveState>();
			if (CurrentPathPoint.LeftPoint) {((PathList.Exists(d => d == CurrentPathPoint.LeftPoint))? marked : unmarked).Add(MoveState.left);}
			if (CurrentPathPoint.RightPoint) {((PathList.Exists(d => d == CurrentPathPoint.RightPoint))? marked : unmarked).Add(MoveState.right);}
			if (CurrentPathPoint.UpPoint) {((PathList.Exists(d => d == CurrentPathPoint.UpPoint))? marked : unmarked).Add(MoveState.up);}
			if (CurrentPathPoint.DownPoint) {((PathList.Exists(d => d == CurrentPathPoint.DownPoint))? marked : unmarked).Add(MoveState.down);}

			if (unmarked.Exists (d => d == direction)) {MarkedPoint++; return direction;}
			else if (unmarked.Count > 0) {MarkedPoint++; return unmarked[UnityEngine.Random.Range(0, unmarked.Count)];}
			else if (MarkedPoint >= prm.MaxPathPoints) {ChangeMode(ModeBehavior.chase); return GetDirection(TypeObject)	;}
			else if (marked.Count > 0) {return marked[UnityEngine.Random.Range(0, marked.Count)];}
			else {return MoveState.stay;}
		}
		else {return MoveState.stay;}
	}
		
	/// <summary>
	/// Возвращает наиболее предпочтительное направление движения
	/// </summary>
	MoveState GetOutrunDirection (MoveState firstDirection, MoveState secondDirection)
	{
		List<MoveState> temp = new List<MoveState>();
		if (CurrentPathPoint.LeftPoint) {temp.Add(MoveState.left);}
		if (CurrentPathPoint.RightPoint) {temp.Add(MoveState.right);}
		if (CurrentPathPoint.UpPoint) {temp.Add(MoveState.up);}
		if (CurrentPathPoint.DownPoint) {temp.Add(MoveState.down);}

		if (temp.Count==0) {return MoveState.stay;}
		else if (temp.Exists(direct => direct == firstDirection)) {return firstDirection;}
		else if (temp.Exists(direct => direct == secondDirection)) {return secondDirection;}
		else {return temp[UnityEngine.Random.Range(0, temp.Count)];}
	}

	/// <summary>
	/// Выбирает направление движения в зависимости от логики бота
	/// </summary>
	public MoveState GetDirection (ObjectType TypeAI)
	{
		switch (BehaviorMode)
		{
		case ModeBehavior.backtobase:	// Привидение возвращается в бункер где сможет снова восстановиться
			if (RespawnPoint == transform.position)
			{
				ChangeMode (ModeBehavior.around);
				return GetDirection (TypeAI);
			}
			else if (CurrentPath.Count > 0)
			{
				// Заменить на GetDirectionFromPathPointToathPoint
				MoveState ms = GetDirectionFrom2PathPoints (CurrentPathPoint, CurrentPath[0]);
				CurrentPath.RemoveAt(0);
				return ms;
			}
			else return GetDirectionToPoint(RespawnPoint);
			return MoveState.stay;
		case ModeBehavior.outrun:	// В этом режиме каждое привидение старается забиться в свой угол
			switch (TypeAI)
			{
			case ObjectType.Blinky:	// в левый верхний угол
				return GetOutrunDirection(MoveState.left, MoveState.up);
			case ObjectType.Pinky:	// в правый верхний угол
				return GetOutrunDirection(MoveState.right, MoveState.up);
			case ObjectType.Inky:	// в левый нижний угол
				return GetOutrunDirection(MoveState.left, MoveState.down);
			case ObjectType.Clyde:	// в правый нижний угол
				return GetOutrunDirection(MoveState.right, MoveState.down);
			default:
				return 	MoveState.stay;
			}
		case ModeBehavior.around:	// Привидение исследует лабиринт и старается выбирать те точки где его ещё не было
			switch (TypeAI)
			{
			case ObjectType.Blinky:	// в левый верхний угол
				return GetNotMarkedDirection(MoveState.up);
			case ObjectType.Pinky:	// в правый верхний угол
				return GetNotMarkedDirection(MoveState.right);
			case ObjectType.Inky:	// в левый нижний угол
				return GetNotMarkedDirection(MoveState.down);
			case ObjectType.Clyde:	// в правый нижний угол
				return GetNotMarkedDirection(MoveState.left);
			default:
				return 	MoveState.stay;
			}
		case ModeBehavior.chase:	// Преследует игрока
			switch (TypeAI)
			{
			case ObjectType.Blinky:
				CurrentPath = GetShortPath (GetAllPathes() , CurrentPathPoint, GetCloserPathPoint(FindObjectOfType<Pacman>().TargetPoint));
				return GetDirectionFrom2PathPoints(CurrentPath[0], CurrentPath[1]);
			case ObjectType.Pinky:
				CurrentPath = GetShortPath (GetAllPathes() , CurrentPathPoint, GetCloserPathPoint(FindObjectOfType<Pacman>().SourceTarget));
				return GetDirectionFrom2PathPoints(CurrentPath[0], CurrentPath[1]);
			case ObjectType.Inky:
				CurrentPath = GetShortPath (GetAllPathes() , CurrentPathPoint, GetCloserPathPoint(FindObjectOfType<Pacman>().transform.position));
				return GetDirectionFrom2PathPoints(CurrentPath[0], CurrentPath[1]);
			default:
				return 	(UnityEngine.Random.Range(0,4)<1 ? MoveState.left :
				       	(UnityEngine.Random.Range(0,4)<2 ? MoveState.right:
				 		(UnityEngine.Random.Range(0,4)<3 ? MoveState.up : MoveState.down)));
			}
			break;
		}
		return MoveState.stay;
	}
}
