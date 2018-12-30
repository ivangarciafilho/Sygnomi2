using UnityEngine;
using System.Collections;
using UnityEditor;

public class TimeScaler : EditorWindow {
	
	int m_start = 0;
	int m_end = 1;
	float m_scale = 1.0f;
	
	
	[MenuItem ("Window/Time Scaler")]
	static void ShowTimeScalerWindow() {
		TimeScaler window = (TimeScaler)EditorWindow.GetWindow (typeof (TimeScaler));
	}
	
		// Use this for initialization
	void Start () {
	
	}
	
	void OnGUI(){
		
		m_scale = EditorGUILayout.Slider ("Time Scale", m_scale, m_start, m_end);
		 
		GUILayout.Label ("Time Range", EditorStyles.boldLabel);
		m_start = EditorGUILayout.IntField("Start", m_start);
		m_end = EditorGUILayout.IntField("End", m_end);
		
		if(GUI.changed)
		{
			if(m_start<0)
			{
				m_start = 0;
			}
			if(m_start>98)
			{
				m_start = 98;
			}
			if(m_end<1)
			{
				m_end = 1;
			}
			if(m_end>99)
			{
				m_end = 99;
			}
			
			if(m_start>=m_end)
			{
				m_end = m_start+1;
			}
			if(m_end<=m_start)
			{
				m_start = m_end-1;				
			}
			
			Time.timeScale = m_scale;
		}
	}
}
