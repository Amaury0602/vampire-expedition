# Combat des unités

## Context
Les unités savent bouger (Idle / MoveToTarget) mais pas se battre. Objectif : deux équipes s'affrontent dans un `Battlefield`. Chaque unité porte son équipe, cible l'ennemi le plus proche (sans portée max), le poursuit, l'attaque avec l'arme équipée (Sword), les dégâts tombent sur un Animation Event, les morts retournent au pool et les survivants retargetent.

## Décisions
- Portée atteinte si `distance(centre hitbox attaquant, centre hitbox cible) - cible.Hitbox.Radius <= weapon.Range`.
- Hitbox = cercle avec son propre rayon + offset, gizmo de debug.
- **AttackSpeed sur l'arme** (attaques/sec). Une épée et un marteau doivent avoir des rythmes différents ; un multiplicateur côté unité pourra s'ajouter plus tard sans casser l'API.
- Dégâts appliqués par Animation Event → méthode publique `AttackHit()` sur `UnitAnimator` (même GameObject que l'Animator : root du prefab) → `Controller.Combat.Hit()`.
- Combat prioritaire sur les ordres de déplacement : si une cible existe, l'unité poursuit/attaque. (Cible prioritaire par clic : plus tard, prévu via `Sensor.ForceTarget(...)` non implémenté maintenant.)
- **Équipe portée par `UnitCombat`** (champ `Team` sérialisé, visible dans l'Inspector du controller). Le `Battlefield` reste la source de vérité : il écrit l'équipe de chaque unité selon la liste où elle est référencée.
- Mort → retrait du Battlefield → `Pooler.Release` si `PooledObject` présent, sinon `SetActive(false)` (unités posées à la main dans la scène). Reset via `IPoolable.OnGet`.
- Items en ScriptableObject.
- `UnitCombat` et `UnitSensor` = classes C# créées dans `UnitController.Awake` (comme `UnitSeparation`).

## Existant réutilisé
- `Scripts/Units/UnitController.cs` : crée modules + states dans `Awake`.
- `Scripts/CharacterController/States/StateManager.cs`, `BaseState.cs` : FSM + events `Entered/Exited`.
- `Scripts/Units/States/UnitIdleState.cs` : `Controller.Steer(Vector2.zero)` → base comportement de `UnitAttackState`.
- `Scripts/Units/States/UnitMoveToTargetState.cs` : pathfinding NavMesh → réutilisé pour la poursuite.
- `Scripts/Units/UnitAnimator.cs` : abonnements states → animator.
- `Scripts/Core/IdentifiedScriptableObject.cs` : base de `Item` (id stable pour la save plus tard).
- `Scripts/Pooling/Pooler.cs`, `IPoolable.cs`, `PooledObject.cs`.
- `UnitController.controller` : trigger `Attack` + état `Unit_Attack` (AnyState → Attack) déjà présents.

## Architecture

### Stats
- `Scripts/Stats/IStatPool.cs`
  - `float Current`, `float Max`, `bool IsEmpty`, `bool IsFull`
  - `void Fill()`, `void Empty()`, `void Gain(float amount)`, `void Lose(float amount)`
  - `event Action<float, float> Changed` (current, max), `event Action Emptied`
- `Scripts/Units/Health.cs` : MonoBehaviour, implémente `IStatPool`, `[field: SerializeField] Max`, `Fill()` au `Awake`. `Emptied` déclenché une seule fois (passage à 0).

### Hitbox
- `Scripts/Units/Hitbox.cs` : MonoBehaviour, `Radius`, `Offset`, `Vector2 Center`, `OnDrawGizmos` (cercle wire).

### Items
- `Scripts/Items/Item.cs` : `abstract class Item : IdentifiedScriptableObject`.
- `Scripts/Items/Weapon.cs` : `Weapon : Item`, `Damage`, `Range`, `AttackSpeed`, `[CreateAssetMenu]`.
- `Scripts/Items/Equipment.cs` : MonoBehaviour, `List<Item> Items`, `Weapon Weapon` = premier `Weapon` de la liste (cache au `Awake`).
- Asset `Assets/_Game/Data/Items/Sword.asset`.

### Battlefield
- `Scripts/Battle/Team.cs` : `enum Team { Player, Other }`.
- `Scripts/Battle/Battlefield.cs` : MonoBehaviour, `[SerializeField] List<UnitController> _playerUnits`, `_otherUnits`.
  - `Start` : pour chaque unité → `unit.Combat.Join(this, team)`.
  - `IReadOnlyList<UnitController> GetEnemies(Team team)`.
  - `Remove(UnitController unit)` appelé à la mort.

### UnitCombat (classe C# sérialisée dans `UnitController`)
- `Team Team` (sérialisé), `Battlefield Battlefield`, `Weapon Weapon` (depuis `Equipment`).
- `bool IsInRange(UnitController target)`, `bool CanStartAttack` (cooldown prêt et pas en train d'attaquer).
- `StartAttack(target)` : mémorise la cible, lance le cooldown `1 / AttackSpeed`, event `AttackStarted` → `UnitAnimator` déclenche trigger `Attack`.
- `Hit()` (appelé par l'Animation Event) : si cible vivante et à portée → `target.Health.Lose(Damage)`. Fin de l'attaque.

### UnitSensor (classe C#)
- `UnitController Target`, event `TargetChanged`.
- `Update(dt)` : tick toutes les `0.25s`, timer initial aléatoire pour étaler les unités.
- Scan : boucle sur `Battlefield.GetEnemies(Team)`, `sqrMagnitude`, ignore `Health.IsEmpty`, zéro allocation.
- Abonnement à `Target.Health.Emptied` → désabonnement + rescan immédiat.
- `Clear()` pour le pool.

### States
- `UnitChaseState` : hérite de `UnitMoveToTargetState`. Met à jour la position cible avec la position de `Sensor.Target` à chaque repath ; passe en `UnitAttackState` dès `IsInRange`. Plus de cible → `UnitIdleState`.
  - Refacto légère de `UnitMoveToTargetState` : méthode `protected virtual void OnTargetReached()` (Idle par défaut) + accès protégé à `SetTargetPosition`.
- `UnitAttackState` : steer zéro (comme Idle). Si `Combat.CanStartAttack` → `StartAttack`. Hors portée (et pas en plein swing) → Chase. Plus de cible → Idle.
- Transitions globales dans `UnitController.Update` : si `Sensor.Target != null` et état courant Idle ou MoveToTarget → Chase.
- `UnitDeadState` non nécessaire : mort = sortie du jeu (pool).

### UnitController
- Nouveaux champs : `Health`, `Hitbox`, `Equipment` (GetComponent), `[field: SerializeField] UnitCombat Combat`, `UnitSensor Sensor`.
- Abonnement `Health.Emptied` → `Die()` : `Battlefield.Remove`, release pool / désactivation.
- Implémente `IPoolable` : `OnGet` → `Health.Fill()`, `Sensor.Clear()`, `SetState<UnitIdleState>()`. `OnRelease` → reset vélocité.
- `OnDrawGizmosSelected` : cercle de portée de l'arme.

### UnitAnimator
- `Move` bool aussi sur `UnitChaseState`.
- Abonnement `Combat.AttackStarted` → `SetTrigger(Attack)`.
- `public void AttackHit()` → `_controller.Combat.Hit()`.

## Files
- [new] `Scripts/Stats/IStatPool.cs`
- [new] `Scripts/Units/Health.cs`, `Hitbox.cs`, `UnitCombat.cs`, `UnitSensor.cs`
- [new] `Scripts/Items/Item.cs`, `Weapon.cs`, `Equipment.cs`
- [new] `Scripts/Battle/Team.cs`, `Battlefield.cs`
- [new] `Scripts/Units/States/UnitChaseState.cs`, `UnitAttackState.cs`
- [mod] `Scripts/Units/UnitController.cs`, `UnitAnimator.cs`, `States/UnitMoveToTargetState.cs`
- [asset] `Data/Items/Sword.asset`
- [prefab] `Prefabs/Unit.prefab` : ajout `Health`, `Hitbox`, `Equipment` (Sword).

## Côté éditeur (à faire par toi, je ne touche pas aux transitions)
- Clip `Unit_Attack` : Animation Event `AttackHit` sur la frame d'impact.
- Animator : transition `Unit_Attack` → `Unit_Idle` (exit time) pour sortir de l'attaque.
- Scène de test : un `Battlefield` avec les deux listes remplies.

## Steps
- [x] `IStatPool` + `Health`
- [x] `Hitbox` + gizmo
- [x] `Item`, `Weapon`, `Equipment`, asset `Sword`
- [x] `Team`, `Battlefield`
- [x] `UnitCombat`, `UnitSensor`
- [x] Refacto `UnitMoveToTargetState`, `UnitChaseState`, `UnitAttackState`
- [x] `UnitController` (modules, transitions, mort, `IPoolable`, gizmo portée)
- [x] `UnitAnimator` (chase, trigger attack, `AttackHit`)
- [x] Prefab `Unit` : composants + Sword
- [x] `unity recompile` après chaque script

## Verification
- `unity recompile --project-path <chemin absolu>` sans erreur.
- Scène test : 2 groupes séparés, Battlefield référence chaque groupe.
  - Play → chaque unité poursuit l'ennemi le plus proche, s'arrête à portée, anime l'attaque.
  - PV baissent à la frame d'impact (Inspector `Health`).
  - Mort → unité disparaît, attaquants retargetent immédiatement.
  - Gizmos : hitbox et portée visibles.
  - Clic de déplacement pendant un combat : l'unité reste en combat.
