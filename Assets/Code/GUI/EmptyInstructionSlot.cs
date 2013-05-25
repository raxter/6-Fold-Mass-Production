using UnityEngine;
using System.Collections;

public class EmptyInstructionSlot : MonoBehaviour 
{
	
	DraggableInstruction currentInstruction = null;
	
	public EmptyInstructionSlot nextEmptySlot = null;
	public EmptyInstructionSlot previousEmptySlot = null;
	
	public delegate void OnInstructionChangeDelegate(Grabber.Instruction instruction);
	
	public event OnInstructionChangeDelegate OnInstructionChange = null; 
	
	public void ClearInstructionChangeEvent()
	{
		OnInstructionChange = null;
	}
	
	
	void ClearCurrentInstruction()
	{
		Debug.Log ("Clear "+this.name);
		
		if (currentInstruction != null)
		{
			currentInstruction.occupiedSlot = null;
		}
		ReplaceCurrentInstruction(null);
	}
	
	
	void ReplaceCurrentInstruction(DraggableInstruction newInstruction)
	{
		Debug.Log ("Replace "+this.name+": "+currentInstruction+" -> "+newInstruction);
		if (currentInstruction != null)
		{
			if (currentInstruction.occupiedSlot == null)
			{
				Destroy(currentInstruction.gameObject);
			}
		}
		currentInstruction = newInstruction;
		
		if (OnInstructionChange != null)
		{
			OnInstructionChange(currentInstruction == null ? Grabber.Instruction.NoOp : currentInstruction.instructionRepresented);
		}
		
		if (currentInstruction != null)
		{
			currentInstruction.transform.parent = transform;
			currentInstruction.transform.localPosition = Vector3.zero-Vector3.forward;
	//				
			currentInstruction.occupiedSlot = this;
		}
	}
	
	void ShiftCurrentInstructionUp()
	{
		Debug.Log ("ShiftCurrentInstructionUp "+this.name);
		if (nextEmptySlot != null)
		{
			nextEmptySlot.ShiftCurrentInstructionUp();
			nextEmptySlot.ReplaceCurrentInstruction(currentInstruction);
		}
		
	}
	
	void ShiftNextInstructionDown()
	{
		Debug.Log ("ShiftNextInstructionDown "+this.name);
		if (nextEmptySlot != null)
		{
			ReplaceCurrentInstruction(nextEmptySlot.currentInstruction);
			nextEmptySlot.ShiftNextInstructionDown();
		}
		
	}
	
	
	public void InsertInstruction(DraggableInstruction newInstruction)
	{
		Debug.Log("newInstruction: "+newInstruction);
		Debug.Log("currentInstruction: "+currentInstruction);
		
		if (newInstruction != null)
		{
			if (currentInstruction != null)
			{
				ShiftCurrentInstructionUp();
			}
			if (previousEmptySlot != null && previousEmptySlot.currentInstruction == null)
			{
				previousEmptySlot.InsertInstruction(newInstruction);
			}
			else
			{
				ReplaceCurrentInstruction(newInstruction);
			}
		}
		else // deleting
		{
			
			if (currentInstruction != null)
			{
				ClearCurrentInstruction();
				ShiftNextInstructionDown();
			}
			else
			{
			}
		}
		
	}
}
