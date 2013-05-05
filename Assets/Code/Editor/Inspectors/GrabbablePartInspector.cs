using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(GrabbablePart))]
public class GrabbablePartInspector : Editor
{
	HexMetrics.Direction selectedDirection;
	public override void OnInspectorGUI()
	{
		GrabbablePart part = target as GrabbablePart;
		DrawDefaultInspector();
		
		
		
		
		selectedDirection = (HexMetrics.Direction)EditorGUILayout.EnumPopup("Selected Direction", selectedDirection);
		
		GrabbablePart.ConnectionDescription connDesc = part.connectedParts[(int)selectedDirection];
		if (connDesc != null && connDesc.connectedPart)
		{
			EditorGUILayout.ObjectField("Object at "+selectedDirection, connDesc.connectedPart, typeof(GrabbablePart), false);
		}
		PartType newPart = (PartType)EditorGUILayout.EnumPopup("Create new part", PartType.None);
		
		if (newPart != PartType.None)
		{
			if (connDesc != null && connDesc.connectedPart)
			{
				
			}
		}
		
		
	}
}
