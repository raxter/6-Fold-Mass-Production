using UnityEngine;
using System.Collections;

public static class HexMetrics {
	
	public enum Direction {Up, RightUp, RightDown, Down, LeftDown, LeftUp};
	
	public static readonly Direction UpDirection = Direction.Up;
	public static readonly Direction RightDirection = Direction.RightUp;
	
	public static float Side
	{
		get 
		{
			return 1f;
		}
		private set{}
	}
	
	public static float CornerHeight
	{
		get 
		{
			return 0.86602540378f;
//			return Mathf.Cos(Mathf.Deg2Rad*30f);
		}
		private set{}
	}
	
	public static float CornerWidth
	{
		get 
		{
			return 0.5f;
//			return Mathf.Sin(Mathf.Deg2Rad*30f);
		}
		private set{}
	}
	
	public static float WidthToHeightRatio
	{
		get
		{
			return Width/Height;
		}
	}
	
	public static float Height
	{
		get 
		{
			return CornerHeight*2f;
		}
		private set{}
	}
	
	public static float Width
	{
		get 
		{
			return Side + (CornerWidth*2f);
		}
		private set{}
	}
}
