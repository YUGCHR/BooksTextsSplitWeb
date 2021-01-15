using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundTasksQueue.Models
{
    public class BackgroundProcessingTask
    {
        public int TaskId { get; set; }
        public string ProcessingTaskId { get; set; }
        public Task ProcessingTask { get; set; }
        public CancellationTokenSource CancellationTaskToken { get; set; }
    }
}
