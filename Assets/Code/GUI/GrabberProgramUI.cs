using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GUIEnabler))]
public class GrabberProgramUI : SingletonBehaviour<GrabberProgramUI>
{
	[SerializeField]
	InstructionSlot [] _instructionSlots = null;
	
	[SerializeField]
	GameObject [] _grabberUIObjects = null;
	
	void EnableGUI (bool enabled)
	{
		foreach (InstructionSlot instructionSlot in _instructionSlots)
		{
			instructionSlot.EnableGUI(enabled);
		}
		
	}
	
	Grabber _displayedGrabber = null;
	
	
	void Start()
	{
		GetComponent<GUIEnabler>().onEnableGUI = EnableGUI;
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
			
			SetAllIndecies();	
			
			
			foreach (GameObject uiObject in _grabberUIObjects)
			{
				uiObject.transform.localScale = Vector3.one * (value != null ? 1f : 0f);
			}
		}
	}
	
	#region EZGUI
	
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
	
	void SetAllIndecies()
	{
		for (int i = 0 ; i < _instructionSlots.Length ; i++)
		{
			_instructionSlots[i].SetIndex(i);
		}
		RefreshDisplayedSlots();
	}
	
	public void RefreshDisplayedSlots()
	{
		bool noneOpFound = false;
		for (int i = 0 ; i < _instructionSlots.Length ; i++)
		{
			
			if (noneOpFound)
			{
				_instructionSlots[i].Display(false);
				_instructionSlots[i].EnableGUI(false);
			}
			else
			{
				_instructionSlots[i].Display(true);
				_instructionSlots[i].EnableGUI(true);
				
			}
			
			_instructionSlots[i].DisplayNoneOperation(false);
			
			
			if (_instructionSlots[i].currentInstruction == Grabber.Instruction.None)
			{
				noneOpFound = true;
				
				if (i == 0)
				{
					_instructionSlots[i].DisplayNoneOperation(true);
				}
				else
				{
					_instructionSlots[i-1].DisplayNoneOperation(true);
				}
			}
		}
	}
	
	
	public void CloseAllSlots ()
	{
		foreach (InstructionSlot instructionSlot in _instructionSlots)
		{
			instructionSlot.CloseInstructionPanel();
		}
	}
}
