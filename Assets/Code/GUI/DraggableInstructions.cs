using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DraggableInstructions : MonoBehaviour 
{

//	[SerializeField]
//	DraggableInstruction rotateAntiButton;
	
	[SerializeField]
	List<DraggableInstruction> draggableButtons;
	
	
	Dictionary<IUIObject, DraggableInstruction> copyObject = new Dictionary<IUIObject, DraggableInstruction>();
	
	void Start()
	{
		foreach (var db in draggableButtons)
			SetupDragDropBehaviour(db);
	}
	
	public void SetupDragDropBehaviour(DraggableInstruction draggableButton)
	{
		draggableButton.button.SetDragDropDelegate((parms) => 
		{
			if (parms.evt == EZDragDropEvent.Begin)
			{
				Debug.Log ("Begin "+parms.dragObj);
				if (!draggableButton.isClone)
				{
					copyObject[parms.dragObj] = draggableButton.GetClone();
				}
				else
				{
					copyObject[parms.dragObj] = draggableButton;
				}
					
			}
			if (parms.evt == EZDragDropEvent.Cancelled)
			{
				Debug.Log ("Cancelled "+parms.dragObj);
				EmptyInstructionSlot emptySlot = parms.dragObj.DropTarget == null? null : parms.dragObj.DropTarget.GetComponent<EmptyInstructionSlot>();
				
				if (emptySlot != null)
				{
					emptySlot.SetCurrentInstruction(copyObject[parms.dragObj]);
					SetupDragDropBehaviour(copyObject[parms.dragObj]);
					copyObject.Remove(parms.dragObj);
					
				}
			}
			if (parms.evt == EZDragDropEvent.CancelDone)
			{
				Debug.Log ("CancelDone "+parms.dragObj);
				if (copyObject.ContainsKey(parms.dragObj))
				{
					Destroy(copyObject[parms.dragObj].gameObject);
					copyObject.Remove(parms.dragObj);
				}
			}
		});
	}
}
