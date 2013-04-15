using UnityEngine;
using System.Collections;

public class HUDFunctions : MonoBehaviour 
{
	
	[SerializeField]
	UIButton grabberButton;
	
	
	void Start()
	{
		grabberButton.AddInputDelegate(GrabberInputDelegate);
	}
	
	public void GrabberInputDelegate(ref POINTER_INFO ptr)
	{
		switch (ptr.evt)
		{
			case POINTER_INFO.INPUT_EVENT.PRESS:
				GameManager.instance.SetSelectedMechanistIcon(HexCellPlaceableType.Grabber);
				break;
			case POINTER_INFO.INPUT_EVENT.TAP:
			case POINTER_INFO.INPUT_EVENT.RELEASE:
			case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
				GameManager.instance.UnSelecteMechanistIcon();
				break;
			
		}
	}
	
}
