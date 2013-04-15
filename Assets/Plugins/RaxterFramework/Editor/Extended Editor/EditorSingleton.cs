using UnityEngine;
using UnityEditor;
using System.Collections;


public abstract class EditorScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObject
{
	
	protected static T _instance = null;
	
	public static T instance
	{
		get
		{
			if(_instance != null)
			{
				return _instance;
			}
			
			string editorFolder = "Assets/Editor/Editor Resources";
			
			Debug.Log("Loading " + typeof(T).Name + " from "+editorFolder+" folder");
			_instance = Resources.Load(typeof(T).Name, typeof(T)) as T;
			
			if(_instance == null)
			{
#if UNITY_EDITOR
				Debug.LogWarning(typeof(T).Name + " resource does not exist. Creating in "+editorFolder);
				_instance = ScriptableObject.CreateInstance<T>();
				
				System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(editorFolder);
				if(!directory.Exists)
				{
					directory.Create();
				}
				
				AssetDatabase.CreateAsset(_instance, editorFolder +"/"+ typeof(T).Name + ".editorasset");
				AssetDatabase.SaveAssets();
#else		
				Debug.LogError("Error getting the " + typeof(T).Name + " resource");
#endif
			}
			
			return _instance;
		}
	}
}