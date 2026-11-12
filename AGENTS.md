# AGENTS.md

## Références design

Ce projet Unity trouve ses racines dans le dossier de brainstorm :
`C:\Users\Windows\Documents\GitHub\brainstorm`

Toutes les références design sont là-bas :
- `README.md`
- `chasseurs-contre-dracula.md` / `.json` / `.pdf`
- `idees.md`

Consulter ce dossier avant toute décision de game design ou feature gameplay.

## Unity CLI

Ne pas utiliser `unity` CLI, sauf pour compiler.

Toujours recompiler avec `unity` CLI après chaque modification de script.

Compilation (éditeur ouvert, `unity status` pour vérifier) :

```bash
unity cmd script recompile --no-banner
unity cmd script recompile_status --no-banner
```

Poller `recompile_status` jusqu'à `completed` ou `up_to_date`, vérifier `"failed":false` et `errors` vide.

Ne pas modifier les scènes/prefabs via le CLI : lister les manipulations éditeur à faire par l'utilisateur.

## Architecture

### Boot
- Scène `Scenes/Boot.unity` (build index 0) : prépare les systèmes, reste chargée. `Scenes/Game.unity` = build index 1.
- `Scripts/Boot.cs` lance `SequenceProvider.BootSequence(...)` dans `Start`.
- Séquence : `InitMenusHandler` → `LoadSceneHandler(Game)` → `SetActiveSceneHandler` → `InitGameHandler`.
- Pour tester, lancer le Play depuis Boot (`PlayFromSceneToolbar`), sinon menus et `Game` ne sont pas initialisés.

### Séquences (`Scripts/Sequence`)
- `Sequence` enchaîne des `Handler`. Chaque handler implémente `OnEnter()`, appelle `Continue()` quand il a fini (directement dans `OnEnter` si instantané), nettoie dans `OnExit()`.
- `Handler<TContext>` pour lire/écrire un contexte typé (`LoadLevelContext.LoadedScene`).
- Nouvelle étape = nouveau handler dans `Scripts/Sequence/Handlers/`. Nouvelle séquence = méthode statique dans `SequenceProvider`.
- Ambiguïté avec `PrimeTween.Sequence` : `using Sequence = Sequences.Sequence;`.

### Accès global : `Core.Game`
- Classe statique, point d'accès aux systèmes clés.
- `Game.Init()` (appelé par `InitGameHandler` après chargement de la scène Game) : `FindAnyObjectByType<Level>()` puis `Level.Init()`. Pas d'auto-enregistrement du Level.
- `Game.Level` : MonoBehaviour `Core.Level`, référence les systèmes du niveau (`Battlefield`, `Clock`, `Mana`, `SkillCaster`). `Level.Init()` initialise ses systèmes (ex. `Mana.Init()`).
- `Game.Level` est `null` pendant `Awake`/`OnEnable` des objets de la scène : les systèmes du niveau s'initialisent dans leur `Init()`, appelé par `Level.Init()` (ordre : `Clock`, `Mana`, `SkillCaster`, `Battlefield`).
- Pas de références croisées sérialisées entre systèmes du niveau : seul `Level` les référence, les autres y accèdent directement via `Game.Level.X` (pas de cache en champ privé).
- Exception : objets poolés (`Projectile`, `SkillFx`) gardent l'horloge à laquelle ils sont abonnés, car le Pooler survit au Level.
- Système propre au niveau → propriété sérialisée sur `Level`. Système global → propriété statique sur `Game`, initialisée dans `Game.Init`, remise à zéro dans `ResetStatics`.
- Pas de namespace `Game` (conflit avec la classe) : utiliser `Core`.

### Temps de combat (`Battle/CombatClock`)
- Le gameplay de combat n'utilise jamais `Time.deltaTime` / `Update` Unity : s'abonner à `CombatClock.Ticked` / `FixedTicked` (dt déjà multiplié par la vitesse, 0 en pause).
- Physique 2D en mode `Script`, simulée par l'horloge.
- L'horloge ne tourne qu'après `Level.Init()` (`Clock.Init()`) : rien ne tick avant `Game.Init`.
- Principe : chaque script s'abonne lui-même à l'horloge (`Game.Level.Clock`) dans son `Init()` (systèmes du niveau) ou au lancement (objets poolés), et se désabonne dans `OnDestroy`/`OnRelease`. Pas d'horloge passée en paramètre.
- Exception unités : horloge reçue au `Combat.Join` via `Battlefield.Clock` (= `Game.Level.Clock`, `UnitController.AttachClock`), event `ClockChanged` pour `UnitAnimator`/`UnitVisualFeedback`.
- Délais gameplay : `await clock.Delay(seconds, token)` (UniTask).
- Animator : `speed = clock.Scale` ; tweens PrimeTween : `timeScale = clock.Scale`.
- Inputs et UI restent en temps réel (fonctionnent en pause).

### Skills / personnages
- `SkillSettings` (SO) + effets polymorphes `[SerializeReference] SkillEffect`. Nouvel effet = classe `[Serializable]` dérivant de `SkillEffect`.
- Skills portés par `CharacterSettings` via le composant `Hero` (unités uniques seulement).
- `Game.Level.SkillCaster` : `Slots`, `CanCast`, `BeginCast`, `IsTargeting`, events. Mana : `Game.Level.Mana`.

### Menus (`Menus/`)
- `MenuDatabase` (SO `Menus/MenuDatabase.asset`) = library des prefabs `Menu`. Tout nouveau menu doit y être référencé.
- `MenuManager` statique, initialisé par `InitMenusHandler` : instancie chaque menu une fois (désactivé).
- Usage : `MenuManager.Show<T>()`, `Hide<T>()`, `Get<T>()`.
- Créer/renommer un menu via les outils éditeur `CreateMenuWindow` / `RenameMenuWindow`.

### Plans
- Les plans de features sont dans `plans/*.md` (contexte, décisions, fichiers, steps cochés, vérification).
- Roadmap : Milestone 1 / Proto 1 de `brainstorm/chasseurs-contre-dracula.md`, priorités dans `brainstorm/idees.md` (session 19, Feature Prioritization).

## Conventions de code

- Pas de commentaires ni de `/// <summary>` dans les scripts.
- Pas de méthodes inline (expression-bodied `=>`). Toujours un corps `{ }`.
- Champ éditable dans l'Inspector utilisé seulement en interne : `[SerializeField] private bool _value;`
- Champ éditable dans l'Inspector qui doit être public : auto-propriété sérialisée, jamais champ privé + propriété `=>` :

```csharp
// Non
[SerializeField] private float _acceleration = 20f;
public float Acceleration => _acceleration;

// Oui
[field: SerializeField] public float Acceleration { get; private set; } = 20f;
```

- Exception ScriptableObject : les données sont des champs publics simples (`public string Name;`, `public float Damage;`), sans propriété ni `[field: SerializeField]`.
