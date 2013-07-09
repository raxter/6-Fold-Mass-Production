using UnityEngine;
using System.Collections;

public class GameCommon : SingletonBehaviour<GameCommon> 
{
	
	string levelString;
	bool editorMode = false;
	
	public void LoadLevel(string encodedLevel)
	{
		LoadLevel(encodedLevel, false);
	}
	public void LoadLevel(string encodedLevel, bool loadEditor)
	{
		// setup options for level and save then here
		levelString = encodedLevel;
		
		// change scene
		Application.LoadLevel(GameSettings.instance.BaseLevelSceneName);
		
		Debug.Log ("Level Loaded "+encodedLevel);
	}
	
}
