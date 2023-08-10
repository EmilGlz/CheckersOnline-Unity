using TMPro;
using UnityEngine;

public static class LoadingMenu
{
    private static GameObject LoadingMenuObj => UIManager.Instance.LoadingMenuObj;
    private static TMP_Text LoadingText => LoadingMenuObj.transform.GetChild(0).GetComponent<TMP_Text>();
    public static void ShowLoadingText(string text)
    {
        LoadingText.text = text;
        LoadingMenuObj.SetActive(true);
    }

    public static void HideLoadingText()
    {
        LoadingMenuObj.SetActive(false);
    }

}
