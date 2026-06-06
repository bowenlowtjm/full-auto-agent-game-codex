using UnityEditor;

public static class AutoRefresher
{
    public static void RefreshAssets()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }
}
