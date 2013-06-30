using UnityEngine;
using System.Collections;

public class PartGenerator : Mechanism
{
	
	public override MechanismType MechanismType
	{
		get { return MechanismType.Generator; }
	}
	
	public Construction toGenerateConstruction;
	
	bool placeOnNextTurn = false;
	
	static int generatorCount = 0;
	static int constructionCount = 0;
	
	#region implemented abstract members of Mechanism
	
	protected override void MechanismStart ()
	{
	}

	protected override void MechanismUpdate ()
	{
	}
	#endregion
	
	bool movable = false;
	
	// Grabber code is (movable)(construction)
	public override string Encode()
	{
		string code = movable ? "1" : "0";
		
		if (toGenerateConstruction != null)
		{
			code += toGenerateConstruction.Encode();
		}
		
		
		return code;
	}
	
	public override bool Decode(string encoded)
	{
		movable = encoded[0] == '1';
		
		if (toGenerateConstruction != null)
		{
			ObjectPoolManager.DestroyObject(toGenerateConstruction);
			toGenerateConstruction = null;
		}
		if (encoded.Length > 1)
		{
			toGenerateConstruction = Construction.Decode(encoded.Substring(1));
			toGenerateConstruction.ignoreCollisions = true;
		}
		
		return true;
	}
	
	public void StartSimulation()
	{
		placeOnNextTurn = true;
		generatorCount = 0;
	}

	public Construction StepPreStart ()
	{
//		Debug.Log ("StepPreStart");
		if (hexCell != null)
		{
			if (placeOnNextTurn)
			{
				Construction construction;
				construction = Construction.Decode(toGenerateConstruction.Encode());
				
				construction.idNumber = generatorCount;
//				construction.gameObject.name = toGenerateConstruction.gameObject.name+" "+generatorCount;
//				part.PlaceAtLocation(Location);
				
				construction.transform.position = GridManager.instance.GetHexCell(Location).transform.position;
//				construction.AddToConstruction(construction);
//				construction.transform.localPosition = Vector3.zero;
				
				placeOnNextTurn = false;
				generatorCount += 1;
				return construction;
			}
			else if (hexCell.partOverCell == null)
			{ 
				placeOnNextTurn = true;
			}
		}
		return null;
	}
}



