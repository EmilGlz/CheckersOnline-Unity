using UnityEngine;
using Object = UnityEngine.Object;
public static class ResourceHelper
{
    public static GameObject InstantiatePrefab(string path, Transform parent)
    {
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab != null)
        {
            GameObject instantiatedObject = Object.Instantiate(prefab, parent);
            return instantiatedObject;
        }
        else
        {
            Debug.LogError("Prefab not found in Resources folder: " + path);
            return null;
        }
    }
}
