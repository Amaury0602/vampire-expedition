using Pooling;
using Skills;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillCard : MonoBehaviour, IPoolable, IPointerClickHandler
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _manaCostText;
    public event Action Clicked;

    public void OnGet() {}
    public void OnRelease(){}

    public void Setup(SkillSlot slot)
    {
        _icon.sprite = slot.Settings.Icon;
        _manaCostText.text = slot.Data.ManaCost.ToString();
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            Clicked?.Invoke();
    }

}
