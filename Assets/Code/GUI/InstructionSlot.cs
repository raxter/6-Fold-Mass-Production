using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InstructionSlot : MonoBehaviour 
{
	[SerializeField]
	GrabberProgramUI _grabberProgramUI;
	
	[SerializeField]
	UIButton _currentInstructionButton;
	
	[SerializeField]
	UIPanel _otherInstructionsPanel;
	
	bool panelExpanded = false;
	
	Grabber.Instruction currentInstruction = Grabber.Instruction.None;
	
	[SerializeField]
	InstructionOption [] instructionOptions;
	
	Dictionary<Grabber.Instruction, SpriteBase> instructionIcons = new Dictionary<Grabber.Instruction, SpriteBase>();
	
	// Use this for initialization
	void Start () 
	{
		foreach (InstructionOption option in instructionOptions)
		{
			GameObject iconGameObjectClone = GameObject.Instantiate(option.InstructionIcon.gameObject) as GameObject;
			iconGameObjectClone.transform.parent = _currentInstructionButton.transform;
			iconGameObjectClone.transform.localPosition = new Vector3 (0,0,-1);
			instructionIcons[option.Instruction] = iconGameObjectClone.GetComponent<SpriteBase>();
			
		}
		RefreshInstuctionIcons();
	}
	
	public void CloseInstructionPanel()
	{
		UpdateInstructionsPanel(false);
	}
	
	public void ToggleInstructionPanel()
	{
		UpdateInstructionsPanel(!panelExpanded);
	}
	
	// Update is called once per frame
	void UpdateInstructionsPanel (bool newExpanded) 
	{
		if (newExpanded)
		{
			_grabberProgramUI.CloseAllSlots();
			if (newExpanded != panelExpanded)
			{
				_otherInstructionsPanel.BringIn();
			}
		}
		else
		{
//			_otherInstructionsPanel.Dismiss();
			if (newExpanded != panelExpanded)
			{
				_otherInstructionsPanel.Dismiss();
			}
		}
		panelExpanded = newExpanded;
	}
	
	public void SetSelected (Grabber.Instruction instruction)
	{
		currentInstruction = instruction;
		
		
		RefreshInstuctionIcons();
	}
	
	public void RefreshInstuctionIcons ()
	{
		foreach (InstructionOption option in instructionOptions)
		{
			if (option.Instruction == currentInstruction)
			{
				option.Select();
				
				instructionIcons[option.Instruction].transform.localScale = Vector3.one;
			}
			else
			{
				option.Deselect();
				instructionIcons[option.Instruction].transform.localScale = Vector3.zero;
			}
		}
	}
}




