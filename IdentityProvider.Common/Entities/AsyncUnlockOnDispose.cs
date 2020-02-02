using System;

namespace IdentityProvider.Common.Entities
{
    public class AsyncUnlockOnDispose : IDisposable
    {
        private readonly Action _unlock;

        public AsyncUnlockOnDispose(Action unlock)
        {
            this._unlock = unlock;
        }

        public void Dispose()
        {
            this._unlock();
        }
    }
}
