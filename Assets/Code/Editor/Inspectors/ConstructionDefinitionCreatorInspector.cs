using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ConstructionDefinitionCreator))]
public class ConstructionDefinitionCreatorInspector : Editor
{
	
	
	
	string constructionText = "";
	public override void OnInspectorGUI()
	{
		constructionText = EditorGUILayout.TextField("Create Construction", constructionText);
		
		if (GUILayout.Button("Generate Construction"))
		{
			ConstructionDefinition constructionDef = ConstructionDefinition.Decode(constructionText);
			Debug.Log (constructionDef.constructionElements.Count);
			
			GrabbablePart constructed = constructionDef.GenerateConnectedParts();
			constructed.transform.position = (target as ConstructionDefinitionCreator).transform.position;
		}
		
//		ConstructionDefinitionCreator creator = (target as ConstructionDefinitionCreator);
//		if (GUILayout.Button("Create new Construction Definition"))
//		{
//			ConstructionDefinitionAsset newDefinition = ScriptableObject.CreateInstance<ConstructionDefinitionAsset>();
//			
//			AssetDatabase.CreateAsset(newDefinition, "Assets/Data/NewConstruction.asset");
//			AssetDatabase.SaveAssets();
//		}
//		
//		ConstructionDefinitionAsset selectedConstruction = EditorGUILayout.ObjectField("Load Construction", null, typeof(ConstructionDefinitionAsset), false) as ConstructionDefinitionAsset;
//		
//		if (selectedConstruction != null)
//		{
//			while(creator.gameObject.transform.GetChildCount() > 0)
//			{
//				DestroyImmediate(creator.gameObject.transform.GetChild(0));
//			}
//		}
//		
//		GameObject constructionObject = creator.transform.GetChild
		
	}
	
}
