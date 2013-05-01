using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameSettings : ScriptableObjectSingleton<GameSettings>
{
	
	public HexCell hexCellPrefab;
	
	public int gridFinalCellsFromWidth;
	public int gridFinalCellsFromHeight;
	
	public int gridWidth;
	
	public int gridHeight;

	public List<GrabbablePart> partPrefabs;
	
	public List<Mechanism> mechanismPrefabs;
	
	public PartGenerator generatorPrefab;
}