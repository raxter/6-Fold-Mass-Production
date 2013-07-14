using UnityEngine;
using System.Collections;

public class BackgroundInputCatcher : SingletonBehaviour<BackgroundInputCatcher>
{
	
	public static InputCatcher Catcher { get { return BackgroundInputCatcher.instance._backgroundInputCatcher; } }
	[SerializeField]
	InputCatcher _backgroundInputCatcher;
}

