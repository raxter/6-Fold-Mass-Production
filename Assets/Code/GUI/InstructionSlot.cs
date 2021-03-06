//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//
//public class InstructionSlot : MonoBehaviour
//{
//	bool _inputEnabled = true;
//	public void EnableGUI (bool enabled)
//	{
//		_inputEnabled = enabled;
//	}
//
//	public void Display (bool show)
//	{
//		_currentInstructionButton.renderer.enabled = show;
//		_currentInstructionButton.enabled = show;
//		foreach (SpriteBase sprite in instructionIcons.Values)
//		{
//			sprite.renderer.enabled = show;
//		}
//	}
//
//	public void DisplayNoneOperation (bool show)
//	{
//		instructionOptions[instructionOptions.Length -1].Display(show);
//	}
//
//	public void DisplayNoneOperationAfterTransision (bool show)
//	{
//		_otherInstructionsPanel.OnTransitionFinished(() => DisplayNoneOperation (show));
//	}
//	
//	[SerializeField]
//	GrabberProgramUI _grabberProgramUI = null;
//	
//	[SerializeField]
//	UIButton _currentInstructionButton = null;
//	
//	[SerializeField]
//	UIPanel _otherInstructionsPanel = null;
//	
//	bool panelExpanded = false;
//	
//	public Grabber.Instruction currentInstruction = Grabber.Instruction.None;
//	
//	[SerializeField]
//	InstructionOption [] instructionOptions = null;
//	
//	int _index = -1;
//	
//	Dictionary<Grabber.Instruction, SpriteBase> instructionIcons = new Dictionary<Grabber.Instruction, SpriteBase>();
//	
//	// Use this for initialization
//	void Start () 
//	{
//		
//		foreach (InstructionOption option in instructionOptions)
//		{
//			GameObject iconGameObjectClone = GameObject.Instantiate(option.InstructionIcon.gameObject) as GameObject;
//			iconGameObjectClone.transform.parent = _currentInstructionButton.transform;
//			iconGameObjectClone.transform.localPosition = new Vector3 (0,0,-1);
//			instructionIcons[option.Instruction] = iconGameObjectClone.GetComponent<SpriteBase>();
//			
//		}
//		RefreshInstuctionIcons();
//		
//	}
//	
//	public void SetIndex(int i)
//	{
//		_index = i;
//		SetCurrentSelected ();
//	}
//	
////	public void CloseInstructionPanel()
////	{
////		UpdateInstructionsPanel(false);
////	}
////	
////	public void ToggleInstructionPanel()
////	{
////		UpdateInstructionsPanel(!panelExpanded);
////	}
//	
//	// Update is called once per frame
////	void UpdateInstructionsPanel (bool newExpanded) 
////	{
////		if (GameManager.instance.gameState != GameManager.State.Construction)
////		{
////			return;
////		}
////		if (_inputEnabled == false)
////		{
////			if (!panelExpanded)
////			{
////				return;
////				// panel is closed, do nothing
////			}
////			else
////			{
////				// panel is open, make it closed
////				newExpanded = false;
////			}
////		}
////		
////		
////		if (newExpanded)
////		{
////			_grabberProgramUI.CloseAllSlots();
////			if (!panelExpanded) // only bring in if in construction state
////			{
////				_otherInstructionsPanel.BringIn();
////			}
////		}
////		else
////		{
////			if (panelExpanded)
////			{
////				_otherInstructionsPanel.Dismiss();
////			}
////		}
////		panelExpanded = newExpanded;
////	}
//	
//	void SetCurrentSelected ()
//	{
//		if (_grabberProgramUI.DisplayedGrabber != null)
//		{
//			gameObject.SetActive(true);
//			currentInstruction = _grabberProgramUI.DisplayedGrabber.GetInstruction(_index);
//		}
//		else
//		{
//			gameObject.SetActive(false);
//		}
//		
//		RefreshInstuctionIcons();
//	}
//	
//	public void SetSelected (Grabber.Instruction instruction)
//	{
//		currentInstruction = instruction;
//		
//		if (_grabberProgramUI.DisplayedGrabber != null)
//		{
//			_grabberProgramUI.DisplayedGrabber.SetInstruction(_index, currentInstruction);
//		}
//		RefreshInstuctionIcons();
//		
////		if (_grabberProgramUI.DisplayedGrabber != null)
////		{
////			_grabberProgramUI.InstructionSetAt(_index);
////		}
//	}
//	
//	public void RefreshInstuctionIcons ()
//	{
//		foreach (InstructionOption option in instructionOptions)
//		{
//			if (option.Instruction == currentInstruction)
//			{
//				option.Select();
//				
//				instructionIcons[option.Instruction].transform.localScale = Vector3.one;
//			}
//			else
//			{
//				option.Deselect();
//				instructionIcons[option.Instruction].transform.localScale = Vector3.zero;
//			}
//		}
//		
////		_grabberProgramUI.RefreshDisplayedSlots();
//	}
//}
//
//
//
//
