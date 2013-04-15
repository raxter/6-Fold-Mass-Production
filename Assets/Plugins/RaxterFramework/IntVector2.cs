using UnityEngine;
using System.Collections;

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
	
	public static implicit operator Vector2 (IntVector2 v)
	{
		return new Vector2(v.x, v.y);
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
