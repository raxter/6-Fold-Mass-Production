using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class EncodableInt : IEncodable
{
	public static implicit operator EncodableInt(int i)
	{
		return new EncodableInt(i);
	}
		
	public int intValue { get; private set; }
	
	EncodableInt (int i)
	{
		intValue = i;
	}
	
	public IEnumerable<IEncodable> Encode()
	{
		yield break;
	}
	
	public bool Decode (Encoding encodings)
	{
		intValue = encodings.Int(0);
		
		return true;
	}
}

public class EncodableSubGroup : IEncodable
{
	public IEnumerable<IEncodable> enumerable { get; private set; }
	public EncodableSubGroup (IEnumerable<IEncodable> enumerableParam)
	{
		enumerable = enumerableParam;
	}
	
	public IEnumerable<IEncodable> Encode()
	{
		return enumerable;
	}
	
	public bool Decode (Encoding encodings)
	{
		return true;
	}
}

public interface IEncodable
{
	
	IEnumerable<IEncodable> Encode();
	
	bool Decode (Encoding encodings);
	
}

public enum EncodingType {Group, Int};

public class Encoding 
{
	int? intValue = null;
	
	List<Encoding> encodings;
	
	Encoding(IEnumerable<Encoding> encodingObjects)
	{
		encodings.AddRange(encodingObjects);
	}
	
	public Encoding(int i)
	{
		intValue = i;
	}
		
//	public Encoding() // blank list
//	{
//		encodings = new List<Encoding>();
//	}
	
	public Encoding(string encoding)
	{
		encodings = new List<Encoding>();
		
//		if (encoding == "")
//			return;
		
		encodings.AddRange(SplitEncoding(encoding));
	}
	
	public bool Validate (params EncodingType [] types)
	{
		if (types.Length != encodings.Count) return false;
		
		for (int i = 0 ; i < types.Length ; i++)
		{
			if (types[i] == EncodingType.Int && !IsInt(i))
				return false;
		}
		
		return true;
	}
	
	public bool IsInt(int index)
	{
		return encodings[index].intValue != null;
	}
	
	public int Int(int index)
	{
		return encodings[index].intValue.Value;
	}
	
	public Encoding SubEncoding(int index)
	{
		return encodings[index];
	}
	
	public int Count
	{
		get { return encodings == null ? -1 : encodings.Count; }
	}
	
	public string DebugString()
	{
		return DebugString(0);
	}
	
	public string DebugString(int indentLevel)
	{
		if (intValue.HasValue)
			return intValue.Value.ToString();
		
		string indent = "";
		for (int i = 0 ; i < indentLevel ; i++) indent += "   ";
		
		string ret = "";
		foreach (Encoding encoding in encodings)
		{
			if (encoding.intValue.HasValue)
				ret += indent + encoding.intValue.Value+"\n";
			else
				ret += indent + "-> (" + encoding.Count + ")\n"+encoding.DebugString(indentLevel+1);
		}
		return ret;
	}
	
	
	
	
	
	
	
	static char [] delimeters = {'.', ',', ';', ':', '!', '@', '#', '$', '%', '^', '&', '*'};
	const char safeDelim = '~';

	
	public static void Decode(IEncodable encodable, string encodedString)
	{
//		Debug.Log ("Decoding string \""+encodedString+"\"");
		encodable.Decode(new Encoding(encodedString));
	}
	
	public static void Decode(IEncodable encodable, Encoding encodedElement)
	{
		encodable.Decode(encodedElement);
	}
	
	public static void DecodeCopy(IEncodable encodable, IEncodable toCopy)
	{
		Decode(encodable, Encode(toCopy.Encode()));
	}
	
	public static string Encode(IEncodable encodable)
	{
		return Encode(encodable.Encode());
	}
	
	static string Encode(IEnumerable<IEncodable> enumerable)
	{
		List<string> stringParts = new List<string>();
		
		foreach (IEncodable o in enumerable)
		{
			if (o is EncodableInt)
			{
				stringParts.Add ( ""+NumberToCode((o as EncodableInt).intValue));
			}
			else
			{
//				Debug.Log("Encoding "+o);
				stringParts.Add (Encode(o));
			}
		}
		
		
//		string [] stringParts = children.ConvertAll((child) => Encode(child));
		string safeJoin = string.Join(""+safeDelim, stringParts.ToArray())+safeDelim;
		
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
	
	static IEnumerable<Encoding> SplitEncoding(string encoded)
	{
//		Debug.Log("Splitting \""+encoded+"\"");
		if (encoded == "")
		{
//			Debug.Log("Returning blank list");
			yield break;
		}
		
		int delimIndex = delimeters.Length-1;
		for ( ; delimIndex >= -1  ; delimIndex --)
		{
			if (delimIndex == -1)
			{
				foreach (char c in encoded)
				{
//					Debug.Log ("Returning int "+CodeToNumber(c) +"("+c+")");
					yield return new Encoding(CodeToNumber(c));
				}
				yield break;
			}
			if (encoded.Contains(""+delimeters[delimIndex]))
			{
				break;
			}
		}
		
		if (encoded.Length == 1) // must be a single char delimeter, so must be a blank list
		{
			yield break;
		}
		
//		Debug.Log("Delimeter "+delimeters[delimIndex]);
	
		string [] splits = encoded.Split(delimeters[delimIndex]);
		for(int i = 0 ; i < splits.Length-1 ; i++)
		{
			string s = splits[i];
//			Debug.Log("Split "+s);
			
			if (s.Length == 0 || s.Length > 1) // if it's blank of greater than 1
			{
//				Debug.Log ("Returning encoding \""+s+"\" ("+delimeters[delimIndex]+")");
				yield return new Encoding (s);
			}
			else if (s.Length == 1)
			{
				int encodedInt = CodeToNumber(s[0]);
				if (encodedInt == -1) // not a code, so probably a delimiter ... we hope
				{
//					Debug.Log ("Returning single char encoding \""+s+"\" ("+delimeters[delimIndex]+")");
					yield return new Encoding(s);
				}
				else
				{
//					Debug.Log ("Returning int "+ encodedInt +"("+s[0]+")");
					yield return new Encoding(encodedInt);
				}
			}
			
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
