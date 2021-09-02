using System;

namespace Ez.Enif
{
    public sealed class ErrorEventArgs : EventArgs
    {
        public ErrorEventArgs(string message)
        {
            Message = message;
        }
        public string Message { get; }
    }
}
