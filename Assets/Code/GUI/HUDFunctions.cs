using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GUIEnabler))]
public class HUDFunctions : MonoBehaviour
{
	bool _inputEnabled = true;
	
	[SerializeField]
	UIButton grabberButton;
	
	
	void Start()
	{
		GetComponent<GUIEnabler>().onEnableGUI = (enabled) => _inputEnabled = enabled;
		
		grabberButton.AddInputDelegate(GrabberInputDelegate);
	}
	
	
	
	public void GrabberInputDelegate(ref POINTER_INFO ptr)
	{
		if (!_inputEnabled) return;
		
		switch (ptr.evt)
		{
			case POINTER_INFO.INPUT_EVENT.PRESS:
				GameManager.instance.CreateMechanism(MechanismType.Grabber);
				break;
			case POINTER_INFO.INPUT_EVENT.TAP:
			case POINTER_INFO.INPUT_EVENT.RELEASE:
			case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
//				GameManager.instance.UnSelecteMechanistIcon();
				break;
			
		}
		
		InputCatcher.instance.InputDelegate(ref ptr);
	}
	
}
