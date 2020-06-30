using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class CustomPostProcessBuild
{
    // Process after build
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuild)
    {
        if (target == BuildTarget.iOS)
        {
            // Update Info.plist
            var plistPath = Path.Combine(pathToBuild, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            // Application supports iTunes file sharing
            plist.root.SetBoolean("UIFileSharingEnabled", true);
            plist.WriteToFile(plistPath);
        }
    }
}
