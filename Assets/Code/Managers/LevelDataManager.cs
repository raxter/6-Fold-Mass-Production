using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum SaveType {Level, Solution}
public enum AutoSaveType {Named, AutoSave}

public class LevelDataManager : SingletonBehaviour<LevelDataManager> 
{
	
	public class LevelData
	{
		public string levelName;
		public string levelEncoding;
		public string levelAutosaveEncoding;
		public Dictionary<string, string> namedSaveEncodings = new Dictionary<string, string>();
	}
	
	
	public static string EditorSaveName { get { return "_Editor"; } }
	
	static string standardLevelSetDirectory = "Levels";
	
	static string SaveDir { get { return Application.persistentDataPath+"/"+standardLevelSetDirectory; } }
	
	Dictionary<string, LevelData> levels = new Dictionary<string, LevelData>();
	
	Dictionary<string, LevelData> internalLevels = new Dictionary<string, LevelData>();
	
	public bool Contains (string levelName)
	{
		ReloadLevelData();
		return levelName != EditorSaveName && levels.ContainsKey(levelName);
	}
	
	public IEnumerable<string> SaveList
	{
		get
		{
			ReloadLevelData();
			foreach(string levelName in levels.Keys)
			{
				if (levelName != EditorSaveName)
				{
#if !UNITY_EDITOR
					if (!levelName.StartsWith('_'))
#endif
					{
						yield return levelName;
					}
				}
			}
		
		}
		
	}
	
	void Start () 
	{
		ReloadLevelData();
	}
	
	string FileNameString(string levelName, string solutionName, SaveType saveType)
	{
		return SaveDir+"/"+(levelName + solutionName).Replace(" ", "_")+".6xmp"+(saveType == SaveType.Level ? "l" : "s");
	}
	
	
	string MakeLevelNameSafe(string levelName)
	{
		
		return levelName;
	}
	
	const string autoSaveName = "_Autosave";
	
	public void Save(string levelName, string encodedLevel, SaveType saveType, AutoSaveType autoSaveType)
	{
		Save(levelName, encodedLevel, saveType, autoSaveType, "");
	}
		
	public void Save(string levelName, string encodedLevel, SaveType saveType, AutoSaveType autoSaveType, string solutionName)
	{
		if (levelName != EditorSaveName && levelName.StartsWith("_") && saveType == SaveType.Level)
		{
// if we are in the editor and a Level is saved with an "_" at the beginning, then it is to be saves in resources, not normally
#if UNITY_EDITOR 
			LevelSettings.instance.levels.RemoveAll((obj) => obj.name == levelName);
			
			LevelSettings.instance.levels.Add(new LevelSettings.Level() { name = levelName, encodedLevel = encodedLevel,});
			EditorUtility.SetDirty(LevelSettings.instance);
			return;
// otherwise we are playing the game normally and we just trim the "_" for a level save, 
// all other save types keep the underscore as they must reference the full name, even an underscored (Resources) level
#else
			levelName.TrimStart('_');
#endif
		}
		
		solutionName.TrimStart('_');
		
		if (autoSaveType == AutoSaveType.AutoSave)
		{
			solutionName = autoSaveName;
		}
		
		
		System.IO.File.WriteAllText(FileNameString(levelName, solutionName, saveType), levelName+"\n"+solutionName+"\n"+encodedLevel);
		Debug.Log("Saving "+FileNameString(levelName, solutionName, saveType));
		
	}
	
	public string Load(string levelName, SaveType saveType, AutoSaveType autoSaveType)
	{
		return Load(levelName, saveType, autoSaveType, "");
	}
	
	public string Load(string levelName, SaveType saveType, AutoSaveType autoSaveType, string solutionName)
	{
		
		Debug.Log("Loading "+levelName+" "+solutionName +":"+ saveType +":"+autoSaveType+":"+solutionName);
		
		ReloadLevelData();
		if (!levels.ContainsKey(levelName))
			return "";
		
		if (autoSaveType == AutoSaveType.AutoSave)
			solutionName = autoSaveName;
		
		switch(saveType)
		{
		case SaveType.Level: 
			if (autoSaveType == AutoSaveType.AutoSave)
				return levels[levelName].levelAutosaveEncoding;
			else
				return levels[levelName].levelEncoding;
		case SaveType.Solution: 
			if (!levels[levelName].namedSaveEncodings.ContainsKey(solutionName))
				return "";
			return levels[levelName].namedSaveEncodings[solutionName];
		default: return "";
		}
	}
	
	public void Delete(string levelName, SaveType saveType, AutoSaveType autoSaveType)
	{
		Delete(levelName, saveType, autoSaveType, "");
	}
	public void Delete(string levelName, SaveType saveType, AutoSaveType autoSaveType, string solutionName)
	{
		if (autoSaveType == AutoSaveType.AutoSave)
			solutionName = autoSaveName;
		
		System.IO.File.Delete(FileNameString(levelName, solutionName, saveType));
	}
	
#if UNITY_EDITOR
	
	public static void DeleteAll()
	{
		foreach ( string filePath in System.IO.Directory.GetFiles(SaveDir, "*.6xmpl"))
		{
			Debug.Log("Deleting "+filePath);
			System.IO.File.Delete(filePath);
		}
		foreach ( string filePath in System.IO.Directory.GetFiles(SaveDir, "*.6xmps"))
		{
			Debug.Log("Deleting "+filePath);
			System.IO.File.Delete(filePath);
		}
	}
	
#endif
	
	void ReloadLevelData()
	{
		levels.Clear();
		
		if (!System.IO.Directory.Exists(SaveDir))
			System.IO.Directory.CreateDirectory(SaveDir);
		
		foreach ( string filePath in System.IO.Directory.GetFiles(SaveDir, "*.6xmpl"))
		{
			ParseLevelFile(filePath);
		}
		
		foreach ( LevelSettings.Level level in LevelSettings.instance.levels)
		{
			levels[level.name] = 	new LevelData() 
									{ 
										levelName = level.name, 
										levelEncoding = level.encodedLevel, 
									};
		}
		
		foreach ( string filePath in System.IO.Directory.GetFiles(SaveDir, "*.6xmps"))
		{
			ParseSolutionFile(filePath);
		}
		
		
	}
	void ParseSolutionFile(string filePath)
	{
		string fileText = System.IO.File.ReadAllText(filePath);
		string [] data = fileText.Split('\n');
		
		if (data.Length < 2)
		{
			Debug.LogWarning(filePath+ " not a valid 6xMP solution file");
			return;
		}
		
		string levelName = data[0].Trim();
		string solutionName = data[1].Trim();
		string encodedLevel = data[2].Trim();
		
		if (levels.ContainsKey(levelName))
		{
			levels[levelName].namedSaveEncodings[solutionName] = encodedLevel;
		}
		else
		{
			Debug.LogWarning(filePath+ " is a solution for a level ("+solutionName+") that does not exist (\""+levelName+"\")");
			return;
		}
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
		
		string levelName = data[0].Trim();
		string solutionName = data[1].Trim();
		string encodedLevel = data[2].Trim();
		
		if (!levels.ContainsKey(levelName))
		{
			levels[levelName] = new LevelData();
		}
		if (solutionName == autoSaveName)
			levels[levelName].levelAutosaveEncoding = encodedLevel;
		else
			levels[levelName].levelEncoding = encodedLevel;
			
	}

	
	
	
	
	
}










