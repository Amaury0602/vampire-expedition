using System;
using System.Collections.Generic;

namespace Economy
{
    /// <summary>
    /// Runtime balance holder for one account (e.g. "main"). Ported from MineIdle's Account with
    /// the same shape; callers of Spend/TrySpend must respect the returned bool (MineIdle's
    /// EconomyManager.Spend used to ignore it, letting purchases succeed against the UI's
    /// affordability flag alone rather than the real balance).
    /// </summary>
    public class Account
    {
        private readonly Dictionary<Currency, float> _balances = new();

        public string Id { get; }
        public event Action Updated;

        public Account(string id)
        {
            Id = id;
        }

        public void Add(Currency currency, float amount)
        {
            if (currency == null || amount == 0f)
                return;

            _balances.TryGetValue(currency, out float current);
            _balances[currency] = current + amount;
            Updated?.Invoke();
        }

        public bool TrySpend(Currency currency, float amount)
        {
            if (currency == null)
                return false;

            if (!_balances.TryGetValue(currency, out float current) || current < amount)
                return false;

            _balances[currency] = current - amount;
            Updated?.Invoke();
            return true;
        }

        public bool CanBuy(Price price)
        {
            if (price.Amount <= 0f)
                return true;

            return _balances.TryGetValue(price.Currency, out float current) && current >= price.Amount;
        }

        public bool TrySpend(Price price) => price.Amount <= 0f || TrySpend(price.Currency, price.Amount);

        public float GetAmount(Currency currency)
        {
            if (currency == null)
                return 0f;

            _balances.TryGetValue(currency, out float amount);
            return amount;
        }

        public IReadOnlyDictionary<Currency, float> Balances => _balances;
    }
}
