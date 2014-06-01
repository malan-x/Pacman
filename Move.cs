using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Скрипт отвечающий за передвижения персонажей
public class Move : MonoBehaviour {

	public enum MoveState {stay=0, left, right, up, down}	//Возможные направления движения персонажа
	public MoveState CurrentMoveStateUnit = MoveState.stay;
	public MoveState NextMoveStateUnit = MoveState.stay;

	public Vector3 TargetPoint;	// точка на карте, достигнув которой персонаж сможет поменять направление движения
	public float speed;			// скорость персонажа

	public enum ObjectType {Pacman=0, Blinky, Pinky, Inky, Clyde}	// Типы персонажей
	public ObjectType TypeObject;	// тип объекта (пакман или один из призраков)
	public float BuffSpeed;			// Коэффициент ускорения/замедления персонажа

	public bool CanChangeDirection=true;	// может ли объект менять направление движения (разрешается только на перекрестках)

	public PathPoint CurrentPathPoint;	// текущая ключевая точка

	public List<PathPoint> PathList;	//список пройденного пути
	public int MarkedPoint = 0;	//количество пройденных на карте уникальных ключевых точек
	
	// Use this for initialization
	void Start () {
	}
	
	/// <summary>
	/// Функция проверяет свои стартовые условия
	/// </summary>
	public void CheckStartParams ()
	{
		speed = (TypeObject == ObjectType.Pacman) ? prm.PacmanSpeed : prm.GhostSpeed;
		BuffSpeed = 1f;
		switch (TypeObject)
		{
		case ObjectType.Blinky:
			gameObject.SetActive(prm.CurrentMap.Blinky);
			break;
		case ObjectType.Pinky:
			gameObject.SetActive(prm.CurrentMap.Pinky);
			break;
		case ObjectType.Inky:
			gameObject.SetActive(prm.CurrentMap.Inky);
			break;
		case ObjectType.Clyde:
			gameObject.SetActive(prm.CurrentMap.Clyde);
			break;
		}
		MarkedPoint = 0;
		PathList.Clear();
	}

	/// <summary>
	/// Возвращает вектор движения
	/// </summary>
	Vector3 GetMoveVector ()
	{
		Vector3 V3 = Vector3.zero;
		switch (CurrentMoveStateUnit)
		{
		case MoveState.right:
			V3 = Vector3.right * Time.deltaTime;
			break;
		case MoveState.left:
			V3 = Vector3.left * Time.deltaTime;
			break;
		case MoveState.up:
			V3 = Vector3.forward * Time.deltaTime;
			break;
		case MoveState.down:
			V3 = Vector3.back * Time.deltaTime;
			break;
		case MoveState.stay:
			break;
		}
		V3 *= speed * BuffSpeed * prm.GameSpeed;

		// Если на этом кадре персонаж достигнет ключевой точки, то направление можно будет изменить
		if (Vector3.Distance(transform.position, TargetPoint) <= Vector3.Distance(Vector3.zero,V3))
		{
			V3 = TargetPoint - transform.position;
			CanChangeDirection = true;
		}

		return V3;
	}

	/// <summary>
	/// Возвращает ближайшую доступную в выбранном направлении точку
	/// </summary>
	public Vector3 GetTarget (MoveState direction)
	{
		if (CurrentPathPoint)
		{
			switch (direction)
			{
			case MoveState.left:
				return  (CurrentPathPoint.DistanceToLeft>0) ? CurrentPathPoint.LeftPoint.transform.position : transform.position;
			case MoveState.right:
				return  (CurrentPathPoint.DistanceToRight>0) ? CurrentPathPoint.RightPoint.transform.position : transform.position;
			case MoveState.up:
				return  (CurrentPathPoint.DistanceToUp>0) ? CurrentPathPoint.UpPoint.transform.position : transform.position;
			case MoveState.down:
				return  (CurrentPathPoint.DistanceToDown>0) ? CurrentPathPoint.DownPoint.transform.position : transform.position;
			}
			return transform.position;
		}
		else {return TargetPoint;}
	}

	/// <summary>
	/// Возвращает, ближайшую к заданному вектору, ключевую точку
	/// </summary>
	public PathPoint GetCloserPathPoint (Vector3 target)
	{
		PathPoint pp = null;
		target = new Vector3 (Mathf.RoundToInt(target.x), 0f, Mathf.RoundToInt(target.z));
		float d = 0, max = float.MaxValue;
		if (FindObjectOfType<Game>().PathPoints.Find(p => p.transform.localPosition == target)) {return FindObjectOfType<Game>().PathPoints.Find(p => p.transform.localPosition == target);}
		if ((d = Vector3.Distance(target, GetPathPoint(MoveState.left))) > 0) { if (d < max) {max = d; pp = FindObjectOfType<Game>().PathPoints.Find(p => p.transform.localPosition == GetPathPoint(MoveState.left));}}
		if ((d = Vector3.Distance(target, GetPathPoint(MoveState.right))) > 0) { if (d < max) {max = d; pp = FindObjectOfType<Game>().PathPoints.Find(p => p.transform.localPosition == GetPathPoint(MoveState.right));}}
		if ((d = Vector3.Distance(target, GetPathPoint(MoveState.up))) > 0) { if (d < max) {max = d; pp = FindObjectOfType<Game>().PathPoints.Find(p => p.transform.localPosition == GetPathPoint(MoveState.up));}}
		if ((d = Vector3.Distance(target, GetPathPoint(MoveState.down))) > 0) { if (d < max) {max = d; pp = FindObjectOfType<Game>().PathPoints.Find(p => p.transform.localPosition == GetPathPoint(MoveState.down));}}

		return pp;
	}

	/// <summary>
	/// Найти ближайшую ключевую точку в заданном направлении
	/// </summary>
	public Vector3 GetPathPoint (MoveState direction)
	{
		
		int x = (int) transform.localPosition.x;
		int y = - (int) transform.localPosition.z;
		int cx = x;
		int cy = y;
		int dx=0, dy=0;
		int max=0;
		
		switch (direction)
		{
		case Move.MoveState.left:
			max = prm.CurrentMap.width;
			dx = prm.CurrentMap.width - 1;
			break;
		case Move.MoveState.right:
			max = prm.CurrentMap.width;
			dx = 1;
			break;
		case Move.MoveState.down:
			max = prm.CurrentMap.height;
			dy = 1;
			break;
		case Move.MoveState.up:
			max = prm.CurrentMap.height;
			dy = prm.CurrentMap.height - 1;
			break;
		default:
			break;
		}
		
		bool blockray = false;
		int raylength = 0;
		while ((raylength<max)&&!blockray)
		{
			switch (Game.pointsMap[cx = ((cx + dx) % prm.CurrentMap.width), cy = ((cy + dy) % prm.CurrentMap.height)])
			{
			case 'X':
				return new Vector3(cx, 0, -cy);
			case '0':
				blockray = true;
				break;
			default:
				raylength++;
				break;
			}
		}
		return new Vector3(x, 0, -y);
	}


	/// <summary>
	/// Возвращает кратчайший путь
	/// </summary>
	public List<PathPoint> GetShortPath (List<List<PathPoint>> AllPathes, PathPoint beginPP, PathPoint endPP)
	{
		if (beginPP&&endPP&&(beginPP!=endPP)&&(AllPathes.Count>0)&&(AllPathes[0].Count>0))
		{
			List<PathPoint> SourceList = new List<PathPoint>();
			List<PathPoint> ShortList = new List<PathPoint>();
			foreach (List<PathPoint> lpp in AllPathes)
			{
				bool findbegin = false;
				bool findend = false;
				for (int i = lpp.Count-1; i >= 0; i--)
				{
					if (lpp[i] == beginPP)
					{
						findbegin = true;
						findend = false;
						SourceList.Clear();
						SourceList.Add(lpp[i]);
					}
					else if ((lpp[i] == endPP)&&(findbegin))
					{
						findend = true;
						findbegin = false;
						SourceList.Add(lpp[i]);
						SourceList = CutPath(SourceList);
						if ((GetPathLength(ShortList) == 0? int.MaxValue : GetPathLength(ShortList)) > GetPathLength(SourceList)) {ShortList.Clear(); ShortList.AddRange(SourceList);}
						SourceList.Clear();
					}
					else
					{
						if (findbegin) {SourceList.Add(lpp[i]);}
					}

				}
			}
			return ShortList;
		}
		else 
		{
			return null;
		}
	}

	/// <summary>
	/// Избавляет путь от петель
	/// </summary>
	List<PathPoint> CutPath (List<PathPoint> path)
	{
		List<PathPoint> shortpath = new List<PathPoint>();
		for (int i = 0; i < path.Count; i++)
		{
			shortpath.Add(path[i]);
			for (int j = i+1; j < path.Count; j++)
			{
				if (path[i] == path[j])
				{
					i = j;
					j = path.Count;
				}

			}
		}
		return shortpath;
	}

	/// <summary>
	/// Возвращает длину пути
	/// </summary>
	int GetPathLength (List<PathPoint> path)
	{
		int p = 0;
		for (int i = 0; i < path.Count-1; i++)
		{
			if (path[i].LeftPoint == path[i+1]) {p += path[i].DistanceToLeft;}
			else if (path[i].RightPoint == path[i+1]) {p += path[i].DistanceToRight;}
			else if (path[i].UpPoint == path[i+1]) {p += path[i].DistanceToUp;}
			else if (path[i].DownPoint == path[i+1]) {p += path[i].DistanceToDown;}
			else {return p;}
		}
		return p;
	}


	// Update is called once per frame
	public virtual void Update () {

		transform.Translate (GetMoveVector (), Space.World);

		// Проверяем выходит ли персонаж за границы
		switch (CurrentMoveStateUnit)
		{
		case MoveState.left:
			if (transform.position.x<0f) {transform.Translate (new Vector3 (((float) prm.CurrentMap.width)+transform.position.x, 0f, 0f), Space.World);}
			break;
		case MoveState.right:
			if (transform.position.x>prm.CurrentMap.width) {transform.Translate (new Vector3 (-((2* ((float) prm.CurrentMap.width))-transform.position.x), 0f, 0f), Space.World);}
			break;
		case MoveState.up:
			if (transform.position.z>0f) {transform.Translate (new Vector3 (0f, 0f, ((float) prm.CurrentMap.height)-transform.position.z), Space.World);}
			break;
		case MoveState.down:
			if (transform.position.z < -prm.CurrentMap.height) {transform.Translate (new Vector3 (0f, 0f, (2*(float) prm.CurrentMap.height)+transform.position.z), Space.World);}
			break;
		}
	}

}
