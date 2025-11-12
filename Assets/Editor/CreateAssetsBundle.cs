using System;
using UnityEditor;
using UnityEngine;
using System.IO;

public class CreateAssetBundles
{

    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        Debug.Log("Building bundled asset");
        try
        {
            string assetBundleDirectory = "Assets/StreamingAssets";
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(assetBundleDirectory);
            }

            BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None,
              EditorUserBuildSettings.activeBuildTarget);
            AssetDatabase.Refresh();

        }
        catch (Exception e)
        {
            Debug.Log(" failed to create bundled assets" + e);
            throw;
        }

    }
}
