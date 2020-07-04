/*
 * CustomPostProcessBuild.cs
 * Copyright (c) 2020 東亜プリン秘密研究所. All rights reserved.
 */
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class CustomPostProcessBuild
{
    // Process after build
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuild)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            // Update Info.plist
            var plistPath = Path.Combine(pathToBuild, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            // Application supports iTunes file sharing
            plist.root.SetBoolean("UIFileSharingEnabled", true);
            // Allow Arbitrary Loads
            var securityDict = plist.root.CreateDict("NSAppTransportSecurity");
            securityDict.SetBoolean("NSAllowsArbitraryLoads", true);
            // Write to Info.plist
            plist.WriteToFile(plistPath);

            // Update Build.Settings
            var projPath = PBXProject.GetPBXProjectPath(pathToBuild);
            var proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));
            var targetGuide = proj.TargetGuidByName("Unity-iPhone");
            // Bitcode
            proj.SetBuildProperty(targetGuide, "ENABLE_BITCODE", "NO");
            // Write to Build.Settings
            File.WriteAllText(projPath, proj.WriteToString());
        }
    }
}
