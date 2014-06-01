using UnityEngine;
using System.Collections;

public class Bonuses : MonoBehaviour {

	public enum TypeBounes {Point=0, PowerUp, Fruit}
	public TypeBounes BonusType;
	public int score;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
	
	}


	/*void OnTriggerEnter (Collider other)
	{	
		//print (other.tag);
		if (other.tag=="PacMan") 
		{
			FindObjectOfType<Game>().AddScore(score);
			switch (BonusType)
			{
			case TypeBounes.PowerUp:
				foreach (GhostAI g in FindObjectsOfType<GhostAI>()) {g.ChangeMode(GhostAI.ModeBehavior.outrun);}
				break;
			case TypeBounes.Point:
				if ( --prm.PointsCount<=0) {FindObjectOfType<Game>().NextLevel();}
				break;
			}


			gameObject.SetActive(false);
		}
	}*/
}
