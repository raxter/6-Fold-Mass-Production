using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grabber : Mechanism, IPooledObject
{
	
	public override MechanismType MechanismType { get { return MechanismType.Grabber; } }
	
	public enum Instruction {None, TurnClock, TurnAnti, Extend, Retract, RotateClock, RotateAnti, Grab, Drop, GrabDrop, RestartMark, NoOp};
	
	public static int maximumInstuctions { get { return 12; } }
//	Instruction [] instructions = new Instruction [16];
	List<Instruction> instructions = new List<Instruction>();
	
	bool saveOnUpdate = false;

	#region IPooledObject implementation
	public void OnPoolActivate ()
	{
	}

	public void OnPoolDeactivate ()
	{
		instructions.Clear();
	}
	#endregion	
	public void ReplaceInstruction(int i, Instruction instruction)
	{
		SetInstruction(i, Instruction.None);
		SetInstruction(i, instruction);
	}
	public void SetInstruction(int i, Instruction instruction)
	{
		if (instruction == Instruction.None)
		{
			if (i < instructions.Count)
			{
				instructions.RemoveAt(i);
			}
		}
		else
		{
			if (i < instructions.Count)
				instructions.Insert(i, instruction);
			else
				instructions.Add(instruction);
		}
		saveOnUpdate = true;
	}
	public Instruction GetInstruction(int i)
	{
		if (i < instructions.Count)
			return instructions[i];
		else
			return Instruction.None;
	}
	
	int _instructionCounter = -1;
	public int InstructionCounter { get { return _instructionCounter; } }
	
	public int _stepsPerInstruction = -1;
	
	public int _stepCounter = -1;
	
	int _restartMark = 0;
	
	[SerializeField]
	GameObject [] _arms;
	
	[SerializeField]
	GameObject clamp;
	
	[SerializeField]
	MeshRenderer _clampOpenRenderer;
	[SerializeField]
	MeshRenderer _clampClosedRenderer;
	
	
	// Grabber code is (direction)(extention)(instruction0)(instruction1)...
	public override string Encode()
	{
		string code = "";
		
		code += (int)_startState.direction;
		code += _startState.extention;
		
		foreach (Instruction instruction in instructions)
		{
			if (instruction == Instruction.None)
			{
				break;
			}
			code += CharSerializer.ToCode((int)instruction);
		}
		
		return code;
	}
	
	public override bool Decode(string encoded)
	{
		_startState.direction = (HexMetrics.Direction)CharSerializer.ToNumber(encoded[0]);
		_startState.extention = CharSerializer.ToNumber(encoded[1]);
		MoveToStartState();
		
		for (int i = 0 ; i < instructions.Count ; i++)
		{
			instructions[i] = Instruction.None;
		}
		int instructionOffset = 2;
		for (int i = 0 ; i < encoded.Length-instructionOffset ; i++)
		{
			instructions.Add((Instruction)CharSerializer.ToNumber(encoded[i+instructionOffset]));
		}
		
		return true;
	}
	
	bool ClampOpen
	{
		get
		{
			return _clampOpenRenderer.enabled;
		}
		set
		{
			_clampOpenRenderer.enabled = value;
			_clampClosedRenderer.enabled = !value;
		}
	}
	
	struct GrabberState
	{
		public GrabberState(HexMetrics.Direction dir, int ext, bool clampOpen)
		{
			direction = dir;
			extention = ext;
			this.clampOpen = clampOpen;
			rotation = 0;
		}
		public HexMetrics.Direction direction;
		public int rotation;
		public int extention;
		public bool clampOpen;
	}
	
//	public GrabbablePart heldPart 
//	{ 
//		get {return _heldPart; } 
//		private set
//		{
//			
////			if (_heldPart == null && value != null)
////			{
////				Debug.Log ("Held Part "+_heldPart+" newheldpart "+value, value);
////				Debug.Log (value.ParentConstruction, value.ParentConstruction);
////				if (value.ParentConstruction.heldAndMovingGrabber != null) // we are grabbing a part that is already grabbed!
////				{
////					Debug.Log (value.ParentConstruction.heldAndMovingGrabber, value.ParentConstruction.heldAndMovingGrabber);
////					Debug.LogWarning("Multiple grab");
////					GameManager.instance.MultipleGrabOccured(value.ParentConstruction.heldAndMovingGrabber, this);
////					return;
////				}
////			}
//			
//			if (_heldPart != null)
//			{
//				Debug.Log ("Dropping part", this);
//				Debug.Log ("Dropping part", _heldPart);
//				_heldPart.ParentConstruction.heldAndMovingGrabber = null;
//			}
//			else
//			{
//				Debug.Log ("Empty", this);
//				Debug.Log ("Empty", value);
//				if (value != null && value.ParentConstruction.heldAndMovingGrabber != null)
//				{
//					// part is already grabber
//					Debug.LogWarning("Multiple grab, part already grabbed", value);
//					GameManager.instance.MultipleGrabOccured(value.ParentConstruction.heldAndMovingGrabber, this);
//				}
//			}
//			_heldPart = value;
//			if (_heldPart != null)
//			{
//				_heldPart.ParentConstruction.heldAndMovingGrabber = this;
//				Debug.Log ("Grabbing part "+_currentInstruction, this);
//				Debug.Log ("Grabbing part", _heldPart);
//			}
//		} 
//	}
//	GrabbablePart _heldPart;
//	System.Action _doAtEndOfInstruction;
	
	
	void Grab(GrabbablePart part)
	{
		GrabberManager.instance.RegisterGrab(this, part);
	}
	void Drop(GrabbablePart part)
	{
		GrabberManager.instance.RegisterDrop(this, part);
	}
	
	GrabberState _startState;
	
	GrabberState _startStepState;
	GrabberState _endStepState;
	Instruction _currentInstruction = Instruction.None;
	bool _isMoveingInstruction = false;
	
	
	public void ExtendStartState()
	{
		_startState.extention = Mathf.Clamp(_startState.extention + 1, 0, 2);
		MoveToStartState();
		GridManager.instance.SaveLayout();
	}
	public void RetractStartState()
	{
		_startState.extention = Mathf.Clamp(_startState.extention - 1, 0, 2);
		MoveToStartState();
		GridManager.instance.SaveLayout();
	}
	public void RotateClockStartState()
	{
		_startState.direction = (HexMetrics.Direction)(((int)_startState.direction + 1) % 6);
		MoveToStartState();
		GridManager.instance.SaveLayout();
	}
	public void RotateAntiStartState()
	{
		_startState.direction = (HexMetrics.Direction)(((int)_startState.direction + 5) % 6);
		MoveToStartState();
		GridManager.instance.SaveLayout();
	}
	
	
	public void StartSimulation(int stepsPerInstruction)
	{
		_stepsPerInstruction = stepsPerInstruction;
		_instructionCounter = 0;
		_restartMark = 0;
		_endStepState = _startState;
		ClampOpen = _startState.clampOpen;
	}
	
	
	public void EndSimulation()
	{
		MoveToStartState();
	}
	
	void MoveToStartState()
	{
		MoveToState(_startState);
	}
	
	void MoveToState(GrabberState grabberState)
	{
		MoveToState(grabberState.extention, ((int)grabberState.direction)*-60f, 0f);
	}
	
	void MoveToState(float extentionValue, float directionValue, float rotationValue)
	{
		GrabbablePart heldPart = GrabberManager.instance.GetPartHeldBy(this);
		
		
		
		
		if (heldPart != null) 
		{
			if (heldPart.ParentConstruction == null)
			{
				Debug.LogError("Parent is null!", heldPart);
			}
			heldPart.ParentConstruction.gameObject.transform.parent = clamp.transform;
		}
		
		clamp.transform.localRotation = Quaternion.Euler(0, 0, rotationValue);
		
//		Debug.Log(clamp.transform.localRotation.eulerAngles);
		
		transform.localRotation = Quaternion.Euler(0, 0, directionValue);
		for (int i = 0 ; i < _arms.Length ; i++)
		{
			_arms[i].transform.localPosition = new Vector3(0, extentionValue*GameSettings.instance.hexCellPrefab.Height/(_arms.Length), 1);
		}
		
		if (heldPart != null) 
		{
			heldPart.ParentConstruction.gameObject.transform.parent = null;
		}
		
//		clamp.transform.localRotation = Quaternion.Euler(0, 0, 0);
	}
	
	// returns true for succesful
	public void PerformInstruction ()
	{
//		if (instructions[_instructionCounter] == Instruction.None)
//		{
//			_instructionCounter = _restartMark; // will be 0 is no restart mark set
//		}
		
//		Debug.Log("Deregistering "+heldPart+" at "+LocationAtClampEnd().x+":"+LocationAtClampEnd().y);
		
//		Debug.Log ("PerformInstruction "+_currentInstruction+": "+instructions[_instructionCounter]);
		
		_startStepState.rotation = 0;
		_startStepState = _endStepState;
	
//		Debug.Break ();
//		Debug.Log ("before: "+_endStepState.direction);
		
//		Debug.Log ("instructionCounter "+_instructionCounter);
//		Debug.Log ("instruction = "+instructions[_instructionCounter]);
		
		if (instructions.Count == 0)
		{
			_currentInstruction = Instruction.None;
		}
		else
		{
			_currentInstruction = instructions[_instructionCounter];
		}
		_isMoveingInstruction = false;
//		_doAtEndOfInstruction = null;
		
		switch (_currentInstruction)
		{
			case Instruction.TurnClock:
			{
				_endStepState.direction += 1;
				_isMoveingInstruction = true;
			}
			break;
			case Instruction.TurnAnti:
			{
				_endStepState.direction += 5;
				_isMoveingInstruction = true;
			}
			break;
			case Instruction.Extend:
			{
				_endStepState.extention += 1;
				_isMoveingInstruction = true;
			}
			break;
			case Instruction.Retract:
			{
				_endStepState.extention -= 1;
				_isMoveingInstruction = true;
			}
			break;
			case Instruction.RotateClock:
			{
				_endStepState.rotation += 1;
				_isMoveingInstruction = true;
			}
			break;
			case Instruction.RotateAnti:
			{
				_endStepState.rotation -= 1;
				_isMoveingInstruction = true;
			}
			break;
			case Instruction.RestartMark:
			{
				_restartMark = _instructionCounter+1;
			}
			break;
			case Instruction.Grab:
			{
				// dealt with in another function
			}
			break;
			case Instruction.Drop:
			{
				GrabbablePart heldPart = GrabberManager.instance.GetPartHeldBy(this);
				if (heldPart != null)
				{
				
					IntVector2 locationAtClamp = LocationAtClamp();
					if (GridManager.instance.GetHexCell(locationAtClamp) == null)
					{
						// trying to drop into non grid location
						
						GameManager.instance.gameState = GameManager.State.SimulationFailed;
						heldPart.highlighted = true;
						break;
					}
					
					GrabberManager.instance.RegisterDrop(this, heldPart);
				}
//				else
//				{
//					Debug.Log ("No part to drop", this);
//				}
			}
			break;
		}
		
		
		_endStepState.direction = (HexMetrics.Direction)((int)_endStepState.direction % 6);
		_endStepState.extention = Mathf.Clamp(_endStepState.extention, 0, 2);
		
		
//		Debug.Log ("after: "+_endStepState.direction);
		
		_stepCounter = 0;
		_instructionCounter += 1;
		if (_instructionCounter == instructions.Count)
		{
			_instructionCounter = _restartMark;
			if (_instructionCounter == instructions.Count)
			{
				_instructionCounter = instructions.Count - 1;
			}
		}
		
	}
	
	
	public void PerformPostInstruction ()
	{
		if (_currentInstruction == Instruction.Grab)
		{
			GrabbablePart heldPart = GrabberManager.instance.GetPartHeldBy(this);
			if (heldPart == null)
			{
				HexCell underClamp = GridManager.instance.GetHexCell(LocationAtClamp());
				if (underClamp == null)// clamp is over a non cell
				{
					return;
				}
				GrabbablePart partUnderClamp = underClamp.partOverCell;
				
				if (partUnderClamp == null) // there is nothing to grab *or* there is a part, but it is held and is about to move this turn
				{
					return;
				}
				// else
				
				GrabberManager.instance.RegisterGrab(this, partUnderClamp);
				
				
			}
		}
	}
	
	
	
	IntVector2 LocationAtClampEnd()
	{
		return Location+HexMetrics.GetGridOffset(_endStepState.direction)*(_endStepState.extention+1);
	}
	IntVector2 LocationAtClamp()
	{
		return Location+HexMetrics.GetGridOffset(_startStepState.direction)*(_startStepState.extention+1);
	}
	
	void EndOfInstruction()
	{
//		if (_heldPart != null)
//		{
//			_heldPart.ParentConstruction.heldAndMovingGrabber = null;
//		}
	}
	
	// returns true if finished
	public bool PerformStep ()
	{
		_stepCounter += 1 ;
		if (StepFinished())
		{
			EndOfInstruction();
			return true;
		}
		
		
		// check for multiple holds
		GrabbablePart heldPart = GrabberManager.instance.GetPartHeldBy(this);
		if (heldPart != null)
		{
			Grabber otherMovingGrabber = null;
			foreach(Grabber grabber in GrabberManager.instance.GetAllGrabbersHolding(heldPart.ParentConstruction))
			{
				if (grabber != this && grabber._isMoveingInstruction)
				{
					otherMovingGrabber = grabber;
				}
				
				if (otherMovingGrabber != null)
				{
					GrabberManager.instance.GetAllGrabbersHolding(heldPart.ParentConstruction);
					Debug.LogWarning("Multiple grab, two parts welded together while held", heldPart);
					Debug.LogWarning("Multiple grab, two parts welded together while held", this);
					Debug.LogWarning("Multiple grab, two parts welded together while held", otherMovingGrabber);
					Debug.LogWarning("Multiple grab, two parts welded together while held", GrabberManager.instance.GetPartHeldBy(this));
					Debug.LogWarning("Multiple grab, two parts welded together while held", GrabberManager.instance.GetPartHeldBy(otherMovingGrabber));
					GameManager.instance.MultipleGrabOccured(this, otherMovingGrabber);
					
					return false;
				}
			}
		}
		
		float percStep = (float)_stepCounter/(float)_stepsPerInstruction;
		
		
		float startRotation = (int)_startStepState.rotation*-60f;
		float endRotation = (int)_endStepState.rotation*-60f;
		
		if (endRotation - startRotation > 180)
			endRotation -= 360;
		if (endRotation - startRotation < -180)
			endRotation += 360;
		
		float rotationValue = (startRotation*(1f-percStep)) + (endRotation*percStep);
		
		float startAngle = (int)_startStepState.direction*-60f;
		float endAngle = (int)_endStepState.direction*-60f;
		
		if (endAngle - startAngle > 180)
			endAngle -= 360;
		if (endAngle - startAngle < -180)
			endAngle += 360;
		
		float angleValue = (startAngle*(1f-percStep)) + (endAngle*percStep);
		
		float startExtention = _startStepState.extention;
		float endExtention = _endStepState.extention;
		
		float extentionValue = (startExtention*(1f-percStep)) + (endExtention*percStep);
		
		MoveToState(extentionValue, angleValue, rotationValue);
		
		
		if (_stepCounter == /**/1)/*/_stepsPerInstruction/2)/**/
		{
//			Debug.Log(_stepCounter+" == "+_stepsPerInstruction/2+":"+instructions[_instructionCounter]);
			switch (_currentInstruction)
			{
				case Instruction.Grab:
				{
					ClampOpen = false;
				}
				break;
				case Instruction.Drop:
				{
					ClampOpen = true;
				}
				break;
				case Instruction.GrabDrop:
				{
					ClampOpen = !ClampOpen;
				}
				break;
			}
		}
		
		
		
		return false;
	}
	
	
	
	bool StepFinished()
	{
		return _stepCounter > _stepsPerInstruction;
	}
	
	protected override void MechanismUpdate()
	{
		if (saveOnUpdate)
		{
			GridManager.instance.SaveLayout();
			saveOnUpdate = false;
		}
	}
	
	protected override void MechanismStart()
	{
		Quaternion rotation = transform.rotation;
		transform.rotation = Quaternion.Euler(0,0,0);
		clamp.transform.position = transform.position+new Vector3(0,GameSettings.instance.hexCellPrefab.Height*(_startState.extention+1),0);
		transform.rotation = rotation;
//		_startState = new GrabberState(HexMetrics.Direction.Up, 0, true);
	}
	
//	protected override void MechanismUpdate()
//	{	
//		
//			
//		if (Input.GetKeyDown(KeyCode.Alpha1))
//		{
//			ExtendArm(0, 1f);
//		}
//		if (Input.GetKeyDown(KeyCode.Alpha2))
//		{
//			ExtendArm(1, 1f);
//		}
//		if (Input.GetKeyDown(KeyCode.Alpha3))
//		{
//			ExtendArm(2, 1f);
//		}
//		
//		if (Input.GetKeyDown(KeyCode.A))
//		{
//			targetRotation += 60;
//			rotationTimeRemaining = 1;
//		}
//		if (Input.GetKeyDown(KeyCode.D))
//		{
//			targetRotation -= 60;
//			rotationTimeRemaining = 1;
//		}
//		
//		while (targetRotation < 0)  targetRotation += 360;
//		while (targetRotation >= 360) targetRotation -= 360;
//		
//		foreach(ArmInfo arm in _arms)
//		{
//			if (Time.deltaTime > arm.timeRemaining)
//			{
//				arm.sprite.SetOffset(new Vector3(0, arm.targetOffset, 0));
//				arm.timeRemaining = 0;
//			}
//			else
//			{
//				float remainingLength = arm.targetOffset - arm.sprite.offset.y;
//				float timeRatio = Time.deltaTime/arm.timeRemaining;
//				float lengthToMove = timeRatio*remainingLength;
//				arm.timeRemaining -= Time.deltaTime;
//				arm.sprite.SetOffset(arm.sprite.offset+new Vector3(0,lengthToMove,0));
//			}
//		}
//		
//		if (Time.deltaTime > rotationTimeRemaining)
//		{
//			transform.localRotation = Quaternion.Euler(0,0,targetRotation);
//			rotationTimeRemaining = 0;
//		}
//		else
//		{
////			Debug.Log (targetRotation+":"+transform.localRotation.eulerAngles.z);
//			float remainingAngle = targetRotation - transform.localRotation.eulerAngles.z;
//			while (remainingAngle >= 180) remainingAngle -= 360;
//			while (remainingAngle < -180) remainingAngle += 360;
//			float timeRatio = Time.deltaTime/rotationTimeRemaining;
//			float amountToRotate = timeRatio*remainingAngle;
//			rotationTimeRemaining -= Time.deltaTime;
//			transform.localRotation = Quaternion.Euler(0,0,transform.localRotation.eulerAngles.z + amountToRotate);
//		}
//		
//	}
	
	
//	public void ExtendArm(int length, float time)
//	{
////		Debug.Log ("ExtendArm(int length = "+length+", float time = "+time+")");
//		float extendLength = length*_hexCell.Height;
//		
//		float partialLength = extendLength/(float)(_arms.Length-1);
//		
//		for (int i = 0 ; i < _arms.Length ; i++)
//		{
////			_arms[i].startOffset = _arms[i].sprite.offset.y;
//			_arms[i].targetOffset = partialLength*i;
//			_arms[i].timeRemaining = time;
////			_arms[i].SetOffset(new Vector3(0,partialLength*i,0));
//		}
//	}
	
}












