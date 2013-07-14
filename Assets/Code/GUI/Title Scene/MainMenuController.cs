using UnityEngine;
using System.Collections;

public class MainMenuController : MonoBehaviour 
{
	
	
	void QuickPlay()
	{
		GameCommon.instance.LoadLevel(LevelSettings.instance.levels[0].name);
	}
	
	
	void StartLevelEditor()
	{
		GameCommon.instance.LoadEditor();
	}
	
}
