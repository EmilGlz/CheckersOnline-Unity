using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class Popup
{
    private const string _toolbarName = "Prefabs/Popups/Toolbar";
    protected virtual string ItemTemplateName { get; }
    protected virtual GameObject ItemTemplate { get; set; }
    protected GameObject Toolbar { get; set; }
    protected virtual bool FromTop { get; }
    protected virtual void OpenAnimation(Action callback = null) { }
    protected virtual void CloseAnimation(Action callback = null) { }
    protected virtual float AnimationDuration { get; }
    protected virtual void Show()
    {
        InitToolbar();
    }
    protected virtual void InitToolbar()
    {
        if (ItemTemplate == null)
            return;
        var res = ResourceHelper.InstantiatePrefab(_toolbarName, ItemTemplate.transform);
        if (res == null)
            return;
        Toolbar = res;
        var toolbarImg = Toolbar.GetComponent<Image>();
        var toolbarRect = Toolbar.GetComponent<RectTransform>();
        if (FromTop)
        {
            GuiUtils.SetIcon(toolbarImg, "Sprites/BackgroundRoundBottom@2x");
            toolbarRect.pivot = new Vector2(0.5f, 1f);
            toolbarRect.anchorMax = new Vector2(1, 0);
            toolbarRect.anchorMin = new Vector2(0, 0);
            toolbarRect.anchoredPosition = new Vector2(0, 0);
        }
        else
        {
            GuiUtils.SetIcon(toolbarImg, "Sprites/BackgroundRoundTop@2x");
            toolbarRect.pivot = new Vector2(0.5f, 0f);
            toolbarRect.anchorMax = new Vector2(1, 1);
            toolbarRect.anchorMin = new Vector2(0, 1);
            toolbarRect.anchoredPosition = new Vector2(0, 0);
        }
    }
}
