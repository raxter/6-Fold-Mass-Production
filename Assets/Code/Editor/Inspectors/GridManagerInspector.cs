using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(GridManager))]
public class GridManagerInspector : Editor 
{
	
	[MenuItem("Custom/Clear Editor Save")]
	public void ClearEditorSave()
	{
		LevelDataManager.DeleteAll();
	}
	
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		
		GridManager gridManager = target as GridManager;
		
		
		if (GUILayout.Button("Create HexCell Grid"))
		{
			gridManager.CreateHexCellMap();
			foreach (HexCell hexCell in gridManager.GetAllCells())
			{
				EditorUtility.SetDirty(hexCell);
			}
		}
		
		if (GUILayout.Button("Destroy HexCell Grid"))
		{
			gridManager.DestroyHexCellMap();
		}
	}
	
	
	
}
