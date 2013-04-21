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
			RefreshAllSlots();	
		}
	}
	
	
	void RefreshAllSlots()
	{
		for (int i = 0 ; i < _instructionSlots.Length ; i++)
		{
			_instructionSlots[i].SetIndex(i);
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
