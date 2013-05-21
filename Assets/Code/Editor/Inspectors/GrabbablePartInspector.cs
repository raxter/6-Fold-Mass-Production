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
	
	public class MyClass
	{
	}
	
	IEnumerable MyEnumerable()
	{
		yield return null;
		yield return "hello";
		yield return new MyClass();
	}
	
	IEnumerable<string> StringEnumerable()
	{
		yield return "I";
		yield return "must";
		yield return "enumerate!";
	}
	
	public override void OnInspectorGUI()
	{
		if (GUILayout.Button("Test Button"))
		{
//			foreach (string s in StringEnumerable())
//			{
//				Debug.Log(o);
//			}
			int i = 10;
			IEnumerator enumer = MyEnumerable().GetEnumerator();
			while (enumer.MoveNext())
			{
				Debug.Log (enumer.Current);
				i--;
				if (i == 0)
					break;
			}
		}
		
		
		
		
		part = target as GrabbablePart;
		DrawDefaultInspector();
		
		
		
		HexMetrics.Direction newOrietation = (HexMetrics.Direction)EditorGUILayout.EnumPopup( part.SimulationOrientation );
		part.SetAbsoluteOrientation(newOrietation);
		
		for (int i = 0 ; i < 6 ; i++)
		{
			HexMetrics.Direction iDir = (HexMetrics.Direction)i;
			GrabbablePart connectedPart = part.GetConnectedPart(iDir);
			EditorGUILayout.BeginHorizontal();
//			EditorGUILayout.Label(""+iDir);
			if (GUILayout.Button("GOTO" + ( connectedPart != null && connectedPart.transform == part.transform.parent ? "(P)" : "" ), GUILayout.Width(75)))
			{
				Selection.activeObject = connectedPart;
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
//			else 
//				//if (GUILayout.Button(""+connectedPart.partType+(connectedPart.transform.parent == part.transform ? "(p)" : "")))
//			{
//				PartType newPart = (PartType)EditorGUILayout.EnumPopup(PartType.None);
//				Selection.activeObject = connectedPart;
//			}
//			EditorGUILayout.ObjectField(, typeof(GrabbablePart), false);
			part.SetPhysicalConnection(iDir, (GrabbablePart.PhysicalConnectionType)EditorGUILayout.EnumPopup(part.GetConnectionType(iDir)));
			part.SetAuxilaryConnections(iDir, EditorGUILayout.MaskField(	
																	part.GetAuxilaryConnectionTypes(iDir), 
																	System.Enum.GetNames(typeof(GrabbablePart.AuxilaryConnectionType))  ));
			
//			EditorGUILayout.EnumPopup(part.GetAbsoluteDirection(iDir));
			EditorGUILayout.EndHorizontal();
		}
		
		EditorUtility.SetDirty(part);
//		
//		selectedDirection = (HexMetrics.Direction)EditorGUILayout.EnumPopup("Selected Direction", selectedDirection);
//		selectedOrientation = (HexMetrics.Direction)EditorGUILayout.EnumPopup("Selected Orientation", selectedOrientation);
//		
////		GrabbablePart.ConnectionDescription connDesc = part.connectedParts[(int)selectedDirection];
//		GrabbablePart connectedPartInDirection = part.GetConnectedPart(selectedDirection);
//		
//		if ( connectedPartInDirection == null )
//		{
//			EditorGUILayout.ObjectField("Object at "+selectedDirection, connectedPartInDirection, typeof(GrabbablePart), false);
//		}
//		else if ( GUILayout.Button("Delete Part") )
//		{
//			GameObject toDestroy = part.RemoveConnectedPart(selectedDirection).gameObject;
//			GameObject.DestroyImmediate(toDestroy);
//			
//		}
//		PartType newPart = (PartType)EditorGUILayout.EnumPopup("Create new part", PartType.None);
//		
//		if (newPart != PartType.None)
//		{
//			if (part.GetConnectedPart(selectedDirection) == null)
//			{
//				GrabbablePart partPrefab = GameSettings.instance.GetPartPrefab(newPart);
//				GrabbablePart newConnectedPart = GameObject.Instantiate(partPrefab) as GrabbablePart;
//				part.ConnectPartAndPlace(newConnectedPart, selectedDirection, selectedOrientation);
////				
//			}
//		}
		
		
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
					
					if (connectedPart != null)
					{
						HexMetrics.Direction direction = (HexMetrics.Direction)(((int)lpart.SimulationOrientation + i)%6);
						Vector3 relativeLocation = GameSettings.instance.hexCellPrefab.GetDirection((HexMetrics.Direction)(((int)direction+5)%6));
						relativeLocation.Normalize();
						relativeLocation *= 3;
						Handles.DrawLine(lpart.transform.position+relativeLocation,   connectedPart.transform.position+relativeLocation);
						Handles.DrawLine(lpart.transform.position+relativeLocation*2, connectedPart.transform.position+relativeLocation);
//						Handles.DrawLine(lpart.transform.position, lpart.transform.position+(relativeLocation*10));
						
					}
				}
			}
			
		}
	}
}










