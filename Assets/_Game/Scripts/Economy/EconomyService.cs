using System.Collections.Generic;

namespace Economy
{
    /// <summary>
    /// Owns every Account for the session and its persistence. Unlike MineIdle's EconomyManager
    /// this is a plain instance (owned by the static Game facade, see Game.cs) rather than a
    /// static class loading its database through Addressables — smash-like has no Addressables
    /// package, and a direct serialized reference removes the async-init complexity that forced
    /// MineIdle's TalentTreeBuilder.Build() to be `async void`.
    /// </summary>
    public class EconomyService
    {
        public const string MainAccountId = "main";

        private readonly CurrencySave _save;
        private readonly Dictionary<string, Currency> _currencyById = new();
        private readonly Dictionary<string, Account> _accounts = new();

        public EconomyService(CurrencySave save, IReadOnlyList<Currency> currencyCatalog)
        {
            _save = save;

            for (int i = 0; i < currencyCatalog.Count; i++)
            {
                Currency currency = currencyCatalog[i];
                if (currency != null)
                    _currencyById[currency.Id] = currency;
            }

            _save.Load();
            for (int i = 0; i < _save.Data.Accounts.Count; i++)
            {
                AccountData accountData = _save.Data.Accounts[i];
                var account = new Account(accountData.AccountId);
                for (int j = 0; j < accountData.Balances.Count; j++)
                {
                    BalanceData balance = accountData.Balances[j];
                    if (_currencyById.TryGetValue(balance.CurrencyId, out Currency currency))
                        account.Add(currency, balance.Amount);
                }
                _accounts[account.Id] = account;
            }
        }

        public Account MainAccount => Get(MainAccountId);

        public Account Get(string id)
        {
            if (!_accounts.TryGetValue(id, out Account account))
            {
                account = new Account(id);
                _accounts[id] = account;
            }
            return account;
        }

        /// <summary>Captures every live account into the save data and writes it to disk.</summary>
        public void Flush()
        {
            _save.Data.Accounts.Clear();
            foreach (Account account in _accounts.Values)
            {
                var data = new AccountData { AccountId = account.Id };
                foreach (var balance in account.Balances)
                    data.Balances.Add(new BalanceData { CurrencyId = balance.Key.Id, Amount = balance.Value });
                _save.Data.Accounts.Add(data);
            }
            _save.Save();
        }
    }
}
