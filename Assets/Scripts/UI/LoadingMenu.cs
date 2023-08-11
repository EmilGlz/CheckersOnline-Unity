using TMPro;
using UnityEngine;

public static class LoadingMenu
{
    private static GameObject LoadingMenuObj => UIManager.Instance.LoadingMenuObj;
    private static TMP_Text LoadingText => LoadingMenuObj.transform.GetChild(0).GetComponent<TMP_Text>();
    public static void Show(string text)
    {
        LoadingText.text = text;
        LoadingMenuObj.SetActive(true);
    }

    public static void Hide()
    {
        LoadingMenuObj.SetActive(false);
    }

}
