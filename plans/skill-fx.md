# FX de skills et niveaux de skill

## Context
Matérialiser visuellement un skill (prefab FX) et faire évoluer skill + FX avec son niveau (niv. 1 petit FX, niv. 2 gros FX).

Brainstorm : pas encore de système de niveau de skill défini (`idees.md:836` : montée de niveau des chats, récompenses = capacité, passif, stats). Ici on pose la donnée et l'API, la source du niveau (héros, upgrade) viendra plus tard.

## Décisions
- **Deux hooks dans `SkillEffect`** : `OnCast(context)` une fois par lancement, `Apply(context, target)` par cible. Les deux virtuels vides. Un FX de zone = 1 instance, pas 1 par cible.
- **FX = effet** : `SpawnFxEffect` dans la liste `Effects`, ordonnable et mixable avec dégâts/soin.
  - `Anchor` : `Origin` (centre zone / lanceur), `Caster`, `EachTarget`.
  - `ScaleMode` : `None` ou `Radius` (prefab conçu pour rayon 1, `localScale = Radius × Multiplier`). Le visuel colle à la zone gameplay.
- **Composant `SkillFx` sur le prefab** (poolé, `IPoolable`) :
  - `Play(position, scale, level)`.
  - Variantes par niveau : sous-objets activés si `MinLevel <= level <= MaxLevel` (0 = pas de max).
  - Temps de combat : `ParticleSystem.main.simulationSpeed` et `Animator.speed` = `clock.Scale` (suivi via `ScaleChanged`), release après `clock.Delay(_duration)`. Pas de `Pooler.Release(go, delay)` (temps réel).
- **Niveaux dans `SkillSettings`** : `List<SkillLevel> Levels`, chaque niveau a `ManaCost`, `Radius`, `Effects`. Contrôle total par niveau (autre FX possible au niv. 2), au prix de duplication.
- **Niveau runtime sur `SkillSlot`** (`Level`, 1-based, clampé à `MaxLevel`), copié dans `SkillContext` (`Level`, `LevelData`).
- Debug : `Shift + F1..F4` monte le niveau du slot (cycle).

## Files
- `Scripts/Skills/SkillLevel.cs` (nouveau)
- `Scripts/Skills/SkillSettings.cs` : `Levels`, `MaxLevel`, `GetLevel(int)`
- `Scripts/Skills/SkillSlot.cs` : `Level`, `Data`, `SetLevel`, `LevelChanged`
- `Scripts/Skills/SkillContext.cs` : `Level`, `LevelData`
- `Scripts/Skills/SkillCaster.cs` : données de niveau, `OnCast` puis `Apply`
- `Scripts/Skills/Effects/SkillEffect.cs` : hooks
- `Scripts/Skills/Effects/SpawnFxEffect.cs` (nouveau)
- `Scripts/Skills/Fx/SkillFx.cs`, `SkillFxLevelVariant.cs`, `SkillFxAnchor.cs`, `SkillFxScaleMode.cs` (nouveaux)
- `Scripts/Skills/Debug/DebugSkillInput.cs` : level up debug
- Assets `ScriptableObjects/Skills/*.asset` : migration des champs vers `Levels[0]`

## Steps
- [x] `SkillLevel`, `SkillSettings.Levels`, migration des 3 assets
- [x] `SkillSlot.Level`, `SkillContext.Level/LevelData`
- [x] Hooks `SkillEffect` + `SkillCaster.Execute`
- [x] `SkillFx` + variantes + `SpawnFxEffect`
- [x] Debug level up
- [x] Recompilation CLI

## Côté éditeur (à faire par toi)
- Vérifier les 3 skills : champs migrés dans `Levels[0]`. Ajouter un `Levels[1]` (rayon plus grand, dégâts plus forts).
- Créer un prefab FX (ex. `Prefabs/Fx/HolyWaterFx`) :
  - racine avec `SkillFx`, `_duration` ≈ durée des particules ;
  - visuel dimensionné pour **rayon 1** ;
  - `ParticleSystem` : `Scaling Mode = Hierarchy` (sinon le scale du transform ne s'applique pas), `Play On Awake` off, `Stop Action` none ;
  - sous-objets optionnels pour le niv. 2 dans `Variants` (`MinLevel = 2`).
- Dans `HolyWater.Levels[n].Effects` : ajouter `SpawnFxEffect` (`Anchor = Origin`, `ScaleMode = Radius`, `Prefab = HolyWaterFx`).

## Verification
- Recompilation CLI sans erreur.
- Play depuis Boot :
  - `HolyWater` niv. 1 : FX au point de clic, taille = cercle de preview.
  - `Shift+F1` puis cast : FX plus gros (rayon niv. 2) + variantes niv. 2 visibles.
  - Pause pendant le FX : particules figées, reprise ok. x4 : FX accéléré.
  - FX `EachTarget` sur `Prayer` : 1 FX par allié soigné.
  - Pas de leak : instances retournent au pool `Pooler/Pool_*`.
