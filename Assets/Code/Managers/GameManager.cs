using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GameManager : SingletonBehaviour<GameManager> 
{
	
	[SerializeField]
	List<Mechanism> mechanismPrefabs = null;
	
	
	[SerializeField]
	PartGenerator generatorPrefab = null;
	
	Dictionary<MechanismType, Mechanism> cellMechanisms;
	
	Mechanism unplacedMechanism = null;
	
	[SerializeField]
	GUIEnabler [] _enableableGUIObjects = null;
	
//	[SerializeField]
//	UIButton _playButton = null;
	[SerializeField]
	UIButton _stopButton = null;
	
	[SerializeField]
	SpriteText _targetText = null;
	
	LevelSettings.Level _currentLevel = null;
	
	int _completedConstructions = -1;
	int completedConstructions
	{
		get { return _completedConstructions; }
		set 
		{ 
			_completedConstructions = value; 
			_targetText.Text = "Target\n"+_completedConstructions+"/"+_currentLevel.targetConstructions;
		}
	}
	
	[SerializeField]
	float instructionsPerSecondNormal = 1;
	[SerializeField]
	float instructionsPerSecondFast = 2;
	[SerializeField]
	float instructionsPerSecondFaster = 4;
	[SerializeField]
	float instructionsPerSecondFastest = 16;
	
	float _stepsPerSecond = 80f;
	int _stepsPerInstruction = 60;
	
	public bool guiEnabled { get; private set; }
	
	public enum State {Construction, Simulation, SimulationFailed};
	
	public State gameState;
	
	public string loadLevelOnStart = "";
	
	void Start()
	{
		LoadLevel(loadLevelOnStart);
		
		gameState = State.Construction;
		cellMechanisms = new Dictionary<MechanismType, Mechanism>();
		
		cellMechanisms.Add(MechanismType.None, null);
		
		foreach (Mechanism mechanism in mechanismPrefabs)
		{
			cellMechanisms.Add(mechanism.MechanismType, mechanism);
		}
		StopSimulation();
	}

	void LoadLevel (string leveName)
	{
		// clear all parts first when reloading! TODO
		
		_currentLevel = LevelSettings.instance.GetLevel(leveName);
		
		foreach (LevelSettings.GeneratorDetails generatorDetails in _currentLevel.generators)
		{
			PartGenerator generator = (GameObject.Instantiate(generatorPrefab.gameObject) as GameObject).GetComponent<PartGenerator>();
			generator.toGeneratePrefab = generatorDetails.toGeneratePrefab;
			generator.PlaceAtLocation(generatorDetails.location);
		}
		completedConstructions = 0;
	}

	public void AddCompletedConstruction ()
	{
		completedConstructions += 1;
		
		if (_completedConstructions >= _currentLevel.targetConstructions)
		{
		}
	}

	public void IncompleteConstruction ()
	{
		throw new System.NotImplementedException ();
	}
	
	
	public void CreateMechanism(MechanismType cellMechanismType)
	{
		if (cellMechanismType == MechanismType.None)
			return;
		
		unplacedMechanism = (GameObject.Instantiate(cellMechanisms[cellMechanismType].gameObject) as GameObject).GetComponent<Mechanism>();
		
		InputManager.instance.StartDraggingUnplaced(unplacedMechanism);
		
	}
	
	IEnumerator SimulationCoroutine()
	{
		int programCounter = -1;
		List<HexCellPlaceable> placeables = new List<HexCellPlaceable>();
		List<Grabber> grabbers = new List<Grabber>();
		List<PartGenerator> generators = new List<PartGenerator>();
		
		HashSet<GrabbablePart> parts = new HashSet<GrabbablePart>();
		
		foreach (HexCell hexCell in GridManager.instance.GetAllCells())
		{
			if (hexCell.placedPlaceable != null)
			{
				placeables.Add(hexCell.placedPlaceable);
				if (hexCell.placedPlaceable is Grabber)
				{
					grabbers.Add(hexCell.placedPlaceable as Grabber);
				}
				if (hexCell.placedPlaceable is PartGenerator)
				{
				generators.Add(hexCell.placedPlaceable as PartGenerator);
				}
			}
		}
		
		
		int stepsToDoThisFrame = 0;
		float spareTime = 0;
		
		foreach (PartGenerator generator in generators)
		{
			generator.StartSimulation();
		}
		foreach (Grabber grabber in grabbers)
		{
			grabber.StartSimulation(_stepsPerInstruction);
		}
		
		while (true)
		{
			
			foreach (PartGenerator generator in generators)
			{
				GrabbablePart newPart = generator.StepPreStart();
				if (newPart)
				{
					parts.Add(newPart);
				}
			}
			// perform instruction
			foreach (Grabber grabber in grabbers)
			{
				grabber.PerformInstruction();
//				Debug.Log("PerformInstruction "+grabber._instructionCounter);
			}
			foreach (Grabber grabber in grabbers)
			{
				grabber.PerformPostInstruction();
//				Debug.Log("PerformPostInstruction "+grabber._instructionCounter);
			}
			
			// perform step
			while (true)
			{
				
				bool debugForceExit = false;
				
				if (debugForceExit) // make this true in debugger to get out of an infinite loop
				{
					yield break;
				}
				
				if (gameState == State.SimulationFailed)
				{
					yield return null;
					continue;
				}
				
				if (gameState == State.Construction)
				{
					Debug.Log ("Simulation ending");
					
					foreach (Grabber grabber in grabbers)
					{
						grabber.EndSimulation();
					}
					yield break;
				}
				
				
				if (stepsToDoThisFrame == 0)
				{
					yield return null;
					// in new frame!
					float timeThisFrame = Time.deltaTime+spareTime;
					float stepTime = 1f/_stepsPerSecond;
					stepsToDoThisFrame = (int)(timeThisFrame/stepTime);
					spareTime = timeThisFrame - stepsToDoThisFrame*stepTime;
//					Debug.Log (timeThisFrame+"("+spareTime+")");
				}
				
				stepsToDoThisFrame -= 1;
				
				
				bool allFinished = true;
				
				foreach (Grabber grabber in grabbers)
				{
					if (!grabber.PerformStep())
					{
						allFinished = false;
					}
//					Debug.Log("PerformStep "+grabber._stepCounter);
				}
				
				
//				foreach (Grabber grabber in grabbers)
//				{
//					if (!grabber.StepFinished())
//					{
//						allFinished = false;
//					}
//				}
				// check for collisions
			
				// if collision, pause and exit
				foreach (GrabbablePart part in parts)
				{
					GrabbablePart other = part.CheckForCollisions();
					if (other != null)
					{
						part.selected = true;
						other.selected = true;
						gameState = State.SimulationFailed;
						continue;
					}
				}
				
				if (allFinished)
				{
				
					break;
				}
				
				
			}
			
		}
		
		
		
		// repeat
		
		yield break;
	}
	
	
	public void PlaySimulationNormal()
	{
		_stepsPerSecond =_stepsPerInstruction * instructionsPerSecondNormal;
		PlaySimulation();
	}
	
	public void PlaySimulationFast()
	{
		_stepsPerSecond =_stepsPerInstruction * instructionsPerSecondFast;
		PlaySimulation();
	}
	
	public void PlaySimulationFaster()
	{
		_stepsPerSecond =_stepsPerInstruction * instructionsPerSecondFaster;
		PlaySimulation();
	}
	
	public void PlaySimulationFastest()
	{
		_stepsPerSecond =_stepsPerInstruction * instructionsPerSecondFastest;
		PlaySimulation();
	}
	
	
	public void PlaySimulation()
	{
		if (gameState != State.Construction)
		{
			return;
		}
		
		Debug.Log ("PlaySimulation");
		// Disable Input
		foreach (GUIEnabler guiElement in _enableableGUIObjects)
		{
			guiElement.EnableGUI(false);
			guiEnabled = false;
		}
		
//		_playButton.transform.localScale = Vector3.zero;
		_stopButton.transform.localScale = Vector3.one;
		
		
		gameState = State.Simulation;
		
		StartCoroutine(SimulationCoroutine());
		
		
	}
	
//	IEnumerator SimulationStopCoroutine()
//	{
//		yield return null;
//	}
	
	public void StopSimulation()
	{
		gameState = State.Construction;
		
		Debug.Log ("StopSimulation");
		// Reenable Input
		
//		StartCoroutine(SimulationStopCoroutine());
		
		foreach (HexCell hexCell in GridManager.instance.GetAllCells())
		{
			GrabbablePart part = hexCell.part;
			if (part != null)
			{
				part.PlaceAtLocation(null);
				GameObject.Destroy(part.gameObject);
			}
			
			Grabber grabber = hexCell.placedMechanism as Grabber;
			if (grabber != null)
			{
				grabber.ClearClamp();
			}
		}
		
		foreach (GUIEnabler guiElement in _enableableGUIObjects)
		{
			guiElement.EnableGUI(true);
			guiEnabled = true;
		}
		
		// change stop to play
//		_playButton.transform.localScale = Vector3.one;
		_stopButton.transform.localScale = Vector3.zero;
		
		
		completedConstructions = 0;
	}
	
//	public void UnSelecteMechanistIcon ()
//	{
//		if (unplacedMechanism != null)
//		{
//			unplacedMechanism.StopDrag();
//			
//		}
//	}
	
	public void PartCollisionOccured (GrabbablePart part, GrabbablePart otherPart)
	{
		Debug.Log ("SimulationFailed");
//		gameState = State.SimulationFailed;
	}
}









