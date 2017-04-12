using System;

namespace FluentMigrator.Infrastructure {
    public class Gate : IGate {
        public DateTime? Start { get; private set; }
        public DateTime? End { get; private set; }

        public bool IsOpen {
            get {
                var now = DateTime.Now;

                var hasStartAndEnd = Start.HasValue && End.HasValue;
                var startOk = Start.HasValue && now >= Start.Value;
                var endOk = End.HasValue && now <= End.Value;

                return hasStartAndEnd ? startOk & endOk :
                                        startOk || endOk;
            }
        }

        public void SetGate(DateTime? start, DateTime? end) {
            Start = start;
            End = end;
        }
    }
}