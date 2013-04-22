using UnityEngine;
using System.Collections;

public class InputCatcher : SingletonBehaviour<InputCatcher>
{
	
	public UIButton _catcher;
	
	
	
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
		
		InputManager.instance.HandleScreenPoint(ptr.devicePos, pressState);
//		if (pressState != InputManager.PressState.Released)
//		{
//			InputManager.instance.HandleScreenPoint(ptr.devicePos, pressState);
//		}
			
//#if UNITY_IPHONE || UNITY_ANDROID
//#else
//		
//		if (Input.GetMouseButtonDown(0))
//		{
//			pressState = PressState.Down;
//		}
//		else if (Input.GetMouseButtonUp(0))
//		{
//			pressState = PressState.Up;
//		}
//		else if (Input.GetMouseButton(0))
//		{
//			pressState = PressState.Pressed;
//		}
//		
//		HandleScreenPoint(Input.mousePosition, pressState);
//		
//		
//#endif
		
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
