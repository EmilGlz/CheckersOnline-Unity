using Assets.Scripts.Utils;
using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Collections.Specialized.BitVector32;
using Object = UnityEngine.Object;

public class NotificationBox : IPopup, IDisposable
{
    public string ItemTemplateName => "Prefabs/Popups/NotificationPopup";
    public GameObject ItemTemplate { get; set; }
    public bool FromTop => true;
    private readonly int _autoHideDelayTime = 2;
    public float AnimationDuration => .3f;
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
    private void Show()
    {
        var res = ResourceHelper.InstantiatePrefab(ItemTemplateName, UIManager.Instance.PopupsCanvas.transform);
        if (res == null)
            Dispose();
        ItemTemplate = res;
        ItemTemplate.name = typeof(NotificationBox).Name;
        var titleText = GuiUtils.FindGameObject("Text", ItemTemplate).GetComponent<TMP_Text>();
        titleText.text = _title;
        InitAnimationParameters();
        OpenAnimation();
        Hide(_autoHideDelayTime);
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
        if (FromTop)
        {
            GuiUtils.SetIcon(ItemTemplate.GetComponent<Image>(), "Sprites/BackgroundRoundBottom@2x.png");
            popupRect.anchorMax = new Vector2(1, 1);
            popupRect.anchorMin = new Vector2(0, 1);
            popupRect.SetPosY(popupHeight);
        }
        else
        {
            popupRect.anchorMax = new Vector2(0, 0);
            popupRect.anchorMin = new Vector2(1, 0);
            popupRect.SetPosY(0);   
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
    public void OpenAnimation(Action callback = null)
    {
        var popupRect = ItemTemplate.GetComponent<RectTransform>();
        var value = popupRect.GetPosY();
        void UpdateValue() => popupRect.SetPosY(value);
        if (FromTop)
            DOTween.To(() => value, x => value = x, 0, AnimationDuration)
            .SetEase(Ease.Linear)
            .OnUpdate(UpdateValue);
        else { }
    }
    public void CloseAnimation(Action callback = null)
    {
        var popupRect = ItemTemplate.GetComponent<RectTransform>();
        var value = popupRect.GetPosY();
        void UpdateValue() => popupRect.SetPosY(value);
        var destination = popupRect.GetHeight();
        if (FromTop)
            DOTween.To(() => value, x => value = x, destination, AnimationDuration)
            .SetEase(Ease.Linear)
            .OnUpdate(UpdateValue)
            .OnComplete(() => callback?.Invoke());
        else { }
    }
}
