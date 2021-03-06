using UnityEngine;
using UnityEditor;
using System.Collections;

public class CharSerializerTest: EditorWindow
{
	
	[MenuItem("Window/Char Serialization Test")]
	public static void GetWindow()
	{
		EditorWindow.GetWindow(typeof(CharSerializerTest));
	}
	
	Vector2 scrollPosition;
	
	int min = 0, max = 0;
	
	void OnGUI()
	{
		min = EditorGUILayout.IntSlider("Min",min,0,max);
		max = EditorGUILayout.IntField("Max", max);
		
		GUILayoutOption maxWidth = GUILayout.MaxWidth (50);
		
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
		for (int i = min ; i <= max ; i++)
		{
			EditorGUILayout.BeginHorizontal();
			char code = Encoding.ToCode(i);
			string longCode = Encoding.ToLongCode(i);
			EditorGUILayout.LabelField(""+i, maxWidth);
			EditorGUILayout.LabelField(""+code, maxWidth);
			EditorGUILayout.LabelField(""+Encoding.ToNumber(code), maxWidth);
			EditorGUILayout.LabelField(""+longCode, maxWidth);
			EditorGUILayout.LabelField(""+Encoding.ToNumber(longCode), maxWidth);
			EditorGUILayout.EndHorizontal();
		}
		
		EditorGUILayout.EndScrollView();
	}
}
