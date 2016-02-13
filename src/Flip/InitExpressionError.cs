using System;
using System.Reflection;
using static System.Environment;

namespace Flip
{
    internal class InitExpressionError
    {
        public InitExpressionError(
            ConstructorInfo constructor,
            string detailMessage)
        {
            if (constructor == null)
                throw new ArgumentNullException(nameof(constructor));
            if (detailMessage == null)
                throw new ArgumentNullException(nameof(detailMessage));

            Constructor = constructor;
            DetailMessage = detailMessage;
        }

        public ConstructorInfo Constructor { get; }

        public string DetailMessage { get; set; }

        public string GetExceptionMessage() =>
            "Failed to create a initializing expression " +
            $"with the constructor {Constructor.GetFriendlyName()}. " +
            $"See the detail message below.{NewLine}{DetailMessage}";
    }
}
