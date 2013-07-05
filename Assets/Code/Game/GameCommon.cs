using UnityEngine;
using System.Collections;

public class GameCommon : AutoSingletonBehaviour<GameCommon> 
{
	
	string levelString;
	
	public void LoadLevel(string encodedLevel)
	{
		// setup options for level and save then here
		levelString = encodedLevel;
		
		// change scene
		Application.LoadLevel(GameSettings.instance.BaseLevelSceneName);
		
		Debug.Log ("Level Loaded "+encodedLevel);
	}
	
}
