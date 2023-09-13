using Assets.Scripts.Utils;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class Popup : IDisposable
{
    private const string _toolbarName = "Prefabs/Popups/Toolbar";
    protected virtual string ItemTemplateName { get; }
    protected virtual GameObject ItemTemplate { get; set; }
    protected GameObject Toolbar { get; set; }
    protected virtual bool FromTop { get; }
    protected virtual float OpeningDestinationY { get; }
    protected virtual float ClosingDestinationY { get; }
    protected virtual void OpenAnimation(Action callback = null)
    {
        GuiUtils.ForceUpdateLayout(ItemTemplate);
        var popupRect = ItemTemplate.GetComponent<RectTransform>();
        var value = popupRect.GetPosY();
        var destination = OpeningDestinationY;
        void UpdateValue() => popupRect.SetPosY(value);
        DOTween.To(() => value, x => value = x, destination, AnimationDuration)
        .SetEase(Ease.Linear)
        .OnUpdate(UpdateValue);
    }
    protected virtual void CloseAnimation(Action callback = null)
    {
        var popupRect = ItemTemplate.GetComponent<RectTransform>();
        var value = popupRect.GetPosY();
        void UpdateValue() => popupRect.SetPosY(value);
        var destination = ClosingDestinationY;
        DOTween.To(() => value, x => value = x, destination, AnimationDuration)
        .SetEase(Ease.Linear)
        .OnUpdate(UpdateValue)
        .OnComplete(() => callback?.Invoke());
    }
    protected virtual float AnimationDuration { get; }
    protected virtual void Show()
    {
        var res = ResourceHelper.InstantiatePrefab(ItemTemplateName, UIManager.Instance.PopupsCanvas.transform);
        if (res == null)
            Dispose();
        ItemTemplate = res;
        InitToolbar();
        InitAnimationParameters();
        OpenAnimation();
    }
    protected void InitAnimationParameters()
    {
        if (ItemTemplate == null)
            return;
        var popupRect = ItemTemplate.GetComponent<RectTransform>();
        popupRect.SetLeft(0);
        popupRect.SetRight(0);
        GuiUtils.ForceUpdateLayout(popupRect);
        var popupHeight = popupRect.GetHeight();
        var toolbarHeight = 30;
        if (FromTop)
        {
            popupRect.anchorMax = new Vector2(1, 1);
            popupRect.anchorMin = new Vector2(0, 1);
            popupRect.SetPosY(popupHeight + toolbarHeight);
        }
        else
        {
            popupRect.anchorMax = new Vector2(1, 0);
            popupRect.anchorMin = new Vector2(0, 0);
            popupRect.SetPosY(-toolbarHeight);
        }
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
    public virtual void Dispose()
    {
    }
}
