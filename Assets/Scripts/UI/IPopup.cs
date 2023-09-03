using System;
using UnityEngine;

public interface IPopup 
{
    string ItemTemplateName { get; }
    GameObject ItemTemplate{ get; set; }
    bool FromTop { get; }
    void OpenAnimation(Action callback = null);
    void CloseAnimation(Action callback = null);
    float AnimationDuration { get; }
}
