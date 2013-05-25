using UnityEngine;
using System.Collections;

public class EmptyInstructionSlot : MonoBehaviour 
{
	DraggableInstruction currentInstruction = null;
	
	
	
	public void SetCurrentInstruction(DraggableInstruction draggedInstruction)
	{
		if (currentInstruction != null)
		{
			Destroy(currentInstruction.gameObject);
		}
		currentInstruction = draggedInstruction;
		Debug.Log("draggedInstruction: "+draggedInstruction);
		if (currentInstruction != null)
		{
			
			currentInstruction.transform.parent = transform;
			currentInstruction.transform.localPosition = Vector3.zero-Vector3.forward;
			
//			currentInstruction.button.IsDraggable = true;
			
//			currentInstruction.button.SetDragDropDelegate((currentParms) => 
//			{
//				if (currentParms.evt == EZDragDropEvent.Cancelled)
//				{
//					Debug.Log("Cancelled "+currentParms.dragObj.DropTarget+":"+currentInstruction.gameObject);
//				}
//			});
		}
		
	}
}
