﻿#if UNITYMVVMTOOLKIT_UNITASK_SUPPORT

namespace UnityMvvmToolkit.UniTask.Interfaces
{
    public interface IBaseAsyncCommand
    {
        bool IsRunning { get; }
        bool AllowConcurrency { get; set; }
        bool DisableOnExecution { get; set; }

        void Cancel();
    }
}

#endif