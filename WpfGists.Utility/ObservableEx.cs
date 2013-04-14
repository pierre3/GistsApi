using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace WpfGists.Utility
{
  public static class ObservableEx
  {
    public static IObservable<T> Create<T>(Func<IObserver<T>,CancellationToken, Task> subscribe)
    {
      return Observable.Create<T>(observer =>
      {
        var cts = new CancellationTokenSource();
        var task = subscribe(observer,cts.Token);
        var subscription = task.ToObservable().Subscribe(_ => { }, observer.OnError, observer.OnCompleted);
        return new CompositeDisposable(subscription,new CancellationDisposable(cts));
      });
    }

    public static IObservable<T> Create<T>(Func<IObserver<T>, CancellationToken, Task> subscribe,CancellationTokenSource cts)
    {
      return Observable.Create<T>(observer =>
      {
        var task = subscribe(observer, cts.Token);
        var subscription = task.ToObservable().Subscribe(_ => { }, observer.OnError, observer.OnCompleted);
        return new CompositeDisposable(task, subscription, new CancellationDisposable(cts));
      });
    }
  }

}
