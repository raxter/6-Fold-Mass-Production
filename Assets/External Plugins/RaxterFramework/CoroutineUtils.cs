using UnityEngine;
using System.Collections;

public class CoroutineUtils : AutoSingletonBehaviour<CoroutineUtils> 
{
	
	
	public Coroutine WaitOneFrameAndDo(System.Action action)
	{
		return StartCoroutine(WaitOneFrameAndDoCoroutine(action));
	}
	
	
	private IEnumerator WaitOneFrameAndDoCoroutine(System.Action action)
	{
		yield return null;
		if (action != null)
		{
			action();
		}
	}
}
