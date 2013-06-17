using UnityEngine;
using System.Collections;

public class GrabberProgramUI : SingletonBehaviour<GrabberProgramUI>
{
//	[SerializeField]
//	InstructionSlot [] _instructionSlots = null;
	
	[SerializeField]
	GameObject [] _grabberUIObjects = null;
	
//	bool guiEnabled = false;
	
//	void EnableGUI (bool enabled)
//	{
//		guiEnabled = enabled;
//
//
//		RefreshGrabberUIObjects();
//	}
	
	
	Grabber _displayedGrabber = null;
	
	
	void Start()
	{
//		GameManager.instance.GameStateChangedEvent += () => 
//		{
//			EnableGUI(GameManager.instance.gameState == GameManager.State.Construction);
//		};
		
		GameManager.instance.GameStateChangedEvent += () =>
		{
			RefreshGrabberUIObjects();
		};
	}
	
	public Grabber DisplayedGrabber 
	{
		get 
		{
			return _displayedGrabber;
		}
		set
		{
			_displayedGrabber = value;
			
			
//			EnableGUI (value != null);
			RefreshGrabberUIObjects();
		}
	}
	
	#region EZGUI
	
	public void RefreshGrabberUIObjects()
	{
		foreach (GameObject uiObject in _grabberUIObjects)
		{
			uiObject.transform.localScale = Vector3.one * (_displayedGrabber != null && /*guiEnabled &&*/ GameManager.instance.gameState == GameManager.State.Construction ? 1f : 0f);
		}
	}

//	public void InstructionSetAt (int _index)
//	{
//		if (_index + 1 < _instructionSlots.Length && DisplayedGrabber.GetInstruction(_index+1) == Grabber.Instruction.None)
//		{
//			_instructionSlots[_index+1].ToggleInstructionPanel();
//		}
//	}
	
	void ExtendGrabber()
	{
		if (DisplayedGrabber != null)
		{
			DisplayedGrabber.ExtendStartState();
		}
	}
	void RetractGrabber()
	{
		if (DisplayedGrabber != null)
		{
			DisplayedGrabber.RetractStartState();
		}
	}
	void RotateAntiGrabber()
	{
		if (DisplayedGrabber != null)
		{
			DisplayedGrabber.RotateAntiStartState();
		}
	}
	void RotateClockGrabber()
	{
		if (DisplayedGrabber != null)
		{
			DisplayedGrabber.RotateClockStartState();
		}
	}
	
	#endregion
	
	
}
