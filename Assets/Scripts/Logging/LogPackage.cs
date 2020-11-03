

namespace Logging
{

    public class LogPackage
    {
        readonly string key;
        readonly string message;
        readonly LogLevel level;

        public LogPackage(string key, string message, LogLevel level)
        {
            this.key = key;
            this.message = message;
            this.level = level;
        }

        public LogLevel Level => level;

        public string Message => message;

        public string Key => key;
    }
}