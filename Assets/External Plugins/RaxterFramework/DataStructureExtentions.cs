using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class DataStructureExtentions 
{

	public static T First <T>(this HashSet<T> hashSet) where T : class
	{
		foreach (T t in hashSet)
		{
			return t;
		}
		return null;
	}
	
	public static bool TrueForOneShortCircuited<T>(this List<T> list, System.Predicate<T> predicate)
	{
		foreach (T t in list)
		{
			if (predicate(t))
				return true;
		}
		return false;
	}
}
