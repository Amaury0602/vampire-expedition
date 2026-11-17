using Pooling;

namespace Sequences
{
    public class ClearLevelHandler : Handler
    {
        protected override void OnEnter()
        {
            Pooler.ClearAll();
            Continue();
        }
    }
}
