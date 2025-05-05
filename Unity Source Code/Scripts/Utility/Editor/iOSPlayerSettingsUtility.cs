using UnityEditor.iOS;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build;

public static class iOSPlayerSettingsUtility
{
    // Setting all `App` icons for iOS
    public static void SetAppIcons(Texture2D[] textures)
    {
        NamedBuildTarget platform = NamedBuildTarget.iOS;
        PlatformIconKind kind = iOSPlatformIconKind.Application;

        PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(platform, kind);

        //Assign textures to each available icon slot.
        for (int i = 0; i < icons.Length; i++)
        {
            icons[i].SetTextures(textures[i]);
        }
        PlayerSettings.SetPlatformIcons(platform, kind, icons);
    }
}