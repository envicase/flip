using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Flip
{
    public static class ObservableExtensions
    {
        private static string GetMemberName(Expression expression)
        {
            var body = expression as MemberExpression;
            if (body != null)
                return body.Member.Name;

            if (expression.NodeType == ExpressionType.Convert)
            {
                var unaryExpression = expression as UnaryExpression;
                body = unaryExpression.Operand as MemberExpression;
                if (body != null)
                {
                    return body.Member.Name;
                }
            }

            return null;
        }

        private static IObservable<TProperty> ObserveImpl
            <TComponent, TProperty>(
            TComponent component,
            Expression<Func<TComponent, TProperty>> propertyExpression,
            IScheduler scheduler)
            where TComponent : INotifyPropertyChanged
        {
            string propertyName = GetMemberName(propertyExpression.Body);
            if (propertyName == null)
            {
                throw new ArgumentException(
                    "The expression is not a property expression.",
                    paramName: nameof(propertyExpression));
            }

            Func<TComponent, TProperty> compiled = propertyExpression.Compile();

            IObservable<TComponent> source =
                from e in Observable.FromEventPattern<PropertyChangedEventArgs>(
                    component, nameof(component.PropertyChanged), scheduler)
                where e.EventArgs.PropertyName == propertyName
                select component;

            return Observable
                .Start(() => compiled.Invoke(component), scheduler)
                .Concat(source.Select(compiled));
        }

        public static IObservable<TProperty> Observe<TComponent, TProperty>(
            this TComponent component,
            Expression<Func<TComponent, TProperty>> propertyExpression,
            IScheduler scheduler)
            where TComponent : INotifyPropertyChanged
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));
            if (propertyExpression == null)
                throw new ArgumentNullException(nameof(propertyExpression));
            if (scheduler == null)
                throw new ArgumentNullException(nameof(scheduler));

            return ObserveImpl(component, propertyExpression, scheduler);
        }

        public static IObservable<TProperty> Observe<TComponent, TProperty>(
            this TComponent component,
            Expression<Func<TComponent, TProperty>> propertyExpression)
            where TComponent : INotifyPropertyChanged =>
            Observe(component, propertyExpression, Scheduler.Default);

        public static IObservable<TResult> Observe
            <TComponent, TProperty, TResult>(
            this TComponent component,
            Expression<Func<TComponent, TProperty>> propertyExpression,
            Func<TProperty, TResult> selector,
            IScheduler scheduler)
            where TComponent : INotifyPropertyChanged
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));
            if (propertyExpression == null)
                throw new ArgumentNullException(nameof(propertyExpression));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));
            if (scheduler == null)
                throw new ArgumentNullException(nameof(scheduler));

            IObservable<TProperty> source = ObserveImpl(
                component, propertyExpression, scheduler);
            return source.Select(selector);
        }

        public static IObservable<TResult> Observe
            <TComponent, TProperty, TResult>(
            this TComponent component,
            Expression<Func<TComponent, TProperty>> propertyExpression,
            Func<TProperty, TResult> selector)
            where TComponent : INotifyPropertyChanged =>
            Observe(component, propertyExpression, selector, Scheduler.Default);
    }
}
