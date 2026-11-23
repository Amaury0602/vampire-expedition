using System;
using System.Collections.Generic;
using Sequences;
using UnityEngine;
using Sequence = Sequences.Sequence;

namespace Worlds
{
    public class World
    {
        public WorldSettings Settings { get; }
        public PlayerRosterSettings Roster { get; }
        public IReadOnlyList<Room> Rooms { get; }
        public int RoomIndex { get; private set; }
        public bool IsOver { get; private set; }
        public bool IsVictory { get; private set; }

        public Room CurrentRoom
        {
            get { return RoomIndex >= 0 && RoomIndex < Rooms.Count ? Rooms[RoomIndex] : null; }
        }

        public event Action<Room> RoomStarted;
        public event Action<bool> Ended;

        private Sequence _sequence;

        public World(WorldSettings settings, PlayerRosterSettings roster)
        {
            Settings = settings;
            Roster = roster;
            Rooms = BuildRooms(settings);
        }

        public void Start()
        {
            RoomIndex = 0;
            IsOver = false;
            IsVictory = false;

            if (Rooms.Count == 0)
            {
                End(true);
                return;
            }

            StartCurrentRoom();
        }

        public void CompleteRoom(bool victory)
        {
            if (IsOver)
            {
                return;
            }

            if (!victory)
            {
                End(false);
                return;
            }

            RoomIndex++;
            if (RoomIndex >= Rooms.Count)
            {
                End(true);
            }
        }

        private static List<Room> BuildRooms(WorldSettings settings)
        {
            List<Room> rooms = new List<Room>();
            int count = Mathf.Max(0, settings.RoomCount);
            for (int i = 0; i < count; i++)
            {
                rooms.Add(new BattleRoom(i, settings.RollEnemies()));
            }

            return rooms;
        }

        private void StartCurrentRoom()
        {
            while (!IsOver)
            {
                Room room = CurrentRoom;
                Debug.Log($"[World] Room {RoomIndex + 1}/{Rooms.Count}");

                if (room is BattleRoom battle)
                {
                    RoomStarted?.Invoke(room);
                    _sequence = SequenceProvider.BattleRoomSequence(battle);
                    _sequence.Completed += OnRoomSequenceCompleted;
                    _sequence.Start();
                    return;
                }

                Debug.LogWarning($"[World] Room {RoomIndex + 1} unsupported, skipped.");
                CompleteRoom(true);
            }
        }

        private void OnRoomSequenceCompleted()
        {
            if (_sequence != null)
            {
                _sequence.Completed -= OnRoomSequenceCompleted;
                _sequence = null;
            }

            if (!IsOver)
            {
                StartCurrentRoom();
            }
        }

        private void End(bool victory)
        {
            IsOver = true;
            IsVictory = victory;

            if (victory)
            {
                Debug.Log($"[World] World complete : {Settings.Name}");
            }
            else
            {
                Debug.Log($"[World] Defeat at room {RoomIndex + 1}/{Rooms.Count}");
            }

            Ended?.Invoke(victory);
        }
    }
}
