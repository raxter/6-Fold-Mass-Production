using UnityEngine;
using System.Collections;

public class InstructionOption : MonoBehaviour 
{
	
	[SerializeField]
	InstructionSlot _instructionSlotFunctions;

	[SerializeField]
	SpriteBase _instructionIcon;
	
	public SpriteBase InstructionIcon { get { return _instructionIcon; } }
	
	[SerializeField]
	UIButton _instructionButton;
	
	[SerializeField]
	Grabber.Instruction _instruction;
	
	public Grabber.Instruction Instruction { get { return _instruction; } }
	
	public void Start()
	{
		Deselect ();
	}
	
	#region EZGUI
	public void SetSelected ()
	{
		if (!Selected)
		{
			_instructionSlotFunctions.SetSelected(_instruction);
		}
	}
	
	#endregion EZGUI
	
	
	#region selection stuff
	bool Selected { get { return _instructionIcon.transform.localScale == Vector3.zero; } }
	
	public void Select ()
	{
		_instructionIcon.transform.localScale = Vector3.zero;
	}
	
	public void Deselect ()
	{
		_instructionIcon.transform.localScale = Vector3.one;
	}
	
	#endregion selection stuff
	
}
