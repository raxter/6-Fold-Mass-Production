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
	
	string SaveDir { get { return Application.persistentDataPath+"/"+standardLevelSetDirectory; } }
	
	Dictionary<string, LevelData> levels = new Dictionary<string, LevelData>();
	
	public IEnumerable SaveList
	{
		get
		{
			ReloadLevelData();
			foreach(string levelName in levels.Keys)
			{
				if (levelName != editorSaveName)
					yield return levelName;
			}
		}
	}
	
	void Start () 
	{
		ReloadLevelData();
	}
	
	public void Save(string levelName, string encodedLevel, SaveType saveType)
	{
		System.IO.File.WriteAllText(SaveDir+"/"+levelName.Replace(" ", "_")+".6xmpl", levelName+"\n"+encodedLevel);
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
		
		if (!System.IO.Directory.Exists(SaveDir))
			System.IO.Directory.CreateDirectory(SaveDir);
		
		foreach ( string filePath in System.IO.Directory.GetFiles(SaveDir, "*.6xmpl"))
		{
			ParseLevelFile(filePath);
		}
//		Object [] levelObjects = Resources.LoadAll(standardLevelSetDirectory, typeof(TextAsset));
		
//		foreach(Object obj in levelObjects)
//			if (obj is TextAsset)
//				ParseLevelFile(obj as TextAsset);
	}
	
	void ParseLevelFile(string filePath)
	{
		string fileText = System.IO.File.ReadAllText(filePath);
		string [] data = fileText.Split('\n');
		
		if (data.Length < 2)
		{
			Debug.LogWarning(filePath+ " not a valid 6xMP level file");
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










