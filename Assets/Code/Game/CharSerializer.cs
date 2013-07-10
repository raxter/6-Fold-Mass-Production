using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//public interface EncodableBase
//{
//}

public interface Encodable// : EncodableBase
{
	IEnumerable Encode();
}

//public interface EncodableWithContext<T> : EncodableBase
//{
//	IEnumerable EncodeWithContext(T t);
//}
//public interface EncodesWithContext<T> : EncodableBase
//{
//	T GetEncodingContext();
//}


//
//public struct Encoding
//{
////	public Encoding(params int [] numbers)
////	{

////		encoded = "";
////		foreach (int i in numbers)
////		{
////			encoded += NumberToCode(i);
////		}
////		
////	}
////	public Encoding(params Encoding [] encodings)
////	{
////		encoded = "";
////		encoded = JoinCodes(encodings);
////	}
////	public Encoding(int [] numbers, params Encoding [] encodings)
////	{
////		encoded = "";
////		foreach (int i in numbers)
////		{
////			encoded += NumberToCode(i);
////		}
////		encoded = JoinCodes(encodings);
////	}
//	
//	public Encoding(Encodable encodable)
//	{
//		encoded = encodable.Encode();
//	}
//	
//	
//	public Encoding(string encodedString)
//	{
//		encoded = encodedString;
//	}
//	string encoded;
//	
//	public string EncodedString { get { return encoded; } }
//	
//	static string [] delimeters = {" ", ".", ",", ";", ":", "!", "@", "#", "$", "%", "^", "&", "*"};
//	const string safeDelim = "~";
//	
//	static string JoinCodes(params Encodable [] parts)
//	{
//		string [] stringParts = System.Array.ConvertAll<Encodable, string>(parts, (encodable) => encodable.Get3CharUniqueID() + encodable.Encode());
//		string safeJoin = string.Join(safeDelim, stringParts);
//		
//		int delimIndex = 0;
//		for ( ; delimIndex <= delimeters.Length ; delimIndex ++)
//		{
//			if (delimIndex == delimeters.Length)
//			{
//				Debug.LogError("Run out of delimeters");
//				return "";
//			}
//			if (!safeJoin.Contains(delimeters[delimIndex]))
//			{
//				break;
//			}
//		}
//		
//		return safeJoin.Replace(safeDelim, delimeters[delimIndex]);
//	}
//	
//	
//	public IEnumerable<Encoding> SplitEncoding()
//	{
//		return SplitEncoding(encoded);
//	}
//	
//	static IEnumerable<Encoding> SplitEncoding(string encoded)
//	{
//		int delimIndex = delimeters.Length-1;
//		for ( ; delimIndex >= 0  ; delimIndex --)
//		{
//			if (delimIndex == -1)
//			{
//				Debug.LogError("NO delimeters found");
//				yield break;
//			}
//			if (!safeJoin.Contains(delimeters[delimIndex]))
//			{
//				break;
//			}
//		}
//		
//		foreach(string s in encoded.Split(delimeters[delimIndex]))
//		{
//			yield return new Encoding(s);
//		}
//	}
//	
//	
//}

public static class CharSerializer 
{
	
	static char [] delimeters = {' ', '.', ',', ';', ':', '!', '@', '#', '$', '%', '^', '&', '*'};
	const char safeDelim = '~';
	
	public static string Encode(Encodable encodable)
	{
		return Encode (encodable.Encode());
	}
		
	static string Encode(IEnumerable enumerable)
	{
		List<string> stringParts = new List<string>();
		
		string intList = "";
		foreach (object o in enumerable)
		{
			if (o is int)
			{
				intList += NumberToCode((int)o);
			}
			
			if (o is Encodable || o is IEnumerable)
			{
				if (intList != "")
				{
					stringParts.Add (intList);
					intList = "";
				}
				
				stringParts.Add ( o is Encodable ? Encode(o as Encodable) : Encode(o as IEnumerable));
				Debug.Log("Encoded "+stringParts[stringParts.Count-1]);
			}
		}
		if (intList != "")
			stringParts.Add (intList);
//		string [] stringParts = children.ConvertAll((child) => Encode(child));
		string safeJoin = string.Join(""+safeDelim, stringParts.ToArray());
		
		int delimIndex = 0;
		for ( ; delimIndex <= delimeters.Length ; delimIndex ++)
		{
			if (delimIndex == delimeters.Length)
			{
				Debug.LogError("Run out of delimeters");
				return "";
			}
			if (!safeJoin.Contains(""+delimeters[delimIndex]))
			{
				break;
			}
		}
		
		return safeJoin.Replace(safeDelim, delimeters[delimIndex]);
	}
	
	public static IEnumerable<string> SplitEncoding(string encoded)
	{
		Debug.Log("Splitting \""+encoded+"\"");
		int delimIndex = delimeters.Length-1;
		for ( ; delimIndex >= -1  ; delimIndex --)
		{
			if (delimIndex == -1)
			{
				yield return encoded;
				yield break;
			}
			if (encoded.Contains(""+delimeters[delimIndex]))
			{
				break;
			}
		}
		
		Debug.Log("Delimeter "+delimeters[delimIndex]);
		foreach(string s in encoded.Split(delimeters[delimIndex]))
		{
			yield return s;
		}
	}
	
	
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
	
//	public static IEnumerator<int> DecodeToInts(string code)
//	{
//		
//		foreach (char c in code)
//		{
//			yield return CodeToNumber(c);
//		}
//		
//	}
}
