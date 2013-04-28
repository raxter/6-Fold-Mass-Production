using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grabber : Mechanism
{
	
	public override MechanismType MechanismType { get { return MechanismType.Grabber; } }
	
	public enum Instruction {None, RotateClock, RotateAnti, Extend, Retract, Grab, Drop, GrabDrop, RestartMark, NoOp};
	
	public Instruction [] instructions = new Instruction [16];
	
	public int _instructionCounter = -1;
	
	public int _stepsPerInstruction = -1;
	
	public int _stepCounter = -1;
	
	[SerializeField]
	GameObject [] _arms;
	
	[SerializeField]
	GameObject clamp;
	
	[SerializeField]
	MeshRenderer _clampOpenRenderer;
	[SerializeField]
	MeshRenderer _clampClosedRenderer;
	
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
		}
		public HexMetrics.Direction direction;
		public int extention;
		public bool clampOpen;
	}
	
	GrabbablePart heldPart;
	System.Action _doAtEndOfInstruction;
	
	GrabberState _startState;
	
	GrabberState _startStepState;
	GrabberState _endStepState;
	Instruction _currentInstruction = Instruction.None;

	public void ClearClamp ()
	{
		if (heldPart != null)
		{
			GameObject.Destroy(heldPart.gameObject);
			heldPart = null;
		}
	}
	
	protected override void PlaceableStart()
	{
		_startState = new GrabberState(HexMetrics.Direction.Up, 0, true);
	}
	
	
	public void ExtendStartState()
	{
		_startState.extention = Mathf.Clamp(_startState.extention + 1, 0, 2);
		MoveToStartState();
	}
	public void RetractStartState()
	{
		_startState.extention = Mathf.Clamp(_startState.extention - 1, 0, 2);
		MoveToStartState();
	}
	public void RotateClockStartState()
	{
		_startState.direction = (HexMetrics.Direction)(((int)_startState.direction + 1) % 6);
		MoveToStartState();
	}
	public void RotateAntiStartState()
	{
		_startState.direction = (HexMetrics.Direction)(((int)_startState.direction + 5) % 6);
		MoveToStartState();
	}
	
	
	public void StartSimulation(int stepsPerInstruction)
	{
		_stepsPerInstruction = stepsPerInstruction;
		_instructionCounter = 0;
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
		MoveToState(grabberState.extention, ((int)grabberState.direction)*-60f);
	}
	
	void MoveToState(float extentionValue, float directionValue)
	{
		
		transform.localRotation = Quaternion.Euler(0, 0, directionValue);
		for (int i = 0 ; i < _arms.Length ; i++)
		{
			_arms[i].transform.localPosition = new Vector3(0, extentionValue*hexCell.Height/(_arms.Length), 1);
		}
		
	}
	
	// returns true for succesful
	public void PerformInstruction ()
	{
		if (instructions[_instructionCounter] == Instruction.None)
		{
			// goto mark if there is a mark
			// TODO
			// otherwise go to start
			_instructionCounter = 0;
		}
		
		Debug.Log("Deregistering "+heldPart+" at "+LocationAtClampEnd().x+":"+LocationAtClampEnd().y);
		GridManager.instance.GetHexCell(LocationAtClampEnd()).partHeldOverCell = null;
		
//		Debug.Log ("PerformInstruction "+_currentInstruction+": "+instructions[_instructionCounter]);
		
		_startStepState = _endStepState;
	
//		Debug.Break ();
//		Debug.Log ("before: "+_endStepState.direction);
		
//		Debug.Log ("instructionCounter "+_instructionCounter);
//		Debug.Log ("instruction = "+instructions[_instructionCounter]);
		
		_currentInstruction = instructions[_instructionCounter];
		
		_doAtEndOfInstruction = null;
		
		switch (_currentInstruction)
		{
			case Instruction.RotateClock:
			{
				_endStepState.direction += 5;
			}
			break;
			case Instruction.RotateAnti:
			{
				_endStepState.direction += 1;
			}
			break;
			case Instruction.Extend:
			{
				_endStepState.extention += 1;
			}
			break;
			case Instruction.Retract:
			{
				_endStepState.extention -= 1;
			}
			break;
			case Instruction.Grab:
			{
			// performed in post
//				if (heldPart == null)
//				{
//					HexCell underClamp = GridManager.instance.GetHexCell(LocationAtClamp());
//					GrabbablePart part = underClamp.part;
//					if (part != null)
//					{
//						part.PlaceAtLocation(null);
//						part.gameObject.transform.parent = clamp.transform;
//						heldPart = part;
//					}
//				}
			}
			break;
			case Instruction.Drop:
			{
				if (heldPart != null)
				{
				
					IntVector2 locationAtClamp = LocationAtClamp();
					if (GridManager.instance.GetHexCell(locationAtClamp) == null)
					{
						// trying to drop into non grid location
						
						GameManager.instance.gameState = GameManager.State.SimulationFailed;
						heldPart.selected = true;
						break;
					}
				
					GrabbablePart partToCheck = heldPart;
					_doAtEndOfInstruction = () => 
					{
						partToCheck.CheckForFinish();
					};
//					HexCell underClamp = GridManager.instance.GetHexCell(LocationAtClamp());
					heldPart.gameObject.transform.parent = null;
					
					heldPart.PlaceAtLocation(locationAtClamp);
					heldPart = null;
				}
			}
			break;
		}
		_endStepState.direction = (HexMetrics.Direction)((int)_endStepState.direction % 6);
		_endStepState.extention = Mathf.Clamp(_endStepState.extention, 0, 2);
		
		
//		Debug.Log ("after: "+_endStepState.direction);
		
		_stepCounter = 0;
		_instructionCounter += 1;
		if (_instructionCounter == instructions.Length)
		{
			_instructionCounter = 0;
		}
		
	}
	
	
	public void PerformPostInstruction ()
	{
		if (_currentInstruction == Instruction.Grab)
		{
			if (heldPart == null)
			{
				HexCell underClamp = GridManager.instance.GetHexCell(LocationAtClamp());
				GrabbablePart part = underClamp.partOnCell;
				if (part != null)
				{
					part.PlaceAtLocation(null);
					part.gameObject.transform.parent = clamp.transform;
					heldPart = part;
				}
			}
		}
	}
	
	
	IntVector2 LocationAtClampEnd()
	{
		return Location+HexMetrics.GetRelativeLocation(_endStepState.direction)*(_endStepState.extention+1);
	}
	IntVector2 LocationAtClamp()
	{
		return Location+HexMetrics.GetRelativeLocation(_startStepState.direction)*(_startStepState.extention+1);
	}
	
	void EndOfInstruction()
	{
		
	}
	
	// returns true if finished
	public bool PerformStep ()
	{
		if (StepFinished())
		{
			IntVector2 locationAtClampEnd = LocationAtClampEnd();
			Debug.Log("Registering "+heldPart+" at "+locationAtClampEnd.x+":"+locationAtClampEnd.y);
			HexCell overClampCell = GridManager.instance.GetHexCell(locationAtClampEnd);
			overClampCell.partHeldOverCell = heldPart;
			
			if (_doAtEndOfInstruction != null)
			{
				_doAtEndOfInstruction();
				_doAtEndOfInstruction = null;
			}
			return true;
		}
		
		float percStep = (float)_stepCounter/(float)_stepsPerInstruction;
		
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
		
		MoveToState(extentionValue, angleValue);
		
		
		if (_stepCounter == 0)//_stepsPerInstruction/2)
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
		
		
		_stepCounter += 1 ;
		
		return false;
	}
	
	
	
	bool StepFinished()
	{
		return _stepCounter > _stepsPerInstruction;
	}
	
	protected override void MechanismUpdate()
	{
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
		
	}
	
	
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
