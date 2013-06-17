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
		
		
		if (EditorApplication.isPlaying)
		{
			GUILayout.Label("Construction editor disabled while playing");
			return;
		}
		
		
		InstantiatePrefabDelegate instantiateFunction = (prefab) => PrefabUtility.InstantiatePrefab(prefab) as GameObject;
		
		EditorGUILayout.BeginHorizontal();
		HexMetrics.Direction oldOrietation = part.SimulationOrientation;
		HexMetrics.Direction newOrietation = (HexMetrics.Direction)EditorGUILayout.EnumPopup( part.SimulationOrientation );
		if (newOrietation != oldOrietation)
		{
			part.SimulationOrientation = newOrietation;
		}
		if (GUILayout.Button("<-"))
		{
			part.SimulationOrientation = (HexMetrics.Direction)(((int)(part.SimulationOrientation) - 1) % 6);
		}
		if (GUILayout.Button("->"))
		{
			part.SimulationOrientation = (HexMetrics.Direction)(((int)(part.SimulationOrientation) + 1) % 6);
		}
		EditorGUILayout.EndHorizontal();
		
		for (int i = 0 ; i < 6 ; i++)
		{
			HexMetrics.Direction iDir = (HexMetrics.Direction)i;
			GrabbablePart connectedPart = part.GetConnectedPart(iDir);
			
			EditorGUILayout.BeginHorizontal();
			
			
			if (GUILayout.Button("GOTO", GUILayout.Width(75)))
			{
				if (connectedPart != null)
				{
					Selection.activeObject = connectedPart;
				}
			}
			
			// check for other components
			Vector3 connectedPosition = part.transform.position + (Vector3)GameSettings.instance.hexCellPrefab.GetDirection(iDir);
			Collider [] colliders = Physics.OverlapSphere(connectedPosition, 1);
			
			GrabbablePart contact = null;
			foreach(Collider c in colliders)
			{
				contact = c.attachedRigidbody.GetComponent<GrabbablePart>();
				
				if (contact != null) break;
			}
			if (contact != null && contact.ParentConstruction != null && contact.ParentConstruction != part.ParentConstruction )
			{
				Vector3 difference = contact.transform.position - connectedPosition;
//				Debug.Log(difference);
				if (difference != Vector3.zero)
				{
					if (GUILayout.Button("Line Up"))
					{
						Debug.Log (difference);
						Transform toMove = (contact.ParentConstruction == null ? null : contact.ParentConstruction.transform) ?? contact.transform;
						toMove.position -= difference;
					}
				}
				else
				{
					if (GUILayout.Button("Connect ("+(GrabbablePart.PhysicalConnectionType)1+")"))
					{
						part.ParentConstruction.AddToConstruction(contact);
						part.ConnectPartAndPlaceAtRelativeDirection(contact, GrabbablePart.PhysicalConnectionType.Weld, iDir);
//						part.SetPhysicalConnection(iDir, GrabbablePart.PhysicalConnectionType.Weld);
					}
				}
			}
			else
			{
				PartType newPartType = (PartType)EditorGUILayout.EnumPopup( connectedPart == null ? PartType.None : connectedPart.partType );
			
			
				if (connectedPart == null)
				{
					if (newPartType != PartType.None)
					{
						// create part
						
						
						GrabbablePart partPrefab = GameSettings.instance.GetPartPrefab(newPartType);
						GrabbablePart newConnectedPart = PrefabUtility.InstantiatePrefab(partPrefab) as GrabbablePart;
						part.ConnectPartAndPlaceAtRelativeDirection(newConnectedPart, GrabbablePart.PhysicalConnectionType.Weld, iDir);
						part.SimulationOrientation = part.SimulationOrientation;
//						part.SetPhysicalConnection(iDir, GrabbablePart.PhysicalConnectionType.Weld, instantiateFunction);
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
			
				GrabbablePart.PhysicalConnectionType oldConnectionType = part.GetPhysicalConnectionType(iDir);
				GrabbablePart.PhysicalConnectionType newConnectionType = (GrabbablePart.PhysicalConnectionType)EditorGUILayout.EnumPopup(oldConnectionType);
				if (oldConnectionType != newConnectionType)
				{
					part.SetPhysicalConnection(iDir, newConnectionType);
				}
				
				int oldAuxTypes = part.GetAuxilaryConnectionTypes(iDir);
				int newAuxTypes = EditorGUILayout.MaskField(	oldAuxTypes, System.Enum.GetNames(typeof(GrabbablePart.AuxilaryConnectionType)) );
				if (oldAuxTypes != newAuxTypes)
					part.SetAuxilaryConnections(iDir, newAuxTypes);
				
			}
			
			EditorGUILayout.EndHorizontal();
			
		}
		
		EditorUtility.SetDirty(part);

	}
	
	
	public void OnSceneGUI()
	{
		if (part == null)
		{
			return;
		}
//		Debug.DrawLine(part.transform.position, );
		for(int i = 0 ; i < 6 ; i++)
		{
			
			HexMetrics.Direction iDir = (HexMetrics.Direction)i;
			Handles.DrawSolidDisc(part.transform.position + (Vector3)GameSettings.instance.hexCellPrefab.GetDirection(iDir), Vector3.forward, 1);
		}
	}
//		bool debugVal = true;
//		if (part != null && debugVal)
//		{
////			Debug.Log ("----");
//			foreach(var locatedPart in part.GetAllPartsWithLocation())
//			{
//				GrabbablePart lpart = locatedPart.part;
//				Handles.Label(lpart.transform.position-(Vector3.forward*4), ""+locatedPart.location.x+":"+locatedPart.location.y);
//				Handles.color = (lpart == part ? Color.blue : Color.red);
//				Handles.DrawWireDisc(lpart.transform.position, Vector3.forward, 20);
//				Handles.color = Color.red;
//				for(int i = 0 ; i < 6 ; i++)
//				{
//					
//					HexMetrics.Direction iDir = (HexMetrics.Direction)i;
//					GrabbablePart connectedPart = lpart.GetConnectedPart(iDir);
////					Vector3 relativeLocation = GameSettings.instance.hexCellPrefab.GetDirection(iDir);
//					
////					if (connectedPart != null)
////					{
////						Handles.DrawWireDisc(lpart.transform.position + (relativeLocation)/5, Vector3.forward, 1);
////					}
//					if (connectedPart != null)
//					{
//						
//						
//						HexMetrics.Direction direction = lpart.AbsoluteDirectionFromRelative(iDir);
//						Vector3 relativeLocation = GameSettings.instance.hexCellPrefab.GetDirection(direction);
//					
//						float inRad  = 0.5f;
//						float midRad = 0.5f;
//						float outRad = 0.5f;
//						Handles.color = Color.red;
//						if (connectedPart != null)
//						{
//							if (connectedPart.transform == lpart.transform.parent)
//							{
//								outRad = 1.50f;
//								midRad = 1.33f;
//								inRad  = 1.16f;
//								Handles.color = Color.black;
//							}
//							if (connectedPart.transform.parent == lpart.transform)
//							{
//								outRad = 1.66f;
//								midRad = 1.82f;
//								inRad  = 2.00f;
//								Handles.color = Color.blue;
//							}
//						}
////						Handles.DrawWireDisk
//						Handles.DrawSolidDisc(lpart.transform.position + (relativeLocation)/7f*1f, Vector3.forward, inRad*2);
//						Handles.DrawSolidDisc(lpart.transform.position + (relativeLocation)/7f*2f, Vector3.forward, midRad*2);
//						if (lpart.GetPhysicalConnectionType(iDir) != GrabbablePart.PhysicalConnectionType.None)
//						{
//							Handles.color = Color.green;
//						}
//						Handles.DrawSolidDisc(lpart.transform.position + (relativeLocation)/7f*3f, Vector3.forward, outRad*2);
////						Handles.DrawLine(lpart.transform.position, lpart.transform.position + (relativeLocation)/3f);
////						relativeLocation.Normalize();
////						relativeLocation *= 3;
////						Handles.DrawLine(lpart.transform.position+relativeLocation,   connectedPart.transform.position+relativeLocation);
////						Handles.DrawLine(lpart.transform.position+relativeLocation*2, connectedPart.transform.position+relativeLocation);
//////						Handles.DrawLine(lpart.transform.position, lpart.transform.position+(relativeLocation*10));
////						
//					}
//				}
//			}
////			Debug.Log ("----");
//			
//		}
//	}
}










