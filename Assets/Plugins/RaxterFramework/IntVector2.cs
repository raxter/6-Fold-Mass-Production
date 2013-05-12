using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class IntVector2
{
	public IntVector2(int x, int y)
	{
		this.x = x;
		this.y = y;
	}
	
	public int x;
	public int y;
	
	public static IntVector2 zero
	{
		get
		{
			return new IntVector2(0,0);
		}
	}
	
	
	public class IntVectorEqualityComparer : IEqualityComparer<IntVector2>
	{
		#region IEqualityComparer[IntVector2] implementation
		public bool Equals (IntVector2 x, IntVector2 y)
		{
			return x.IsEqualTo(y);
		}

		public int GetHashCode (IntVector2 obj)
		{
			return (obj.x*obj.y) ^ obj.y;
		}
		#endregion


	}
	
	public bool IsEqualTo(IntVector2 b)
	{
		return x == b.x && y == b.y;
	}
	
	public static implicit operator Vector2 (IntVector2 v)
	{
		return new Vector2(v.x, v.y);
	}
	
	public static implicit operator Vector3 (IntVector2 v)
	{
		return new Vector3(v.x, v.y, 0);
	}
	
	public static IntVector2 operator+(IntVector2 a, IntVector2 b)
	{
		return new IntVector2(a.x+b.x, a.y+b.y);
	}
	
	public static IntVector2 operator-(IntVector2 a, IntVector2 b)
	{
		return new IntVector2(a.x-b.x, a.y-b.y);
	}
	
	public static IntVector2 operator*(IntVector2 a, int b)
	{
		return new IntVector2(a.x*b, a.y*b);
	}
	
	public static IntVector2 operator/(IntVector2 a, int b)
	{
		return new IntVector2(a.x/b, a.y/b);
	}
	
	public static IntVector2 operator%(IntVector2 a, int b)
	{
		return new IntVector2(a.x%b, a.y%b);
	}
	
	
}
