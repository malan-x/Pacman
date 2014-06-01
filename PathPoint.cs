using UnityEngine;
using System.Collections;

/// <summary>
/// Скрипт отвечающий за поиск путей
/// Объект с этим скриптом будет располагаться на каждой развилке или тупике
/// </summary>
[RequireComponent (typeof(SphereCollider))]
public class PathPoint : MonoBehaviour {

	public enum PointType {pass =0, deadlock, fork}
	public PointType PathPointType;

	public PathPoint LeftPoint;
	public PathPoint RightPoint;
	public PathPoint UpPoint;
	public PathPoint DownPoint;

	public int DistanceToLeft=0;
	public int DistanceToRight=0;
	public int DistanceToUp=0;
	public int DistanceToDown=0;


	/// <summary>
	/// Автоматически становится текущей ключевой точке у персонажа и заносится в общий список пройденных точек
	/// </summary>
	void OnTriggerEnter (Collider other)
	{	
		if (other.tag=="PacMan"||other.tag=="Ghosts") 
		{
			other.GetComponent<Move>().CurrentPathPoint = this;
			other.GetComponent<Move>().PathList.Add(this);

		}
	}
}
