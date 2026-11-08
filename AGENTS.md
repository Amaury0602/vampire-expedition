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
