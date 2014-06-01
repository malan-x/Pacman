using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
/// <summary>
/// Скрипт с информацией о карте
/// </summary>
public class Map : MonoBehaviour {

	public string title;							// название карты
	[Range (8, 16)] public int width = 10; 			// ширина карты
	[Range (8, 16)] public int height = 10; 			// высота карты
	public string[] map; 							// карта
	[HideInInspector] public UILabel TitleLabel;
	public Vector2 PacmanSpawn = new Vector2(4,7); 	// Место респауна игрока
	public Vector2 Ghosts = new Vector2(4,4); 		// Место бункера с привидениями
	public bool Blinky=true;						// 
	public bool Pinky=true;							//
	public bool Inky=true;							//
	public bool Clyde=true;							//
	public string GUID;



	// Update is called once per frame
	void Update () {
		UpdateTitle ();
	}

	void UpdateTitle ()
	{
		TitleLabel.text = title+" ("+width.ToString()+" x "+height.ToString()+")";
	}

	void OnClick ()
	{
		MainController.Instance._LevelEditor.CurrentMap = this;
	}
}
