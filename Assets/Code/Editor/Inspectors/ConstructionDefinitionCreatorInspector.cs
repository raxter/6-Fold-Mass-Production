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
			Construction construction = Construction.DecodeCreate(constructionText);
			Debug.Log (construction.PartsList.Count);
			
			EditorUtility.SetDirty(construction);
			foreach (GrabbablePart part in construction.Parts)
			{
				EditorUtility.SetDirty(part);
			}
			
//			GrabbablePart constructed = constructionDef.GenerateConnectedParts();
			construction.transform.parent = (target as ConstructionDefinitionCreator).transform;
			construction.transform.localPosition = Vector3.zero;
		}
		
	}
	
}
