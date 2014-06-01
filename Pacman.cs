using UnityEngine;
using System.Collections;

/// <summary>
/// Скрипт отвечающий за управление игроком
/// </summary>
[RequireComponent (typeof(SphereCollider))]
public class Pacman : Move {

	public Vector3 SourceTarget;

	void Start()
	{
		InputSystem.Instance.eventOnSlideBegin += OnSlideBegin;
	}


	/// <summary>
	/// Управление с помощью жестов
	/// </summary>
	public void OnSlideBegin(Vector3 beginPosition, Vector3 currentPosition) {
		
		Vector3 direction = (currentPosition - beginPosition).normalized;
		float angleToUp = Vector3.Angle(direction, Vector3.up);
		float angleToLeft = Vector3.Angle(direction, Vector3.left);
		
		
			if(angleToUp <= 45) {
				NextMoveStateUnit = MoveState.up;
			} 
			else {
				if(angleToUp >= 135) {
					NextMoveStateUnit = MoveState.down;
				}
				else {
					if(angleToLeft <= 45) {
						NextMoveStateUnit = MoveState.left;
					}
					else {
						if(angleToLeft >= 135) {
							NextMoveStateUnit = MoveState.right;
						}
					}
				}
			} 

		
		
		
	}

	// Update is called once per frame
	void Update () {
		// Считываем нажатие клавиш
		if (Input.GetKeyDown(KeyCode.LeftArrow)) {NextMoveStateUnit = MoveState.left;}
		else if (Input.GetKeyDown(KeyCode.RightArrow)) {NextMoveStateUnit = MoveState.right;}
		else if (Input.GetKeyDown(KeyCode.UpArrow)) {NextMoveStateUnit = MoveState.up;}
		else if (Input.GetKeyDown(KeyCode.DownArrow)) {NextMoveStateUnit = MoveState.down;}


		// Игрок может сменить направление движения на противоположное в любой момент
		if (CurrentPathPoint&&!CanChangeDirection)
		{
			Vector3 cc = Vector3.zero;
			if (((CurrentMoveStateUnit == MoveState.left) && (NextMoveStateUnit == MoveState.right))||
				((CurrentMoveStateUnit == MoveState.right) && (NextMoveStateUnit == MoveState.left))||
			    ((CurrentMoveStateUnit == MoveState.up) && (NextMoveStateUnit == MoveState.down))||
				((CurrentMoveStateUnit == MoveState.down) && (NextMoveStateUnit == MoveState.up))) {SwapDirections();}
		}

		// Игрок может сменить направление на любое в ключевой точке
		if (CanChangeDirection)
		{
			if (!CurrentPathPoint)
			{
				SourceTarget = transform.position;
				if (GetPathPoint(MoveState.left) != transform.position) {CurrentMoveStateUnit = MoveState.left; TargetPoint = GetPathPoint(MoveState.left);}
				else if (GetPathPoint(MoveState.right) != transform.position) {CurrentMoveStateUnit = MoveState.right; TargetPoint = GetPathPoint(MoveState.right);}
				else if (GetPathPoint(MoveState.up) != transform.position) {CurrentMoveStateUnit = MoveState.up; TargetPoint = GetPathPoint(MoveState.up);}
				else if (GetPathPoint(MoveState.down) != transform.position) {CurrentMoveStateUnit = MoveState.down; TargetPoint = GetPathPoint(MoveState.down);}
			}
			else
			{
				TargetPoint = GetTarget(NextMoveStateUnit);
				if (Vector3.Distance(TargetPoint, transform.position) >= 0.5f) {CurrentMoveStateUnit = (NextMoveStateUnit == MoveState.stay) ? CurrentMoveStateUnit : NextMoveStateUnit;}
			}
			CanChangeDirection = false;
		}



		base.Update();

		//При достижении ключевой точки записываем её как исходную
		if (TargetPoint == transform.position) {SourceTarget = TargetPoint;}
	}

	/// <summary>
	/// Меняем направление движения на противоположное
	/// </summary>
	void SwapDirections ()
	{
		CurrentMoveStateUnit = NextMoveStateUnit;
		Vector3 c = SourceTarget;
		SourceTarget = TargetPoint;
		TargetPoint = c;
	}


	/// <summary>
	/// Обрабатываем столкновения с другими объектами
	/// </summary>
	void OnTriggerEnter (Collider other)
	{	
		switch (other.tag)
		{
		case "Bonuses":
			Bonuses bonus = other.GetComponent<Bonuses>();
			FindObjectOfType<Game>().AddScore(bonus.score);
			switch (bonus.BonusType)
			{
			case Bonuses.TypeBounes.PowerUp:
				foreach (GhostAI g in FindObjectsOfType<GhostAI>()) {g.ChangeMode(GhostAI.ModeBehavior.outrun);}
				break;
			case Bonuses.TypeBounes.Point:
				if ( --prm.PointsCount<=0) {FindObjectOfType<Game>().NextLevel();}
				break;
			}
			bonus.gameObject.SetActive(false);
			break;
		case "Ghosts":
			GhostAI ghost = other.GetComponent<GhostAI>();
			switch (ghost.BehaviorMode)
			{
			case GhostAI.ModeBehavior.chase:
			case GhostAI.ModeBehavior.around:
				FindObjectOfType<Game>().ShowGameMessage(Game.MessageState.kill);
				break;
			case GhostAI.ModeBehavior.outrun:
				ghost.ChangeMode(GhostAI.ModeBehavior.backtobase);
				FindObjectOfType<Game>().AddScore(200);
				break;
			case GhostAI.ModeBehavior.backtobase:
				break;
			}
			break;
		}
	}
}
