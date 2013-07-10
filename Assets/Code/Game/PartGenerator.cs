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

	public override string Get3CharUniqueID ()
	{
		return "GEN";
	}
	// Grabber code is (movable)(construction)
	public override IEnumerable Encode()
	{
		yield return movable ? 1 : 0;
		
		if (toGenerateConstruction != null)
		{
			yield return toGenerateConstruction.Encode();
		}
		
	}
	
	public override bool Decode(string encoded)
	{
		Debug.Log ("Decoding Generator: "+encoded);
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
	
	string encodedConsruction = "";
	
	public void StartSimulation()
	{
		placeOnNextTurn = true;
		generatorCount = 0;
		encodedConsruction = CharSerializer.Encode(toGenerateConstruction);
	}

	public Construction StepPreStart ()
	{
//		Debug.Log ("StepPreStart");
		if (hexCell != null)
		{
			if (placeOnNextTurn)
			{
				Construction construction;
				construction = Construction.Decode(encodedConsruction);
				
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



