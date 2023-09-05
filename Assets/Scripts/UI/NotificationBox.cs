using Assets.Scripts.Utils;
using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class NotificationBox : Popup, IDisposable
{
    protected override string ItemTemplateName => "Prefabs/Popups/NotificationPopup";
    protected override GameObject ItemTemplate { get; set; }
    protected override bool FromTop => false;
    protected override float AnimationDuration => .3f;
    private readonly int _autoHideDelayTime = 2;
    private readonly string _title;
    public NotificationBox(string title)
    {
        _title = title;
    }
    public static void Create(string title)
    {
        var popup = new NotificationBox(title);
        popup.Show();
    }
    protected override void Show()
    {
        var res = ResourceHelper.InstantiatePrefab(ItemTemplateName, UIManager.Instance.PopupsCanvas.transform);
        if (res == null)
            Dispose();
        ItemTemplate = res;
        ItemTemplate.name = typeof(NotificationBox).Name;
        var titleText = GuiUtils.FindGameObject("Text", ItemTemplate).GetComponent<TMP_Text>();
        titleText.text = _title;
        var vlg = ItemTemplate.GetComponent<VerticalLayoutGroup>();
        vlg.padding.top = FromTop ? 10 : 0;
        vlg.padding.bottom = FromTop ? 0 : 10;
        InitAnimationParameters();
        OpenAnimation();
        Hide(_autoHideDelayTime);
        base.Show();
    }
    private void InitAnimationParameters()
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
    private void Hide(int delayTime = 0)
    {
        GameController.Instance.StartCoroutine(HideFunc(delayTime));
    }
    private IEnumerator HideFunc(int delayTime)
    {
        if (delayTime > 0)
            yield return new WaitForSeconds(delayTime);
        if (ItemTemplate == null)
            yield break;
        CloseAnimation(Dispose);

    }
    public void Dispose()
    {
        if (ItemTemplate != null)
        {
            Object.Destroy(ItemTemplate);
            ItemTemplate = null;
        }
    }
    protected override void OpenAnimation(Action callback = null)
    {
        GuiUtils.ForceUpdateLayout(ItemTemplate);
        var popupRect = ItemTemplate.GetComponent<RectTransform>();
        var value = popupRect.GetPosY();
        var destination = FromTop ? 0 : popupRect.GetHeight();
        void UpdateValue() => popupRect.SetPosY(value);
        DOTween.To(() => value, x => value = x, destination, AnimationDuration)
        .SetEase(Ease.Linear)
        .OnUpdate(UpdateValue);
    }
    protected override void CloseAnimation(Action callback = null)
    {
        var toolbarHeight = 30;
        var popupRect = ItemTemplate.GetComponent<RectTransform>();
        var value = popupRect.GetPosY();
        void UpdateValue() => popupRect.SetPosY(value);
        var destination = FromTop ? popupRect.GetHeight() + toolbarHeight : -toolbarHeight;
        DOTween.To(() => value, x => value = x, destination, AnimationDuration)
        .SetEase(Ease.Linear)
        .OnUpdate(UpdateValue)
        .OnComplete(() => callback?.Invoke());
    }
}
