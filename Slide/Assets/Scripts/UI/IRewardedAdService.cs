using System;

namespace UI
{
    public interface IRewardedAdService
    {
        bool IsShowing { get; }
        void Show(Action successCallback);
        void Cancel();
    }
}
