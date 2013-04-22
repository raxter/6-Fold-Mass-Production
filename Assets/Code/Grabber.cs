using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grabber : HexCellPlaceable
{
	
	public override HexCellPlaceableType MechanismType { get { return HexCellPlaceableType.Grabber; } }
	
	public enum Instruction {None, RotateClock, RotateAnti, Extend, Retract, Grab, Drop, GrabDrop, Mark, GoToMark};
	
	public Instruction [] instructions = new Instruction [16];
	
	public int _instructionCounter = -1;
	
	public int _stepsPerInstruction = -1;
	
	public int _stepCounter = -1;
	
	int instructionPointer = 0;
	
	[SerializeField]
	GameObject [] _arms;
	
	float targetRotation = 0;
	float rotationTimeRemaining = 0;
	
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
	
	GrabberState _startState;
	
	GrabberState _startStepState;
	GrabberState _endStepState;
	
	protected override void PlaceableStart()
	{
		_startState = new GrabberState(HexMetrics.Direction.Up, 0, true);
	}
	
	public void StartSimulation(int stepsPerInstruction)
	{
		_stepsPerInstruction = stepsPerInstruction;
		_instructionCounter = 0;
		_endStepState = _startState;
		ClampOpen = _startState.clampOpen;
	}
	
	public void PerformInstruction ()
	{
		_startStepState = _endStepState;
	
//		Debug.Break ();
//		Debug.Log ("before: "+_endStepState.direction);
		
//		Debug.Log ("instructionCounter "+_instructionCounter);
//		Debug.Log ("instruction = "+instructions[_instructionCounter]);
		switch (instructions[_instructionCounter])
		{
			case Instruction.RotateClock:
			{
				_endStepState.direction += 1;
			}
			break;
			case Instruction.RotateAnti:
			{
				_endStepState.direction += 5;
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
	
	public void PerformStep ()
	{
		if (StepFinished())
		{
			return;
		}
		
		float percStep = (float)_stepCounter/(float)_stepsPerInstruction;
		
		float startAngle = (int)_startStepState.direction*60f;
		float endAngle = (int)_endStepState.direction*60f;
		
		if (endAngle - startAngle > 180)
			endAngle -= 360;
		if (endAngle - startAngle < -180)
			endAngle += 360;
		
//		Debug.Log (startAngle+":"+endAngle);
//		Debug.Log (percStep+" : "+((startAngle*(1f-percStep)) + (endAngle*percStep)));
		transform.localRotation = Quaternion.Euler(0, 0, (startAngle*(1f-percStep)) + (endAngle*percStep));
		
		float startExtention = _startStepState.extention;
		float endExtention = _endStepState.extention;
		
		float extentionValue = (startExtention*(1f-percStep)) + (endExtention*percStep);
		
		for (int i = 0 ; i < _arms.Length ; i++)
		{
			_arms[i].transform.localPosition = new Vector3(0, extentionValue*_hexCell.Height/(_arms.Length), 1);
		}
		
		
		if (_stepCounter == _stepsPerInstruction/2)
		{
			Debug.Log(_stepCounter+" == "+_stepsPerInstruction/2+":"+instructions[_instructionCounter]);
			switch (instructions[_instructionCounter])
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
	}
	
	
	
	public bool StepFinished()
	{
		return _stepCounter >= _stepsPerInstruction;
	}
	
	protected override void PlaceableUpdate()
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
