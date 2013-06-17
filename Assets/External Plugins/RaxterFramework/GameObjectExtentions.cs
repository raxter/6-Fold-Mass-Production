using UnityEngine;
using System.Collections;

public static class GameObjectExtentions
{

	public static void SetLayerRecursively(this GameObject go, int layerMask)
	{
		go.layer = layerMask;
		foreach (Transform child in go.transform)
		{
			child.gameObject.SetLayerRecursively(layerMask);
		}
	}
}
