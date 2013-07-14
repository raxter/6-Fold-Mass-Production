using UnityEngine;
using System.Collections;

public class CoroutineUtils : AutoSingletonBehaviour<CoroutineUtils> 
{
	
	
	public static Coroutine WaitOneFrameAndDo(System.Action action)
	{
		return instance.StartCoroutine(WaitOneFrameAndDoCoroutine(action));
	}
	
	
	private static IEnumerator WaitOneFrameAndDoCoroutine(System.Action action)
	{
		yield return null;
		if (action != null)
		{
			action();
		}
	}
}
