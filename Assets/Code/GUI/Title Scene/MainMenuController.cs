using UnityEngine;
using System.Collections;

public class MainMenuController : MonoBehaviour 
{
	
	
	void QuickPlay()
	{
		GameCommon.instance.LoadLevel(LevelSettings.instance.levels[0].name); // should pass the encoded string in rather
	}
	
	
	void StartLevelEditor()
	{
		
	}
	
}
