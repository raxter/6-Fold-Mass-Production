using UnityEngine;
using System.Collections;

public class LevelEditorGUI : SingletonBehaviour<LevelEditorGUI> 
{
	public Camera EditorCamera { get { return _editorCamera; } }
	
	[SerializeField]
	Camera _editorCamera;
	
	
	public bool editorEnabled 
	{
		get 
		{
			return gameObject.activeSelf;
		}
		set
		{
			gameObject.SetActive(value);
		}
	}
	
	
}
