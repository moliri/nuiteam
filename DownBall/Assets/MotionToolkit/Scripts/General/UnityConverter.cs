using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Convert Framework data structures to Unity and vice versa
/// </summary>
public class UnityConverter
{
    /// <summary>
    /// Convert Framework Vector3 to Unity without any scaling
    /// </summary>
    /// <param name="inVector">input vector</param>
    /// <returns>a Unity vector</returns>
    public static UnityEngine.Vector3 ToUnity(OmekFramework.Common.BasicTypes.SpaceTypes.Vector3 inVector)
    {
        return new UnityEngine.Vector3(inVector.x, inVector.y, inVector.z);
    }

    /// <summary>
    /// Convert float[] representing Vector3 to Unity without any scaling
    /// </summary>
    /// <param name="inVector">input vector</param>
    /// <returns>a Unity vector</returns>
    public static UnityEngine.Vector3 ToUnityVector3(float[] inVector)
    {
        return new UnityEngine.Vector3(inVector[0], inVector[1], inVector[2]);
    }

    /// <summary>
    /// Convert Framework Vector3 to Unity and change the coordinate system by inverting the y axis direction
    /// </summary>
    /// <param name="inVector">input vector</param>
    /// <returns>a Unity vector</returns>
    public static UnityEngine.Vector3 ToUnitySpace(OmekFramework.Common.BasicTypes.SpaceTypes.Vector3 inVector)
    {
        return new UnityEngine.Vector3(inVector.x, -inVector.y, inVector.z);
    }

    /// <summary>
    /// Convert Unity Vector3 to Framework without any scaling
    /// </summary>
    /// <param name="inVector">input vector</param>
    /// <returns>a Framework vector</returns>
    public static OmekFramework.Common.BasicTypes.SpaceTypes.Vector3 ToFramework(UnityEngine.Vector3 inVector)
    {
        return new OmekFramework.Common.BasicTypes.SpaceTypes.Vector3(inVector.x, inVector.y, inVector.z);
    }

    /// <summary>
    /// Convert Unity Vector3 to Framework and change the coordinate system by inverting the y axis direction
    /// </summary>
    /// <param name="inVector">input vector</param>
    /// <returns>a Framework vector</returns>
    public static OmekFramework.Common.BasicTypes.SpaceTypes.Vector3 ToFrameworkSpace(UnityEngine.Vector3 inVector)
    {
        return new OmekFramework.Common.BasicTypes.SpaceTypes.Vector3(inVector.x, -inVector.y, inVector.z);
    }

    /// <summary>
    /// Convert Framework Quaternion to Unity and change the coordinate system from right handed to left handed
    /// The coordinate system conversion is done by multiplying the X and W component by -1
    /// </summary>
    /// <param name="inVector">input Quaternion</param>
    /// <returns>a Unity Quaternion</returns>
    public static UnityEngine.Quaternion ToUnity(OmekFramework.Common.BasicTypes.SpaceTypes.Quaternion inQuat)
    {
        return new UnityEngine.Quaternion(-inQuat.x, inQuat.y, inQuat.z,-inQuat.w);
    }

    /// <summary>
    /// Convert float[] representing Quaternion to Unity and change the coordinate system from right handed to left handed
    /// The coordinate system conversion is done by multiplying the X and W component by -1
    /// </summary>
    /// <param name="inVector">input Quaternion</param>
    /// <returns>a Unity Quaternion</returns>
    public static UnityEngine.Quaternion ToUnityQuaternion(float[] inQuat)
    {
        return new UnityEngine.Quaternion(-inQuat[1], inQuat[2], inQuat[3], -inQuat[0]);
    }

    /// <summary>
    /// Convert Unity Quaternion to Framework and change the coordinate system from left handed to right handed
    /// The coordinate system conversion is done by multiplying the X and W component by -1
    /// </summary>
    /// <param name="inVector">input Quaternion</param>
    /// <returns>a Framework Quaternion</returns>
    public static OmekFramework.Common.BasicTypes.SpaceTypes.Quaternion ToFramework(UnityEngine.Quaternion inQuat)
    {
        return new OmekFramework.Common.BasicTypes.SpaceTypes.Quaternion(-inQuat.x, inQuat.y, inQuat.z, -inQuat.w);
    }

    /// <summary>
    /// Convert Framework Vector2 to Unity without any scaling
    /// </summary>
    /// <param name="inVector">input vector</param>
    /// <returns>a Unity vector</returns>
    public static UnityEngine.Vector2 ToUnity(OmekFramework.Common.BasicTypes.SpaceTypes.Vector2 inVector)
    {
        return new UnityEngine.Vector2(inVector.x, inVector.y);
    }    

    /// <summary>
    /// Convert Unity Vector2 to Framework without any scaling
    /// </summary>
    /// <param name="inVector">input vector</param>
    /// <returns>a Framework vector</returns>
    public static OmekFramework.Common.BasicTypes.SpaceTypes.Vector2 ToFramework(UnityEngine.Vector2 inVector)
    {
        return new OmekFramework.Common.BasicTypes.SpaceTypes.Vector2(inVector.x, inVector.y);
    }

}

