using System;

namespace FluentMigrator.Infrastructure {
    public class Gate : IGate {
        public DateTime? Start { get; private set; }
        public DateTime? End { get; private set; }

        public Gate(DateTime? start, DateTime? end) {
            Start = start;
            End = end;
        }
    }
}