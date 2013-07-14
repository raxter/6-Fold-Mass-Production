using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Construction))]
public class ConstructionInspector : Editor
{
	string encodedString = "";
	public override void OnInspectorGUI()
	{
		Construction construction = target as Construction;
		
		
		if (!construction.HasChild)
		{
			PartType newPartType = (PartType)EditorGUILayout.EnumPopup("Create initial part", PartType.None);
			
			if (newPartType != PartType.None)
			{
				
				GrabbablePart partPrefab = GameSettings.instance.GetPartPrefab(newPartType);
				GrabbablePart newPart = PrefabUtility.InstantiatePrefab(partPrefab) as GrabbablePart;
				newPart.transform.position = construction.transform.position;
				
				construction.AddToConstruction(newPart);
			}
		}
		else if (GUILayout.Button("Select first part"))
		{
			Selection.activeObject = construction.FirstPart;
		}
		if (GUILayout.Button("Regenerate encoded construction"))
		{
			encodedString = "";
		}
		if (encodedString == "")
		{
			encodedString = Encoding.Encode(construction);
		}
		EditorGUILayout.SelectableLabel(encodedString);
		
	}
}
