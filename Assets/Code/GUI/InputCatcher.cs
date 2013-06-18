using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Up and Down are for mouse released and pressed ( single frame), Released and Pressed are for constant states (multiple frames)
public enum PressState {Down, Up, Pressed, Released};


public class InputCatcher : SingletonBehaviour<InputCatcher>
{
	
	public UIButton _catcher;
	
	public delegate void HandleInputDelegate (Vector3 pointerPos, PressState pressState);
	public event HandleInputDelegate OnInputEvent;
	
	event HandleInputDelegate OnInputEventOverride;
	
	HashSet<HandleInputDelegate> overrideInputFunctions = new HashSet<HandleInputDelegate>();
	
	public void RequestInputOverride(HandleInputDelegate handleInput)
	{
		OnInputEventOverride += handleInput;
	}
	
	public void ReleaseInputOverride(HandleInputDelegate handleInput)
	{
		OnInputEventOverride -= handleInput;
	}
	
	public void InputDelegate(ref POINTER_INFO ptr)
	{
//		Debug.Log (ptr.devicePos);
		
//		Debug.Log (ptr.evt);
		
		PressState pressState = PressState.Released;
		
		
		
		if (ptr.evt == POINTER_INFO.INPUT_EVENT.PRESS)
		{
			pressState = PressState.Down;
		}
		if (ptr.evt == POINTER_INFO.INPUT_EVENT.RELEASE)
		{
			pressState = PressState.Up;
		}
		if (ptr.evt == POINTER_INFO.INPUT_EVENT.TAP)
		{
			pressState = PressState.Up;
		}
		
		if (OnInputEventOverride == null)
		{
			OnInputEvent(ptr.devicePos, pressState);
//			InputManager.instance.HandleScreenPoint(ptr.devicePos, pressState);
		}
		else
		{
			OnInputEventOverride(ptr.devicePos, pressState);
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
