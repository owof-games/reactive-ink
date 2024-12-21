using System;
using R3;

namespace ReactiveInk
{
    /// <summary>
    ///     Pipe to add an observable to a <see cref="CompositeObservable{T}" />.
    /// </summary>
    /// <typeparam name="T">The type of values in the observable.</typeparam>
    public interface ICompositeObservablePipe<T>
    {
        /// <summary>
        ///     Pipes the contents of an observable in the composite observable.
        /// </summary>
        /// <param name="newStream">The observable to pipe in the composite observable.</param>
        void PipeIn(Observable<T> newStream);
    }

    /// <summary>
    ///     A composite observable is the result of merging multiple observables into one. This is used to have an
    ///     observable that starts by producing no items, and can get new ones at runtime, without having to detach or
    ///     reattach to a new observable.
    /// </summary>
    /// <typeparam name="T">The type of items in the observable.</typeparam>
    public class CompositeObservable<T> : IDisposable
    {
        private readonly Subject<Observable<T>> _observableStream = new();

        // create a new composite observable, and saves the resulting composed observable in the given variable
        public CompositeObservable()
        {
            ComposedObservable = _observableStream.Merge();
            CompositeObservablePipe = new CompositeObservablePipeImpl(this);
        }

        /// <summary>
        ///     The resulting composed observable.
        /// </summary>
        public Observable<T> ComposedObservable { get; }

        /// <summary>
        ///     The pipe where new observables can be added in.
        /// </summary>
        public ICompositeObservablePipe<T> CompositeObservablePipe { get; }

        public void Dispose()
        {
            _observableStream?.Dispose();
        }

        private readonly struct CompositeObservablePipeImpl : ICompositeObservablePipe<T>
        {
            private readonly CompositeObservable<T> _compositeObservable;

            public CompositeObservablePipeImpl(CompositeObservable<T> compositeObservable)
            {
                _compositeObservable = compositeObservable;
            }

            public void PipeIn(Observable<T> newStream)
            {
                _compositeObservable._observableStream.OnNext(newStream);
            }
        }
    }
}