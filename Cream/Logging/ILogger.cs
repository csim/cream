using System;
namespace Cream.Logging
{
    public interface ILogger : IDisposable
    {
        void Write(string category, Exception ex);
        void Write(string category, string message, bool timestamp = false);
        void Write(string message);
        void Write(System.Text.StringBuilder builder, string category, Exception ex);
        void Write(System.Text.StringBuilder builder, string category, string message, bool timestamp = false);
    }
}
