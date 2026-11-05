using SaveSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Economy
{
    [CreateAssetMenu(fileName = "CurrencySave", menuName = "SO/Save/CurrencySave")]
    public class CurrencySave : SaveBase<CurrencySaveData>
    {
    }

    [Serializable]
    public class CurrencySaveData : SaveData
    {
        public List<AccountData> Accounts = new();
    }

    [Serializable]
    public class AccountData
    {
        public string AccountId;
        public List<BalanceData> Balances = new();
    }

    [Serializable]
    public class BalanceData
    {
        // Currency assets are looked up by Id (see IdentifiedScriptableObject) rather than by
        // direct object reference — JsonUtility cannot round-trip a UnityEngine.Object reference
        // for a plain class serialized outside of a scene/prefab.
        public string CurrencyId;
        public float Amount;
    }
}
