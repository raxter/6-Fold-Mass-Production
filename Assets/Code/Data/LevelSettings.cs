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
		
		public int targetConstructions;
		
		public ConstructionDefinition targetConstruction;
	}
	
	
	public List<Level> levels;
	
	public List<string> GetLevelNames()
	{
		return levels.ConvertAll((level) => level.name );
	}
	
	public Level GetLevel(string name)
	{
		return levels.Find( (e) => e.name == name );
	}
}

public static class ListExtentention
{
	
	public static bool TrueForOne <T> (this List<T> list, System.Predicate<T> checkFunc)
	{
		return false;
	}
}