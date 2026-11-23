using System;
using System.Threading;
using Battle;
using Core;
using Cysharp.Threading.Tasks;
using Menus;
using Units;

namespace Sequences
{
    public class StartBattleHandler : Handler<BattleContext>
    {
        private readonly float _endDelay;

        private CancellationTokenSource _cts;
        private bool _isEnding;

        public StartBattleHandler(float endDelay = 2f)
        {
            _endDelay = endDelay;
        }

        protected override void OnEnter()
        {
            _isEnding = false;
            _cts = new CancellationTokenSource();

            Battlefield battlefield = Game.Level.Battlefield;
            battlefield.UnitRemoved += OnUnitRemoved;

            MenuManager.Show<CombatSkillsMenu>();
            Game.Level.Clock.Play();

            CheckEnd();
        }

        protected override void OnExit()
        {
            Game.Level.Battlefield.UnitRemoved -= OnUnitRemoved;

            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }

        private void OnUnitRemoved(UnitController unit)
        {
            CheckEnd();
        }

        private void CheckEnd()
        {
            if (_isEnding)
            {
                return;
            }

            Battlefield battlefield = Game.Level.Battlefield;
            bool defeat = battlefield.PlayerUnits.Count == 0;
            bool victory = battlefield.OtherUnits.Count == 0;
            if (!defeat && !victory)
            {
                return;
            }

            _isEnding = true;
            Context.Victory = !defeat;
            EndAsync(_cts.Token).Forget();
        }

        private async UniTaskVoid EndAsync(CancellationToken token)
        {
            try
            {
                await Game.Level.Clock.Delay(_endDelay, token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            MenuManager.Hide<CombatSkillsMenu>();
            Continue();
        }
    }
}
