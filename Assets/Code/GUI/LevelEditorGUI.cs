using UnityEngine;
using System.Collections;

public class LevelEditorGUI : SingletonBehaviour<LevelEditorGUI> 
{
	
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
	
	void SaveGame()
	{
		
	}
	
	#endregion
	
}
