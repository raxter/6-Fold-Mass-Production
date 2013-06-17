using UnityEngine;
using UnityEditor;
using System.Collections;

public class IconReferences : ScriptableObject
{
	public IconReferences()
	{
	}
	
	static Texture2D _eyeOpen = null;
	public static Texture2D eyeOpen
	{
		get
		{
			if (_eyeOpen == null)
				_eyeOpen = AssetDatabase.LoadAssetAtPath(getDirectoryPath ()+"eyeOpen.png", typeof(Texture2D)) as Texture2D;
			
			return _eyeOpen;
		}
		private set{}
	}
	
	static Texture2D _eyeClosed = null;
	public static Texture2D eyeClosed
	{
		get
		{
			if (_eyeClosed == null)
				_eyeClosed = AssetDatabase.LoadAssetAtPath(getDirectoryPath ()+"eyeClosed.png", typeof(Texture2D)) as Texture2D;
			
			return _eyeClosed;
		}
		private set{}
	}
	
	static Texture2D _insert = null;
	public static Texture2D insert
	{
		get
		{
			if (_insert == null)
				_insert = AssetDatabase.LoadAssetAtPath(getDirectoryPath ()+"insert.png", typeof(Texture2D)) as Texture2D;
			
			return _insert;
		}
		private set{}
	}
	
	
	static Texture2D _insertSelected = null;
	public static Texture2D insertSelected
	{
		get
		{
			if (_insertSelected == null)
				_insertSelected = AssetDatabase.LoadAssetAtPath(getDirectoryPath ()+"insertSelected.png", typeof(Texture2D)) as Texture2D;
			
			return _insertSelected;
		}
		private set{}
	}
	
	
	static Texture2D _replace = null;
	public static Texture2D replace
	{
		get
		{
			if (_replace == null)
				_replace = AssetDatabase.LoadAssetAtPath(getDirectoryPath ()+"replace.png", typeof(Texture2D)) as Texture2D;
			
			return _replace;
		}
		private set{}
	}
	
	static Texture2D _replaceSelected = null;
	public static Texture2D replaceSelected
	{
		get
		{
			if (_replaceSelected == null)
				_replaceSelected = AssetDatabase.LoadAssetAtPath(getDirectoryPath ()+"replaceSelected.png", typeof(Texture2D)) as Texture2D;
			
			return _replaceSelected;
		}
		private set{}
	}
	
	private static string getDirectoryPath ()
	{
		string fullPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(new IconReferences()));
//		Debug.Log (fullPath.Substring(0, fullPath.LastIndexOf('/')+1));
		return fullPath.Substring(0, fullPath.LastIndexOf('/')+1);
	}
	
	
}
