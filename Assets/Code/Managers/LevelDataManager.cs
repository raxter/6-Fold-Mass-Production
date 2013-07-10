using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelDataManager : SingletonBehaviour<LevelDataManager> 
{
	
	string standardLevelSetDirectory = "Levels";
	
	Dictionary<string, string> levels = new Dictionary<string, string>();
	
	void Start () 
	{
		ReloadLevelData();
	}
	
	public void SaveLevel(string levelName, string encodedLevel)
	{
		System.IO.File.WriteAllText("Assets/"+standardLevelSetDirectory+"/"+levelName.Replace(" ", "_")+".txt", levelName+"\n"+encodedLevel);
	}
	
	public IEnumerator<string> GetLevelList()
	{
		return levels.Keys.GetEnumerator();
	}
	
	void ReloadLevelData()
	{
		levels.Clear();
		Object [] levelObjects = Resources.LoadAll(standardLevelSetDirectory, typeof(TextAsset));
		
		foreach(Object obj in levelObjects)
			if (obj is TextAsset)
				ParseLevelFile(obj as TextAsset);
	}
	
	void ParseLevelFile(TextAsset textAsset)
	{
		string [] data = textAsset.text.Split('\n');
		
		if (data.Length < 2)
		{
			Debug.LogWarning(textAsset.name+" not a valid 6xMP save file");
			return;
		}
		
		string name = data[0].Trim();
		string encodedLevel = data[1].Trim();
		
		// TODO check if it's a valid save
		levels[name] = encodedLevel;
	}

	
	
	
	
	
}










