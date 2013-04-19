using UnityEngine;
using System.Collections;

public class GrabberProgramUI : MonoBehaviour 
{
	[SerializeField]
	InstructionSlot [] _instructionSlots;
	
	
	
	
	
	public void CloseAllSlots ()
	{
		foreach (InstructionSlot instructionSlot in _instructionSlots)
		{
			instructionSlot.CloseInstructionPanel();
		}
	}
}
