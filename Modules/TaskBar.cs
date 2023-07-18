using TaskBarProgress;
using TaskBarProgress.Enums;

namespace CryptoEat.Modules;

internal static class TaskBar
{
    private static TaskbarStates _currentState = TaskbarStates.NoProgress;

    internal static void SetEmpty()
    {
        _currentState = TaskbarStates.NoProgress;
        Progress.SetState(TaskbarStates.NoProgress);
    }

    internal static void SetProgress(decimal progress)
    {
        if (_currentState != TaskbarStates.Normal)
        {
            _currentState = TaskbarStates.Normal;
            Progress.SetState(TaskbarStates.Normal);
        }

        Progress.SetValue((double) progress, 100d);
    }
}