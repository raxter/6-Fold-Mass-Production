using UnityEngine;
using System.Collections;

public class InstructionSlotFunctions : MonoBehaviour {
	
	[SerializeField]
	UIPanel _otherInstructionsPanel;
	
	bool panelExpanded = false;
	
	// Use this for initialization
	void Start () 
	{
		UpdateInstructionsPanel();
	}
	
	public void ToggleInstructionPanel()
	{
		panelExpanded = !panelExpanded;
		UpdateInstructionsPanel();
	}
	
	// Update is called once per frame
	void UpdateInstructionsPanel () 
	{
		
		if (panelExpanded)
		{
			_otherInstructionsPanel.BringIn();
		}
		else
		{
			_otherInstructionsPanel.Dismiss();
		}
	}
	
}
