namespace Logger
{
    internal class RWLocker : IDisposable
    {
        internal struct ReadLocker : IDisposable
        {
            private readonly ReaderWriterLockSlim locker;

            public ReadLocker(ReaderWriterLockSlim locker)
            {
                this.locker = locker;
                locker.EnterReadLock();
            }

            public void Dispose() => locker.ExitReadLock();
        }

        internal struct WriteLocker : IDisposable
        {
            private readonly ReaderWriterLockSlim locker;

            public WriteLocker(ReaderWriterLockSlim locker)
            {
                this.locker = locker;
                locker.EnterWriteLock();
            }

            public void Dispose() => locker.ExitWriteLock();
        }


        private readonly ReaderWriterLockSlim locker = new();

        public ReadLocker StartRead() => new ReadLocker(locker);
        public WriteLocker StartWrite() => new WriteLocker(locker);
        public void Dispose() => locker.Dispose();
    }
}
