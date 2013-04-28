using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GUIEnabler))]
public class GrabberProgramUI : SingletonBehaviour<GrabberProgramUI>
{
	[SerializeField]
	InstructionSlot [] _instructionSlots = null;
	
	[SerializeField]
	GameObject [] _grabberUIObjects = null;
	
	bool guiEnabled = false;
	
	void EnableGUI (bool enabled)
	{
		guiEnabled = enabled;
		foreach (InstructionSlot instructionSlot in _instructionSlots)
		{
			instructionSlot.EnableGUI(enabled);
		}
		if (!enabled)
		{
			CloseAllSlots();
		}
		RefreshGrabberUIObjects();
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
			
			EnableGUI (value != null);
//			RefreshGrabberUIObjects();
		}
	}
	
	#region EZGUI
	
	public void RefreshGrabberUIObjects()
	{
		foreach (GameObject uiObject in _grabberUIObjects)
		{
			uiObject.transform.localScale = Vector3.one * (_displayedGrabber != null && guiEnabled && GameManager.instance.gameState == GameManager.State.Construction ? 1f : 0f);
		}
	}

	public void InstructionSetAt (int _index)
	{
		if (_index + 1 < _instructionSlots.Length && DisplayedGrabber.instructions[_index+1] == Grabber.Instruction.None)
		{
			_instructionSlots[_index+1].ToggleInstructionPanel();
		}
	}
	
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
			
			
			if (!noneOpFound && _instructionSlots[i].currentInstruction == Grabber.Instruction.None)
			{
				noneOpFound = true;
				
				if (i == 0)
				{
					_instructionSlots[i].DisplayNoneOperation(false);
				}
				else
				{
					_instructionSlots[i-1].DisplayNoneOperationAfterTransision(true);
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
