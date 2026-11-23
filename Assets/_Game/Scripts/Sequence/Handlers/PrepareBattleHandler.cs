using System;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using Menus;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sequences
{
    public class PrepareBattleHandler : Handler<BattleContext>
    {
        private CancellationTokenSource _cts;

        protected override void OnEnter()
        {
            Level level = Game.Level;
            level.Clock.Stop();
            level.Deployment.PlacePlayers();
            level.Deployment.SpawnEnemies(Context.Room.Enemies);

            MenuManager.Hide<CombatSkillsMenu>();

            Debug.Log("[PrepareBattle] Press Enter to fight.");
            _cts = new CancellationTokenSource();
            WaitForFightAsync(_cts.Token).Forget();
        }

        protected override void OnExit()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }

        private async UniTaskVoid WaitForFightAsync(CancellationToken token)
        {
            try
            {
                while (!IsFightPressed())
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }

            Continue();
        }

        private bool IsFightPressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            return keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame;
        }
    }
}
