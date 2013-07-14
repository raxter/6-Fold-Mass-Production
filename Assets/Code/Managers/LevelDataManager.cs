using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SaveType {Level, AutoSaveSolution, NamedSolution}

public class LevelDataManager : SingletonBehaviour<LevelDataManager> 
{
	
	public class LevelData
	{
		public string levelName;
		public string levelEncoding;
		public string autosaveEncoding;
		public Dictionary<string, string> namedSaveEncodings = new Dictionary<string, string>();
	}
	
	
	public const string editorSaveName = "_Editor";
	
	string standardLevelSetDirectory = "Levels";
	
	Dictionary<string, LevelData> levels = new Dictionary<string, LevelData>();
	
	void Start () 
	{
		ReloadLevelData();
	}
	
	public void Save(string levelName, string encodedLevel, SaveType saveType)
	{
		System.IO.File.WriteAllText("Assets/Resources/"+standardLevelSetDirectory+"/"+levelName.Replace(" ", "_")+".txt", levelName+"\n"+encodedLevel);
	}
	
	public string Load(string levelName, SaveType saveType)
	{
		ReloadLevelData();
		
		if (!levels.ContainsKey(levelName))
			return "";
		
		switch(saveType)
		{
		case SaveType.Level: 
			return levels[levelName].levelEncoding;
			break;
		case SaveType.AutoSaveSolution: 
			return levels[levelName].autosaveEncoding;
			break;
		default: return "";
		}
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
		levels[name] = new LevelData() 
		{ 
			levelName = name, 
			levelEncoding = encodedLevel, 
		};
	}

	
	
	
	
	
}










