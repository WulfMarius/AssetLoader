using UnityEngine;
using UnityEditor;

public class CreateAssetBundle : MonoBehaviour
{

    [MenuItem("Assets/Build AssetBundle")]
    static void ExportResource()
    {
        //string path = "Assets/AssetBundle/ExampleObject.unity3d";
        Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

        foreach (Object eachObject in selection)
        {
            if (eachObject is GameObject)
            {
                Debug.Log(eachObject);

                GameObject gameObject = (GameObject)eachObject;

                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = gameObject.name;

                string path = AssetDatabase.GetAssetPath(eachObject);
                build.assetNames = new string[] { path };

                BuildPipeline.BuildAssetBundles("Assets/AssetBundles",
                    new AssetBundleBuild[] { build },
                    BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle,
                    BuildTarget.StandaloneWindows);
            }
        }


        //BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, path,
        //                               BuildAssetBundleOptions.CollectDependencies
        //                             | BuildAssetBundleOptions.CompleteAssets, BuildTarget.Android);
    }
}
