using UnityEngine;
using System.Collections;

public class GrabberProgramUI : SingletonBehaviour<GrabberProgramUI>
{
//	[SerializeField]
//	InstructionSlot [] _instructionSlots = null;
	
	[SerializeField]
	GameObject [] _grabberUIObjects = null;
	
//	bool guiEnabled = false;
	
//	void EnableGUI (bool enabled)
//	{
//		guiEnabled = enabled;
//
//
//		RefreshGrabberUIObjects();
//	}
	
	
	Grabber _displayedGrabber = null;
	
	
	IEnumerator Start () 
	{
//		GameManager.instance.GameStateChangedEvent += () => 
//		{
//			EnableGUI(GameManager.instance.gameState == GameManager.State.Construction);
//		};
		
		LevelManager.instance.GameStateChangedEvent += () =>
		{
			RefreshGrabberUIObjects();
		};
		
		GridManager.instance.OnGridChangedEvent += () =>
		{
			RefreshGrabberUIObjects();
		};
		
		yield return null;
		DisplayedGrabber = null;
		
		InputManager.instance.OnSelectionChange += OnSelectionChange;
	
	}
	
	
	void OnDestroy()
	{
		if (InputManager.hasInstance)
		{
			InputManager.instance.OnSelectionChange -= OnSelectionChange;
		}
	}
	
	void OnSelectionChange(System.Collections.Generic.List<HexCellPlaceable> selectedPlacables) 
	{
		Grabber selectedGrabber = null;
		foreach (HexCellPlaceable placable in selectedPlacables)
		{
			if (placable is Grabber)
			{
				if (selectedGrabber != null)
				{
					selectedGrabber = null;
					break;
				}
				selectedGrabber = placable as Grabber;
			}
		}
		
		DisplayedGrabber = selectedGrabber;
		
	}
	
	public Grabber DisplayedGrabber 
	{
		get 
		{
			return _displayedGrabber;
		}
		set
		{
			_displayedGrabber = value;
			
			
//			EnableGUI (value != null);
			RefreshGrabberUIObjects();
		}
	}
	
	#region EZGUI
	
	public void RefreshGrabberUIObjects()
	{
		bool active = true;
		if (_displayedGrabber == null)
			active = false;
		else
		{
			active = !GridManager.instance.IsLevelOptionActive(LevelOption.DisableGrabberAdjustments) &&
					 LevelManager.instance.gameState == LevelManager.State.Construction;
			
			if (LevelEditorGUI.hasActiveInstance)
				active = true;
		}
		
		transform.localScale = Vector3.one * ( active ? 1f : 0f );
//		foreach (GameObject uiObject in _grabberUIObjects)
//		{
//			uiObject.transform.localScale = Vector3.one * ( active ? 1f : 0f );
//		}
	}

//	public void InstructionSetAt (int _index)
//	{
//		if (_index + 1 < _instructionSlots.Length && DisplayedGrabber.GetInstruction(_index+1) == Grabber.Instruction.None)
//		{
//			_instructionSlots[_index+1].ToggleInstructionPanel();
//		}
//	}
	
	void ExtendGrabber()
	{
		if (DisplayedGrabber != null)
		{
			DisplayedGrabber.ExtendStartState();
		}
	}
	void RetractGrabber()
	{
		if (DisplayedGrabber != null)
		{
			DisplayedGrabber.RetractStartState();
		}
	}
	void RotateAntiGrabber()
	{
		if (DisplayedGrabber != null)
		{
			DisplayedGrabber.RotateAntiStartState();
		}
	}
	void RotateClockGrabber()
	{
		if (DisplayedGrabber != null)
		{
			DisplayedGrabber.RotateClockStartState();
		}
	}
	
	#endregion
	
	
}
