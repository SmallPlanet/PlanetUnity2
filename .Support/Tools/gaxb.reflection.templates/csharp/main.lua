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

TYPEMAP = {};
TYPEMAP["boolean"] = "bool";
TYPEMAP["short"] = "short";
TYPEMAP["int"] = "int";
TYPEMAP["nonNegativeInteger"] = "int";
TYPEMAP["positiveInteger"] = "int";
TYPEMAP["enum"] = "int";
TYPEMAP["long"] = "long";
TYPEMAP["string"] = "string";
TYPEMAP["base64Binary"] = "byte[]";
TYPEMAP["string"] = "string";
TYPEMAP["decimal"] = "float";
TYPEMAP["float"] = "float";
TYPEMAP["double"] = "double";
TYPEMAP["byte"] = "char";
TYPEMAP["date"] = "DateTime";
TYPEMAP["dateTime"] = "DateTime";


NULLTYPEMAP = {};
--NULLTYPEMAP["bool"] = "bool?";
NULLTYPEMAP["short"] = "short?";
NULLTYPEMAP["int"] = "int?";
NULLTYPEMAP["long"] = "long?";
NULLTYPEMAP["float"] = "float?";
NULLTYPEMAP["double"] = "double?";
NULLTYPEMAP["char"] = "char?";

NULLTYPEMAP["Vector2"] = "Vector2?";
NULLTYPEMAP["Vector3"] = "Vector3?";
NULLTYPEMAP["Vector4"] = "Vector4?";
NULLTYPEMAP["Color"] = "Color?";

function printAllKeys(t)
	print("===============")
	for k,v in pairs(t) do
		v = t[k];
		
		if(type(v) ~= "userdata") then
			print(k.." : "..type(v).." = "..tostring(v))
		else
			print(k.." : "..type(v))
		end
	end
	print("===============")
end

function table.val_to_str ( v )
  if "string" == type( v ) then
    v = string.gsub( v, "\n", "\\n" )
    if string.match( string.gsub(v,"[^'\"]",""), '^"+$' ) then
      return "'" .. v .. "'"
    end
    return '"' .. string.gsub(v,'"', '\\"' ) .. '"'
  else
    return "table" == type( v ) and table.tostring( v ) or
      tostring( v )
  end
end

function table.key_to_str ( k )
  if "string" == type( k ) and string.match( k, "^[_%a][_%a%d]*$" ) then
    return k
  else
    return "[" .. table.val_to_str( k ) .. "]"
  end
end

function table.tostring( tbl )
  local result, done = {}, {}
  if (tbl == nil) then return end
  for k, v in ipairs( tbl ) do
    table.insert( result, table.val_to_str( v ) )
    done[ k ] = true
  end
  for k, v in pairs( tbl ) do
    if not done[ k ] then
      table.insert( result,
        table.key_to_str( k ) .. "=" .. table.val_to_str( v ) )
    end
  end
  return "{" .. table.concat( result, "," ) .. "}"
end


function table.insertIfNotPresent(t,x)
	for _,v in ipairs(t) do
		if (v == x) then
			return
		end
	end
	table.insert(t,x)
end

function string.split(str,sep)
        local sep, fields = sep or ":", {}
        local pattern = string.format("([^%s]+)", sep)
        str:gsub(pattern, function(c) fields[#fields+1] = c end)
        return fields
end

function capitalizedString(x)
	if (x == nil) then return nil; end
	return (x:gsub("^%l", string.upper));
end

function namespaceInitials(x)
	if (x == nil) then return nil; end
	x = capitalizedString(x);
	return (x:gsub("[^A-Z]", ""));
end

function lowercasedString(x)
	return (x:gsub("^.", string.lower,1));
end

function isDictionaryName(v)
	return string.sub(v,1,string.len("Dictionary-"))=="Dictionary-";
end

function cleanedName(v)
	-- adds underscore to certain reserved names like class, id, restrict
	if (v == "id") then
		return "_id";
	elseif (v == "class") then
		return "_class";
	elseif (v == "restrict") then
		return "_restrict";
	else
		return v;
	end
end

function className(x)
	--printAllKeys(x)
	return namespaceInitials(x.namespace)..capitalizedString(x.name);
end

function pluralName(n)
	return n.."s";
end

function isPlural(v)
	if(v.maxOccurs ~= "1") then
		return true;
	end
	return false;
end

function superclassForItem(v)
	if(v.extension ~= nil) then
		return className(v.extension)
	end
	return "I"..capitalizedString(v.namespace);
end

function superclassFieldNameForItem(v)
	if(v.extension ~= nil) then
		return capitalizedString(v.extension.name);
	end
	return capitalizedString(v.namespace);
end

function hasSuperclass(v)
	return (v.extension ~= nil)
end

function classNameFromRef(r)
	local parts = string.split(r,":");
	if (#parts == 2) then
		return namespaceInitials(parts[1])..capitalizedString(parts[2]);
	else
		return "UNKNOWN_REF"
	end
end

function nameWithoutNamespace(r)
	local parts = string.split(r,":");
	return parts[2];
end

function simpleTypeForItem(v)
	local t = TYPEMAP[v.type];
	if (t ~= nil) then
		return v.type;
	end

	-- if t is nil then this is not a simple schema type.  We need to handle all possibilities here:
	t = v;
		
	-- If type is a function, then this is a reference to another type.  Call the function to dereference the other type
	if(type(t.type) == "table") then
		t = t.type;
	end
		
	if(t.type == "element") then
		return className(t);
	end
	if(t.type == "simple") then
		-- if simpleType, just grab the original type from xml
		return lowercasedString(v.type.namespace)..":"..v.type.name;
	end
	if (t.ref ~= nil) then
		return 	classNameFromRef(t.ref)
	end
	print(table.tostring(t))	
	print(typeForItem(v))
	return "UNDEFINEDX"
end

function isEnumForItem(v)
	local t = TYPEMAP[v.type];
	if (t == nil) then
		t = v;
		-- If type is a function, then this is a reference to another type.  Call the function to dereference the other type
		if(type(t.type) == "table") then
			t = t.type;
		end
		if(t.type == "simple") then
			-- if ENUM, then this is an int
			-- if ENUM_MASK, then this is an int
			-- if NAMED_ENUM, then this is the enum name
			-- if TYPEDEF, then this is a string
			local appinfo = gaxb_xpath(t.xml, "./XMLSchema:annotation/XMLSchema:appinfo");
			if(appinfo ~= nil) then
				appinfo = appinfo[1].content;
			end
			
			if(appinfo == "ENUM" or appinfo == "ENUM_MASK" or appinfo == "NAMED_ENUM") then
				return "true"
			end
		end
	end
	return false;
end

function nullableTypeForItem(v)
	local sourceT = typeForItem(v);
	local nulledV = NULLTYPEMAP[sourceT];
	if(nulledV ~= null) then
		return nulledV;
	end
	
	-- Need to handle ENUM_MASK and NAMED_ENUM
	t = TYPEMAP[v.type];
	if(t == nil) then
		t = v;
	
		-- If type is a function, then this is a reference to another type.  Call the function to dereference the other type
		if(type(t.type) == "table") then
			t = t.type;
		end
		
		if(t.type == "simple") then
			local appinfo = gaxb_xpath(t.xml, "./XMLSchema:annotation/XMLSchema:appinfo");
			if(appinfo ~= nil) then
				appinfo = appinfo[1].content;
			end
			
			if(appinfo == "ENUM_MASK" or appinfo == "NAMED_ENUM") then
				return capitalizedString(t.namespace).."."..t.name.."?";
			end
		end
	end
	
	return sourceT;
end

function typeForItem(v)
	local t = TYPEMAP[v.type];
	if(t == nil) then
		-- if t is nil then this is not a simple schema type.  We need to handle all possibilities here:
		t = v;
		
		-- If type is a function, then this is a reference to another type.  Call the function to dereference the other type
		if(type(t.type) == "table") then
			t = t.type;
		end
		
		if(t.ref ~= nil) then
			return classNameFromRef(t.ref);
		end
		
		if(t.type == "element") then
			return className(t);
		end
		
		if(t.type == "simple") then
			-- if ENUM, then this is an int
			-- if ENUM_MASK, then this is an int
			-- if NAMED_ENUM, then this is the enum name
			-- if TYPEDEF, then this is a string
			local appinfo = gaxb_xpath(t.xml, "./XMLSchema:annotation/XMLSchema:appinfo");
			if(appinfo ~= nil) then
				appinfo = appinfo[1].content;
			end
			
			if(appinfo == "ENUM") then
				return "int";
			end
			if(appinfo == "ENUM_MASK" or appinfo == "NAMED_ENUM") then
				return capitalizedString(t.namespace).."."..t.name;
			end
			if(appinfo == "TYPEDEF") then
				return "string"
			end
			
			-- If there is an appinfo, use that
			local appinfo = gaxb_xpath(t.xml, "./XMLSchema:annotation/XMLSchema:appinfo/XMLSchema:objc");
			if(appinfo == nil) then
				appinfo = gaxb_xpath(t.xml, "./XMLSchema:annotation/XMLSchema:appinfo");
			end
			if(appinfo ~= nil) then
				appinfo = appinfo[1].content;
				local type = TYPEMAP[appinfo];
				if(type == nil) then
					return appinfo;
				else
					return type;
				end
			end
			
			-- If there is no appinfo, then use the restriction
			local restrict = gaxb_xpath(t.xml, "./XMLSchema:restriction");
			if(restrict ~= nil) then
				-- is this schema type; need to resolve
				return TYPEMAP[restrict[1].attributes.base];
			end
			
			return "N/A_typeForItem"
		end
		
		if(t.maxOccurs == "1") then
			
			for k,v in pairs(schema.simpleTypes) do
				if (nameWithoutNamespace(t.type) == v.name) then
					local appinfo = gaxb_xpath(v.xml, "./XMLSchema:annotation/XMLSchema:appinfo");
					if(appinfo ~= nil) then
						return appinfo[1].content;
					end
				end
			end
		end
		
		return "UNDEFINED_typeForItem";
	end
	
	return t;
end

function typeNameForItem(v)
	-- return just the classname without " *" or the type name
	local t = typeForItem(v);
	if (string.sub(t,-2) == " *") then
		return string.sub(t,1,-3);
	elseif (string.sub(t,-1) == "*") then
		return string.sub(t,1,-2);
	end
	return t;
end

function isObject(v)
	-- return true if v represents an NSObject descendant, false otherwise (ie should we retain this guy)
	local t = TYPEMAP[v.type];
	if (t ~= nil) then
		-- t is not nil so we know the type already.  Check to see if it ends in *
		if (string.sub(t,-1) == "*") then
			return true;
		else
			return false;
		end
	else	
		-- if t is nil then this is not a simple schema type.  We need to handle all possibilities here:
		t = v;
		
		-- If type is a function, then this is a reference to another type.  Call the function to dereference the other type
		if(type(t.type) == "table") then
			t = t.type;
		end
		
		if(t.ref ~= nil) then
			return true;
		end
		
		if(t.type == "element") then
			return false;
		end

		if(t.type == "simple") then
			-- if ENUM, then this is an int
			-- if ENUM_MASK, then this is an int
			-- if NAMED_ENUM, then this is the enum name
			-- if TYPEDEF, then this is a string
			local appinfo = gaxb_xpath(t.xml, "./XMLSchema:annotation/XMLSchema:appinfo");
			if(appinfo ~= nil) then
				appinfo = appinfo[1].content;
			end
			
			if(appinfo == "ENUM" or appinfo == "ENUM_MASK" or appinfo == "NAMED_ENUM") then
				return false
			end
			if(appinfo == "TYPEDEF") then
				return true
			end
			
			-- If there is an appinfo, use that
			if(appinfo ~= nil) then
				-- is this schema type? If it is, it isn't an object
				local type = TYPEMAP[appinfo];
				if (type == nil or string.sub(type,-1) == "*") then
					return true;
				else
					return false;
				end
			end
			
			-- If there is no appinfo, then use the restriction
			local restrict = gaxb_xpath(t.xml, "./XMLSchema:restriction");
			if(restrict ~= nil) then
				-- is this schema type; need to resolve
				local type =  TYPEMAP[restrict[1].attributes.base];
				if (string.sub(type,-1) == "*") then
					return true;
				else
					return false;
				end
			end
			-- what to do if we get here?
			print("unhandled case in isObject(): 1")
			return "N/A_isObject"
		end
		-- or here?
		print("unhandled case in isObject(): 2 *** ")
		--print(table.tostring(v));
		return "UNDEFINED_isObject"
	end
end
	
function string:split(sep)
    local sep, fields = sep or ":", {}
    local pattern = string.format("([^%s]+)", sep)
    self:gsub(pattern, function(c) fields[#fields+1] = c end)
    return fields
end
	
function split2(pString, pPattern)
   local Table = {}  -- NOTE: use {n = 0} in Lua-5.0
   local fpat = "(.-)" .. pPattern
   local last_end = 1
   local s, e, cap = pString:find(fpat, 1)
   while s do
      if s ~= 1 or cap ~= "" then
		  table.insert(Table,cap)
      end
      last_end = e+1
      s, e, cap = pString:find(fpat, last_end)
   end
   if last_end <= #pString then
      cap = pString:sub(last_end)
      table.insert(Table, cap)
   end
   return Table
end

function allRequiredAttributesForClass(v)
	attributes = {};
	
	while ( v ~= null ) do
		for i,a in ipairs(v.attributes) do
			if(a.use == "required") then
				table.insert(attributes, a);
			end
		end
		v = v.extension;
	end
	
	return attributes;
end

function allAttributesForClass(v)
	attributes = {};
	
	while ( v ~= null ) do
		for i,a in ipairs(v.attributes) do
			table.insert(attributes, a);
		end
		v = v.extension;
	end
	
	return attributes;
end

function mixedAttributesForClass(v)
	attributes = {};
	u = v;
	
	while ( v ~= null ) do
		if(u ~= v) then
			for i,a in ipairs(v.attributes) do
				if(a.use == "required") then
					if(a.id ~= "GAXBIgnoreConstructor") then
						table.insert(attributes, a);
					end
				end
			end
		else
			for i,a in ipairs(v.attributes) do
				if(a.id ~= "GAXBIgnoreConstructor") then
					table.insert(attributes, a);
				end
			end
		end
		v = v.extension;
	end
	
	return attributes;
end

SCHEMA_NAMESPACE = schema.namespace;

gaxb_template("global.cs", capitalizedString(schema.namespace)..".cs", schema);

ALL_CLASSES = {};

-- Run through all of the elements and generate code files for them
for k,v in pairs(schema.elements) do
	-- if not in the schema namespace, skip
	if (schema.namespace == v.namespace) then	
		v.name = cleanedName(v.name);
		v.name = string.gsub(v.name, "Dictionary%-", "")
		for k1,v1 in pairs(v.attributes) do
			v1.originalName = v1.name;
			v1.isDictionary = isDictionaryName(v1.name);
			v1.name = cleanedName(v1.name);
			v1.name = string.gsub(v1.name, "Dictionary%-", "")
		end
		for k1,v1 in pairs(v.sequences) do
			v1.originalName = v1.name;
			v1.isDictionary = isDictionaryName(v1.name);
			v1.name = cleanedName(v1.name);
			v1.name = string.gsub(v1.name, "Dictionary%-", "")
		end
		
		
		
		ALL_CLASSES[className(v)] = v;
		
		print("Generating class file "..className(v).."...")
		gaxb_template("element_base.cs", className(v).."Base.cs", v);
		gaxb_template("element.cs", className(v)..".cs", v, false);
	end
end

















