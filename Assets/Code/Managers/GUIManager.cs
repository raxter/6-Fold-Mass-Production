using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour
{
	
//	[SerializeField]
//	UIButton _playButton = null;
	[SerializeField]
	UIButton _stopButton = null;
	[SerializeField]
	UIButton _playNormalButton = null;
	[SerializeField]
	UIButton _pauseButton = null;
	
	[SerializeField]
	SpriteText _targetText = null;
	
	
	void Start()
	{
		// save number of targets in save game file
		LevelManager.instance.ConstructionCompletedEvent += 
		 	() => _targetText.Text = "Target\n"+LevelManager.instance.completedConstructions+"/"+GridManager.instance.targetConstructions;
	
	
		LevelManager.instance.SimulationSpeedChangedEvent += 
		() =>
		{
			LevelManager.SimulationSpeed currentSpeed = LevelManager.instance.currentSpeed;
			
			if (currentSpeed == LevelManager.SimulationSpeed.Stopped)
			{
				// change stop to play
//				_playNormalButton.transform.localScale = Vector3.one;
//				_pauseButton.transform.localScale = Vector3.zero;
				_stopButton.transform.localScale = Vector3.zero;
						
			}
//			else if (currentSpeed == GameManager.SimulationSpeed.Paused)
//			{
////				_playNormalButton.transform.localScale = Vector3.one;
////				_pauseButton.transform.localScale = Vector3.zero;
//			}
			else
			{
//				_playNormalButton.transform.localScale = Vector3.zero;
//				_pauseButton.transform.localScale = Vector3.one;
				_stopButton.transform.localScale = Vector3.one;
			}
		};
	
	}
	
}
