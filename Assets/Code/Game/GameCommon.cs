using UnityEngine;
using System.Collections;

public class GameCommon : SingletonBehaviour<GameCommon> 
{
	
	
	public string LevelNameString { get; private set; }
	
	[SerializeField]
	bool _editorMode = false;
	
	public bool editorMode
	{
		get { return _editorMode; }
		private set { _editorMode = value; }
	}
	
	public void LoadEditor()
	{
		LoadLevel("", true); // TODO load level to edit immediately, right now "" should mean load the last autosave
	}
	
	public void LoadLevel(string levelName)
	{
		LoadLevel(levelName, false);
	}
		
	void LoadLevel(string levelName, bool editorModeParam)
	{
		editorMode = editorModeParam;
		// setup options for level and save then here
		LevelNameString = levelName;
		
		// change scene
		Application.LoadLevel(GameSettings.instance.BaseLevelSceneName);
		
		Debug.Log ("Level Loaded "+levelName);
	}
	
}
