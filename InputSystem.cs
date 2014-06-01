using UnityEngine;
using System.Collections;

// Скрипт для обработки жестов
public class InputSystem : Singleton<InputSystem> {
	
	
	public float minDistanceBetweenTouchOnPC = 10f;
	public float minDistanceBetweenTouch = 10f;
	public float minTimeToTapOnPC = 0.06f;

	public delegate void delegateOnSlideBegin(Vector3 beginPosition, Vector3 currentPosition);
	public delegate void delegateOnTap(int countTap);

	public delegateOnSlideBegin eventOnSlideBegin;
	public delegateOnTap eventOnTap;
	
	Vector3 m_beginPosition;
	bool m_isSlideBegin = false;
	float m_mouseDownTime;
	int m_tapCount;
	

	// Update is called once per frame
	void Update () {
	
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_STANDALONE_WIN
		if(Input.GetMouseButtonDown(0)) {
			m_beginPosition = Input.mousePosition;
			m_isSlideBegin = false;
			m_mouseDownTime = Time.time;
		}
		else {
			if(Input.GetMouseButton(0)) {
				float dist = Vector3.Distance(m_beginPosition, Input.mousePosition);
				if(dist > minDistanceBetweenTouchOnPC) {
					if(!m_isSlideBegin) {
						if(eventOnSlideBegin != null) {
							eventOnSlideBegin(m_beginPosition, Input.mousePosition);
							m_isSlideBegin = true;
						}
					}
				}
			}
			else 
				m_isSlideBegin = false;
		}
#endif

#if UNITY_IPHONE || UNITY_ANDROID
		if(Input.touchCount > 0) {
			TouchPhase phase = Input.touches[0].phase;
			if(phase == TouchPhase.Began) {
				m_beginPosition = Input.touches[0].position;
				m_isSlideBegin = false;
			}
			else {
				if(phase == TouchPhase.Moved) {
					float dist = Vector3.Distance(m_beginPosition, Input.touches[0].position);
					if(dist > minDistanceBetweenTouch) {
						if(!m_isSlideBegin) {
							if(eventOnSlideBegin != null) {
								eventOnSlideBegin(m_beginPosition, Input.touches[0].position);
								m_isSlideBegin = true;
							}
						}
					}
				}
				else {
					if((phase == TouchPhase.Canceled) || (phase == TouchPhase.Ended)) {
						m_isSlideBegin = false;
					}
				}
			}

		}
#endif

	}
}
