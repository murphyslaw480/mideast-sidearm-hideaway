using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;
using System.Collections;
using System.IO;

public static class DataManager
{
    const string DataFileDir = "data/";
    const string c_refKeyword = "Ref";

    static Dictionary<Type, Dictionary<string, object>> s_dataDict;

    static DataManager()
    {
        s_dataDict = new Dictionary<Type, Dictionary<string, object>>();
    }

    /// <summary>
    /// Get the root element of the file that should store data elements of type T
    /// </summary>
    /// <typeparam name="T">Type of data element</typeparam>
    /// <returns></returns>
    static XElement getRoot<T>()
    {
        Type type = typeof(T);
        //try type and supertypes successively to find appropriate xml file
        while (type != null)
        {
            string xmlPath = DataFileDir + type.Name + ".xml";
            if (File.Exists(xmlPath))
            {
                return XElement.Load(xmlPath);
            }
            type = type.BaseType;
        }

        throw new Exception(String.Format("Could not find file for {0}", typeof(T).Name));
    }

    static XElement GetDataElement<T>(string key)
    {
        string typeName = typeof(T).Name;
        //find XElement matching name
        XElement root = getRoot<T>();
        XElement data = root.Descendants(typeName)
                            .FirstOrDefault(
                                el => el.Attribute("Name") != null
                                   && el.Attribute("Name").Value.Equals(key)
                                   || el.Element("Name") != null
                                   && el.Element("Name").Value.Equals(key));
        if (data == null)
        {
            throw new DataManagerException(typeof(T), key);
        }
        return data;
    }

    static void loadData<T>(string key)
    {
        Type dataType = typeof(T);
        object data = ParseElement<T>(GetDataElement<T>(key));

        if (!s_dataDict.ContainsKey(dataType))
        {
            s_dataDict.Add(dataType, new Dictionary<string,object>());
        }
        s_dataDict[dataType].Add(key, data);
    }

    public static T[] ParseArray<T>(XElement el) 
    {
        List<T> list = new List<T>();
        foreach (XElement x in el.Descendants())
        {
            list.Add((T)ParseElement<T>(x));
        }

        return list.ToArray<T>();
    }

    public static T[] ParseAttributeArray<T>(XAttribute at) 
    {
        List<T> list = new List<T>();
        foreach (string s in at.Value.Split(','))
        {
            list.Add((T)Convert.ChangeType(s,typeof(T)));
        }

        return list.ToArray<T>();
    }
    
    /// <summary>
    /// Parse an XElement into a data object of type T
    /// </summary>
    /// <typeparam name="T">Data Type</typeparam>
    /// <param name="el">XML Element</param>
    /// <returns>A Data object of type T</returns>
    public static T ParseElement<T>(XElement el)
    {
        Type dataType = typeof(T);

        try
        {   //primitive type
            return (T)Convert.ChangeType(el.Value, dataType);
        }
        catch 
        {   //complex sub-type
            //get type of field
            //parse attributes
            T data = Activator.CreateInstance<T>();
            if (el.Name.LocalName.EndsWith(c_refKeyword))
            {
                populateDataRef<T>(ref data, GetData<T>(el.Attribute("Name").Value));
            }

            foreach (XAttribute at in el.Attributes())
            {
                string fieldName = at.Name.LocalName;
                System.Reflection.FieldInfo p = dataType.GetField(fieldName);
                if (p.FieldType.IsArray)
                {
                    MethodInfo method = typeof(DataManager).GetMethod("ParseAttributeArray");
                    MethodInfo genericMethod = method.MakeGenericMethod(new Type[] { p.FieldType.GetElementType() });
                    dataType.GetField(fieldName).SetValue(data, genericMethod.Invoke(null, new object[] { at }));
                }
                else
                {
                    p.SetValue(data, Convert.ChangeType(at.Value, p.FieldType));
                }
            }

            foreach (XElement subel in el.Elements())
            {
                //get field from element name
                string fieldName = subel.Name.LocalName;
                if (fieldName.EndsWith(c_refKeyword))
                {   //strip ref keyword from end if present
                    fieldName = fieldName.Remove(fieldName.Length - c_refKeyword.Length, c_refKeyword.Length);
                }
                System.Reflection.FieldInfo p = dataType.GetField(fieldName);
                Type subElType = p.FieldType;

                if (subElType.IsArray)
                {   //special handling for sub-arrays
                    Type arrayElementType = subElType.GetElementType();
                    MethodInfo method = typeof(DataManager).GetMethod("ParseArray");
                    MethodInfo genericMethod = method.MakeGenericMethod(new Type[] { arrayElementType });
                    dataType.GetField(fieldName).SetValue(data, genericMethod.Invoke(null, new object[] {subel}));
                }
                else
                {
                    //parse sub-elements with a recursive call to parseElement
                    MethodInfo method = typeof(DataManager).GetMethod("ParseElement");
                    MethodInfo genericMethod = method.MakeGenericMethod(new Type[] { subElType });
                    var subData = genericMethod.Invoke(null, new object[] { subel });
                    dataType.GetField(fieldName).SetValue(data, Convert.ChangeType(subData, subElType));
                }
            }

            return data;
        }

    }

    static void populateDataRef<T>(ref T refData, T sourceData)
    {
        Type type = typeof(T);
        foreach (FieldInfo field in type.GetFields())
        {
            field.SetValue(refData, field.GetValue(sourceData));
        }
    }

    public static T GetData<T>(string key)
    {
        Type dataType = typeof(T);
        if (!s_dataDict.ContainsKey(dataType) || !s_dataDict[dataType].ContainsKey(key))
        {
            loadData<T>(key);
        }
        return (T)s_dataDict[dataType][key];
    }
}

public class DataManagerException : Exception
{
    Type _dataType;
    string _dataKey;

    public DataManagerException (Type dataType, string key)
	{
        _dataType = dataType;
        _dataKey = key;
	}

    public override string Message
    {
        get
        {
            return String.Format("Could not find data object {0} : {1}", _dataKey, _dataType.Name);
        }
    }

}