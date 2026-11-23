# World + Rooms

## Contexte
Proto 1 (`brainstorm/chasseurs-contre-dracula.md`, `idees.md` : « salles + embranchement + Rentrer », « décisions entre les salles »). Besoin d'une boucle de run :
World (SO) → liste ordonnée de rooms (5). Pour l'instant uniquement des batailles. Chaque bataille tire un pool aléatoire d'ennemis. Le joueur a un pool d'unités (prefabs `UnitController`, comme les ennemis), pool de départ défini par un SO.

Boucle : `PrepareBattle` (placement auto, attente du joueur) → `StartBattle` (combat auto) → `MoveToNextRoom` → room suivante.

## Décisions
- Unité de pool = **prefab `UnitController` direct** (pas de SO unité).
- Bataille = liste de candidats + `MinCount`/`MaxCount`, tirage uniforme (doublon dans la liste = poids).
- Persistance : unités joueur **mortes perdues**, survivantes **gardent leurs PV** (instances conservées entre rooms, jamais re-`Get`).
- Fin : victoire = plus d'ennemis → room suivante. Défaite = plus d'unités joueur → run stoppé + log. Fin du world → log. Pas de menu résultat pour l'instant.
- Placement : automatique en formation dans la zone joueur. **Pas de drag & drop pour l'instant** (prévu plus tard via `Deployment`).
- Start : touche **Enter** (pas de menu pour l'instant ; `PrepareBattleMenu` plus tard).
- Transition : même scène, reset instant (ennemis nettoyés, survivants replacés en formation).
- World : liste ordonnée fixe de `RoomSettings`.

## Existant réutilisé
- `Battlefield.Add(unit, team)` / `Remove` / `UnitJoined` / `UnitRemoved` (`Scripts/Battle/Battlefield.cs`). La mort appelle `UnitCombat.Leave()` → `Battlefield.Remove`.
- `Pooler.Get<T>(prefab, pos, rot)` / `Pooler.Release` (`Scripts/Pooling/Pooler.cs`), `Pool` ajoute `PooledObject` si absent. Mort → release auto après `_deathDuration` (horloge).
- `Formation.GetSlot(center, index, spacing)` (`Scripts/Units/Formation.cs`) pour placement initial.
- `SkillCaster` s'abonne déjà à `UnitJoined/UnitRemoved` → slots des `Hero` gérés automatiquement.
- `Sequence` / `Handler<TContext>` / `SequenceProvider` (`Scripts/Sequence`).
- `CombatClock.Delay` pour l'attente de fin de bataille.
- `InitPoolsHandler` : logique de collecte projectiles/FX à réutiliser sur les prefabs du world/roster.

## Approche

### Data (SO, dossier `Scripts/Worlds/`, namespace `Worlds`)
- `WorldSettings` : `public string Name; public List<RoomSettings> Rooms;`
- `RoomSettings` (abstract SO) : `public string Name;`
- `BattleRoomSettings : RoomSettings` : `public List<UnitController> Enemies; public int MinCount = 3; public int MaxCount = 5;` + méthode `RollEnemies()` → `List<UnitController>`.
- `PlayerRosterSettings` : `public List<UnitController> Units;` (pool de départ).

### Runtime
- `Worlds.Run` (classe C# pure) : `World`, `Roster`, `RoomIndex`, `CurrentRoom`, `IsOver`, `Victory/Defeat` log. `Start()` lance la séquence de la room courante ; `Completed` de la séquence → room suivante ou fin. C'est la boucle.
- `Game.Run` : propriété statique globale, créée par `Game.InitRun(world, roster)`, reset dans `ResetStatics`.
- `Battle.Deployment` (MonoBehaviour sur Level, `Level.Deployment`) : `[field: SerializeField] Rect PlayerZone / EnemyZone`, `Spacing`, gizmos. Méthodes : `SpawnPlayers(roster)`, `PlacePlayers()` (formation dans PlayerZone), `SpawnEnemies(prefabs)` (formation dans EnemyZone + `Battlefield.Add(Team.Other)`), `ClearEnemies()`. Pas d'input pour l'instant.
- `CombatClock` : ajout `Stop()` (IsRunning = false) et `Play()`. Clock stoppée pendant la préparation → aucune IA, aucune physique, mana figée.

### Séquences
- `BattleContext : SequenceContext` : `BattleRoomSettings Room; bool Victory;`
- `SequenceProvider.BattleRoomSequence(BattleRoomSettings room)` :
  1. `PrepareBattleHandler` : `Clock.Stop()`, `Deployment.PlacePlayers()`, `Deployment.SpawnEnemies(room.RollEnemies())`, hide `CombatSkillsMenu`, show `PrepareBattleMenu`, attend `PrepareBattleMenu.FightRequested` → `Continue()`. `OnExit` : désabonnement, hide menu.
  2. `StartBattleHandler` : show `CombatSkillsMenu`, `Clock.Play()`, écoute `Battlefield.UnitRemoved` : `OtherUnits.Count == 0` → Victory, `PlayerUnits.Count == 0` → Defeat. Puis `await clock.Delay(_endDelay)` (anims de mort) → `Continue()`. `OnExit` : cancel token + désabonnement.
  3. `MoveToNextRoomHandler` : `Deployment.ClearEnemies()` (release restants/corps), `Game.Run.CompleteRoom(victory)` → incrémente index ou marque fin (log « Defeat » / « World complete »). `Continue()`.
- `Run` sur `Completed` : si `!IsOver` → `BattleRoomSequence(next).Start()`.
- Boot : `... InitGameHandler → InitRunHandler(world, roster) → InitPoolsHandler → StartRunHandler`. `StartRunHandler` : `Deployment.SpawnPlayers(roster)` puis `Game.Run.Start()`.
- `InitPoolsHandler` : collecte depuis `Game.Run` (prefabs roster + ennemis de toutes les `BattleRoomSettings`) au lieu des unités du Battlefield ; prewarm aussi les prefabs d'unités.
- `Level.Init` : retirer `MenuManager.Show<CombatSkillsMenu>()` (géré par les handlers).

## Fichiers
Nouveaux :
- `Scripts/Worlds/WorldSettings.cs`, `RoomSettings.cs`, `BattleRoomSettings.cs`, `PlayerRosterSettings.cs`, `Run.cs`
- `Scripts/Battle/Deployment.cs`
- `Scripts/Sequence/Handlers/InitRunHandler.cs`, `StartRunHandler.cs`, `PrepareBattleHandler.cs`, `StartBattleHandler.cs`, `MoveToNextRoomHandler.cs`
- `Menus/PrepareBattleMenu/PrepareBattleMenu.cs` (généré par `CreateMenuWindow`, complété : `[SerializeField] Button _fightButton`, `event Action FightRequested`)

Modifiés :
- `Scripts/Core/Game.cs` (`Run`, `InitRun`, reset)
- `Scripts/Core/Level.cs` (`Deployment`, retrait Show menu)
- `Scripts/Battle/CombatClock.cs` (`Stop`/`Play`)
- `Scripts/Sequence/SequenceProvider.cs` (Boot + `BattleRoomSequence`, `BattleContext`)
- `Scripts/Sequence/Handlers/InitPoolsHandler.cs`
- `Scripts/Boot.cs` (`_world`, `_roster`)

## Steps
- [x] 0. (Toi) Créer `PrepareBattleMenu` via `CreateMenuWindow` (bouton « Fight » dans le prefab).
- [x] 1. SO : `WorldSettings`, `RoomSettings`, `BattleRoomSettings` (+ `RollEnemies`), `PlayerRosterSettings`.
- [x] 2. `CombatClock.Stop/Play`.
- [x] 3. `Deployment` (spawn, placement formation, clear ennemis, gizmos zones) + `Level.Deployment`.
- [x] 4. `Run` + `Game.Run` / `InitRun` / reset.
- [x] 5. `PrepareBattleMenu` : bouton → `FightRequested`.
- [x] 6. Handlers `PrepareBattle`, `StartBattle`, `MoveToNextRoom`, `BattleContext`, `SequenceProvider.BattleRoomSequence`.
- [x] 7. `InitRunHandler`, `StartRunHandler`, `InitPoolsHandler` adapté, Boot sequence + `Boot` fields, retrait Show dans `Level.Init`.
- [x] 8. Recompile (`unity cmd script recompile`), 0 erreur.

## Manips éditeur (toi, après code)
1. Créer assets : `ScriptableObjects/Worlds/World_01` (5 `BattleRoomSettings`), `Roster_Default` (`PlayerRosterSettings`).
2. `Boot` : assigner `_world`, `_roster`.
3. Scène Game : ajouter `Deployment` (sur Level ou enfant), régler `PlayerZone`/`EnemyZone`, l'assigner sur `Level`.
4. Scène Game : vider `Battlefield._playerUnits/_otherUnits` et supprimer les unités posées en scène.

## Vérification
- Play depuis Boot : unités roster spawnées dans PlayerZone, ennemis (count entre min/max) dans EnemyZone, rien ne bouge, menu Prepare visible.
- Fight : menu Prepare caché, skills visibles, combat démarre.
- Victoire : après délai, ennemis nettoyés, survivants replacés avec PV conservés, nouvelle room (log index), nouveaux ennemis.
- Unité joueur morte : absente des rooms suivantes ; slot skill du Hero retiré.
- Défaite : log « Defeat », plus de room. Après room 5 : log « World complete ».
- Pause/vitesse (Space/1-2-3) sans effet gênant pendant Prepare.

## Révision (post-implémentation)
- Le pool d'ennemis est porté par `WorldSettings` (`Enemies`, `MinEnemiesPerRoom`, `MaxEnemiesPerRoom`, `RoomCount`, `RollEnemies()`).
- Rooms = classes C# (`Room` abstraite, `BattleRoom` avec `Enemies` tirés) construites dans le constructeur de `World`.
- `Run` → `World` (runtime), `Game.Run` → `Game.World`, `InitRunHandler`/`StartRunHandler` → `InitWorldHandler`/`StartWorldHandler`.
- `RoomSettings` / `BattleRoomSettings` (SO) supprimés.
