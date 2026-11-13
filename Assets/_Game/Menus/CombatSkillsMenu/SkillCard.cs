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
    [SerializeField] private Image _raycastImage;
    [SerializeField] private Image _lockedOverlay;
    [SerializeField] private TMP_Text _manaCostText;
    public event Action<SkillCard> Clicked;

    public SkillSlot Skill { get; private set; }

    public void OnGet() {}
    public void OnRelease(){}

    public void Setup(SkillSlot slot)
    {
        Skill = slot;
        _icon.sprite = slot.Settings.Icon;
        _manaCostText.text = slot.Data.ManaCost.ToString();
    }

    public void SetLocked(bool locked)
    {
        _raycastImage.raycastTarget = !locked;
        _lockedOverlay.gameObject.SetActive(locked);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            Clicked?.Invoke(this);
    }
}
