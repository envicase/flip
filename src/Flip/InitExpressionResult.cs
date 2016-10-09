namespace Flip
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;

    internal class InitExpressionResult<TDelegate>
        where TDelegate : class
    {
        public InitExpressionResult(Expression<TDelegate> expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            Expression = expression;
            Function = expression.Compile();
            Errors = Enumerable.Empty<InitExpressionError>();
        }

        private InitExpressionResult(IEnumerable<InitExpressionError> errors)
        {
            Expression = null;
            Function = null;
            Errors = new ReadOnlyCollection<InitExpressionError>(
                     new List<InitExpressionError>(errors));
        }

        public bool IsSuccess => Expression != null;

        public Expression<TDelegate> Expression { get; }

        public TDelegate Function { get; }

        public IEnumerable<InitExpressionError> Errors { get; }

        public static InitExpressionResult<TDelegate> WithErrors(
            IEnumerable<InitExpressionError> errors)
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            return new InitExpressionResult<TDelegate>(errors);
        }

        public static InitExpressionResult<TDelegate> WithError(
            InitExpressionError error)
        {
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            return new InitExpressionResult<TDelegate>(new[] { error });
        }
    }
}
