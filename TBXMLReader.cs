// TBXMLReader.cs
//
// TBXMLReader is a C# adaptation of the fabulously performant TBXML ( https://github.com/71squared/TBXML )
//
// The MIT License (MIT)
// 
// Copyright (c) 2016 Rocco Bowling
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


using System;
using System.Text;
using System.Collections.Generic;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace TB
{
	public struct TBXMLAttribute {
		public long nameIdx;
		public long valueIdx;
	}

	public class TBXMLElement {
		public TBXMLReader tbxml;
		public long nameIdx = 0;
		public long textIdx = 0;
		public List<TBXMLAttribute> attributes = new List<TBXMLAttribute> ();

		#if ENABLE_IL2CPP
		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
		[Il2CppSetOption(Option.DivideByZeroChecks, false)]
		#endif
		public string GetName() {
			if (nameIdx == 0) {
				return "";
			}
			return System.Text.UTF8Encoding.Default.GetString (tbxml.bytes, (int)nameIdx, (int)tbxml.strlen (nameIdx));
		}

		#if ENABLE_IL2CPP
		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
		[Il2CppSetOption(Option.DivideByZeroChecks, false)]
		#endif
		public string GetText() {
			if (textIdx == 0) {
				return "";
			}
			return System.Text.UTF8Encoding.Default.GetString (tbxml.bytes, (int)textIdx, (int)tbxml.strlen (textIdx));
		}

		#if ENABLE_IL2CPP
		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
		[Il2CppSetOption(Option.DivideByZeroChecks, false)]
		#endif
		public string GetAttribute(string aName) {
			byte[] name = Encoding.ASCII.GetBytes (aName);

			for(int i = 0; i < attributes.Count; i++) {
				TBXMLAttribute attribute = attributes [i];
				if (tbxml.strlen (attribute.nameIdx) == name.Length && tbxml.strncmp (attribute.nameIdx, name, name.Length) == 0) {
					return System.Text.UTF8Encoding.Default.GetString (tbxml.bytes, (int)attribute.valueIdx, (int)tbxml.strlen (attribute.valueIdx));
				}
			}
			return null;
		}


	}

	public class TBXMLReader
	{
		const int TBXML_ATTRIBUTE_NAME_START = 0;
		const int TBXML_ATTRIBUTE_NAME_END = 1;
		const int TBXML_ATTRIBUTE_VALUE_START = 2;
		const int TBXML_ATTRIBUTE_VALUE_END = 3;
		const int TBXML_ATTRIBUTE_CDATA_END = 4;

		public byte[] bytes;
		public long bytesLength;

		public TBXMLReader(byte[] bytes, Action<TBXMLElement> onStartElement, Action<TBXMLElement> onEndElement, bool useDuplicateBytes = false) {

			// set up the bytes array
			if (useDuplicateBytes) {
				this.bytes = (byte[])bytes.Clone ();
			} else {
				this.bytes = bytes;
			}

			bytesLength = bytes.Length;

			// ensure null termination
			bytes[bytesLength-1] = 0;

			DecodeBytes(onStartElement, onEndElement);
		}
			
		public TBXMLReader(string xmlString, Action<TBXMLElement> onStartElement, Action<TBXMLElement> onEndElement) : this(Encoding.UTF8.GetBytes (xmlString), onStartElement, onEndElement, false) {

		}

		#region DECODE XML


		#if ENABLE_IL2CPP
		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
		[Il2CppSetOption(Option.DivideByZeroChecks, false)]
		#endif
		private long strstr1(long idx, byte b0){
			long bytesLengthMinusSearchSize = bytesLength - 1;
			byte[] localBytes = bytes;

			while (idx < bytesLengthMinusSearchSize) {
				if (localBytes [idx] == 0)
					break;

				if (localBytes [idx] == b0) {
					return idx;
				}
				idx++;
			}
			return bytesLength;
		}

		#if ENABLE_IL2CPP
		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
		[Il2CppSetOption(Option.DivideByZeroChecks, false)]
		#endif
		private long strstr2(long idx, byte b0, byte b1){
			long bytesLengthMinusSearchSize = bytesLength - 2;
			byte[] localBytes = bytes;

			while (idx < bytesLengthMinusSearchSize) {
				if (localBytes [idx] == 0) {
					break;
				}

				if (localBytes [idx] == b0) {
					if (localBytes [idx + 1] == b1) {
						return idx;
					}
				}
				idx++;
			}
			return bytesLength;
		}

		#if ENABLE_IL2CPP
		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
		[Il2CppSetOption(Option.DivideByZeroChecks, false)]
		#endif
		private long strstr3(long idx, byte b0, byte b1, byte b2){
			long bytesLengthMinusSearchSize = bytesLength - 3;
			byte[] localBytes = bytes;

			while (idx < bytesLengthMinusSearchSize) {
				if (localBytes [idx] == 0) {
					break;
				}

				if (localBytes [idx] == b0) {
					if (localBytes [idx + 1] == b1) {
						if (localBytes [idx + 2] == b2) {
							return idx;
						}
					}
				}
				idx++;
			}
			return bytesLength;
		}

		#if ENABLE_IL2CPP
		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
		[Il2CppSetOption(Option.DivideByZeroChecks, false)]
		#endif
		private long strstr9(long idx, byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8){
			long bytesLengthMinusSearchSize = bytesLength - 9;
			byte[] localBytes = bytes;
			byte r;

			while (idx < bytesLengthMinusSearchSize) {
				r = localBytes [idx];
				if (r == 0)
					return bytesLength;
				if (r == b0 && localBytes [idx + 8] == b8 && localBytes [idx + 7] == b7 && localBytes [idx + 6] == b6 && localBytes [idx + 5] == b5 && localBytes [idx + 4] == b4 && localBytes [idx + 3] == b3 && localBytes [idx + 2] == b2 && localBytes [idx + 1] == b1)
					return idx;

				idx++;
			}
			return bytesLength;
		}

		#if ENABLE_IL2CPP
		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
		[Il2CppSetOption(Option.DivideByZeroChecks, false)]
		#endif
		private long strpbrk3(long idx, byte b1, byte b2, byte b3){
			// From strpbrk man page:
			// The strpbrk() function locates in the null-terminated string s the first occurrence of any character in the string charset
			// and returns a pointer to this character.  If no characters from charset occur anywhere in s strpbrk() returns NULL.
			byte[] localBytes = bytes;
			byte r;

			// unroll the first few loops
			r = localBytes [idx+0]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx;
			r = localBytes [idx+1]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+1;
			r = localBytes [idx+2]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+2;
			r = localBytes [idx+3]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+3;
			r = localBytes [idx+4]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+4;
			r = localBytes [idx+5]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+5;
			r = localBytes [idx+6]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+6;
			r = localBytes [idx+7]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+7;
			r = localBytes [idx+8]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+8;
			r = localBytes [idx+9]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+9;
			r = localBytes [idx+10]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+10;
			r = localBytes [idx+11]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+11;
			r = localBytes [idx+12]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+12;
			r = localBytes [idx+13]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+13;
			r = localBytes [idx+14]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+14;
			r = localBytes [idx+15]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+15;
			r = localBytes [idx+16]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+16;
			r = localBytes [idx+17]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+17;
			r = localBytes [idx+18]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+18;
			r = localBytes [idx+19]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+19;
			r = localBytes [idx+20]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+20;
			r = localBytes [idx+21]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+21;
			r = localBytes [idx+22]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+22;
			r = localBytes [idx+23]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+23;
			r = localBytes [idx+24]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+24;
			r = localBytes [idx+25]; if (r == 0 || r == b1 || r == b2 || r == b3) return idx+25;

			idx += 26;

			do {
				r = localBytes [idx];
				if (r == 0 || r == b1 || r == b2 || r == b3) {
					return idx;
				}
				idx++;
			} while (idx < bytesLength);

			return idx;
		}

		#if ENABLE_IL2CPP
		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
		[Il2CppSetOption(Option.DivideByZeroChecks, false)]
		#endif
		private long strpbrk2(long idx, byte b1, byte b2){
			// From strpbrk man page:
			// The strpbrk() function locates in the null-terminated string s the first occurrence of any character in the string charset
			// and returns a pointer to this character.  If no characters from charset occur anywhere in s strpbrk() returns NULL.
			byte[] localBytes = bytes;
			byte r;

			// unroll the first few loops
			r = localBytes [idx+0]; if (r == 0 || r == b1 || r == b2) return idx;
			r = localBytes [idx+1]; if (r == 0 || r == b1 || r == b2) return idx+1;
			r = localBytes [idx+2]; if (r == 0 || r == b1 || r == b2) return idx+2;
			r = localBytes [idx+3]; if (r == 0 || r == b1 || r == b2) return idx+3;
			r = localBytes [idx+4]; if (r == 0 || r == b1 || r == b2) return idx+4;
			r = localBytes [idx+5]; if (r == 0 || r == b1 || r == b2) return idx+5;
			r = localBytes [idx+6]; if (r == 0 || r == b1 || r == b2) return idx+6;
			r = localBytes [idx+7]; if (r == 0 || r == b1 || r == b2) return idx+7;
			r = localBytes [idx+8]; if (r == 0 || r == b1 || r == b2) return idx+8;
			r = localBytes [idx+9]; if (r == 0 || r == b1 || r == b2) return idx+9;
			r = localBytes [idx+10]; if (r == 0 || r == b1 || r == b2) return idx+10;
			r = localBytes [idx+11]; if (r == 0 || r == b1 || r == b2) return idx+11;
			r = localBytes [idx+12]; if (r == 0 || r == b1 || r == b2) return idx+12;
			r = localBytes [idx+13]; if (r == 0 || r == b1 || r == b2) return idx+13;
			r = localBytes [idx+14]; if (r == 0 || r == b1 || r == b2) return idx+14;
			r = localBytes [idx+15]; if (r == 0 || r == b1 || r == b2) return idx+15;
			r = localBytes [idx+16]; if (r == 0 || r == b1 || r == b2) return idx+16;
			r = localBytes [idx+17]; if (r == 0 || r == b1 || r == b2) return idx+17;
			r = localBytes [idx+18]; if (r == 0 || r == b1 || r == b2) return idx+18;
			r = localBytes [idx+19]; if (r == 0 || r == b1 || r == b2) return idx+19;
			r = localBytes [idx+20]; if (r == 0 || r == b1 || r == b2) return idx+20;
			r = localBytes [idx+21]; if (r == 0 || r == b1 || r == b2) return idx+21;
			r = localBytes [idx+22]; if (r == 0 || r == b1 || r == b2) return idx+22;
			r = localBytes [idx+23]; if (r == 0 || r == b1 || r == b2) return idx+23;
			r = localBytes [idx+24]; if (r == 0 || r == b1 || r == b2) return idx+24;
			r = localBytes [idx+25]; if (r == 0 || r == b1 || r == b2) return idx+25;

			idx += 26;

			// and a failsafe loop for when we need it
			do {
				r = localBytes [idx];
				if (r == 0 || r == b1 || r == b2) {
					break;
				}
				idx++;
			} while (idx < bytesLength);

			return idx;
		}

		#if ENABLE_IL2CPP
		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
		[Il2CppSetOption(Option.DivideByZeroChecks, false)]
		#endif
		public long strncmp(long idx, byte[] b, long n){
			// From stncmp() man page:
			// The strcmp() and strncmp() functions return an integer greater than, equal to, or less than 0, according as the string s1 is
			// greater than, equal to, or less than the string s2.  The comparison is done using unsigned characters, so that `\200' is greater than `\0'.
			long i = 0;
			byte[] localBytes = bytes;

			while (i < n && idx < bytesLength && localBytes [idx] == b [i] && localBytes [idx] != 0) {
				i++;
				idx++;
			}

			if (i != b.Length) {
				// failed to match
				return -1;
			}

			return 0;
		}

		#if ENABLE_IL2CPP
		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
		[Il2CppSetOption(Option.DivideByZeroChecks, false)]
		#endif
		public long strlen(long idx){
			// From strlen man page:
			// The strlen() function returns the number of characters that precede the terminating NUL character.  The strnlen() function
			// returns either the same result as strlen() or maxlen, whichever is smaller.
			long startIdx = idx;
			byte[] localBytes = bytes;
			while (idx < bytesLength && localBytes [idx] != 0) {
				idx++;
			}
			return idx - startIdx;
		}

		#if ENABLE_IL2CPP
		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
		[Il2CppSetOption(Option.DivideByZeroChecks, false)]
		#endif
		private void memcpy(long dstIdx, long srcIdx, long n){
			while (n-- > 0) {
				bytes [dstIdx] = bytes [srcIdx];
				dstIdx++;
				srcIdx++;
			}
		}

		#if ENABLE_IL2CPP
		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
		[Il2CppSetOption(Option.DivideByZeroChecks, false)]
		#endif
		private void memset(long idx, byte v, long n){
			while (n-- > 0) {
				bytes [idx] = v;
				idx++;
			}
		}

		#if ENABLE_IL2CPP
		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
		[Il2CppSetOption(Option.DivideByZeroChecks, false)]
		#endif
		private int memcmp(long sIdx1, long sIdx2, long n) {
			// From memcmp man page:
			// The memcmp() function returns zero if the two strings are identical, otherwise returns the difference between the first two
			// differing bytes (treated as unsigned char values, so that `\200' is greater than `\0', for example).  Zero-length strings are
			// always identical.  This behavior is not required by C and portable code should only depend on the sign of the returned value.
			for (long i = 0; i < n; i++) {
				if (sIdx1 + i >= bytesLength || sIdx2 + i >= bytesLength) {
					return -1;
				}
				if(bytes [sIdx1] != bytes [sIdx2]) {
					return -1;
				}
			}

			return 0;
		}

		#if ENABLE_IL2CPP
		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
		[Il2CppSetOption(Option.DivideByZeroChecks, false)]
		#endif
		private bool isspace(byte v){
			return (v == ' ' || v == '\t' || v == '\n' || v == '\r' || v == '\v' || v == '\f');
		}
			
		#if ENABLE_IL2CPP
		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
		[Il2CppSetOption(Option.DivideByZeroChecks, false)]
		#endif
		private void DecodeBytes(Action<TBXMLElement> onStartElement, Action<TBXMLElement> onEndElement) {

			byte[] localBytes = bytes;
			long localBytesLength = bytesLength;

			Stack<TBXMLElement> elementStack = new Stack<TBXMLElement> ();
			Stack<TBXMLElement> freeElementList = new Stack<TBXMLElement> ();

			TBXMLAttribute xmlAttribute = new TBXMLAttribute ();

			// set elementStart pointer to the start of our xml
			long elementStartIdx = 0;

			// find next element start
			while ((elementStartIdx = strstr1 (elementStartIdx, (byte)'<')) < localBytesLength) {

				// detect comment section
				//if (strncmp (elementStartIdx, commentStartArray, commentStartArray.Length) == 0) {
				if (localBytes [elementStartIdx] == (byte)'<' && localBytes [elementStartIdx + 1] == (byte)'!' && localBytes [elementStartIdx + 2] == (byte)'-') {
					elementStartIdx = strstr3 (elementStartIdx, (byte)'-', (byte)'-', (byte)'>') + 3;
					continue;
				}

				// detect cdata section within element text
				//long isCDATA = strncmp (elementStartIdx, cdataStartArray, cdataStartArray.Length);
				// NOTE: logic is switched here to match the output of the strncmp() method, in which 0 == true
				long isCDATA = 1;
				if (elementStartIdx < localBytesLength - 9 && localBytes [elementStartIdx] == (byte)'<' && localBytes [elementStartIdx + 8] == (byte)'[' && localBytes [elementStartIdx + 1] == (byte)'!' && localBytes [elementStartIdx + 2] == (byte)'[' && localBytes [elementStartIdx + 3] == (byte)'C' && localBytes [elementStartIdx + 4] == (byte)'D' && localBytes [elementStartIdx + 5] == (byte)'A' && localBytes [elementStartIdx + 6] == (byte)'T' && localBytes [elementStartIdx + 7] == (byte)'A') {
					isCDATA = 0;
				}


				long elementEndIdx;

				// if cdata section found, skip data within cdata section and remove cdata tags
				if (isCDATA == 0) {

					// find end of cdata section
					long CDATAEndIdx = strstr3 (elementStartIdx, (byte)']', (byte)']', (byte)'>');

					// find start of next element skipping any cdata sections within text
					elementEndIdx = CDATAEndIdx;

					// find next open tag
					elementEndIdx = strstr1 (elementEndIdx, (byte)'<');
					// if open tag is a cdata section
					while (localBytes [elementEndIdx] == (byte)'<' && localBytes [elementEndIdx + 8] == (byte)'[' && localBytes [elementEndIdx + 1] == (byte)'!' && localBytes [elementEndIdx + 2] == (byte)'[' && localBytes [elementEndIdx + 3] == (byte)'C' && localBytes [elementEndIdx + 4] == (byte)'D' && localBytes [elementEndIdx + 5] == (byte)'A' && localBytes [elementEndIdx + 6] == (byte)'T' && localBytes [elementEndIdx + 7] == (byte)'A') {
						// find end of cdata section
						elementEndIdx = strstr3 (elementEndIdx, (byte)']', (byte)']', (byte)'>');
						// find next open tag
						elementEndIdx = strstr1 (elementEndIdx, (byte)'<');
					}

					// calculate length of cdata content
					long CDATALength = elementEndIdx - elementStartIdx;

					// calculate total length of text
					long textLength = elementEndIdx - elementStartIdx;

					// remove begining cdata section tag
					memcpy (elementStartIdx, elementStartIdx + 9, CDATAEndIdx - elementStartIdx - 9);

					// remove ending cdata section tag
					memcpy (CDATAEndIdx - 9, CDATAEndIdx + 3, textLength - CDATALength - 3);

					// blank out end of text
					memset (elementStartIdx + textLength - 12, (byte)' ', 12);

					// set new search start position 
					elementStartIdx = CDATAEndIdx - 9;
					continue;
				}


				// find element end, skipping any cdata sections within attributes
				elementEndIdx = elementStartIdx + 1;		
				while ((elementEndIdx = strpbrk2 (elementEndIdx, (byte)'<', (byte)'>')) < localBytesLength) {
					//if (strncmp (elementEndIdx, cdataStartArray, cdataStartArray.Length) == 0) {
					if (localBytes [elementEndIdx] == (byte)'<' && localBytes [elementEndIdx + 8] == (byte)'[' && localBytes [elementEndIdx + 1] == (byte)'!' && localBytes [elementEndIdx + 2] == (byte)'[' && localBytes [elementEndIdx + 3] == (byte)'C' && localBytes [elementEndIdx + 4] == (byte)'D' && localBytes [elementEndIdx + 5] == (byte)'A' && localBytes [elementEndIdx + 6] == (byte)'T' && localBytes [elementEndIdx + 7] == (byte)'A') {
						elementEndIdx = strstr3 (elementEndIdx, (byte)']', (byte)']', (byte)'>') + 3;
					} else {
						break;
					}
				}

				// check for end of everything
				if (elementEndIdx >= localBytesLength) {
					elementEndIdx = localBytesLength - 1;
				}

				// null terminate element end
				localBytes [elementEndIdx] = 0;

				// null terminate element start so previous element text doesnt overrun
				localBytes [elementStartIdx] = 0;

				// get element name start
				long elementNameStartIdx = elementStartIdx + 1;

				// ignore tags that start with ? or ! unless cdata "<![CDATA"
				if (localBytes [elementNameStartIdx] == (byte)'?' || (localBytes [elementNameStartIdx] == (byte)'!' && isCDATA != 0)) {
					elementStartIdx = elementEndIdx + 1;
					continue;
				}

				// ignore attributes/text if this is a closing element
				if (localBytes [elementNameStartIdx] == (byte)'/') {
					elementStartIdx = elementEndIdx + 1;

					// end of an element
					TBXMLElement closeElement = elementStack.Pop ();
					onEndElement (closeElement);
					closeElement.attributes.Clear ();
					freeElementList.Push (closeElement);

					if (elementStack.Count > 0) {
						TBXMLElement parentElement = elementStack.Peek (); 
						if (parentElement.textIdx > 0) {
							// trim whitespace from start of text
							while (isspace (localBytes [parentElement.textIdx]))
								parentElement.textIdx++;

							// trim whitespace from end of text
							long endIdx = parentElement.textIdx + strlen (parentElement.textIdx) - 1;
							while (endIdx > parentElement.textIdx && isspace (localBytes [endIdx])) {
								localBytes [endIdx] = 0;
								endIdx--;
							}
						}

							/*
						parentElement = parentXMLElement.parentElement;

						// if parent element has children clear text
						if (parentXMLElement != null && parentXMLElement.firstChild != null)
							parentXMLElement.textIdx = 0;
						*/
					}




					continue;
				}


				// is this element opening and closing
				bool selfClosingElement = (localBytes [elementEndIdx - 1] == '/');

				// create new xmlElement struct
				TBXMLElement xmlElement = null;
				if (freeElementList.Count > 0) {
					xmlElement = freeElementList.Pop ();
				} else {
					xmlElement = new TBXMLElement ();
					xmlElement.tbxml = this;
				}

				elementStack.Push (xmlElement);

				// set element name
				xmlElement.nameIdx = elementNameStartIdx;

				// in the following xml the ">" is replaced with \0 by elementEnd. 
				// element may contain no atributes and would return null while looking for element name end
				// <tile> 
				// find end of element name
				long elementNameEndIdx = strpbrk3 (xmlElement.nameIdx, (byte) ' ', (byte) '/', (byte) '\n');


				// if end was found check for attributes
				if (elementNameEndIdx < localBytesLength) {

					// null terminate end of elemenet name
					localBytes [elementNameEndIdx] = 0;

					long chrIdx = elementNameEndIdx;
					long nameIdx = 0;
					long valueIdx = 0;
					long CDATAStartIdx = 0;
					long CDATAEndIdx = 0;
					bool singleQuote = false;

					int mode = TBXML_ATTRIBUTE_NAME_START;

					// loop through all characters within element
					byte localChrByte;
					while (chrIdx++ < elementEndIdx) {
						localChrByte = localBytes [chrIdx];

						switch (mode) {
						// look for start of attribute name
						case TBXML_ATTRIBUTE_NAME_START:
							if (localChrByte == ' ' || localChrByte == '\t' || localChrByte == '\n' || localChrByte == '\r' || localChrByte == '\v' || localChrByte == '\f')
								continue;
							nameIdx = chrIdx;
							mode = TBXML_ATTRIBUTE_NAME_END;
							break;
						// look for end of attribute name
						case TBXML_ATTRIBUTE_NAME_END:
							if (localChrByte == ' ' || localChrByte == '\t' || localChrByte == '\n' || localChrByte == '\r' || localChrByte == '\v' || localChrByte == '\f' || localChrByte == '=') {
								localBytes [chrIdx] = 0;
								mode = TBXML_ATTRIBUTE_VALUE_START;
							}
							break;
						// look for start of attribute value
						case TBXML_ATTRIBUTE_VALUE_START:
							if (localChrByte == ' ' || localChrByte == '\t' || localChrByte == '\n' || localChrByte == '\r' || localChrByte == '\v' || localChrByte == '\f')
								continue;
							if (localChrByte == '"' || localChrByte == '\'') {
								valueIdx = chrIdx + 1;
								mode = TBXML_ATTRIBUTE_VALUE_END;
								if (localChrByte == '\'')
									singleQuote = true;
								else
									singleQuote = false;
							}
							break;
						// look for end of attribute value
						case TBXML_ATTRIBUTE_VALUE_END:
							if (localChrByte == (byte)'<' && localBytes [chrIdx + 8] == (byte)'[' && localBytes [chrIdx + 1] == (byte)'!' && localBytes [chrIdx + 2] == (byte)'[' && localBytes [chrIdx + 3] == (byte)'C' && localBytes [chrIdx + 4] == (byte)'D' && localBytes [chrIdx + 5] == (byte)'A' && localBytes [chrIdx + 6] == (byte)'T' && localBytes [chrIdx + 7] == (byte)'A') {
								mode = TBXML_ATTRIBUTE_CDATA_END;
							} else if ((localChrByte == '"' && singleQuote == false) || (localChrByte == '\'' && singleQuote == true)) {
								localBytes [chrIdx] = 0;

								// remove cdata section tags

								while ((CDATAStartIdx = strstr9 (valueIdx, (byte)'<', (byte)'!', (byte)'[', (byte)'C', (byte)'D', (byte)'A', (byte)'T', (byte)'A', (byte)'[')) < localBytesLength) {

									// remove begin cdata tag
									memcpy (CDATAStartIdx, CDATAStartIdx + 9, strlen (CDATAStartIdx) - 8);

									// search for end cdata
									CDATAEndIdx = strstr3 (CDATAStartIdx, (byte)']', (byte)']', (byte)'>');

									// remove end cdata tag
									memcpy (CDATAEndIdx, CDATAEndIdx + 3, strlen (CDATAEndIdx) - 2);
								}

								// create new attribute
								xmlAttribute.nameIdx = nameIdx;
								xmlAttribute.valueIdx = valueIdx;
								xmlElement.attributes.Add (xmlAttribute);

								// clear name and value pointers
								nameIdx = 0;
								valueIdx = 0;

								// start looking for next attribute
								mode = TBXML_ATTRIBUTE_NAME_START;
							}
							break;
						// look for end of cdata
						case TBXML_ATTRIBUTE_CDATA_END:
							if (localChrByte == ']') {
								if (localChrByte == (byte)']' && localBytes [chrIdx + 1] == (byte)']' && localBytes [chrIdx + 2] == (byte)'>') {
									mode = TBXML_ATTRIBUTE_VALUE_END;
								}
							}
							break;						
						default:
							break;
						}
					}
				}



					
				// if tag is not self closing, set parent to current element
				if (!selfClosingElement) {
					// set text on element to element end+1
					if (localBytes [elementEndIdx + 1] != '>') {
						xmlElement.textIdx = elementEndIdx + 1;
					}
				}

				onStartElement (elementStack.Peek ());

				if (selfClosingElement) {
					TBXMLElement closeElement = elementStack.Pop ();
					onEndElement (closeElement);
					closeElement.attributes.Clear ();
					freeElementList.Push (closeElement);
				}

				// start looking for next element after end of current element
				elementStartIdx = elementEndIdx + 1;

			}

			while (elementStack.Count > 0) {
				onEndElement (elementStack.Pop ());
			}
		}

		#endregion

	}
}
