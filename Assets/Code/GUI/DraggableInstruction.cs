using UnityEngine;
using System.Collections;

public class DraggableInstruction : MonoBehaviour 
{
	
	public UIButton button;
	
	public Grabber.Instruction instructionRepresented = Grabber.Instruction.NoOp;
	
	public EmptyInstructionSlot occupiedSlot { get; set; }
	
	
	void Start()
	{
		LevelManager.instance.GameStateChangedEvent += UpdateDraggable;
		InputManager.instance.OnSelectionChange += GameSelectionChangedEvent;
	}
	void OnDestroy()
	{
		if (LevelManager.hasInstance)
		{
			LevelManager.instance.GameStateChangedEvent -= UpdateDraggable;
		}
		if (InputManager.hasInstance)
		{
			InputManager.instance.OnSelectionChange -= GameSelectionChangedEvent;
		}
	}
	
	void GameSelectionChangedEvent(System.Collections.Generic.List<HexCellPlaceable> selectedPlacables)
	{
		UpdateDraggable();
	}
	
	void UpdateDraggable()
	{
		button.IsDraggable = LevelManager.instance.gameState == LevelManager.State.Construction;
	}
	
//	bool _isClone = false;
//	
//	public bool isClone { get { return _isClone; } }
//	
	public DraggableInstruction GetClone()
	{
		DraggableInstruction clone = Instantiate(this) as DraggableInstruction;
		System.Array.ForEach<SpriteRoot>(clone.GetComponentsInChildren<SpriteRoot>(),(obj) => obj.isClone = true);
		clone.transform.parent = transform.parent;
		clone.transform.localPosition = Vector3.zero;
//		clone._isClone = true;
		
		return clone;
	}
	
	
}
