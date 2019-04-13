﻿
using UnityEngine;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Reflection;

// Right now if is just a utility holder for random math stuff
using System.Collections.Specialized;
using System.Collections;



static class ArrayExtension
{
	public static void Shuffle<T>(this IList<T> list)  
	{  
		int n = 5000;
		for(int i = 0; i < n; i++) {
			int k = UnityEngine.Random.Range(0, list.Count);
			int j = UnityEngine.Random.Range(0, list.Count);

			T value = list[k];  
			list[k] = list[j];  
			list[j] = value;  
		}
	}

	public static T Random<T>(this IList<T> list)  
	{  
		int k = UnityEngine.Random.Range(0, list.Count);
		return list[k];  
	}
	public static T Random<T>(this IList<T> list, System.Random r)  
	{  
		int k = r.Next () % (list.Count);
		return list[k];  
	}
}

public class MathR
{
	public static float DegreeToRadian(float angle)
	{
		return Mathf.PI * angle / 180.0f;
	}

	public static float RadianToDegree(float angle)
	{
		return angle * (180.0f / Mathf.PI);
	}

	public static float Bezier(float p1, float p2, float p3, float mu) {
		float mum1,mum12,mu2;
		mu2 = mu * mu;
		mum1 = 1 - mu;
		mum12 = mum1 * mum1;

		return (p1) * mum12 + 2 * (p2) * mum1 * mu + (p3) * mu2;
	}

	public static Vector2 Bezier(Vector2 p1, Vector2 p2, Vector2 p3, float mu) {
		float mum1,mum12,mu2;
		mu2 = mu * mu;
		mum1 = 1 - mu;
		mum12 = mum1 * mum1;

		return new Vector2 (
			(p1) [0] * mum12 + 2 * (p2) [0] * mum1 * mu + (p3) [0] * mu2,
			(p1) [1] * mum12 + 2 * (p2) [1] * mum1 * mu + (p3) [1] * mu2
		);
	}

	public static Vector3 Bezier(Vector3 p1, Vector3 p2, Vector3 p3, float mu) {
		float mum1,mum12,mu2;
		mu2 = mu * mu;
		mum1 = 1 - mu;
		mum12 = mum1 * mum1;

		return new Vector3 (
			(p1) [0] * mum12 + 2 * (p2) [0] * mum1 * mu + (p3) [0] * mu2,
			(p1) [1] * mum12 + 2 * (p2) [1] * mum1 * mu + (p3) [1] * mu2,
			(p1) [2] * mum12 + 2 * (p2) [2] * mum1 * mu + (p3) [2] * mu2
		);
	}

	public static float CatmullRomSpline(float x, float v0, float v1, float v2, float v3)
	{
		const float M12	= 1.0f;
		const float M21	= -0.5f;
		const float M23	= 0.5f;
		const float M31	= 1.0f;
		const float M32	= -2.5f;
		const float M33	= 2.0f;
		const float M34	= -0.5f;
		const float M41	= -0.5f;
		const float M42	= 1.5f;
		const float M43	= -1.5f;
		const float M44	= 0.5f;
		float c1,c2,c3,c4;

		c1 = M12*v1;
		c2 = M21*v0 + M23*v2;
		c3 = M31*v0 + M32*v1 + M33*v2 + M34*v3;
		c4 = M41*v0 + M42*v1 + M43*v2 + M44*v3;

		return(((c4*x + c3)*x +c2)*x + c1);
	}

	public static Vector3 CatmullRomSpline(float x, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
	{
		return new Vector3 (
			CatmullRomSpline (x, v0.x, v1.x, v2.x, v3.x),
			CatmullRomSpline (x, v0.y, v1.y, v2.y, v3.y),
			CatmullRomSpline (x, v0.z, v1.z, v2.z, v3.z));
	}

}

public class RandomR
{
	// Implementation of take from http://www.opensource.apple.com/source/Libc/Libc-583/stdlib/FreeBSD/rand.c
	// Under the following open source licences
	/*-
	 * Copyright (c) 1990, 1993
	 *	The Regents of the University of California.  All rights reserved.
	 *
	 * Redistribution and use in source and binary forms, with or without
	 * modification, are permitted provided that the following conditions
	 * are met:
	 * 1. Redistributions of source code must retain the above copyright
	 *    notice, this list of conditions and the following disclaimer.
	 * 2. Redistributions in binary form must reproduce the above copyright
	 *    notice, this list of conditions and the following disclaimer in the
	 *    documentation and/or other materials provided with the distribution.
	 * 3. All advertising materials mentioning features or use of this software
	 *    must display the following acknowledgement:
	 *	This product includes software developed by the University of
	 *	California, Berkeley and its contributors.
	 * 4. Neither the name of the University nor the names of its contributors
	 *    may be used to endorse or promote products derived from this software
	 *    without specific prior written permission.
	 *
	 * THIS SOFTWARE IS PROVIDED BY THE REGENTS AND CONTRIBUTORS ``AS IS'' AND
	 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
	 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
	 * ARE DISCLAIMED.  IN NO EVENT SHALL THE REGENTS OR CONTRIBUTORS BE LIABLE
	 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
	 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
	 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
	 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
	 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
	 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
	 * SUCH DAMAGE.
	 *
	 * Posix rand_r function added May 1999 by Wes Peters <wes@softweyr.com>.
	 */

	const uint RAND_MAX = 0x7fffffff;

	public static uint Rand(ref uint ctx)
	{
		/*
	     * Compute x = (7^5 * x) mod (2^31 - 1)
	     * wihout overflowing 31 bits:
	     *      (2^31 - 1) = 127773 * (7^5) + 2836
	     * From "Random number generators: good ones are hard to find",
	     * Park and Miller, Communications of the ACM, vol. 31, no. 10,
	     * October 1988, p. 1195.
	     */
		long hi, lo, x;

		/* Can't be initialized with 0, so use another value. */
		if (ctx == 0)
			ctx = 123459876;
		hi = ctx / 127773;
		lo = ctx % 127773;
		x = 16807 * lo - 2836 * hi;
		if (x < 0)
			x += 0x7fffffff;
		return ((ctx = (uint)x) % (RAND_MAX + 1));
	}

	public static float Randf(ref uint rnd) {
		return ((1.0f / RAND_MAX) * Rand(ref rnd));
	}

	public static float Range(float min, float max, ref uint rnd){
		return Randf (ref rnd) * (max - min) + min;
	}

	public static List<T> RandomList<T>(List<T> list, ref uint rnd) {
		List<T> s = new List<T>(list);
		uint count = (uint)list.Count;

		for(int i = 0; i < count; i++) {
			uint x = RandomR.Rand(ref rnd) % count;
			uint y = RandomR.Rand(ref rnd) % count;

			T t = s [(int)x];
			s [(int)x] = s [(int)y];
			s [(int)y] = t;
		}

		return s;
	}

	public static object RandomObjectFromOrderedDictionary(OrderedDictionary list, ref uint rnd) {
		if(list.Count == 0) return null;
		return list [(int)(RandomR.Rand(ref rnd) % list.Count)];
	}

	public static T RandomObjectFromList<T>(List<T> list, ref uint rnd) {
		if(list.Count == 0) return default(T);
		return list [(int)(RandomR.Rand(ref rnd) % list.Count)];
	}

	public static T RandomObjectFromArray<T>(T[] list, ref uint rnd) {
		if(list.Length == 0) return default(T);
		return list [(int)(RandomR.Rand(ref rnd) % list.Length)];
	}
}

public static class ListExtensions
{
	public static void RemoveOne<T>(this List<T> source, T obj)
	{
		int idx = source.IndexOf (obj);
		if (idx >= 0) {
			source.RemoveAt (idx);
		}
	}

	public static void RemoveOneRange<T>(this List<T> self, List<T> otherArray) {
		// The normal version of this removes ALL INSTANCES of objects in otherArray from myself.
		// In OUR version of this we want to remove just one of each that are in otherArray
		foreach (T obj in otherArray) {
			int idx = self.IndexOf (obj);
			if (idx >= 0) {
				self.RemoveAt (idx);
			}
		}
	}
}


public static class OrderedDictionaryExtensions
{
	public static void SortByValue(this OrderedDictionary source, Func<object,object,int> sorter)
	{
		List<DictionaryEntry> sortedList = new List<DictionaryEntry> ();
		
		foreach (DictionaryEntry entry in source) {
			sortedList.Add(entry);
		}

		sortedList.Sort((a, b) => sorter(a, b));
		source.Clear ();
		
		foreach (DictionaryEntry entry in sortedList) {
			source[entry.Key] = entry.Value;
		}
	}

	public static void SortByValue(this OrderedDictionary source)
	{
		List<DictionaryEntry> sortedList = new List<DictionaryEntry> ();
		
		foreach (DictionaryEntry entry in source) {
			sortedList.Add(entry);
		}
		
		sortedList.Sort((a, b) => a.ToString().CompareTo(b));
		source.Clear ();
		
		foreach (DictionaryEntry entry in sortedList) {
			source[entry.Key] = entry.Value;
		}
	}

	public static List<object> ToList(this OrderedDictionary source)
	{
		List<object> range = new List<object> ();
		foreach (DictionaryEntry entry in source) {
			if(entry.Value != null){
				range.Add(entry.Value);
			}
		}
		return range;
	}

	public static List<object> GetRange(this OrderedDictionary source, int start, int length)
	{
		List<object> range = new List<object> ();
		for(int i = start; i < start+length; i++){
			range.Add(source[i]);
		}
		return range;
	}

	public static void Add(this OrderedDictionary source, object thing)
	{
		source.Add (thing, thing);
	}

	public static void Insert(this OrderedDictionary source, int idx, object thing)
	{
		source.Insert (idx, thing, thing);
	}
	
	public static void AddRange(this OrderedDictionary source, List<object> other)
	{
		foreach (object x in other) {
			source.Add (x, x);
		}
	}

	public static void AddRange(this OrderedDictionary source, OrderedDictionary other)
	{
		foreach (object key in other.Keys) {
			source[key] = other[key];
		}
	}

	public static int IndexOf(this OrderedDictionary source, object obj){
		for(int i = 0; i < source.Count; i++){
			if(source[i] == obj){
				return i;
			}
		}
		return -1;
	}
	
	public static void RemoveOne(this OrderedDictionary source, object obj)
	{
		int idx = source.IndexOf (obj);
		if (idx >= 0) {
			source.RemoveAt (idx);
		}
	}
	
	public static void RemoveOneRange(this OrderedDictionary self, List<object> otherArray) {
		// The normal version of this removes ALL INSTANCES of objects in otherArray from myself.
		// In OUR version of this we want to remove just one of each that are in otherArray
		foreach (object obj in otherArray) {
			int idx = self.IndexOf (obj);
			if (idx >= 0) {
				self.RemoveAt (idx);
			}
		}
	}
}


public static class IntExtension
{
	// n.AmountAsString("Ship", "Ships");
	public static string AmountAsString(this int n, string label, string labelPlural) {
		if (n == 1) {
			return string.Format ("{0} {1}", n, label);
		}
		return string.Format ("{0} {1}", n, labelPlural);
	}
}

public static class FloatExtension
{
	static public bool GrowValueTowardsValueLinear(this float n, ref float current, float target, float velocity)
	{
		// animate one turbo size per second
		float v = velocity * Time.deltaTime;
		if (current < target) {
			current += v;
			if (current > target) {
				current = target;
			}
			return true;
		} else if (current > target) {
			current -= v;
			if (current < target) {
				current = target;
			}
			return true;
		}
		return false;
	}
}


public static class StringExtension
{
	public static int NumberOfOccurancesOfChar(this string source, char c)
	{
		char[] testchars = source.ToCharArray();
		int length = testchars.Length;
		int count = 0;
		for (int n = length-1; n >= 0; n--)
		{
			if (testchars[n] == c)
				count++;
		}
		return count;
	}
}

public static class Vector2Extension
{
	public static Vector2 PUParse(this Vector2 v, string value)
	{
		var elements = value.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
		if(elements.Length > 0)
			v.x = float.Parse (elements [0], System.Globalization.CultureInfo.InvariantCulture);
		if(elements.Length > 1)
			v.y = float.Parse (elements [1], System.Globalization.CultureInfo.InvariantCulture);
		return v;
	}

	public static string PUToString(this Vector2 v)
	{
		return string.Format ("{0:0.##},{1:0.##}", v.x, v.y);
	}

	public static float AngleSignedBetweenVectors(this Vector2 a, Vector2 b)
	{
		const float Epsilon = 1.192092896e-07F;
		Vector2 a2 = a.normalized;
		Vector2 b2 = b.normalized;

		float angle = Mathf.Atan2(a2.x * b2.y - a2.y * b2.x, Vector2.Dot(a2, b2));
		if( Mathf.Abs(angle) < Epsilon ) return 0.0f;
		return angle;
	}

	public static float SqrDistance(this Vector2 a, Vector2 b)
	{
		return (b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y);
	}

	public static Vector2 RotateLeft(this Vector2 v)
	{
		float x = -v.y;
		float y = v.x;
		return new Vector2(x, y);
	}

	public static Vector2 RotateZ(this Vector2 v, float radians)
	{
		float x = v.x * Mathf.Cos (radians) - v.y * Mathf.Sin (radians);
		float y = v.x * Mathf.Sin (radians) + v.y * Mathf.Cos (radians);
		return new Vector2(x, y);
	}

	public static Vector2 RotateByAngle(this Vector2 v, Vector2 pivot, float angle) {
		float rx = v.x - pivot.x;
		float ry = v.y - pivot.y;

		float t = rx;
		float cosa = (float)Math.Cos (angle), sina = (float)Math.Sin (angle);
		rx = t * cosa - ry * sina;
		ry = t * sina + ry * cosa;
		return new Vector2(rx + pivot.x, ry + pivot.y);
	}

	public static bool GrowValueTowardsValueLinear(this Vector2 x, ref Vector2 current, Vector2 target, float velocity) {

		bool didChangeSomething = false;

		if (current.x < target.x) {
			current.x += velocity;
			if (current.x > target.x) {
				current.x = target.x;
			}
			didChangeSomething = true;
		} else if (current.x > target.x) {
			current.x -= velocity;
			if (current.x < target.x) {
				current.x = target.x;
			}
			didChangeSomething = true;
		}

		if (current.y < target.y) {
			current.y += velocity;
			if (current.y > target.y) {
				current.y = target.y;
			}
			didChangeSomething = true;
		} else if (current.y > target.y) {
			current.y -= velocity;
			if (current.y < target.y) {
				current.y = target.y;
			}
			didChangeSomething = true;
		}

		return didChangeSomething;
	}

}

public static class Vector3Extension
{
	public static Vector3 PUParse(this Vector3 v, string value)
	{
		var elements = value.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
		if(elements.Length > 0)
			v.x = float.Parse (elements [0], System.Globalization.CultureInfo.InvariantCulture);
		if(elements.Length > 1)
			v.y = float.Parse (elements [1], System.Globalization.CultureInfo.InvariantCulture);
		if(elements.Length > 2)
			v.z = float.Parse (elements [2], System.Globalization.CultureInfo.InvariantCulture);
		return v;
	}

	public static string PUToString(this Vector3 v)
	{
		return string.Format ("{0:0.##},{1:0.##},{2:0.##}", v.x, v.y, v.z);
	}

	public static Vector3 RotateLeft(this Vector3 v)
	{
		float x = -v.y;
		float y = v.x;
		v.x = x;
		v.y = y;
		return v;
	}

	public static Vector3 RotateRight(this Vector3 v)
	{
		float x = v.y;
		float y = v.x;
		v.x = x;
		v.y = y;
		return v;
	}

	public static Vector3 RotateLeftAboutY(this Vector3 v)
	{
		float x = -v.z;
		float z = v.x;
		v.x = x;
		v.z = z;
		return v;
	}

	public static Vector3 RotateRightAboutY(this Vector3 v)
	{
		float x = v.z;
		float z = -v.x;
		v.x = x;
		v.z = z;
		return v;
	}

	public static Vector3 RotateZ(this Vector3 v, float radians)
	{
		float x = v.x * Mathf.Cos (radians) - v.y * Mathf.Sin (radians);
		float y = v.x * Mathf.Sin (radians) + v.y * Mathf.Cos (radians);
		v.x = x;
		v.y = y;
		return v;
	}

	public static float SqrDistance(this Vector3 a, Vector3 b)
	{
		return (b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y) + (b.z - a.z) * (b.z - a.z);
	}

	public static float AngleSignedBetweenVectors(this Vector3 a, Vector3 b)
	{
		return Vector3.Angle (a, b);
	}

	public static bool GrowValueTowardsValueLinear(this Vector3 x, ref Vector3 current, Vector3 target, float velocity) {

		bool didChangeSomething = false;

		if (current.x < target.x) {
			current.x += velocity;
			if (current.x > target.x) {
				current.x = target.x;
			}
			didChangeSomething = true;
		} else if (current.x > target.x) {
			current.x -= velocity;
			if (current.x < target.x) {
				current.x = target.x;
			}
			didChangeSomething = true;
		}

		if (current.y < target.y) {
			current.y += velocity;
			if (current.y > target.y) {
				current.y = target.y;
			}
			didChangeSomething = true;
		} else if (current.y > target.y) {
			current.y -= velocity;
			if (current.y < target.y) {
				current.y = target.y;
			}
			didChangeSomething = true;
		}

		if (current.z < target.z) {
			current.z += velocity;
			if (current.z > target.z) {
				current.z = target.z;
			}
			didChangeSomething = true;
		} else if (current.z > target.z) {
			current.z -= velocity;
			if (current.z < target.z) {
				current.z = target.z;
			}
			didChangeSomething = true;
		}
			
		return didChangeSomething;
	}

}

public static class Vector4Extension
{
	public static Vector4 PUParse(this Vector4 v, string value)
	{
		var elements = value.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
		if(elements.Length > 0)
			v.x = float.Parse (elements [0], System.Globalization.CultureInfo.InvariantCulture);
		if(elements.Length > 1)
			v.y = float.Parse (elements [1], System.Globalization.CultureInfo.InvariantCulture);
		if(elements.Length > 2)
			v.z = float.Parse (elements [2], System.Globalization.CultureInfo.InvariantCulture);
		if(elements.Length > 3)
			v.w = float.Parse (elements [3], System.Globalization.CultureInfo.InvariantCulture);
		return v;
	}

	public static string PUToString(this Vector4 v)
	{
		return string.Format ("{0:0.##},{1:0.##},{2:0.##},{3:0.##}", v.x, v.y, v.z, v.w);
	}

	public static float Width(this Vector4 v)
	{
		return v.z;
	}

	public static float Height(this Vector4 v)
	{
		return v.w;
	}

	public static bool GrowValueTowardsValueLinear(this Vector4 x, ref Vector4 current, Vector4 target, float velocity) {

		bool didChangeSomething = false;

		if (current.x < target.x) {
			current.x += velocity;
			if (current.x > target.x) {
				current.x = target.x;
			}
			didChangeSomething = true;
		} else if (current.x > target.x) {
			current.x -= velocity;
			if (current.x < target.x) {
				current.x = target.x;
			}
			didChangeSomething = true;
		}

		if (current.y < target.y) {
			current.y += velocity;
			if (current.y > target.y) {
				current.y = target.y;
			}
			didChangeSomething = true;
		} else if (current.y > target.y) {
			current.y -= velocity;
			if (current.y < target.y) {
				current.y = target.y;
			}
			didChangeSomething = true;
		}

		if (current.z < target.z) {
			current.z += velocity;
			if (current.z > target.z) {
				current.z = target.z;
			}
			didChangeSomething = true;
		} else if (current.z > target.z) {
			current.z -= velocity;
			if (current.z < target.z) {
				current.z = target.z;
			}
			didChangeSomething = true;
		}

		if (current.w < target.w) {
			current.w += velocity;
			if (current.w > target.w) {
				current.w = target.w;
			}
			didChangeSomething = true;
		} else if (current.w > target.w) {
			current.w -= velocity;
			if (current.w < target.w) {
				current.w = target.w;
			}
			didChangeSomething = true;
		}

		return didChangeSomething;
	}

}

public static class ColorExtension
{
	public static Color PUParse(this Color c, string value)
	{
		if (value.StartsWith ("#")) {
			try{
				int argb = Int32.Parse(value.Substring(1), NumberStyles.HexNumber);
				c.r = (float)((argb & 0xFF000000) >> 24) / 255.0f;
				c.g = (float)((argb & 0x00FF0000) >> 16) / 255.0f;
				c.b = (float)((argb & 0x0000FF00) >> 8) / 255.0f;
				c.a = (float)((argb & 0x000000FF) >> 0) / 255.0f;
				return c;
			}catch(Exception e){
				return Color.black;
			}
		}

		var elements = value.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
		c.r = float.Parse (elements [0], System.Globalization.CultureInfo.InvariantCulture);
		c.g = float.Parse (elements [1], System.Globalization.CultureInfo.InvariantCulture);
		c.b = float.Parse (elements [2], System.Globalization.CultureInfo.InvariantCulture);
		c.a = float.Parse (elements [3], System.Globalization.CultureInfo.InvariantCulture);
		return c;
	}

	public static string PUToString(this Color c)
	{
		return c.ToHex();
	}

	public static string ToHex(this Color color)
	{
		Color32 c = color;
		return "#" + c.r.ToString ("X2") + c.g.ToString ("X2") + c.b.ToString ("X2") + c.a.ToString ("X2");
	}
}

public static class PlayerPrefsExtension
{
	public static void SaveVector2(Vector2 v, string prefKey) {
		PlayerPrefs.SetString (prefKey, v.PUToString());
	}

	public static void SaveVector3(Vector3 v, string prefKey) {
		PlayerPrefs.SetString (prefKey, v.PUToString());
	}

	public static void SaveVector4(Vector4 v, string prefKey) {
		PlayerPrefs.SetString (prefKey, v.PUToString());
	}

	public static void SaveColor(Color v, string prefKey) {
		PlayerPrefs.SetString (prefKey, v.PUToString());
	}


	public static void LoadVector2(ref Vector2 v, string prefKey) {
		if (PlayerPrefs.HasKey (prefKey) == false) {
			return;
		}
		v = Vector2.zero.PUParse (PlayerPrefs.GetString (prefKey));
	}

	public static void LoadVector3(ref Vector3 v, string prefKey) {
		if (PlayerPrefs.HasKey (prefKey) == false) {
			return;
		}
		v = Vector3.zero.PUParse (PlayerPrefs.GetString (prefKey));
	}

	public static void LoadVector4(ref Vector4 v, string prefKey) {
		if (PlayerPrefs.HasKey (prefKey) == false) {
			return;
		}
		v = Vector4.zero.PUParse (PlayerPrefs.GetString (prefKey));
	}

	public static void LoadColor(ref Color v, string prefKey) {
		if (PlayerPrefs.HasKey (prefKey) == false) {
			return;
		}
		v = Color.black.PUParse (PlayerPrefs.GetString (prefKey));
	}





	public static void LoadVector2(Action<Vector2> v, string prefKey) {
		if (PlayerPrefs.HasKey (prefKey) == false) {
			return;
		}
		v (Vector2.zero.PUParse (PlayerPrefs.GetString (prefKey)));
	}

	public static void LoadVector3(Action<Vector3> v, string prefKey) {
		if (PlayerPrefs.HasKey (prefKey) == false) {
			return;
		}
		v (Vector3.zero.PUParse (PlayerPrefs.GetString (prefKey)));
	}

	public static void LoadVector4(Action<Vector4> v, string prefKey) {
		if (PlayerPrefs.HasKey (prefKey) == false) {
			return;
		}
		v (Vector4.zero.PUParse (PlayerPrefs.GetString (prefKey)));
	}

	public static void LoadColor(Action<Color> v, string prefKey) {
		if (PlayerPrefs.HasKey (prefKey) == false) {
			return;
		}
		v (Color.black.PUParse (PlayerPrefs.GetString (prefKey)));
	}


}