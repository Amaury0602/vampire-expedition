namespace Player
{
    [System.Serializable]
    public class NavigationSettings
    {
        public float Acceleration = 20f;
        public float StopDistance = 0.5f;
        public float RepathInterval = 0.3f;
        public float CornerReachDistance = 0.15f;
    }
}
