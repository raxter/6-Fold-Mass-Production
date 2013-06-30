using UnityEngine;
using System.Collections;

public class HUDFunctions : MonoBehaviour
{
	bool _inputEnabled = true;
	
	[SerializeField]
	UIButton grabberButton = null;
	[SerializeField]
	UIButton welderButton = null;
	[SerializeField]
	UIButton generatorButton = null;
	
	
	void Start()
	{
//		GetComponent<GUIEnabler>().onEnableGUI = (enabled) => _inputEnabled = enabled;
		GameManager.instance.GameStateChangedEvent += () => 
		{
			_inputEnabled = (GameManager.instance.gameState == GameManager.State.Construction);
		};
		
		grabberButton.AddInputDelegate(GrabberInputDelegate);
		welderButton.AddInputDelegate(WelderInputDelegate);
		generatorButton.AddInputDelegate(GeneratorInputDelegate);
	}
	
	void GrabberInputDelegate(ref POINTER_INFO ptr)
	{
		InputDelegate(ref ptr, MechanismType.Grabber);
	}
	
	void WelderInputDelegate(ref POINTER_INFO ptr)
	{
		InputDelegate(ref ptr, MechanismType.WeldingRig);
	}
	void GeneratorInputDelegate(ref POINTER_INFO ptr)
	{
		InputDelegate(ref ptr, MechanismType.Generator);
	}
	
	
	public void InputDelegate(ref POINTER_INFO ptr, MechanismType mechanismType)
	{
		if (!_inputEnabled) return;
		
		switch (ptr.evt)
		{
			case POINTER_INFO.INPUT_EVENT.PRESS:
				GameManager.instance.CreateMechanismForDragging(mechanismType);
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
