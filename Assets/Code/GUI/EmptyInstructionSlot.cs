using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EmptyInstructionSlot : MonoBehaviour 
{
	public List<DraggableInstruction> draggableInstructions = null;
	
	Grabber.Instruction currentInstruction = Grabber.Instruction.None;
	public int instructionIndex = -1;
	
	public Grabber.Instruction CurrentInstruction
	{
		get
		{
			return currentInstruction;
		}
		set
		{
			currentInstruction = value;
			foreach(DraggableInstruction draggableInstruction in draggableInstructions)
			{
				draggableInstruction.gameObject.SetActive(draggableInstruction.instructionRepresented == currentInstruction);
			}
			
//			if (onInstructionChange != null)
//				onInstructionChange(currentInstruction);
		}
	}
	
	
}
