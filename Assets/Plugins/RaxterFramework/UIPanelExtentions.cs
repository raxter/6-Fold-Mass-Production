using UnityEngine;
using System.Collections;

public static class UIPanelExtentions
{

	public static void OnTransitionFinished(this UIPanel panel, System.Action onFinish)
	{
		CoroutineUtils.instance.StartCoroutine(WaitFortransistionFinish(panel, onFinish));
	}
	
	static IEnumerator WaitFortransistionFinish(UIPanel panel, System.Action onFinish)
	{
		yield return null;
		while (panel.IsTransitioning)
		{
			yield return null;
		}
		
		if (onFinish != null)
		{
			onFinish();
		}
	}
}
