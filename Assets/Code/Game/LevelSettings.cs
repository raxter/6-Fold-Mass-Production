using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelSettings : ScriptableObjectSingleton<LevelSettings> 
{
	[System.Serializable]
	public class GeneratorDetails
	{
		public GrabbablePart toGeneratePrefab;
		public IntVector2 location;
	}
	
	
	[System.Serializable]
	public class Level
	{
		public string name;
		public List<GeneratorDetails> generators;
	}
	
	
	public List<Level> levels;
	
	
	public Level GetLevel(string name)
	{
		return levels.Find( (e) => e.name == name );
	}
}
