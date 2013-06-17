using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelSettings : ScriptableObjectSingleton<LevelSettings> 
{
	[System.Serializable]
	public class GeneratorDetails
	{
//		public PartType toGenerate;
		public string toGenerate;
		public IntVector2 location;
		
//		public PartType GetPartType()
//		{
//			if (toGenerate.Length == 1)
//			{
//				return (PartType)CharSerializer.CodeToNumber(toGenerate[0]);
//			}
//			return PartType.None;
//		}
//		public string GetConstructionDefinition()
//		{
//			if (toGenerate.Length == 1)
//			{
//				return "";
//			}
//			return toGenerate;
//		}
	}
	
	
	[System.Serializable]
	public class Level
	{
		public string name;
		public List<GeneratorDetails> generators;
		
		public int targetConstructions;
		
		public string targetConstruction;
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
	
//	public static bool TrueForOne <T> (this List<T> list, System.Predicate<T> checkFunc)
//	{
//		return false;
//	}
}