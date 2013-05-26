using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(GrabbablePart))]
public class GrabbablePartInspector : Editor
{
//	HexMetrics.Direction selectedDirection;
//	HexMetrics.Direction selectedOrientation;
	
	GrabbablePart part = null;
	
	
	public override void OnInspectorGUI()
	{
		
		
		
		part = target as GrabbablePart;
		DrawDefaultInspector();
		
		
		HexMetrics.Direction oldOrietation = part.SimulationOrientation;
		HexMetrics.Direction newOrietation = (HexMetrics.Direction)EditorGUILayout.EnumPopup( part.SimulationOrientation );
//		if (newOrietation != oldOrietation)
		{
			part.SimulationOrientation = newOrietation;
		}
		
		for (int i = 0 ; i < 6 ; i++)
		{
			HexMetrics.Direction iDir = (HexMetrics.Direction)i;
			GrabbablePart connectedPart = part.GetConnectedPart(iDir);
			
			EditorGUILayout.BeginHorizontal();
			
			string relation = "";
			if (connectedPart != null)
			{
				if (connectedPart.transform == part.transform.parent)
				{
					relation = "(P)";
				}
				if (connectedPart.transform.parent == part.transform)
				{
					relation = "(C)";
				}
			}
			
			if (GUILayout.Button("GOTO" + relation, GUILayout.Width(75)))
			{
				if (connectedPart != null)
				{
					Selection.activeObject = connectedPart;
				}
			}
			
			PartType newPartType = (PartType)EditorGUILayout.EnumPopup( connectedPart == null ? PartType.None : connectedPart.partType );
			
			if (connectedPart == null)
			{
				if (newPartType != PartType.None)
				{
					// create part
					
					
					GrabbablePart partPrefab = GameSettings.instance.GetPartPrefab(newPartType);
					GrabbablePart newConnectedPart = PrefabUtility.InstantiatePrefab(partPrefab) as GrabbablePart;
					part.ConnectPartAndPlace(newConnectedPart, iDir);
				}
			}
			else
			{
				if (newPartType == PartType.None)
				{
					GameObject toDestroy = part.RemoveConnectedPart(iDir).gameObject;
					GameObject.DestroyImmediate(toDestroy);
				}
			}
			
			part.SetPhysicalConnection(iDir, (GrabbablePart.PhysicalConnectionType)EditorGUILayout.EnumPopup(part.GetConnectionType(iDir)));
			part.SetAuxilaryConnections(iDir, EditorGUILayout.MaskField(	
																	part.GetAuxilaryConnectionTypes(iDir), 
																	System.Enum.GetNames(typeof(GrabbablePart.AuxilaryConnectionType))  ));
			
			
			EditorGUILayout.EndHorizontal();
			
		}
		
		EditorUtility.SetDirty(part);

		
		
	}
	
	
	public void OnSceneGUI()
	{
		bool debugVal = true;
		if (part != null && debugVal)
		{
			foreach(var locatedPart in part.GetAllPartsWithLocation())
			{
				GrabbablePart lpart = locatedPart.part;
				Handles.Label(lpart.transform.position-(Vector3.forward*4), ""+locatedPart.location.x+":"+locatedPart.location.y);
				Handles.color = (lpart == part ? Color.blue : Color.red);
				Handles.DrawWireDisc(lpart.transform.position, Vector3.forward, 20);
				Handles.color = Color.red;
				for(int i = 0 ; i < 6 ; i++)
				{
					
					HexMetrics.Direction iDir = (HexMetrics.Direction)i;
					GrabbablePart connectedPart = lpart.GetConnectedPart(iDir);
//					Vector3 relativeLocation = GameSettings.instance.hexCellPrefab.GetDirection(iDir);
					
//					if (connectedPart != null)
//					{
//						Handles.DrawWireDisc(lpart.transform.position + (relativeLocation)/5, Vector3.forward, 1);
//					}
					if (connectedPart != null)
					{
						
						
						HexMetrics.Direction direction = lpart.GetAbsoluteDirectionFromRelative(iDir);
						Vector3 relativeLocation = GameSettings.instance.hexCellPrefab.GetDirection(direction);
					
						float inRad  = 0.5f;
						float midRad = 0.5f;
						float outRad = 0.5f;
						Handles.color = Color.red;
						if (connectedPart != null)
						{
							if (connectedPart.transform == lpart.transform.parent)
							{
								outRad = 1.50f;
								midRad = 1.33f;
								inRad  = 1.16f;
								Handles.color = Color.black;
							}
							if (connectedPart.transform.parent == lpart.transform)
							{
								outRad = 1.66f;
								midRad = 1.82f;
								inRad  = 2.00f;
								Handles.color = Color.blue;
							}
						}
//						Handles.DrawWireDisk
						Handles.DrawSolidDisc(lpart.transform.position + (relativeLocation)/7f*3f, Vector3.forward, outRad*2);
						Handles.DrawSolidDisc(lpart.transform.position + (relativeLocation)/7f*2f, Vector3.forward, midRad*2);
						Handles.DrawSolidDisc(lpart.transform.position + (relativeLocation)/7f*1f, Vector3.forward, inRad*2);
//						Handles.DrawLine(lpart.transform.position, lpart.transform.position + (relativeLocation)/3f);
//						relativeLocation.Normalize();
//						relativeLocation *= 3;
//						Handles.DrawLine(lpart.transform.position+relativeLocation,   connectedPart.transform.position+relativeLocation);
//						Handles.DrawLine(lpart.transform.position+relativeLocation*2, connectedPart.transform.position+relativeLocation);
////						Handles.DrawLine(lpart.transform.position, lpart.transform.position+(relativeLocation*10));
//						
					}
				}
			}
			
		}
	}
}










