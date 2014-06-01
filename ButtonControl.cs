using UnityEngine;
using System.Collections;

// Скрипт вешается на все основные кнопки для управления переходами между экранами
public class ButtonControl : MonoBehaviour {

	public MainController.TypeSystemActions CurrentAction;
	
	void OnClick ()
	{
		MainController.Instance.MainControl (CurrentAction);
	}
}
