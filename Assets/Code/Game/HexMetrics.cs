using UnityEngine;
using System.Collections;

public static class HexMetrics {
	
	public enum Direction {Up, RightUp, RightDown, Down, LeftDown, LeftUp};
	
	public static readonly Direction UpDirection = Direction.Up;
	public static readonly Direction RightDirection = Direction.RightUp;
	

	public static IntVector2 GetRelativeLocation (HexMetrics.Direction direction)
	{
		switch (direction)
		{
			case HexMetrics.Direction.Up: 		 return new IntVector2( 0, 1);
			case HexMetrics.Direction.RightUp: 	 return new IntVector2( 1, 0);
			case HexMetrics.Direction.RightDown: return new IntVector2( 1,-1);
			case HexMetrics.Direction.Down: 	 return new IntVector2( 0,-1);
			case HexMetrics.Direction.LeftDown:	 return new IntVector2(-1, 0);
			case HexMetrics.Direction.LeftUp:	 return new IntVector2(-1, 1);
		}
		
		return null;
//		return new Dictionary<HexMetrics.Direction, System.Func<IntVector2>>()
//		{
//			{HexMetrics.Direction.Up, 		() => new IntVector2( 0, 1)},
//			{HexMetrics.Direction.RightUp, 	() => new IntVector2( 1, 0)},
//			{HexMetrics.Direction.RightDown,() => new IntVector2( 1,-1)},
//			{HexMetrics.Direction.Down, 	() => new IntVector2( 0,-1)},
//			{HexMetrics.Direction.LeftDown,	() => new IntVector2(-1, 0)},
//			{HexMetrics.Direction.LeftUp,	() => new IntVector2(-1, 1)}
//			
//		}
	}
	
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
