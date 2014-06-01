using UnityEngine;
using System.Collections;

/// <summary>
/// Скрипт просто хранит параметры бонусов (тип и количество призовых очков)
/// </summary>
public class Bonuses : MonoBehaviour {

	public enum TypeBounes {Point=0, PowerUp, Fruit}
	public TypeBounes BonusType;
	public int score;
}
