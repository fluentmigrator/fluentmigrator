#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

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

    public class StopWatch : IStopWatch
    {
        public static Func<DateTime> TimeNow = () => DateTime.Now;

        private DateTime _startTime;
        private DateTime _endTime;

        public void Start()
        {
            _startTime = TimeNow();
        }

        public void Stop()
        {
            _endTime = TimeNow();
        }

        public TimeSpan ElapsedTime()
        {
            return _endTime - _startTime;
        }

        public TimeSpan Time(Action action)
        {
            Start();

            action();

            Stop();

            return ElapsedTime();
        }
    }
}
