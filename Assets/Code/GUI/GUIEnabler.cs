using UnityEngine;
using System.Collections;

public class GUIEnabler : MonoBehaviour 
{

	public System.Action<bool> onEnableGUI = null;
	
	
	public void EnableGUI(bool enabled)
	{
		if (onEnableGUI != null)
		{
			onEnableGUI(enabled);
		}
	}
}
