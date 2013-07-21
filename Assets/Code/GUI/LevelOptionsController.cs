using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelOptionsController : MonoBehaviour 
{
	[SerializeField]
	UIButton grabbersPlacable;
	
	[SerializeField]
	UIButton weldersPlacable;
	
	[SerializeField]
	UIButton grabberAdjustable;
	
	[SerializeField]
	UIButton basicInstructionsOnly;
	
	Dictionary<UIButton, string> startText = new Dictionary<UIButton, string>();
	Dictionary<UIButton, LevelOption> associatedOption = new Dictionary<UIButton, LevelOption>();
	
	void Start()
	{
		startText[grabbersPlacable] = grabbersPlacable.spriteText.Text;
		startText[weldersPlacable] = weldersPlacable.spriteText.Text;
		startText[grabberAdjustable] = grabberAdjustable.spriteText.Text;
		startText[basicInstructionsOnly] = basicInstructionsOnly.spriteText.Text;
		
		associatedOption[grabbersPlacable]      = LevelOption.DisableGrabberPlacement;
		associatedOption[weldersPlacable]       = LevelOption.DisableWelderPlacement;
		associatedOption[grabberAdjustable]     = LevelOption.DisableGrabberAdjustments;
		associatedOption[basicInstructionsOnly] = LevelOption.DisableAdvancedInstructions;
		
		GridManager.instance.OnGridChangedEvent += UpdateText;
		
		UpdateText();
	}
	
	void UpdateText()
	{
		foreach(UIButton button in startText.Keys)
		{
			button.spriteText.Text = startText[button] + "\n" + (GridManager.instance.IsLevelOptionActive(associatedOption[button]) ? "Disabled" : "Enabled");
		}
	}
	
	void ToggleLevelOption(LevelOption levelOption)
	{
		GridManager.instance.SetLevelOption(levelOption, !GridManager.instance.IsLevelOptionActive(levelOption));
		UpdateText();
	}
	
	#region EZGUI
	
	void GrabbersPlacableToggled()
	{
		ToggleLevelOption(LevelOption.DisableGrabberPlacement);
	}
	
	void WeldersPlacableToggled()
	{
		ToggleLevelOption(LevelOption.DisableWelderPlacement);
	}
	
	void GrabberAdjustableToggled()
	{
		ToggleLevelOption(LevelOption.DisableGrabberAdjustments);
	}
	
	void BasicInstructionsOnlyToggled()
	{
		ToggleLevelOption(LevelOption.DisableAdvancedInstructions);
	}
	
	#endregion
	
}
