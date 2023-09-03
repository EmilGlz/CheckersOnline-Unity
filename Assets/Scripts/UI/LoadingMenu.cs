using DG.Tweening;
using UnityEngine;

public static class LoadingMenu
{
    private static GameObject LoadingMenuObj => UIManager.Instance.LoadingMenuObj;
    private static Transform LoadingIcon => LoadingMenuObj.transform.GetChild(0);
    private static Tweener rotationTween;
    public static void Show()
    {
        LoadingMenuObj.SetActive(true);
        LoadingIcon.DORotate(new Vector3(0f, 0f, 360), 2f)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental); // Infinite loop with incremental rotation.
            //.OnStepComplete(OnRotationComplete);
    }

    public static void Hide()
    {
        LoadingMenuObj.SetActive(false);
    }

    public static void StartRotatingIcon()
    {

    }

}
