using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class ExtendedEditor : Editor
{
//	
//	public string CamelCaseToSentence(string s)
//	{
//		string ret = "";
//		char [] charArray = s.ToCharArray();
//		ret += char.ToUpper(charArray[0]);
//		for (int i = 1 ; i < charArray.Length ; i++)
//		{
//			char c = charArray[i];
//			if (char.IsUpper(c))
//			{
//				ret += ' ';
//			}
//			ret += c;
//		}
//		
//		return ret;
//	}
//	
//	
//	private class ObjectIntArrayComparer : IEqualityComparer<List<Pair<Object,int>>>
//	{
//		public bool Equals(List<Pair<Object,int>> a, List<Pair<Object,int>> b)
//		{
//			return ObjectIntArrayComparer.EqualsFunction(a,b);
//		}
//		
//		public static bool EqualsFunction(List<Pair<Object,int>> a, List<Pair<Object,int>> b)
//		{
//			if (a.Count != b.Count)
//				return false;
//			
//			
//			for (int i = 0 ; i < a.Count ; i++)
//			{
//				if (a[i].First != b[i].First || a[i].Second != b[i].Second)
//					return false;
//			}
//			
//			return true;
//		}
//		public static bool IsAChildOf(List<Pair<Object,int>> child, List<Pair<Object,int>> parent)
//		{
//			if (child.Count < parent.Count)
//				return false;
//			
//			
//			for (int i = 0 ; i < parent.Count ; i++)
//			{
//				if (child[i].First != parent[i].First || child[i].Second != parent[i].Second)
//					return false;
//			}
//			
//			return true;
//		}
//		
//		public int GetHashCode(List<Pair<Object,int>> a)
//		{
//			if (a.Count == 0)
//				return 0;
//			
//			int ret = a[0].GetHashCode();
//			for (int i = 1 ; i < a.Count ; i++)
//				ret ^= a[i].GetHashCode();
//			return ret.GetHashCode();
//		}
//	}
//	
//	public static string StackLocationToString(List<Pair<Object,int>> a)
//	{
//		string ret = "";
//		foreach(Pair<Object,int> p in a)
//		{
//			ret += "("+(p.First == null?"null":p.First.ToString())+","+p.Second+")";
//		}
//		return ret;
//	}
//	
//	// should really make this a tree :p
//	Dictionary<List<Pair<Object,int>>, bool> stackExpanded = new Dictionary<List<Pair<Object,int>>, bool>(new ObjectIntArrayComparer());
//	Stack<Pair<Object,int>> currentObjectStack = new Stack<Pair<Object,int>>();
//	
//	
//	HashSet<AdvancedInspectorOption> inspectorOptions = new HashSet<AdvancedInspectorOption>();
//	public enum AdvancedInspectorOption {ListReorder, Recursive}
//	bool allowRecursive = false;
//	bool allowListReording = false;
//	
//	Dictionary<Object, List<Pair<Object,int>>> expandedObjectLocation = new Dictionary<Object, List<Pair<Object,int>>>();
//	
//	public void DrawAdvancedInspector(AdvancedInspectorOption [] options)
//	{
//		
////		Debug.Log ("-------");
//		inspectorOptions.Clear();
//		inspectorOptions.UnionWith(options);
//		
////		expandedObjectLocation.Clear ();
//		if (!expandedObjectLocation.ContainsKey(this.serializedObject.targetObject))
//			expandedObjectLocation.Add (this.serializedObject.targetObject, new List<Pair<Object,int>>());
//		
//		DrawSerializedObject(serializedObject);
//		EditorGUILayout.Separator();
//		
//		if (GUILayout.Button("Hide all"))
//		{
//			stackExpanded.Clear ();
//		}
//		
//		serializedObject.ApplyModifiedProperties();
//	}
//	
//	private void DrawSerializedObject(SerializedObject serializedObj)
//	{
//		
//		EditorGUIUtility.LookLikeInspector();
////		(this as Editor).DrawDefaultInspector();
//		
////		showDebugInfo = EditorGUILayout.Toggle("Show Debug Info", showDebugInfo);
//		
//		
//		int count = 0;
//		
//		SerializedProperty serialisedProperty = serializedObj.GetIterator();
//			
//		bool showChildren = true;
//
//		Stack<Pair<int,int>> indexSizeStack = new Stack<Pair<int, int>>();
//		while (serialisedProperty.NextVisible(showChildren))
//		{	
//			EditorGUILayout.LabelField(""+serialisedProperty);
//			
//			if (indexSizeStack.Count > 0)
//			{
//				Pair<int,int> top = indexSizeStack.Pop();
//				
//				EditorGUI.indentLevel -= 1;
//				top.First += 1;
//				if (top.First < top.Second)
//				{
//					indexSizeStack.Push(top);
//					EditorGUI.indentLevel += 1;
//				}
//				if (top.First == -1)
//					EditorGUI.indentLevel += 1;
//			}
//			
//			if (serialisedProperty.isArray && serialisedProperty.isExpanded)
//			{
//				indexSizeStack.Push (new Pair<int,int>(-2, serialisedProperty.arraySize));
//			}
////			
////			string stackString = "";
////			foreach(Pair<int,int> p in indexSizeStack)
////			{
////				stackString += "("+p.First+"/"+p.Second+")";
////			}
////			EditorGUILayout.LabelField("Stack ",stackString);
//			
//			
//			if (serialisedProperty.propertyType != SerializedPropertyType.ObjectReference)
//			{
//				showChildren = EditorGUILayout.PropertyField(serialisedProperty);
//				continue;
//			}
//			Object objValue = serialisedProperty.objectReferenceValue;
////				Debug.Log ("arrayIndex "+arrayIndex+":"+objValue);
//			
//			
//			int index = indexSizeStack.Count > 0?indexSizeStack.Peek().First:-1;
//			
//			currentObjectStack.Push(new Pair<Object,int>(objValue, index));
//			
//			
//			List<Pair<Object,int>> stackCopy = new List<Pair<Object, int> >(currentObjectStack);
//			stackCopy.Reverse();
//			
////			EditorGUILayout.LabelField ("Stack: "+ (serialisedProperty.name == "m_Script"?"Script":StackLocationToString(stackCopy)));
//			
//			bool showRecursiveFoldout = true;
//			
//			if (serialisedProperty.name == "m_Script" || !inspectorOptions.Contains(AdvancedInspectorOption.Recursive))
//			{
//				showRecursiveFoldout = false;
//			}
//			
//			
//			
//			if (objValue != null)
//			{
//				if (!stackExpanded.ContainsKey(stackCopy))
//					stackExpanded[stackCopy] = false;
//				
//			}
//			else
//			{
//				showRecursiveFoldout = false;
//			}
//			Rect r = EditorGUILayout.BeginHorizontal();
//			r.x = indentSize*(EditorGUI.indentLevel+1);
//			if (showRecursiveFoldout)
//			{
//				r.width = indentSize;
//				
//				
//				GUI.DrawTexture(r, stackExpanded[stackCopy]?IconReferences.eyeOpen:IconReferences.eyeClosed);
//				if( GUI.Button(r, "" , EditorStyles.label))
//				{
//					Debug.Log ("stackExpanded[stackCopy] "+stackExpanded[stackCopy]);
//					if (stackExpanded[stackCopy])
//					{
//						stackExpanded[stackCopy] = false;
//						if (expandedObjectLocation.ContainsKey(objValue))
//						{
//							expandedObjectLocation.Remove(objValue);
//						}
//					}
//					else
//					{
//						
//						Debug.Log ("expandedObjectLocation.ContainsKey(objValue) "+expandedObjectLocation.ContainsKey(objValue));
//						// not expanded
//						if (expandedObjectLocation.ContainsKey(objValue))
//						{
//							Debug.Log ("ObjectIntArrayComparer.EqualsFunction(expandedObjectLocation[objValue], stackCopy) "+ObjectIntArrayComparer.EqualsFunction(expandedObjectLocation[objValue], stackCopy));
//							// the object has an expanded location already somewhere
//							if (ObjectIntArrayComparer.EqualsFunction(expandedObjectLocation[objValue], stackCopy))
//							{
//								// this is the expanded object
//								// collapse it
//								stackExpanded[stackCopy] = false;
//								expandedObjectLocation.Remove(objValue);
//							}
//							else
//							{
//								Debug.Log ("stackCopy / current loc. (child): "+StackLocationToString(stackCopy));
//								Debug.Log ("expandedObjectLocation[objValue]: "+StackLocationToString(expandedObjectLocation[objValue]));
//								Debug.Log ("ObjectIntArrayComparer.IsAChildOf(stackCopy, expandedObjectLocation[objValue]) "+ObjectIntArrayComparer.IsAChildOf(stackCopy, expandedObjectLocation[objValue]));
//							
//								if (ObjectIntArrayComparer.IsAChildOf(stackCopy, expandedObjectLocation[objValue]))
//								{
//									// the expanded object is a parent of this one
//									
//									stackExpanded[stackCopy] = false;
//								}
//								else
//								{
//									// another location with this object is expanded
//									
//									// collapse the currently expanded object
//									stackExpanded[expandedObjectLocation[objValue]] = false;
//									
//									// expand the current object at this location
//									stackExpanded[stackCopy] = true;
//									
//									expandedObjectLocation[objValue] = stackCopy;
//									
//									// force a repaint
//									Repaint();
//								}
//							}
//						}
//						else
//						{
//							// the object does not have an expanded location
//							
//							stackExpanded[stackCopy] = true;
//							expandedObjectLocation.Add(objValue, stackCopy);
//						}
//					}
//				}
//				
////				if (!expandedObjectLocation.ContainsKey(objValue))
////					expandedObjectLocation.Add (objValue, 0);
////				
////				if(stackExpanded[stackCopy])
////				{
////					if(expandedObjectLocation[objValue] > 0)
////						stackExpanded[stackCopy] = false;
////					else
////						expandedObjectLocation[objValue] += 1;
////				}
//			}
//			if (inspectorOptions.Contains(AdvancedInspectorOption.ListReorder))
//			{
//				if (indexSizeStack.Count > 0)
//				{
//					r.x -= 7;
//					r.width = 7;
//					r.height = 7;
//					GUI.DrawTexture(r, IconReferences.replace);
//					if (GUI.Button(r, "" , EditorStyles.label))
//					{
//						Debug.Log ("yay replace");
//					}
//					r.y += 8;
//					r.width = 7;
//					r.height = 7;
//					GUI.DrawTexture(r, IconReferences.insert);
//					if (GUI.Button(r, "" , EditorStyles.label))
//					{
//						Debug.Log ("yay insert");
//					}
//				}
//			}
//			
////			if (showRecursiveFoldout)
////			EditorGUI.indentLevel += 1;
//
//			showChildren = EditorGUILayout.PropertyField(serialisedProperty );//,new GUIContent(""+index+":"+indexSizeStack.Count));
//				
////			EditorGUI.indentLevel -= 1;	
//			EditorGUILayout.EndHorizontal();
//			if (showRecursiveFoldout)
//			{
//				if (stackExpanded[stackCopy])
//				{
//					SerializedObject nextSerializedObject = new SerializedObject(objValue);
////						Debug.Log ("We need to go deeper " + nextSerializedObject.GetIterator());
//					DrawSerializedObject(nextSerializedObject);
//					
////						EditorGUILayout.PropertyField(serialisedProperty);
////						EditorGUILayout.PropertyField(serialisedProperty);
//				}
//			}
//			
//			currentObjectStack.Pop();
//		
//		
//		}
////		
////		Debug.Log ("--------");
//		
//	}
	
	List<InspectorOption> buttonOrder = new List<InspectorOption>()
	{
		InspectorOption.InsertSwitch,
	    InspectorOption.Recursive
	};
	public static float indentSize = 9f;
	
	HashSet<InspectorOption> inspectorOptions = new HashSet<InspectorOption>()
	{
		InspectorOption.InsertSwitch, 
		InspectorOption.Recursive
	};
	
	public enum InspectorOption {Recursive, InsertSwitch};
	public enum InspectorButtons {Insert, Switch, Recurse};
	
	
	bool showDefault = false;
	public void DrawAdvancedInspector(InspectorOption [] options)
	{
//		inspectorOptions.Clear();
//		inspectorOptions.UnionWith(options);
		
		GUILayout.BeginHorizontal();
		{
			foreach(InspectorOption o in System.Enum.GetValues(typeof(InspectorOption)))
			{
				if (EditorGUILayout.Toggle(""+o, inspectorOptions.Contains(o)))
					inspectorOptions.Add(o);
				else
					inspectorOptions.Remove(o);
			}
				
//			if (EditorGUILayout.Toggle("Recursive", inspectorOptions.Contains(InspectorOption.Recursive)))
//				inspectorOptions.Add(InspectorOption.Recursive);
//			else
//				inspectorOptions.Remove(InspectorOption.Recursive);
//			
//			if (EditorGUILayout.Toggle("List Reorder", inspectorOptions.Contains(InspectorOption.InsertSwitch)))
//				inspectorOptions.Add(InspectorOption.InsertSwitch);
//			else
//				inspectorOptions.Remove(InspectorOption.InsertSwitch);
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		{
			if (GUILayout.Button("Reverse Order"))
				buttonOrder.Reverse();
			
			showDefault = EditorGUILayout.Toggle("Show Default Inspector", showDefault);
			
		}
		GUILayout.EndHorizontal();
		
		if (showDefault)
			DrawDefaultInspector();
		else
			DrawSerializedObject(serializedObject);
		
	}
	
	class ArrayCounts : Stack<MutablePair<int,bool>>
	{
		public void NewIndex(bool isArray)
		{
			Push (new MutablePair<int,bool>(-1, isArray));
		}
		
		public void EndedIndex()
		{
			Pop ();
		}
		
		public void IncrementIndex()
		{
			Peek().First += 1;
		}
		
		public bool IsAtArray()
		{
			
			return Peek().First >=0 && Peek().Second;
		}
		
		public override string ToString()
		{
			if (Count == 0)
				return "";
			
			string ret = "";
			foreach(MutablePair<int,bool> i in this)
			{
				ret += ", "+i.First+""+(i.Second?"T":"F");
			}
			return ret.Substring(2);
		}
	}
	
	private void DrawSerializedObject(SerializedObject serializedObj)
	{
		ArrayCounts arrayCounts = new ArrayCounts();
		arrayCounts.NewIndex(false);
		SerializedProperty property = serializedObj.GetIterator();
		bool showChildren = true;
		bool wasArray = false;
		int oldDepth = 0;
		while(property.NextVisible(showChildren))
		{
			if (oldDepth == property.depth-1)
				arrayCounts.NewIndex(wasArray);
			else if (oldDepth == property.depth+1)
			{
				arrayCounts.EndedIndex();
				arrayCounts.IncrementIndex();
			}
			else if (oldDepth == property.depth)
				arrayCounts.IncrementIndex();
			
			wasArray = property.isArray;
			
			oldDepth = property.depth;
			EditorGUI.indentLevel = property.depth+1;
			
			Rect r = EditorGUILayout.BeginHorizontal();
			r.width = indentSize;
			r.x = (EditorGUI.indentLevel+1)*indentSize - 2;
			
			
			
			showChildren = EditorGUILayout.PropertyField(property);
			
			
			HashSet<InspectorOption> toDraw = new HashSet<InspectorOption>();
			if (property.propertyType == SerializedPropertyType.ObjectReference)
			{
				toDraw.Add (InspectorOption.Recursive);
			}
			if (arrayCounts.IsAtArray())
			{
				toDraw.Add (InspectorOption.InsertSwitch);
			}
			
			toDraw.IntersectWith(inspectorOptions);
			HashSet<InspectorButtons> pressed = DrawButtons(ref r, toDraw);
			

			
			
//			EditorGUILayout.PrefixLabel(property.propertyType + " "+r.height);
//			EditorGUILayout.PrefixLabel(property.propertyType+":"+arrayCounts.ToString());

			
			EditorGUILayout.EndHorizontal();
			
		}
		
		serializedObj.ApplyModifiedProperties();
	}
	
	HashSet<InspectorButtons> DrawButtons(ref Rect r, HashSet<InspectorOption> toDraw)
	{
		HashSet<InspectorButtons> pressedButtons = new HashSet<InspectorButtons>();
		
		foreach(InspectorOption o in buttonOrder)
		{
			if (toDraw.Contains(o))
				buttonDelegates[o](ref r, ref pressedButtons);
		}
		
		return pressedButtons;
	}
	
	
	delegate void ButtonReturn(ref Rect r, ref HashSet<InspectorButtons> pressedButtons);
	
	static Dictionary<InspectorOption, ButtonReturn> buttonDelegates = new Dictionary<InspectorOption, ButtonReturn>
	{
	    {InspectorOption.Recursive, RecurseButton},
	    {InspectorOption.InsertSwitch, ListReorderButtons}
	};	
	
	static void RecurseButton(ref Rect r, ref HashSet<InspectorButtons> pressedButtons)
	{
		
		r.width = r.height;
		r.x -= r.width+1;
		
		GUI.DrawTexture(r, IconReferences.eyeClosed);
		
	}
	
	static void ListReorderButtons(ref Rect r, ref HashSet<InspectorButtons> pressedButtons)
	{
		r.width = 8;
		r.height = 8;
		r.x -= 10;
		
		GUI.DrawTexture(r, IconReferences.replace);
		r.y += 8;
		GUI.DrawTexture(r, IconReferences.insert);
		
		r.y -= 8;
		
		r.height = 16;
		
	
	}
	
	
	
	
	
}














