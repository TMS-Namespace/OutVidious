using TMS.Apps.FrontTube.Backend.Core.ViewModels;

namespace TMS.Apps.FrontTube.Backend.Core.Tools
{
    public abstract class ViewModelBase
    {
        public Super Super {get; }

        internal ViewModelBase(Super super)
        
        {
            Super = super ?? throw new ArgumentNullException(nameof(super));
        }
        
        public abstract void Dispose();
    }
}