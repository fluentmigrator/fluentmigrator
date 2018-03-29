using System;

namespace FluentMigrator.Runner
{
    public interface IStopWatch
    {
        void Start();
        void Stop();
        TimeSpan ElapsedTime();
        TimeSpan Time(Action action);
    }
}
