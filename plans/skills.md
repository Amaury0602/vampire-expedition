# Temps de combat, mana, skills et personnages uniques

## Context
Objectif :
- Les chasseurs uniques (nommés) apportent des skills.
- Un skill coûte du mana, pris dans une réserve partagée.
- Le joueur lance un skill puis cible selon son type (direct, une unité, ou une zone avec rayon), avec un preview.
- La pause est déclenchée par le joueur. Le combat tourne sur son **propre temps** (pas `Time.timeScale`).

L'UI (`CombatSkillsMenu`, `SkillCard`, barre de mana, menu de pause) fera l'objet d'un plan séparé. Ce plan expose seulement l'API et les events dont l'UI aura besoin.

Alignement avec le brainstorm (`idees.md`, sessions 16 et 21 ; Proto 1) :
- Mana partagé qui se recharge avec le temps.
- Chaque chasseur apporte ses sorts. **Si un membre meurt, ses sorts disparaissent.**
- La pause active sert à lancer un sort.
- Vitesse x1 / x2 / x4.

Hors scope : UI, mémoire, roster, journal d'événements, main de 4 avec cycle, mana gagné aux kills, lancement automatique des sorts.

## Décisions
- **Skills portés par le personnage** : `CharacterSettings` (preset SO) est assigné sur un composant `Hero` de l'unité. Pas de skills dans `Equipment`.
- **Temps de combat = `CombatClock`** :
  - Il émet `Ticked(dt)` et `FixedTicked(dt)`, où `dt = Time.deltaTime × vitesse`, et 0 en pause.
  - Les unités et projectiles **s'abonnent eux-mêmes** à ces events et n'ont plus de `Update`/`FixedUpdate` Unity.
  - La physique 2D passe en mode `Script` et avance avec le temps de combat (`Physics2D.Simulate`).
  - Les animators utilisent `speed = échelle effective`.
- **Accès à l'horloge** : une unité obtient l'horloge quand elle rejoint le Battlefield (`Combat.Join`). Elle s'y abonne à ce moment et se désabonne au `Leave` ou à la mort.
  - Cela évite le problème d'ordre d'init : les unités posées dans la scène existent avant `Game.Init`, alors que le Battlefield les fait rejoindre dans son `Start`.
  - Une unité hors Battlefield ne tick pas.
- **UniTask** : `CombatClock.Delay(seconds, token)` boucle sur `UniTask.Yield(PlayerLoopTiming.Update)` et accumule `clock.DeltaTime`. Ça couvre les délais du gameplay (release à la mort, etc.). Le `UniTask.Delay` natif n'accepte pas d'horloge custom, d'où ce wrapper.
- **Pause par le joueur** (Espace). Vitesses : touches 1 / 2 / 3 pour x1 / x2 / x4. On peut lancer un skill en pause comme en combat. Le ciblage ne touche pas au temps.
- **Mana dépensé à la confirmation**. Une annulation ne coûte rien.
- **SO simples** (`ScriptableObject`, comme `Item`). `IdentifiedScriptableObject` attendra le besoin de sauvegarde.
- Nom **`SkillCaster`** plutôt que `SkillManager`.

## Approach

### 1. Temps de combat
- `Scripts/Battle/CombatClock.cs` (MonoBehaviour sur le GameObject `Level`) :
  - `Speed` (1 / 2 / 4), `IsPaused`, `Scale` (0 si en pause, sinon `Speed`), `DeltaTime`, `Time` ;
  - `Pause()`, `Resume()`, `TogglePause()`, `SetSpeed(float)` ;
  - events `Ticked(float)`, `FixedTicked(float)`, `ScaleChanged(float)`, `PauseChanged(bool)`.
  - `Awake` : `Physics2D.simulationMode = SimulationMode2D.Script`. `OnDestroy` : retour à `FixedUpdate`.
  - `Update` : `DeltaTime = Time.deltaTime * Scale`, `Time += DeltaTime`, puis `Ticked`.
  - `FixedUpdate` : accumulateur `Time.fixedDeltaTime * Scale`. À chaque pas fixe : `FixedTicked(step)` puis `Physics2D.Simulate(step)`. x4 donne 4 sous-pas, ce qui garde la physique stable.
  - `UniTask Delay(float seconds, CancellationToken token)`.
- `Battlefield` :
  - champ `[field: SerializeField] public CombatClock Clock` ;
  - events `UnitJoined(UnitController)` et `UnitRemoved(UnitController)`.
- `UnitCombat.Join` / `Leave` : exposent `Clock` (`Battlefield.Clock`) et déclenchent l'abonnement / le désabonnement du controller.
- `UnitController` :
  - `Update` devient `OnTicked(dt)`, `FixedUpdate` devient `OnFixedTicked(dt)`, abonnés à l'horloge au `Join`.
  - Désabonnement au `Leave`, à la mort et dans `OnDestroy`.
  - Mort : `Clock.Delay(_deathDuration)` puis `Pooler.Release(gameObject)`. L'horloge est capturée avant `Combat.Leave()`.
- `UnitAnimator` : `_animator.speed = clock.Scale` sur `ScaleChanged` (abonnement au `Join`).
- `UnitVisualFeedback` : `_tween.timeScale = clock.Scale`.
- `Projectile` : `Launch` reçoit l'horloge du tireur (`Combat.Clock`). Il s'abonne à `Ticked` et se désabonne dans `OnRelease`. `Update` devient `OnTicked(dt)`.
- `Scripts/Battle/CombatTimeControls.cs` : clavier (Espace pour la pause, 1 / 2 / 3 pour les vitesses) qui appelle `CombatClock`.
- `Level` : ajout de `Clock`, `Mana`, `SkillCaster`.

### 2. Mana
- `Scripts/Skills/ManaPool.cs` : MonoBehaviour qui implémente `IStatPool`, sur le modèle de `Health`.
  - `Max`, `RegenPerSecond`, `StartFilled`.
  - Régénération sur `Clock.Ticked`. L'event `Changed` servira à l'UI.

### 3. Données skills
- `Scripts/Skills/SkillTargeting.cs` : `enum SkillTargeting { Instant, Unit, Area }`.
- `Scripts/Skills/SkillTargetFilter.cs` : `enum SkillTargetFilter { Self, Allies, Enemies, All }`.
- `Scripts/Skills/SkillSettings.cs` : `ScriptableObject` avec `[CreateAssetMenu]` et des champs publics :
  - `Name`, `Icon`, `Targeting`, `TargetFilter`, `ManaCost`, `Radius` ;
  - `[SerializeReference] public List<SkillEffect> Effects` (sélecteur de type Odin).
- `Scripts/Skills/SkillContext.cs` : `Caster` (UnitController), `TargetUnit`, `Position`, `Targets` (liste réutilisée, sans allocation).
- `Scripts/Skills/Effects/SkillEffect.cs` : `[Serializable] abstract`, avec `abstract void Apply(SkillContext context, UnitController target)`.
- `Scripts/Skills/Effects/DamageEffect.cs` : `Damage`, `DamageTypes`, `KnockbackPower`. Crée un `DamageInfo` dont la direction va du centre (zone) ou du lanceur vers la cible, puis `Health.TakeDamage`.
- `Scripts/Skills/Effects/HealEffect.cs` : `Amount`, puis `Health.Gain`.
- Résolution des cibles par type :
  - `Instant` : toutes les unités valides du filtre (`Self` = le lanceur).
  - `Unit` : l'unité survolée, si elle correspond au filtre.
  - `Area` : les unités du filtre telles que `distance(Position, hitbox.Center) - hitbox.Radius <= Radius`.

### 4. Personnages
- `Scripts/Characters/CharacterSettings.cs` : `ScriptableObject` avec `[CreateAssetMenu]` et les champs publics `Name`, `Portrait`, `List<SkillSettings> Skills`.
- `Scripts/Characters/Hero.cs` : MonoBehaviour sur l'unité unique.
  - `[field: SerializeField] public CharacterSettings Settings`, `UnitController Controller`.
  - Event `Died`, relayé depuis `UnitController.Died`.
- Le `Unit.prefab` générique n'a pas de `Hero`. On ajoute `Hero` seulement sur les instances uniques (ou sur des variantes de prefab).

### 5. SkillCaster
- `Scripts/Skills/SkillSlot.cs` : `Hero Owner`, `SkillSettings Settings`.
- `Scripts/Skills/SkillCaster.cs` : MonoBehaviour sur `Level`.
  - Références : `Battlefield`, `ManaPool`, `SkillTargetingPreview`, `Camera`.
  - `IReadOnlyList<SkillSlot> Slots`, event `SlotsChanged`.
    - Construit via `Battlefield.UnitJoined` / `UnitRemoved` : unités `Team.Player` qui ont un `Hero`.
    - Mort d'un héros : ses slots sont retirés, puis `SlotsChanged`.
  - `bool CanCast(SkillSlot slot)` : héros vivant et `Mana.Current >= ManaCost`.
  - `BeginCast(slot)` :
    - `Instant` : exécution immédiate.
    - Sinon : ciblage en cours, `CastStarted`, preview affiché.
  - Pendant le ciblage (`Update` en temps réel, pour marcher aussi en pause) :
    - Position souris → monde.
    - Survol d'unité via `Hitbox`.
    - Mise à jour du preview (couleur valide / invalide).
    - Clic gauche pour confirmer si valide. Clic droit ou Échap pour annuler.
  - `Confirm` : `Mana.Lose(cost)`, résolution des cibles, `Apply` pour chaque effet et chaque cible, puis `CastEnded`.
  - `IsTargeting`, pour bloquer les autres inputs de clic.
- `Scripts/Skills/SkillTargetingPreview.cs` : MonoBehaviour avec un `SpriteRenderer` cercle.
  - `ShowArea(position, radius, valid)`, `ShowUnit(unit, valid)`, `Hide()`.
  - Le cercle est mis à l'échelle `radius × 2`. Pour une unité, il prend le rayon de la hitbox avec une marge.
- Test sans UI : `Scripts/Skills/Debug/DebugSkillInput.cs`. Les touches F1 à F4 appellent `BeginCast(Slots[i])`.

## Files to modify
- [new] `Scripts/Battle/CombatClock.cs`, `Scripts/Battle/CombatTimeControls.cs`
- [new] `Scripts/Skills/ManaPool.cs`, `SkillTargeting.cs`, `SkillTargetFilter.cs`, `SkillSettings.cs`, `SkillContext.cs`, `SkillSlot.cs`, `SkillCaster.cs`, `SkillTargetingPreview.cs`, `Debug/DebugSkillInput.cs`
- [new] `Scripts/Skills/Effects/SkillEffect.cs`, `DamageEffect.cs`, `HealEffect.cs`
- [new] `Scripts/Characters/CharacterSettings.cs`, `Hero.cs`
- [mod] `Scripts/Core/Level.cs`, `Scripts/Battle/Battlefield.cs`, `Scripts/Units/UnitCombat.cs`, `UnitController.cs`, `UnitAnimator.cs`, `UnitVisualFeedback.cs`, `Scripts/Projectiles/Projectile.cs`
- [asset] `ScriptableObjects/Skills/` : `HolyWater` (zone, dégâts Fire), `Stake` (unité, dégâts Pierce), `Prayer` (instant, soin des alliés)
- [asset] `ScriptableObjects/Characters/` : 2 presets (ex. `Varga`, `Mira`)

## Reuse
- `Scripts/Stats/IStatPool.cs` et `Units/Health.cs` (modèle du pool)
- `Damages/DamageInfo.cs`, `Health.TakeDamage`, `Health.Gain`
- `Battle/Battlefield.cs` (`GetEnemies`, listes `PlayerUnits` / `OtherUnits`), `Units/Targeting.IsValidTarget`
- `Units/Hitbox` (`Center`, `Radius`) pour le survol et la zone
- `UnitCombat.Join` / `Leave` comme point d'abonnement à l'horloge
- `Pooling/Pooler.Release`
- Input `Mouse.current` / `Keyboard.current`, comme dans `DebugTargetSetter`

## Steps
- [x] `CombatClock` (ticks, physique en mode Script, `Delay` UniTask) + `Level.Clock`
- [x] `Battlefield` : `Clock`, events `UnitJoined` / `UnitRemoved`
- [x] Migrer `UnitCombat` / `UnitController` (abonnement au `Join`, mort via `Clock.Delay`), `UnitAnimator` (speed), `UnitVisualFeedback` (timeScale), `Projectile` (horloge du tireur)
- [x] `CombatTimeControls`
- [x] `ManaPool` + `Level.Mana`
- [x] Enums, `SkillSettings`, `SkillContext`, `SkillEffect`, `DamageEffect`, `HealEffect`
- [x] `CharacterSettings`, `Hero`
- [x] `SkillSlot`, `SkillCaster`, `SkillTargetingPreview`, `DebugSkillInput` + `Level.SkillCaster`
- [x] Assets : 3 skills, 2 personnages
- [x] Recompilation avec le CLI `unity` après chaque script

## Côté éditeur (à faire par toi)
- GameObject `Level` (`Game.unity`) : ajouter `CombatClock`, `CombatTimeControls`, `ManaPool`, `SkillCaster` et `DebugSkillInput`, puis assigner les références de `Level` et `Battlefield.Clock`.
- `SkillTargetingPreview` : GameObject avec un `SpriteRenderer` cercle (sprite rond, ordre de tri au-dessus des unités), assigné au `SkillCaster`.
- 2 unités joueur : ajouter `Hero` et assigner les presets.

## Verification
- Recompilation avec le CLI `unity` sans erreur.
- Play depuis Boot :
  - Espace : tout se fige (déplacements, attaques, animations, projectiles, mana). 1 / 2 / 3 changent la vitesse.
  - Le mana se remplit (Inspector `ManaPool`).
  - F1 sur un skill trop cher : refusé.
  - `Stake` : survol d'un ennemi, anneau vert, clic, dégâts, mana dépensé. Clic droit : annulation sans coût.
  - `HolyWater` : un cercle du bon rayon suit la souris. Clic : dégâts sur les ennemis dans le rayon.
  - `Prayer` : soin immédiat des alliés.
  - Lancement possible en pause. Les effets s'appliquent, le temps reste figé.
  - Mort d'un héros : ses slots disparaissent (`SlotsChanged`).
