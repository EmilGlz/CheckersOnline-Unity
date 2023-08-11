using UnityEngine;

public class GuiUtils
{
    public static GameObject FindGameObject(string name, GameObject parentOrSelf)
    {
        if (parentOrSelf == null)
            return null;
        if (parentOrSelf.name == name)
            return parentOrSelf;
        var components = parentOrSelf.GetComponentsInChildren<Transform>(true);
        foreach (Transform component in components)
        {
            if (component.gameObject.name == name)
                return component.gameObject;
        }

        return null;
    }

    public static T FindGameObject<T>(string name, Transform parentOrSelf)
    where T : MonoBehaviour
    {
        if (parentOrSelf == null)
            return null;
        var go = FindGameObject(name, parentOrSelf.gameObject);
        if (go == null) return null;
        return go.GetComponent<T>();
    }
}
