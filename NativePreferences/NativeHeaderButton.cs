using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NativePreferences
{
    public class NativeLocalPlayerHeaderButton
    {
        public GameObject GameObject;

        public NativeLocalPlayerHeaderButton(string text, Sprite icon, Action onClick)
        {
            GameObject = Object.Instantiate(
                    ReMod.Core.VRChat.QuickMenuEx.Instance.field_Private_UIPage_1.transform.Find(
                        "Header_H1/RightItemContainer/Button_Left"),
                    ReMod.Core.VRChat.QuickMenuEx.Instance.field_Private_UIPage_1.transform.Find(
                        "Header_H1/RightItemContainer"))
                .gameObject;
            GameObject.name = text;
            GameObject.GetComponent<VRC.UI.Elements.Tooltips.UiTooltip>().field_Public_String_0 = text;
            GameObject.transform.Find("Icon").GetComponent<Image>().overrideSprite = icon;
            (GameObject.GetComponent<Button>().onClick = new Button.ButtonClickedEvent()).AddListener(onClick);
        }
    }
}