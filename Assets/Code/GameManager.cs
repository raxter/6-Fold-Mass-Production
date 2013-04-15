using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GameManager : SingletonBehaviour<GameManager> 
{
	
	[SerializeField]
	List<HexCellPlaceable> placeablePrefabs;
	
	Dictionary<HexCellPlaceableType, HexCellPlaceable> cellMechanisms;
	
	HexCellPlaceable unplacedMechanism = null;
	
	void Start()
	{
		cellMechanisms = new Dictionary<HexCellPlaceableType, HexCellPlaceable>();
		
		cellMechanisms.Add(HexCellPlaceableType.None, null);
		
		foreach (HexCellPlaceable placeable in placeablePrefabs)
		{
			cellMechanisms.Add(placeable.MechanismType, placeable);
		}
	}
	
	public void SetSelectedMechanistIcon(HexCellPlaceableType cellMechanismType)
	{
		if (cellMechanismType == HexCellPlaceableType.None)
			return;
		
		unplacedMechanism = (GameObject.Instantiate(cellMechanisms[cellMechanismType].gameObject) as GameObject).GetComponent<HexCellPlaceable>();
		
		unplacedMechanism.StartDrag();
		
	}
	
	public void UnSelecteMechanistIcon ()
	{
		if (unplacedMechanism != null)
		{
			unplacedMechanism.StopDrag();
			
		}
	}
	
}









