using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ConstructionDefinitionCreator))]
public class ConstructionDefinitionCreatorInspector : Editor
{
	
	GrabbablePart GetPartPrefabFromType(PartType type)
	{
		foreach(GrabbablePart partPrefab in GameSettings.instance.partPrefabs)
		{
			if (partPrefab.partType == type)
			{
				return partPrefab;
			}
		}
		return null;
	}
	
	
	public override void OnInspectorGUI()
	{
		ConstructionDefinitionCreator creator = (target as ConstructionDefinitionCreator);
		if (GUILayout.Button("Create new Construction Definition"))
		{
			ConstructionDefinition newDefinition = ScriptableObject.CreateInstance<ConstructionDefinition>();
			
			AssetDatabase.CreateAsset(newDefinition, "Assets/Data/NewConstruction.asset");
			AssetDatabase.SaveAssets();
		}
		
		ConstructionDefinition selectedConstruction = EditorGUILayout.ObjectField("Load Construction", null, typeof(ConstructionDefinition), false) as ConstructionDefinition;
		
		if (selectedConstruction != null)
		{
			while(creator.gameObject.transform.GetChildCount() > 0)
			{
				DestroyImmediate(creator.gameObject.transform.GetChild(0));
			}
		}
		
//		GameObject constructionObject = creator.transform.GetChild
		
	}
	
}
