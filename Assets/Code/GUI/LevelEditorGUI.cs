using UnityEngine;
using System.Collections;

public class LevelEditorGUI : SingletonBehaviour<LevelEditorGUI> 
{
	public Camera EditorCamera { get { return _editorCamera; } }
	
	[SerializeField]
	Camera _editorCamera;
	
	
	public bool EditorEnabled 
	{
		get 
		{
			return gameObject.activeSelf;
		}
		set
		{
			gameObject.SetActive(value);
			if (gameObject.activeSelf) // editor is active, so we clear the solution
			{
//				GridManager.instance.SaveCurrentSolution("_Editor");
				GridManager.instance.ClearSolution();
			}
		}
	}
	
	
}
