using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameSettings : ScriptableObjectSingleton<GameSettings>
{
	
	public HexCell hexCellPrefab;
	public Construction constructionPrefab;
	public GameObject weldPrefab;
	
	public int gridFinalCellsFromWidth;
	public int gridFinalCellsFromHeight;
	
	public int gridWidth;
	
	public int gridHeight;

	public List<GrabbablePart> partPrefabs;
	
	public List<Mechanism> mechanismPrefabs;
	
//	public PartGenerator generatorPrefab;
	
	
	public GrabbablePart GetPartPrefab(PartType partType)
	{
		foreach(GrabbablePart partPrefab in partPrefabs)
		{
			if (partPrefab.partType == partType)
			{
				return partPrefab;
			}
		}
		
		return null;
	}
	
	
	public bool debugOutput = false; 
	
	
}
