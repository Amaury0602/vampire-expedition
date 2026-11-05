using System;

namespace Economy
{
    [Serializable]
    public struct Price
    {
        public Currency Currency;
        public float Amount;
    }
}
