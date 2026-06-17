using System;
using System.IO;

namespace ADReplStatus
{
    public static class Logger
    {
        public static void Log(string message)
        {
            var state = AppState.Instance;
            if (state.LoggingEnabled)
            {
                File.AppendAllText(state.LogfileName, $"[{DateTime.Now}] {message}\n");
            }
        }
    }
}
