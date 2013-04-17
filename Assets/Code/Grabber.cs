using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grabber : HexCellPlaceable
{
	
	public override HexCellPlaceableType MechanismType { get { return HexCellPlaceableType.Grabber; } }
	
	public enum Instruction {None, RotateLeft, RotateRight, Extend, Retract, Grab, Drop, GrabDrop, Mark, GoToMark};
	
	public Instruction [] instructions = new Instruction [16];
	
	int instructionPointer = 0;
	
	[System.Serializable]
	public class ArmInfo
	{
		public SpriteBase sprite;
		
		[System.NonSerialized]
		public float targetOffset;
		
		[System.NonSerialized]
		public float timeRemaining;
	}
	
	[SerializeField]
	ArmInfo [] _arms;
	
	
	float targetRotation = 0;
	float rotationTimeRemaining = 0;
	
	
	protected override void PlaceableUpdate()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			ExtendArm(0, 1f);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			ExtendArm(1, 1f);
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			ExtendArm(2, 1f);
		}
		
		if (Input.GetKeyDown(KeyCode.A))
		{
			targetRotation += 60;
			rotationTimeRemaining = 1;
		}
		if (Input.GetKeyDown(KeyCode.D))
		{
			targetRotation -= 60;
			rotationTimeRemaining = 1;
		}
		
		while (targetRotation < 0)  targetRotation += 360;
		while (targetRotation >= 360) targetRotation -= 360;
		
		foreach(ArmInfo arm in _arms)
		{
			if (Time.deltaTime > arm.timeRemaining)
			{
				arm.sprite.SetOffset(new Vector3(0, arm.targetOffset, 0));
				arm.timeRemaining = 0;
			}
			else
			{
				float remainingLength = arm.targetOffset - arm.sprite.offset.y;
				float timeRatio = Time.deltaTime/arm.timeRemaining;
				float lengthToMove = timeRatio*remainingLength;
				arm.timeRemaining -= Time.deltaTime;
				arm.sprite.SetOffset(arm.sprite.offset+new Vector3(0,lengthToMove,0));
			}
		}
		
		if (Time.deltaTime > rotationTimeRemaining)
		{
			transform.localRotation = Quaternion.Euler(0,0,targetRotation);
			rotationTimeRemaining = 0;
		}
		else
		{
//			Debug.Log (targetRotation+":"+transform.localRotation.eulerAngles.z);
			float remainingAngle = targetRotation - transform.localRotation.eulerAngles.z;
			while (remainingAngle >= 180) remainingAngle -= 360;
			while (remainingAngle < -180) remainingAngle += 360;
			float timeRatio = Time.deltaTime/rotationTimeRemaining;
			float amountToRotate = timeRatio*remainingAngle;
			rotationTimeRemaining -= Time.deltaTime;
			transform.localRotation = Quaternion.Euler(0,0,transform.localRotation.eulerAngles.z + amountToRotate);
		}
		
	}
	
	
	public void ExtendArm(int length, float time)
	{
//		Debug.Log ("ExtendArm(int length = "+length+", float time = "+time+")");
		float extendLength = length*_hexCell.Height;
		
		float partialLength = extendLength/(float)(_arms.Length-1);
		
		for (int i = 0 ; i < _arms.Length ; i++)
		{
//			_arms[i].startOffset = _arms[i].sprite.offset.y;
			_arms[i].targetOffset = partialLength*i;
			_arms[i].timeRemaining = time;
//			_arms[i].SetOffset(new Vector3(0,partialLength*i,0));
		}
	}
	
}
