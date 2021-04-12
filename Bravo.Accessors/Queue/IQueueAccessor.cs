﻿using System;

namespace Bravo.Accessors.Queue
{
    public interface IQueueAccessor
    {
        void CreateGenerateInputsMessage(int runId, TimeSpan? delay);

        void CreateRunAnalysisMessage(int runId, TimeSpan? delay);

        void CreateGenerateOutputsMessage(int runId, TimeSpan? delay);
    }
}
