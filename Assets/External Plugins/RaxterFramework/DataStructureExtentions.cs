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
}
