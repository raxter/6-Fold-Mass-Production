using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelDataManager : SingletonBehaviour<LevelDataManager> 
{
	
	string standardLevelSetName = "LevelSet";
	
	void Start () 
	{
		TextAsset standardLevelSetText = Resources.Load(standardLevelSetName, typeof(TextAsset)) as TextAsset;
		
		Debug.Log (" -> \n"+standardLevelSetText);
		
		if (standardLevelSetText == null)
		{
#if UNITY_EDITOR
			System.IO.File.Create("Assets/Resources/"+standardLevelSetName+".txt");
			System.IO.File.WriteAllText("Assets/Resources/"+standardLevelSetName+".txt", "\n");
			standardLevelSetText = Resources.Load(standardLevelSetName, typeof(TextAsset)) as TextAsset;
#endif

		}
		
		Debug.Log (" -> \n"+standardLevelSetText.text);
		
		
		ParseLevelData(standardLevelSetText.text);
	}
	
	void ParseLevelData (string text)
	{
		List<string> levels = text.Split("\n");
		
		foreach(string levelString in levels)
		{
			encodedString = levelString.Trim();
			
			if (encodedString.StartsWith("//"))
			{
				continue;
			}
		}
	}
	
	
}
