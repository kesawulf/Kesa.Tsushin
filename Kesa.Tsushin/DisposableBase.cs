using System;
using System.Threading;

namespace Kesa.Tsushin
{
    public abstract class DisposableBase
    {
        private int _disposedValue;

        public bool IsDisposed => _disposedValue == 1;

        public void Dispose()
        {
            DisposeInternal(true);
        }

        private void DisposeInternal(bool disposeManagedObjects)
        {
            if (Interlocked.CompareExchange(ref _disposedValue, 1, 0) == 0)
            {
                GC.SuppressFinalize(this);
                OnDisposing(disposeManagedObjects);
            }
        }

        protected abstract void OnDisposing(bool disposeManagedObjects);
    }
}
