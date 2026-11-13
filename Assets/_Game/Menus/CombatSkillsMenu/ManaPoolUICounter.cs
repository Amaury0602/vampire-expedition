using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Core;
using System;

public class ManaPoolUICounter : MonoBehaviour
{
    [SerializeField] private Image _fillBar;
    [SerializeField] private TMP_Text _countText;


    public void Init()
    {
        Game.Level.Mana.Changed += UpdateUI;
        Game.Level.Mana.ProgressUpdated += UpdateFill;
        UpdateUI(Game.Level.Mana.Current, Game.Level.Mana.Max);
        UpdateFill(Game.Level.Mana.Progress);

    }

    public void UnSubscribe()
    {
        Game.Level.Mana.Changed -= UpdateUI;
        Game.Level.Mana.ProgressUpdated -= UpdateFill;

    }

    private void UpdateFill(float progress)
    {
        _fillBar.fillAmount = progress;
    }

    private void UpdateUI(float current, float max)
    {
        _countText.text = $"{(int)current}";
    }
}
