using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrabberInstructions : MonoBehaviour 
{
	[SerializeField]
	DraggableInstructions draggableInstructions = null;
	
	[SerializeField]
	EmptyInstructionSlot emptyInstructionPrefab;
	
//	[SerializeField]
//	EmptyInstructionSlot endEmptyInstruction;
	
//	List<EmptyInstructionSlot> emptyInstructions = new List<EmptyInstructionSlot>();
	
	EmptyInstructionSlot [] emptyInstructions;
	
	
	void Start ()
	{
//		emptyInstructions.Add(endEmptyInstruction);
		int numberOfInstructions = 12;
		
		emptyInstructions = new EmptyInstructionSlot[numberOfInstructions];
		
		for (int i = 0 ; i < numberOfInstructions ; i++)
		{
			emptyInstructions[i] = Instantiate(emptyInstructionPrefab) as EmptyInstructionSlot;
			
			emptyInstructions[i].gameObject.name = "Instruction "+i;
			emptyInstructions[i].transform.parent = transform;
			emptyInstructions[i].transform.localPosition = Vector3.zero + (Vector3.down*60f*i);
			
		}
		
		for (int i = 0 ; i < numberOfInstructions-1 ; i++)
		{
			emptyInstructions[i].nextEmptySlot = emptyInstructions[i+1];
			emptyInstructions[i+1].previousEmptySlot = emptyInstructions[i];
		}
		
		InputManager.instance.OnSelectionChange += (selectedPlacables) => 
		{
			foreach (EmptyInstructionSlot emptyInstruction in emptyInstructions)
			{
				emptyInstruction.InsertInstruction(null);
//				emptyInstruction.ClearInstructionChangeEvent();
			}
			
			if (selectedPlacables.Count == 1 && selectedPlacables[0] is Grabber)
			{
				Grabber selected = selectedPlacables[0] as Grabber;
				for (int i = 0 ; i < numberOfInstructions ; i++)
				{
					emptyInstructions[i].InsertInstruction(draggableInstructions.GetDraggableInstructionClone(selected.GetInstruction(i)));
					
//					emptyInstructions[i].OnInstructionChange += (instruction) => selected.SetInstruction(i, instruction);
				}
			}
		};
	}
}
