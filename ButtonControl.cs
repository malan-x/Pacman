using UnityEngine;
using System.Collections;

/// <summary>
/// Скрипт вешается на все основные кнопки для управления переходами между экранами
/// </summary>
public class ButtonControl : MonoBehaviour {

	public MainController.TypeSystemActions CurrentAction;
	
	void OnClick ()
	{
		MainController.Instance.MainControl (CurrentAction);
	}
}
