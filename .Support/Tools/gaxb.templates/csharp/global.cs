<%
-- Copyright (c) 2014 Chimera Software, LLC
--
-- Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files
-- (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge,
-- publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
-- subject to the following conditions:
--
-- The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
--
-- THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
-- MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
-- FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
-- WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 %>

using UnityEngine;

<%
CAP_NAME = capitalizedString(this.namespace);
%>
//
// Autogenerated by gaxb ( https://github.com/SmallPlanet/gaxb )
//

using System.Xml;
using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections;
using TB;

interface I<%=CAP_NAME%>
{
	void gaxb_load(TBXMLElement element, object _parent, Hashtable args);
	void gaxb_appendXML(StringBuilder sb);
}

public class <%=CAP_NAME%> {
	public int baseRenderQueue = 0;

<%

-- simpleType definitions, such as enums
for k,v in pairs(schema.simpleTypes) do
	-- only include defintions from this schema's namespace
	if (schema.namespace == v.namespace) then
		local appinfo = gaxb_xpath(v.xml, "./XMLSchema:annotation/XMLSchema:appinfo");
		local enums = gaxb_xpath(v.xml, "./XMLSchema:restriction/XMLSchema:enumeration");

		if(appinfo ~= nil) then
			appinfo = appinfo[1].content;
		end

		if(appinfo == "NAMED_ENUM") then
			gaxb_print("\tpublic enum "..v.name.." {\n")
			for k,v in pairs(enums) do
				gaxb_print("\t\t"..v.attributes.value..",\n")
			end
			gaxb_print("\t};\n\n")
		end
		if(appinfo == "ENUM") then
			gaxb_print("\tpublic static class "..v.name.." {\n")
			i = 0;
			for k1,v1 in pairs(enums) do
				gaxb_print("\t\tpublic const int "..v1.attributes.value.." = "..i..";\n")
				i = i + 1;
			end

			gaxb_print("\t\tstatic public int PUParse(string s) { \n")
			gaxb_print("\t\t\tstring[] parts = { ")
			for k1,v1 in pairs(enums) do
				gaxb_print('"'..v1.attributes.value..'",')
			end
			gaxb_print(' };\n')

			gaxb_print('\t\t\tint idx = Array.IndexOf(parts, s);\n')
			gaxb_print('\t\t\tif(idx == -1) { idx = (int)float.Parse(s); }\n')

			gaxb_print('\t\t\treturn idx;\n\t\t}\n')

			gaxb_print("\t};\n\n")
		end
		if(appinfo == "ENUM_MASK") then
			local i = 1
			gaxb_print("\tpublic enum "..v.name.." {\n")
			for k,v in pairs(enums) do
				gaxb_print("\t\t"..v.attributes.value.." = "..i..",\n")
				i = i * 2;
			end
			gaxb_print("\t};\n\n")
		end
		if(appinfo == "TYPEDEF") then
			for k,v in pairs(enums) do
				gaxb_print("\tpublic const string "..string.upper(v.attributes.value).." = \""..v.attributes.value.."\";\n")
			end
			gaxb_print("\n");
		end
	end
end

%>

	static public string ConvertClassName(string xmlNamespace, string name)
	{
		return Regex.Replace(xmlNamespace, "[^A-Z]", "")+name;
	}

	static public T clone<T>(T root) {
		return (T)loadXML( System.Text.Encoding.UTF8.GetBytes (writeXML (root)), null, null);
	}

	static public string writeXML(object root) {
		StringBuilder sb = new StringBuilder ();
		MethodInfo mInfo = root.GetType().GetMethod("gaxb_appendXML");
		if(mInfo != null) {
			mInfo.Invoke (root, new[] { sb });
		}
		return sb.ToString();
	}

	static public object loadXML(byte[] xmlBytes, object parentObject, Hashtable args, Action<object,object,TBXMLElement> customBlock)
	{
		object rootEntity = parentObject;
		object returnEntity = null;

		Stack<string> xmlNamespaces = new Stack<string> ();

		new TBXMLReader (xmlBytes, (element) => {

			string elementName = element.GetName ();

      string localXmlNamespace = element.GetAttribute ("xmlns");
			if (localXmlNamespace != null) {
				xmlNamespaces.Push (localXmlNamespace);
			} else if (xmlNamespaces.Count > 0) {
				localXmlNamespace = xmlNamespaces.Peek ();
			} else {
				localXmlNamespace = "";
			}

			try {
				Type entityClass = Type.GetType (ConvertClassName (localXmlNamespace, elementName), true);
				PUObject entityObject = (PUObject)(Activator.CreateInstance (entityClass));

				if (customBlock == null) {
					entityObject.gaxb_load (element, rootEntity, args);
					entityObject.gaxb_init ();
					entityObject.gaxb_final (element, rootEntity, args);
				} else {
					customBlock (entityObject, rootEntity, element);
				}

				rootEntity = entityObject;

				if (returnEntity == null) {
					returnEntity = entityObject;
				}
			} catch (TypeLoadException) {
				// If we get here its not a valid PU class, throw it away
			}
		}, (element) => {
			try {
				string elementName = element.GetName ();
				string localXmlNamespace = xmlNamespaces.Count > 0 ? xmlNamespaces.Peek () : "";
				if (element.GetAttribute ("xmlns") != null) {
					xmlNamespaces.Pop ();
				}

				// is this the closing end of a valid PU object?
				Type entityClass = Type.GetType (ConvertClassName (localXmlNamespace, elementName), true);

				if (entityClass != null) {
					if (customBlock == null) {
						PUObject entityObject = rootEntity as PUObject;
						entityObject.gaxb_complete ();
						entityObject.gaxb_private_complete ();
					}

					object parent = rootEntity.GetType ().GetField ("parent").GetValue (rootEntity);
					if (parent != null) {
						rootEntity = parent;
					}
				}
			} catch (TypeLoadException) {
			}
		});

		return returnEntity;
	}

	static public object loadXML(byte[] xmlBytes, object parentObject, Hashtable args)
	{
		return loadXML(xmlBytes, parentObject, args, null);
	}
}
