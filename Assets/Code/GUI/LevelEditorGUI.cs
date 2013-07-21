using UnityEngine;
using System.Collections;

public class LevelEditorGUI : SingletonBehaviour<LevelEditorGUI> 
{
	public Camera EditorCamera { get { return _editorCamera; } }
	
	[SerializeField]
	Camera _editorCamera;
	
	[SerializeField]
	UIPanel _editorPanel;
	
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
				GridManager.instance.LoadEditorLevel();
			}
			else // we are testing so we load the autosaved solution
			{
				GridManager.instance.LoadEditorSolution();
			}
		}
	}
	
	
}
