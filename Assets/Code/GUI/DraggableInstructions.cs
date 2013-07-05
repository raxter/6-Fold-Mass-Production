using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DraggableInstructions : MonoBehaviour 
{

//	[SerializeField]
//	DraggableInstruction rotateAntiButton;
	
	[SerializeField]
	List<DraggableInstruction> draggableInstructions = null;
	
	[SerializeField]
	EmptyInstructionSlot emptyInstructionPrefab = null;
	
	[SerializeField]
	GameObject emptyInstructionsRoot = null;
	
	[SerializeField]
	GameObject instructionsRoot = null;
	
	[SerializeField]
	SpriteBase selectionIcon = null;
	
	EmptyInstructionSlot [] emptyInstructions = null;
	
	Grabber selectedGrabber = null;
	
	
	void Start()
	{
//			
//			foreach (DraggableInstruction draggableInstruction in draggableInstructions)
//			{
//				draggableInstruction.button.IsDraggable = GameManager.instance.gameState == GameManager.State.Construction;
//			}
//			foreach (EmptyInstructionSlot emptyInstructionSlot in emptyInstructions)
//			{
//				emptyInstructionSlot.button.IsDraggable = GameManager.instance.gameState == GameManager.State.Construction;
//			}
//		};
		
		
		emptyInstructions = new EmptyInstructionSlot[Grabber.maximumInstuctions];
		
		for (int i = 0 ; i < Grabber.maximumInstuctions ; i++)
		{
			emptyInstructions[i] = ObjectPoolManager.GetObject(emptyInstructionPrefab);
			
			emptyInstructions[i].gameObject.name = "Instruction "+i;
			emptyInstructions[i].transform.parent = emptyInstructionsRoot.transform;
			emptyInstructions[i].transform.localPosition = Vector3.zero + (Vector3.down*60f*i);
			
			emptyInstructions[i].instructionIndex = i;
			emptyInstructions[i].draggableInstructions = draggableInstructions.ConvertAll<DraggableInstruction>( (di) => 
			{
				var clone = di.GetClone();
				
				clone.transform.parent = emptyInstructions[i].transform;
				clone.transform.localPosition = Vector3.zero - Vector3.forward;
				SetupDragDropBehaviour(clone);
				clone.occupiedSlot = emptyInstructions[i];
				return clone;
			});
			
			emptyInstructions[i].CurrentInstruction = Grabber.Instruction.None;
		}
		
		InputManager.instance.OnSelectionChange += (selectedPlacables) => 
		{
			
			if (selectedPlacables.Count == 1 && selectedPlacables[0] is Grabber)
			{
				selectedGrabber = selectedPlacables[0] as Grabber;
				RefreshEmptyInstructions();
			}
			else
			{
				HideUI();
			}
		};
		
		selectionIcon.transform.localScale = Vector3.zero;
		
		LevelManager.instance.GameStateChangedEvent += () =>
		{
			selectionIcon.transform.localScale = Vector3.one*(LevelManager.instance.gameState == LevelManager.State.Construction ? 0f : 1f);
		};
		
		LevelManager.instance.InstructionStartedEvent += () =>
		{
//			Debug.LogWarning("Instruction Step!");
			if (selectedGrabber != null)
			{
				selectionIcon.transform.localPosition = new Vector3(0, -60*selectedGrabber.InstructionCounter, 0);
			}
		};
		
		foreach (var db in draggableInstructions)
			SetupDragDropBehaviour(db);
		
		
		HideUI();
	}
	
	void HideUI()
	{
		gameObject.SetActive(false);
	}
	
	void RefreshEmptyInstructions()
	{
		gameObject.SetActive(true);
		bool blankRemaining = false;
		for (int i = 0 ; i < Grabber.maximumInstuctions ; i++)
		{
			
			emptyInstructions[i].CurrentInstruction = selectedGrabber.GetInstruction(i);
			emptyInstructions[i].gameObject.SetActive(!blankRemaining);
			if (emptyInstructions[i].CurrentInstruction == Grabber.Instruction.None)
			{
//				blankRemaining = true;
			}
			
		}
	}
	
	public void SetupDragDropBehaviour(DraggableInstruction draggableButton)
	{
		
		draggableButton.button.SetDragDropDelegate((parms) => 
		{
			if (parms.evt == EZDragDropEvent.Begin)
			{
				Debug.Log ("Begin "+parms.dragObj/*+":"+parms.dragObj.transform.position+":"+parms.ptr.ray*/);
//				Vector3 origin = parms.ptr.ray.origin;
//				origin.z = 0;
//				parms.dragObj.transform.position = origin;
					
			}
			if (parms.evt == EZDragDropEvent.Cancelled)
			{
				Debug.Log ("Cancelled "+parms.dragObj);
				Debug.Log ("DropTarget: "+parms.dragObj.DropTarget);
				
				if (LevelManager.instance.gameState != LevelManager.State.Construction)
				{
					return;
				}
				
				if (draggableButton.occupiedSlot != null)
				{
					selectedGrabber.SetInstruction(draggableButton.occupiedSlot.instructionIndex, Grabber.Instruction.None);
					RefreshEmptyInstructions();
				}
				
				EmptyInstructionSlot instructionSlot = parms.dragObj.DropTarget == null ? null : parms.dragObj.DropTarget.GetComponent<EmptyInstructionSlot>();
				
				
				if (instructionSlot != null && selectedGrabber != null)
				{
					selectedGrabber.SetInstruction(instructionSlot.instructionIndex, draggableButton.instructionRepresented);
//					instructionSlot.CurrentInstruction = draggableButton.instructionRepresented;
					RefreshEmptyInstructions();
				}
//				else
//				{
//					Debug.Log(draggableButton.occupiedSlot);
//				}
				
			}
			if (parms.evt == EZDragDropEvent.CancelDone)
			{
				Debug.Log ("CancelDone "+parms.dragObj);
				
			}
		});
	}
}
