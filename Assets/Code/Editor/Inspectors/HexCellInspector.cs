using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(HexCell))]
public class HexCellInspector : Editor
{
	
	
	[MenuItem("Custom/Clear Editor Save")]
	public void ClearEditorSave()
	{
		foreach (var thing in new List<string>(LevelDataManager.instance.SaveList))
		{
			LevelDataManager.instance.Delete(thing, SaveType.Level);
		}
	}
	
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		
		SpriteBase sprite = (target as HexCell).button;
		
		if (sprite != null)
		{
			sprite.width = EditorGUILayout.FloatField("Hex sprite width", sprite.width);
			
			sprite.height = sprite.width/HexMetrics.WidthToHeightRatio;
			
			sprite.height = EditorGUILayout.FloatField("Hex sprite height", sprite.height);
			
			sprite.width = sprite.height*HexMetrics.WidthToHeightRatio;
			
			// (2w + 1)*s = W
			
			float sideLength = sprite.width/(2f*HexMetrics.CornerWidth+1f);
			sideLength = EditorGUILayout.FloatField("Hex side length", sideLength);
			
			sprite.width = HexMetrics.Width*sideLength;
			sprite.height = HexMetrics.Height*sideLength;
			
			sprite.SetSize(sprite.width, sprite.height);
			
			EditorUtility.SetDirty(sprite);
			
			SphereCollider collider = (target as HexCell).collider as SphereCollider;
			collider.radius = sprite.width/2;
			collider.center = new Vector3(0, 0, sprite.width/2);
		}
		else
		{
			EditorGUILayout.LabelField("Sprite is null");
		}
	}
}
