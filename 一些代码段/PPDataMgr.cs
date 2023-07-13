using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Playables;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UIElements;
using Newtonsoft.Json.Linq;

public class PPDataMgr
{
    private static PPDataMgr instance=new PPDataMgr();

    public static PPDataMgr Instance
    {
        get { return instance; }
    }

    private PPDataMgr()
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data">要存储的数据</param>
    /// <param name="KeyName">数据对象唯一Key</param>
    public void SaveData(object data,string KeyName)
    {
        Type type = data.GetType();
        FieldInfo[] fieldInfos = type.GetFields();

        //Key规则
        //KeyName_数据类型_变量类型_变量名

        string saveKeyName = "";
        foreach (FieldInfo info in fieldInfos) { 
           saveKeyName=KeyName+"_"+type.Name+"_"+info.FieldType.Name+"_"+info.Name;
            SaveValue(info.GetValue(data), saveKeyName);
        }

    }

    private void SaveValue(object value,string keyName)
    {
        Type type=value.GetType();
        //switch只能常量
        if(type==typeof(string))
        {
            PlayerPrefs.SetString(keyName,value.ToString());
        }
        else if(type==typeof(int))
        {
            PlayerPrefs.SetInt(keyName,(int)value);
        }
        else if(type==typeof(float))
        {
            PlayerPrefs.SetFloat(keyName,(float)value);
        }
        else if(type==typeof(bool))
        {
            PlayerPrefs.SetInt(keyName, (bool)value ? 1 : 0);
        }
        else if(typeof(IList).IsAssignableFrom(type))
        {
            IList list=value as IList;
            PlayerPrefs.SetInt(keyName + "_Len", list.Count);
            int index = 0;
            foreach (object item in list)
            {
                SaveValue(item, keyName + index);
                index++;
            }
        }
        else if(typeof(IDictionary).IsAssignableFrom(type))
        {
            IDictionary dict=value as IDictionary;
            PlayerPrefs.SetInt(keyName + "_Len", dict.Count);
            int index = 0;
            foreach (object item in dict.Keys)
            {
                SaveValue(item, keyName + index+"_Key");
                SaveValue(dict[item],keyName+index+"_Value");
                index++;
            }
        }
        else
        {
            SaveData(value, keyName);
        }
        //Debug.Log(keyName + "---存储完毕");
        PlayerPrefs.Save();
    }

    public object LoadData(Type type,string KeyName)
    {
        FieldInfo[] fieldInfos=type.GetFields();
        object temp = Activator.CreateInstance(type);
        string loadKeyName = "";
        foreach (FieldInfo info in fieldInfos)
        {
            loadKeyName=KeyName+"_"+type.Name+"_"+info.FieldType.Name+"_"+info.Name;
            info.SetValue(temp, LoadValue(info.FieldType, loadKeyName));
        }
       
        return temp;
    }

    public object LoadValue(Type type,string keyName)
    {
        //Debug.Log(keyName + "--加载");
        if (type == typeof(string))
        {
           return PlayerPrefs.GetString(keyName, "NULL");
        }
        else if (type == typeof(int))
        {
            return PlayerPrefs.GetInt(keyName, 0);
        }
        else if (type == typeof(float))
        {
            return PlayerPrefs.GetFloat(keyName,0.0f);
        }
        else if (type == typeof(bool))
        {
            return PlayerPrefs.GetInt(keyName, 1)==1?true:false;
        }
        else if (typeof(IList).IsAssignableFrom(type))
        {
            IList temp = Activator.CreateInstance(type) as IList;
            int len = PlayerPrefs.GetInt(keyName + "_Len", 0);
            for (int i = 0; i < len; i++)
            {
                //Debug.Log("test--" + i);
                temp.Add(LoadValue(type.GetGenericArguments()[0], keyName + i));
            }
            return temp;
        }
        else if (typeof(IDictionary).IsAssignableFrom(type))
        {
            IDictionary temp=Activator.CreateInstance(type) as IDictionary;
            int len=PlayerPrefs.GetInt(keyName + "_Len", 0);
            for(int i=0;i<len; i++)
            {
                temp.Add(LoadValue(type.GetGenericArguments()[0], keyName + i + "_Key"),
                         LoadValue(type.GetGenericArguments()[1], keyName + i + "_Value"));
            }
            return temp;
        }
        else
        {
            return LoadData(type, keyName);
        }

    }
}
