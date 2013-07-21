using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	UIButton _fastButton = null;
	[SerializeField]
	UIButton _fasterButton = null;
	[SerializeField]
	UIButton _fastestButton = null;
	
	Dictionary<LevelManager.SimulationSpeed, UIButton> _simulationSpeedButtons = new Dictionary<LevelManager.SimulationSpeed, UIButton>();
	
	[SerializeField]
	GameObject _buttonHolder = null;
	
	[SerializeField]
	SpriteText _targetText = null;
	
	[SerializeField]
	UIButton _testLevelButton;
	
	string _testLevelStartText;
	
	void TestLevel()
	{
		LevelEditorGUI.instance.EditorEnabled = !LevelEditorGUI.instance.EditorEnabled;
		if (!LevelEditorGUI.instance.EditorEnabled)
		{
			// editor disabled (testing level)
			_testLevelButton.spriteText.Text = "Edit Level";
		}
		else
		{
			// back to editing
			_testLevelButton.spriteText.Text = _testLevelStartText;
		}
	}
	
	void Start()
	{
		_simulationSpeedButtons[LevelManager.SimulationSpeed.Stopped] = _stopButton;
		_simulationSpeedButtons[LevelManager.SimulationSpeed.Paused]  = _pauseButton;
		_simulationSpeedButtons[LevelManager.SimulationSpeed.Normal]  = _playNormalButton;
		_simulationSpeedButtons[LevelManager.SimulationSpeed.Fast]    = _fastButton;
		_simulationSpeedButtons[LevelManager.SimulationSpeed.Faster]  = _fasterButton;
		_simulationSpeedButtons[LevelManager.SimulationSpeed.Fastest] = _fastestButton;
		
		_testLevelStartText = _testLevelButton.spriteText.Text;
		// save number of targets in save game file
		LevelManager.instance.ConstructionCompletedEvent += 
		 	() => _targetText.Text = "Target\n"+LevelManager.instance.completedConstructions+"/"+GridManager.instance.targetConstructions;
	
	
		LevelManager.instance.SimulationSpeedChangedEvent += 
		() =>
		{
			
			LevelManager.SimulationSpeed currentSpeed = LevelManager.instance.currentSpeed;
			
			
			foreach (UIButton button in _simulationSpeedButtons.Values)
			{
				button.controlIsEnabled = true;
			}
			_simulationSpeedButtons[currentSpeed].controlIsEnabled = false;
			
			if (currentSpeed == LevelManager.SimulationSpeed.Stopped)
				_stopButton.transform.localScale = Vector3.zero;	
			else
				_stopButton.transform.localScale = Vector3.one;
		};
		
		
		GridManager.instance.OnGridChangedEvent += RefreshButtons;
		RefreshButtons();
	}
	
	void RefreshButtons()
	{
		_buttonHolder.SetActive(!LevelEditorGUI.hasActiveInstance);
	}
}
