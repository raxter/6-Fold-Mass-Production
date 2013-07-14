using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	
	
	public bool movable = false;

	// Grabber code is (movable)(construction)
	public override IEnumerable<IEncodable> Encode()
	{
		yield return new EncodableSubGroup(base.Encode());
		yield return (EncodableInt)(movable ? 1 : 0);
		
		
		yield return toGenerateConstruction as IEncodable ?? (EncodableInt)0;
		 // TODO save and load position of mechanism (put in mechanism class!)
		
	}
	
	public override bool Decode(Encoding encoding)
	{
//		List<object> encoded = new List<object>(encodings);
		
		Debug.Log ("Decoding Generator: "+encoding.DebugString());
		base.Decode(encoding.SubEncoding(0));
		movable = (int)encoding.Int(1) == 1;
		
		if (toGenerateConstruction != null)
		{
			ObjectPoolManager.DestroyObject(toGenerateConstruction);
			toGenerateConstruction = null;
		}
		if (encoding.Count > 1)
		{
			toGenerateConstruction = Construction.DecodeCreate(encoding.SubEncoding(2));
			toGenerateConstruction.ignoreCollisions = true;
		}
		
		return true;
	}
	
	string encodedConsruction = "";
	
	public void StartSimulation()
	{
		placeOnNextTurn = true;
		generatorCount = 0;
		encodedConsruction = Encoding.Encode(toGenerateConstruction);
	}

	public Construction StepPreStart ()
	{
//		Debug.Log ("StepPreStart");
		if (hexCell != null && toGenerateConstruction.Count > 0)
		{
			if (placeOnNextTurn)
			{
				Construction construction;
				construction = Construction.DecodeCreate(encodedConsruction);
				
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



