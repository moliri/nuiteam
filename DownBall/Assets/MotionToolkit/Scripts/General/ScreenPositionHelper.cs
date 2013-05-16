using UnityEngine;
using System.Collections;

public class ScreenPositionHelper 
{
    public enum SnapToScreen
    {
        UpperLeft,
        UpperCenter,
        UpperRight,
        LowerLeft,
        LowerCenter,
        LowerRight,
        CenterLeft,
        CenterCenter,
        CenterRight
        
    };

    public static Rect SnapOnGUIRect(Rect input,SnapToScreen relativePosition)
    {
        Rect placeToDraw = input;
        switch (relativePosition)
        {
            case SnapToScreen.UpperRight:
                placeToDraw.x = Screen.width - placeToDraw.x - placeToDraw.width;
                break;
            case SnapToScreen.UpperCenter:
                placeToDraw.x = Screen.width /2 + placeToDraw.x - placeToDraw.width/2;
                break;
            case SnapToScreen.LowerLeft:
                placeToDraw.y = Screen.height - placeToDraw.y - placeToDraw.height;
                break;
            case SnapToScreen.LowerCenter:
                placeToDraw.y = Screen.height - placeToDraw.y - placeToDraw.height;
                placeToDraw.x = Screen.width / 2 + placeToDraw.x - placeToDraw.width / 2;
                break;
            case SnapToScreen.LowerRight:
                placeToDraw.x = Screen.width - placeToDraw.x - placeToDraw.width;
                placeToDraw.y = Screen.height - placeToDraw.y - placeToDraw.height;
                break;
            case SnapToScreen.CenterLeft:
                placeToDraw.y = Screen.height/2 + placeToDraw.y - placeToDraw.height/2;
                break;
            case SnapToScreen.CenterCenter:
                placeToDraw.y = Screen.height / 2 + placeToDraw.y - placeToDraw.height / 2;
                placeToDraw.x = Screen.width / 2 + placeToDraw.x - placeToDraw.width / 2;
                break;
            case SnapToScreen.CenterRight:
                placeToDraw.x = Screen.width - placeToDraw.x - placeToDraw.width;
                placeToDraw.y = Screen.height / 2 + placeToDraw.y - placeToDraw.height / 2;
                break;
        }
        return placeToDraw;
    }

    public static void SetGUITexturePosition(GUITexture guiTexture, Rect inRect,SnapToScreen relativePosition, float z)
    {
        Rect placeToDraw = inRect;
        guiTexture.transform.localScale = Vector3.zero;
        switch (relativePosition)
        {
            case SnapToScreen.UpperLeft:
                guiTexture.transform.localPosition = new Vector3(0, 1, z);
                placeToDraw.y -= placeToDraw.height;
                break;
            case SnapToScreen.UpperCenter:
                guiTexture.transform.localPosition = new Vector3(0.5f, 1, z);
                placeToDraw.y -= placeToDraw.height;
                placeToDraw.x -= placeToDraw.width / 2;
                break;
            case SnapToScreen.UpperRight:
                guiTexture.transform.localPosition = new Vector3(1, 1, z);
                placeToDraw.y -= placeToDraw.height;
                placeToDraw.x -= placeToDraw.width;
                break;
            case SnapToScreen.LowerLeft:
                guiTexture.transform.localPosition = new Vector3(0, 0, z);
                break;
            case SnapToScreen.LowerCenter:
                guiTexture.transform.localPosition = new Vector3(0.5f, 0, z);
                placeToDraw.x -= placeToDraw.width / 2;
                break;
            case SnapToScreen.LowerRight:
                guiTexture.transform.localPosition = new Vector3(1, 0, z);
                placeToDraw.x -= placeToDraw.width;
                break;
            case SnapToScreen.CenterLeft:
                guiTexture.transform.localPosition = new Vector3(0, 0.5f, z);
                placeToDraw.y -= placeToDraw.height / 2;
                break;
            case SnapToScreen.CenterCenter:
                guiTexture.transform.localPosition = new Vector3(0.5f, 0.5f, z);
                placeToDraw.x -= placeToDraw.width / 2;
                placeToDraw.y -= placeToDraw.height / 2;
                break;
            case SnapToScreen.CenterRight:
                guiTexture.transform.localPosition = new Vector3(1, 0.5f, z);
                placeToDraw.x -= placeToDraw.width;
                placeToDraw.y -= placeToDraw.height / 2;
                break;
        }
        guiTexture.pixelInset = placeToDraw;
    }
}
