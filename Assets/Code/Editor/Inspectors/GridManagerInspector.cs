using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(GridManager))]
public class GridManagerInspector : Editor 
{
	
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		
		GridManager gridManager = target as GridManager;
		
		
		if (GUILayout.Button("Create HexCell Grid"))
		{
			gridManager.CreateHexCellMap();
		}
		
		if (GUILayout.Button("Destroy HexCell Grid"))
		{
			gridManager.DestroyHexCellMap();
		}
	}
	
	
	
}
