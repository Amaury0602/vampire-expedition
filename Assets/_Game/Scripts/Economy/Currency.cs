using Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Economy
{
    /// <summary>
    /// Identity of a currency. The asset itself is the key — no enum, so adding a currency never
    /// touches a shared file.
    /// </summary>
    [CreateAssetMenu(fileName = "Currency", menuName = "SO/Economy/Currency")]
    public class Currency : IdentifiedScriptableObject
    {
        [Title("Data")]
        public string DisplayName;
        public Sprite Icon;
    }
}
