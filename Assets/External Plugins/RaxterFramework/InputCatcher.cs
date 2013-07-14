using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Up and Down are for mouse released and pressed ( single frame), Released, Pressed, and Dragged are for constant states (multiple frames)
public enum ControlState {Starting, Ending, Active, Inactive};


public class InputCatcher : MonoBehaviour
{
	
	[SerializeField]
	float moveZto = 10;
	
	public UIButton Control { get { return _catcher; } }
	[SerializeField]
	UIButton _catcher;
	
	public delegate void HandleInputDelegate (POINTER_INFO pointerInfo, ControlState pressState, ControlState dragState);
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
	
	
	public void Retarget(IUIObject newTarget)
	{
		UIManager.instance.Retarget(_catcher, newTarget);
	}
		
	// Use this for initialization
	void Start () 
	{
		transform.localPosition = new Vector3(0,0,moveZto);
		_catcher.AddInputDelegate(InputDelegate);
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
	
//	public class InputInfo
//	{
//		public Vector3 position;
//		public PressState pressState;
//		
//	}
	
	
	public class ControlStateMachine
	{
		ControlState _state = ControlState.Inactive;
		
		public ControlState state
		{
			get 
			{
				return _state;
			}
		}
		
		bool stateChanged = false;
		
		public void Update ()
		{
			if (stateChanged)
			{
				stateChanged = false;
				return;
			}
			if (_state == ControlState.Starting)
			{
				_state = ControlState.Active;
			}
			if (_state == ControlState.Ending)
			{
				_state = ControlState.Inactive;
			}
		}
		
		public void SetActive(bool active)
		{
			if (active)
			{
				StartActive();
			}
			else
			{
				EndActive();
			}
		}
			
			
		public void StartActive()
		{
//			Debug.Log("Activating");
			if (_state == ControlState.Inactive)
			{
				_state = ControlState.Starting;
				stateChanged = true;
			}
		}
		
		public void EndActive()
		{
//			Debug.Log("Deactivating");
			if (_state == ControlState.Active)
			{
				_state = ControlState.Ending;
				stateChanged = true;
			}
		}
	}
	
	ControlStateMachine pressState = new ControlStateMachine();
	ControlStateMachine dragState = new ControlStateMachine();
	
	public void InputDelegate(ref POINTER_INFO ptr)
	{
//		Debug.Log (ptr.devicePos);
		
//		Debug.Log (ptr.evt);
		
		
		
		if (ptr.evt != POINTER_INFO.INPUT_EVENT.NO_CHANGE)
		{
			switch (ptr.evt)
			{
				
				case POINTER_INFO.INPUT_EVENT.MOVE:
				case POINTER_INFO.INPUT_EVENT.MOVE_OFF:
				{
					break;
				}
				
				case POINTER_INFO.INPUT_EVENT.PRESS:
				{
					pressState.StartActive();
					break;
				}
				
				case POINTER_INFO.INPUT_EVENT.RELEASE:
				case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
				case POINTER_INFO.INPUT_EVENT.TAP:
				{
					pressState.EndActive();
					break;
				}
					
				
			}
		
			dragState.SetActive(ptr.evt == POINTER_INFO.INPUT_EVENT.DRAG);
		}
		
		pressState.Update();
		dragState.Update();
//		Debug.Log(pressState.state+":"+dragState.state+" <- "+ptr.evt);
		
		
		if (OnInputEventOverride == null)
		{
			if (OnInputEvent != null)
			{
				OnInputEvent(ptr, pressState.state, dragState.state);
			}
		}
		else
		{
			OnInputEventOverride(ptr, pressState.state, dragState.state);
		}
	}
	

}
