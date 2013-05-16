using UnityEngine;
using UnityEditor;
using System.Collections;

public static class SerializedObjectUtils
{
    public static bool IsPropertiesEqual(SerializedProperty prop1, SerializedProperty prop2)
    {
        if (prop1.depth != prop2.depth)
        {
            return false;
        }
        int startDepth = prop1.depth;

        SerializedProperty currProp1 = prop1.Copy();
        SerializedProperty currProp2 = prop2.Copy();
        bool prop1HasNext;
        bool prop2HasNext;
        bool hasStarted = false;
        do
        {
            if (currProp1.depth != currProp2.depth || currProp1.propertyType != currProp2.propertyType)
            {
                return false;
            }
            hasStarted = hasStarted | currProp1.depth > startDepth;
            switch (currProp1.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    if (currProp1.boolValue != currProp2.boolValue)
                    {
                        return false;
                    }
                    break;
                case SerializedPropertyType.Bounds:
                    if (currProp1.boundsValue != currProp2.boundsValue)
                    {
                        return false;
                    }
                    break;
                case SerializedPropertyType.Color:
                    if (currProp1.colorValue != currProp2.colorValue)
                    {
                        return false;
                    }
                    break;
                case SerializedPropertyType.Enum:
                    if (currProp1.enumValueIndex != currProp2.enumValueIndex)
                    {
                        return false;
                    }
                    break;
                case SerializedPropertyType.Float:
                    if (currProp1.floatValue != currProp2.floatValue)
                    {
                        return false;
                    }
                    break;
                case SerializedPropertyType.Integer:
                    if (currProp1.intValue != currProp2.intValue)
                    {
                        return false;
                    }
                    break;
                case SerializedPropertyType.ObjectReference:
                    if (currProp1.objectReferenceInstanceIDValue != currProp2.objectReferenceInstanceIDValue)
                    {
                        return false;
                    }
                    break;
                case SerializedPropertyType.Rect:
                    if (currProp1.rectValue != currProp2.rectValue)
                    {
                        return false;
                    }
                    break;
                case SerializedPropertyType.String:
                    if (currProp1.stringValue != currProp2.stringValue)
                    {
                        return false;
                    }
                    break;
                case SerializedPropertyType.Vector2:
                    if (currProp1.vector2Value != currProp2.vector2Value)
                    {
                        return false;
                    }
                    break;
                case SerializedPropertyType.Vector3:
                    if (currProp1.vector3Value != currProp2.vector3Value)
                    {
                        return false;
                    }
                    break;
            }

            prop1HasNext = currProp1.Next(true);
            prop2HasNext = currProp2.Next(true);
        } while (prop1HasNext == true && prop2HasNext == true && (hasStarted == false || currProp1.depth > startDepth));
        return true;
    }

    public static bool IsObjectsEqual(SerializedObject obj1, SerializedObject obj2)
    {
        return IsPropertiesEqual(obj1.GetIterator(), obj2.GetIterator());
    }
}
