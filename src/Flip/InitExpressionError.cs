namespace Flip
{
    using System;
    using System.Reflection;
    using static System.Environment;

    internal class InitExpressionError
    {
        public InitExpressionError(ConstructorInfo constructor, string message)
        {
            if (constructor == null)
                throw new ArgumentNullException(nameof(constructor));

            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Constructor = constructor;
            Message = message;
        }

        public ConstructorInfo Constructor { get; }

        public string Message { get; set; }

        public string GetExceptionMessage() =>
               "Failed to create a initializing expression "
               + $"with the constructor {Constructor.GetFriendlyName()}. "
               + $"See the details below.{NewLine}{Message}";
    }
}
