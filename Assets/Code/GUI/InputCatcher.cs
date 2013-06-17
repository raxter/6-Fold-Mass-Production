using UnityEngine;
using System.Collections;

public class InputCatcher : SingletonBehaviour<InputCatcher>
{
	
	public UIButton _catcher;
	
	public bool blockInput = false;
	
	
	public void InputDelegate(ref POINTER_INFO ptr)
	{
//		Debug.Log (ptr.devicePos);
		
//		Debug.Log (ptr.evt);
		
		InputManager.PressState pressState = InputManager.PressState.Released;
		
		
		
		if (ptr.evt == POINTER_INFO.INPUT_EVENT.PRESS)
		{
			pressState = InputManager.PressState.Down;
		}
		if (ptr.evt == POINTER_INFO.INPUT_EVENT.RELEASE)
		{
			pressState = InputManager.PressState.Up;
		}
		if (ptr.evt == POINTER_INFO.INPUT_EVENT.TAP)
		{
			pressState = InputManager.PressState.Up;
		}
		
		if (!blockInput)
		{
			InputManager.instance.HandleScreenPoint(ptr.devicePos, pressState);
		}		
	}
	
	// Use this for initialization
	void Start () 
	{
		_catcher.AddInputDelegate(InputDelegate);
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
}
