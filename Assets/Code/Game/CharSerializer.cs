using UnityEngine;
using System.Collections;


public static class CharSerializer 
{
	
	
	public static int ToNumber(string s)
	{
		return LongCodeToNumber(s);
	}
	public static int LongCodeToNumber(string s)
	{
		int ret = 0;
		foreach (char c in s)
		{
			ret *= 62;
			ret += ToNumber(c);
		}
		return ret;
	}


	public static int ToNumber(char c)
	{
		return CodeToNumber(c);
	}
	public static int CodeToNumber(char c)
	{
		if (c >= '0' && c <= '9')
		{
			return c-'0';
		}
		if (c >= 'a' && c <= 'z')
		{
			return c-'a'+10;
		}
		if (c >= 'A' && c <= 'Z')
		{
			return c-'A'+36;
		}
		
		return -1;
	}
	
	public static string ToLongCode(int i)
	{
		
		return NumberToLongCode(i);
	}
	public static string NumberToLongCode(int i)
	{
		string code = "";
		int remain = i;
		while(remain != 0)
		{
			code = NumberToCode(remain%62) + code;
			remain /= 62;
			
		}
		return code;
	}
	public static char ToCode(int i)
	{
		return NumberToCode(i);
	}
	public static char NumberToCode(int i)
	{
		if (i <= 9)
		{
			return (char)('0'+i);
		}
		
		if (i >= 10 && i < 36)
		{
			return (char)('a'+(i-10));
		}
		if (i >= 36 && i < 62)
		{
			return (char)('A'+(i-36));
		}
		
		return '!';
	}
}
