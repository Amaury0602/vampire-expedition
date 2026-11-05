namespace Pooling
{
    /// <summary>
    /// À implémenter sur un composant d'un prefab poolé pour réinitialiser son état
    /// à chaque récupération/relâche (vie, vélocité, animation...).
    /// </summary>
    public interface IPoolable
    {
        void OnGet();
        void OnRelease();
    }
}
