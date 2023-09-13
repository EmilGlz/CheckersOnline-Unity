using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

public static class Device
{
    public static int ActualWidth => Screen.width;

    public static int ActualHeight => Screen.height;
    public static int Width
    {
        get
        {
            var popupCanvas = UIManager.Instance.PopupsCanvas.GetComponent<RectTransform>();
            GuiUtils.ForceUpdateLayout(popupCanvas);
            return (int)popupCanvas.rect.width;
        }
    }
    public static int Height
    {
        get
        {
            var popupCanvas = UIManager.Instance.PopupsCanvas.GetComponent<RectTransform>();
            GuiUtils.ForceUpdateLayout(popupCanvas);
            return (int)popupCanvas.rect.height;
        }
    }
}
