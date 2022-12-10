using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ImportPostprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        var assets = AssetDatabase.FindAssets("MixedReality.Toolkit");
        if (assets.Length > 0)
        {
            AddLibrayDefineIfNeeded("HAVE_MRTK");
        }
        else
        {
            var htkAssets = AssetDatabase.FindAssets("HoloToolkitCommon");
            if (htkAssets.Length > 0)
            {
                AddLibrayDefineIfNeeded("HAVE_HTK");
            }
        }
    }

    static void AddLibrayDefineIfNeeded(string define)
    {
        // Get defines.
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

        // Append only if not defined already.
        if (defines.Contains(define))
        {
            return;
        }

        // Append.
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, (defines + ";" + define));
        Debug.LogWarning("<b>" + define + "</b> added to <i>Scripting Define Symbols</i> for selected build target (" + EditorUserBuildSettings.activeBuildTarget.ToString() + ").");
    }
}
