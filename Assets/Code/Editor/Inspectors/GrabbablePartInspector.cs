using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(GrabbablePart))]
public class GrabbablePartInspector : Editor
{
	HexMetrics.Direction selectedDirection;
	HexMetrics.Direction selectedOrientation;
	public override void OnInspectorGUI()
	{
		GrabbablePart part = target as GrabbablePart;
		DrawDefaultInspector();
		
		
		for (int i = 0 ; i < 6 ; i++)
		{
			HexMetrics.Direction iDir = (HexMetrics.Direction)i;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.ObjectField(part.GetConnectedPart((HexMetrics.Direction)i), typeof(GrabbablePart), false);
			part.SetPhysicalConnection(iDir, (GrabbablePart.PhysicalConnectionType)EditorGUILayout.EnumPopup(part.GetConnectionType(iDir)));
			part.SetAuxilaryConnections(iDir, EditorGUILayout.MaskField(	
																	part.GetAuxilaryConnectionTypes(iDir), 
																	System.Enum.GetNames(typeof(GrabbablePart.AuxilaryConnectionType))  ));
			EditorGUILayout.EndHorizontal();
		}
		
		selectedDirection = (HexMetrics.Direction)EditorGUILayout.EnumPopup("Selected Direction", selectedDirection);
		selectedOrientation = (HexMetrics.Direction)EditorGUILayout.EnumPopup("Selected Orientation", selectedOrientation);
		
//		GrabbablePart.ConnectionDescription connDesc = part.connectedParts[(int)selectedDirection];
		GrabbablePart connectedPartInDirection = part.GetConnectedPart(selectedDirection);
		
		if ( connectedPartInDirection == null )
		{
			EditorGUILayout.ObjectField("Object at "+selectedDirection, connectedPartInDirection, typeof(GrabbablePart), false);
		}
		else if ( GUILayout.Button("Delete Part") )
		{
			GameObject toDestroy = part.RemoveConnectedPart(selectedDirection).gameObject;
			GameObject.DestroyImmediate(toDestroy);
			
		}
		PartType newPart = (PartType)EditorGUILayout.EnumPopup("Create new part", PartType.None);
		
		if (newPart != PartType.None)
		{
			if (part.GetConnectedPart(selectedDirection) == null)
			{
				GrabbablePart partPrefab = GameSettings.instance.GetPartPrefab(newPart);
				GrabbablePart newConnectedPart = GameObject.Instantiate(partPrefab) as GrabbablePart;
				part.ConnectPartAndPlace(newConnectedPart, selectedDirection, selectedOrientation);
//				
			}
		}
		
		
	}
}
