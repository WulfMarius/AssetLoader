using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

public class CreateAssetBundle : MonoBehaviour
{

    [MenuItem("Assets/Build AssetBundle")]
    static void ExportResource()
    {
        //string path = "Assets/AssetBundle/ExampleObject.unity3d";
        Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = "binoculars";
        List<string> assetNames = new List<string>();
        

        foreach (Object eachObject in selection)
        {
            if (!(eachObject is GameObject || eachObject is Texture2D))
            {
                continue;
            }

            Debug.Log(eachObject);

            string path = AssetDatabase.GetAssetPath(eachObject);
            assetNames.Add(path);
        }

        foreach (string eachAssetName in assetNames)
        {
            Debug.Log("packing " + eachAssetName);
        }

        build.assetNames = assetNames.ToArray();

        BuildPipeline.BuildAssetBundles("Assets/AssetBundles",
            new AssetBundleBuild[] { build },
            BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle,
            BuildTarget.StandaloneWindows);


        //BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, path,
        //                               BuildAssetBundleOptions.CollectDependencies
        //                             | BuildAssetBundleOptions.CompleteAssets, BuildTarget.Android);
    }
}
