using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using OmekFramework.Common.BasicTypes;

public static class EditorGUIUtils
{
    private static void ObjectGUIIter(ref object obj,Type type, GUIContent objName, bool isReadOnly, int maxDepth, List<bool> foldoutInfo, ref int foldoutIndex, bool parentFoldout)
    {
        FieldInfo[] fields = type.GetFields();
        if (fields.Length == 0)
        {
            EditorGUILayout.LabelField(objName);
            return;
        }
        EditorGUI.indentLevel -= 1;
        if (EditorGUI.indentLevel == 0)
        {
            EditorGUI.indentLevel -= 1;
        }
        if (foldoutIndex >= foldoutInfo.Count)
        {
            foldoutInfo.Add(false);
            RegisterUndo();
        }
        if (parentFoldout == true)
        {
            bool oldValue = foldoutInfo[foldoutIndex];
            foldoutInfo[foldoutIndex] = EditorGUILayout.Foldout(foldoutInfo[foldoutIndex], objName);
            if (oldValue != foldoutInfo[foldoutIndex])
            {
                RegisterUndo();
            }
        }
        bool shouldDisplay = parentFoldout & foldoutInfo[foldoutIndex];
        EditorGUI.indentLevel += 1;
        if (EditorGUI.indentLevel == 0)
        {
            EditorGUI.indentLevel += 1;
        }
        foldoutIndex += 1;        
        foreach (FieldInfo field in fields)
        {
            object fieldObj = field.GetValue(obj);
            if (fieldObj == null)
            {
                if (field.FieldType == typeof(string))
                {
                    fieldObj = "" as object;
                }
                else
                {
                    fieldObj = Activator.CreateInstance(field.FieldType, true);
                }
            }
            GUIContent guiContent = GetGUIContentFromFieldInfo(field);
            if (guiContent != null)
            {
                ObjectGUI(ref fieldObj,field.FieldType, guiContent, shouldDisplay, isReadOnly, maxDepth, foldoutInfo, ref foldoutIndex);
                field.SetValue(obj, fieldObj);
                
            }

        }
        //EditorGUI.indentLevel += identFix;
    } 

    private static void ObjectGUI(ref object obj, Type type, GUIContent guiContent, bool shouldDisplay, bool isReadOnly, int maxDepth, List<bool> foldoutInfo, ref int foldoutIndex)
    {        
        if (type == typeof(float))
        {
            if (shouldDisplay == true)
            {                
                float newValue = EditorGUILayout.FloatField(guiContent, (float)obj);
                if (isReadOnly == false && newValue != (float)obj)
                {
                    RegisterUndo();
                    obj = newValue;
                }
            }
        }
        else if (type == typeof(int))
        {
            if (shouldDisplay == true)
            {
                int newValue = EditorGUILayout.IntField(guiContent, (int)obj);
                if (isReadOnly == false && newValue != (int)obj)
                {
                    RegisterUndo();
                    obj = newValue;
                }
            }
        }
        else if (type == typeof(string))
        {
            if (shouldDisplay == true)
            {
                string newValue = EditorGUILayout.TextField(guiContent, (string)obj);
                if (isReadOnly == false && newValue != (string)obj)
                {
                    RegisterUndo();
                    obj = newValue;
                }
            }
        }
        else if (type == typeof(bool))
        {
            if (shouldDisplay == true)
            {
                bool newValue = EditorGUILayout.Toggle(guiContent, (bool)obj);
                if (isReadOnly == false && newValue != (bool)obj)
                {
                    RegisterUndo();
                    obj = newValue;
                }
            }
        }
        else if (type.IsEnum)
        {
            if (shouldDisplay == true)
            {
                Enum newValue = EditorGUILayout.EnumPopup(guiContent, (Enum)obj);
                if (isReadOnly == false && !newValue.Equals(obj))
                {
                    RegisterUndo();
                    obj = newValue;
                }
            }
        }
        else if (type == typeof(Color) || type == typeof(Color32))
        {
            if (shouldDisplay == true)
            {
                Color newValue = EditorGUILayout.ColorField(guiContent, (Color)obj);
                if (isReadOnly == false && !newValue.Equals(obj))
                {
                    Debug.Log(newValue.ToString() + " " + obj);
                    RegisterUndo();
                    obj = newValue;
                }
            }
        }
        else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
        {
            if (shouldDisplay == true)
            {
                UnityEngine.Object newValue = EditorGUILayout.ObjectField(guiContent, (UnityEngine.Object)obj, type,true);
                if (isReadOnly == false && newValue != obj)
                {
					Debug.Log(string.Format("{2}: newValue-{0}, obj-{1}",(newValue==null),(obj==null),guiContent.text));
					
                    RegisterUndo();
                    obj = newValue;
                }
            }
        }


        else if (type.IsArray)
        {
            if (maxDepth > 0)
            {
                if (foldoutIndex >= foldoutInfo.Count)
                {
                    foldoutInfo.Add(false);
                    RegisterUndo();
                }
                if (shouldDisplay)
                {
                    if (shouldDisplay == true)
                    {
                        EditorGUI.indentLevel -= 1;
                        bool oldValue = foldoutInfo[foldoutIndex];
                        foldoutInfo[foldoutIndex] = EditorGUILayout.Foldout(foldoutInfo[foldoutIndex], guiContent);
                        if (oldValue != foldoutInfo[foldoutIndex])
                        {
                            RegisterUndo();
                        }
                        EditorGUI.indentLevel += 1;
                    }
                }
                bool shouldDisplayChilds = shouldDisplay & foldoutInfo[foldoutIndex];
                foldoutIndex += 1;
                System.Array array = (System.Array)obj;
                if (shouldDisplayChilds)
                {
                    EditorGUI.indentLevel += 1;
                    int length = EditorGUILayout.IntField("Size", array.Length);
                    if (length != array.Length)
                    {
                        RegisterUndo();
                        Array newArray = Array.CreateInstance(array.GetType().GetElementType(), length);
                        Array.Copy(array, 0, newArray, 0, Math.Min(length, array.Length));
                        if (length > array.Length)
                        {
                            for (int i = array.Length; i < length; i++)
                            {

                                if (newArray.GetType().GetElementType() == typeof(String))
                                {
                                    if (i > 0)
                                    {
                                        newArray.SetValue(newArray.GetValue(i-1), i);
                                    }
                                    else
                                    {
                                        newArray.SetValue("", i);
                                    }
                                }
                                else
                                {
                                    newArray.SetValue(Activator.CreateInstance(newArray.GetType().GetElementType(), true), i);
                                }
                            }
                        }
                        array = newArray;
                        obj = array;
                    }
                    EditorGUI.indentLevel -= 1;
                }

                for (int index = 0; index < array.Length; index++)
                {
                    object arrElement = array.GetValue(index);
                    ObjectGUI(ref arrElement,array.GetType().GetElementType(), GetGUIContentFormObject(arrElement, null, index), shouldDisplayChilds, isReadOnly, maxDepth - 1, foldoutInfo, ref  foldoutIndex);
                    //ObjectGUIIter(ref arrElement, array.GetType().GetElementType(), GetGUIContent(arrElement, null, index), isReadOnly, maxDepth - 1, foldoutInfo, ref foldoutIndex, shouldDisplayChilds);
                    array.SetValue(arrElement, index);
                }
                

            }
        }
        else if (type.IsClass == true)
        {
            if (maxDepth > 0)
            {
                EditorGUI.indentLevel += 1;
                ObjectGUIIter(ref obj, type, guiContent, isReadOnly, maxDepth - 1, foldoutInfo, ref foldoutIndex, shouldDisplay);
                EditorGUI.indentLevel -= 1;
            }
        }
    }

    private static GUIContent GetGUIContentFromFieldInfo(FieldInfo field,string name=null)
    {
        bool hideInInspector = (field.GetCustomAttributes(typeof(HideInInspector), true) as HideInInspector[]).Length > 0;
        if (hideInInspector)
        {
            return null;
        }
        GUIContent guiContent;
        if (name == null)
        {
            guiContent = new GUIContent(ObjectNames.NicifyVariableName(field.Name));
        }
        else
        {
            guiContent = new GUIContent(ObjectNames.NicifyVariableName(name));
        }
		TooltipAttribute[] tooltips = field.GetCustomAttributes(typeof(TooltipAttribute), true) as TooltipAttribute[];
        if (tooltips != null && tooltips.Length > 0)
        {
            guiContent.tooltip = tooltips[0].Tooltip;
        }
        return guiContent;
    }

    private static GUIContent GetGUIContentFormObject(System.Object obj,string name,int index = 0)
    {
        GUIContent guiContent;

        if (obj == null)
        {
            if (name != null)
            {
                guiContent = new GUIContent(ObjectNames.NicifyVariableName(name));
            }
            else
            {
                guiContent = new GUIContent("Element " + index);
            }

        }
        else
        {
            System.Type type = obj.GetType();

            if (name != null)
            {
                guiContent = new GUIContent(ObjectNames.NicifyVariableName(name));
            }
            else
            {
                FieldInfo nameField = type.GetField("name", BindingFlags.IgnoreCase);
                if (nameField != null && nameField.FieldType == typeof(string))
                {
                    guiContent = new GUIContent(ObjectNames.NicifyVariableName((string)nameField.GetValue(obj)));
                }
                else
                {
                    guiContent = new GUIContent("Element " + index);
                }
            }
            bool hideInInspector = (type.GetCustomAttributes(typeof(HideInInspector), true) as HideInInspector[]).Length > 0;
            if (hideInInspector)
            {
                return null;
            }
            TooltipAttribute[] tooltips = type.GetCustomAttributes(typeof(TooltipAttribute), true) as TooltipAttribute[];
            if (tooltips != null && tooltips.Length > 0)
            {
                guiContent.tooltip = tooltips[0].Tooltip;
            }
        }
        return guiContent;
    }

    public static List<bool> ObjectGUI(object obj, string objName, bool isReadOnly, int maxDepth, List<bool> foldoutInfo)
    {
        if (foldoutInfo == null)
        {        
            foldoutInfo = new List<bool>();
            RegisterUndo();
        }
        int foldoutIndex = 0;
        GUIContent guiContent = GetGUIContentFormObject(obj,objName);
        if (guiContent != null)
        {
            ObjectGUIIter(ref obj, obj.GetType(), guiContent, isReadOnly, maxDepth, foldoutInfo, ref foldoutIndex, true);        
        }
        return foldoutInfo;
    }

    static UnityEngine.Object undoObj = null;
    public static List<bool> ComponentGUI(UnityEngine.Object obj, List<bool> foldoutInfo)
    {
        undoObj = obj;
        if (foldoutInfo == null)
        {          
            foldoutInfo = new List<bool>();
            RegisterUndo();
        }
        int foldoutIndex = 0;

        FieldInfo[] fields = obj.GetType().GetFields();
        foreach (FieldInfo field in fields)
        {
            object fieldObj = field.GetValue(obj);
            if (fieldObj == null)
            {
                if (field.FieldType == typeof(string))
                {
                    fieldObj = "" as object;
                }
                else
                {
                    fieldObj = Activator.CreateInstance(field.FieldType, true);
                }
            }
            GUIContent guiContent = GetGUIContentFromFieldInfo(field, ObjectNames.NicifyVariableName(field.Name));
            if (guiContent != null)
            {
                ObjectGUI(ref fieldObj, field.FieldType, guiContent, true, false, int.MaxValue, foldoutInfo, ref foldoutIndex);
                field.SetValue(obj, fieldObj);
                
            }
        }
        undoObj = null;

        return foldoutInfo;
    }

    private static void RegisterUndo()
    {
        if (undoObj != null)
        {            
            Undo.RegisterUndo(undoObj, "Inspector");
            EditorUtility.SetDirty(undoObj);
        }
    }
}
