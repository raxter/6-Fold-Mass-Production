using UnityEngine;
using System.Collections;

public class LevelEditorGUI : SingletonBehaviour<LevelEditorGUI> 
{
	public Camera EditorCamera { get { return _editorCamera; } }
	
	[SerializeField]
	Camera _editorCamera;
	
	[SerializeField]
	SpriteText levelName;
	
	
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
	
	
	#region EZ GUI
	
	void SaveLevel()
	{
		LevelDataManager.instance.Save(levelName.Text, GridManager.instance.LevelEncoding, SaveType.Level);
	}
	
	#endregion
	
}
