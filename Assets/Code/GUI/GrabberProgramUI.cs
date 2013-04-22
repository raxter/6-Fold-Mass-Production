using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GUIEnabler))]
public class GrabberProgramUI : SingletonBehaviour<GrabberProgramUI>
{
	[SerializeField]
	InstructionSlot [] _instructionSlots = null;
	
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
		}
	}
	
	
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
		
		// TODOOOOOO this should hide all instructions after a None operation (all slots other than last should not have the non operation as an option!)
//		bool noneOpFound = false;
//		for (int i = 0 ; i < _instructionSlots.Length ; i++)
//		{
//			if (noneOpFound)
//			{
//				_instructionSlots[i].EnableGUI(false);
//			}
//			
//			if (_instructionSlots[i].currentInstruction == Grabber.Instruction.None)
//			{
//				noneOpFound = true;
//			}
//		}
	}
	
	
	public void CloseAllSlots ()
	{
		foreach (InstructionSlot instructionSlot in _instructionSlots)
		{
			instructionSlot.CloseInstructionPanel();
		}
	}
}
