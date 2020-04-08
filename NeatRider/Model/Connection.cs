using System;

namespace NeatRider.Model
{
    public class Connection : Gene, IComparable<Connection>
    {
        public int SplitIndex;

        public Connection(Node from, Node to)
        {
            weight = 1;
            enabled = true;
            SplitIndex = 0;
            this.from = from;
            this.to = to;
        }

        public Node to { get; set; }
        public Node from { get; set; }
        public double weight { get; set; }
        public bool enabled { get; set; }

        public int CompareTo(Connection other)
        {
            if (innovationNumber < other.innovationNumber) return -1;
            return innovationNumber > other.innovationNumber ? 1 : 0;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != GetType()) return false;
            var other = (Connection) obj;
            return from.innovationNumber == other.from.innovationNumber && to.innovationNumber == other.to.innovationNumber;
        }

        public override int GetHashCode()
        {
            return from.innovationNumber * Config.MAX_GENES + to.innovationNumber;
        }

        public Connection clone()
        {
            var c = new Connection(from, to);
            c.enabled = enabled;
            c.weight = weight;
            c.SplitIndex = SplitIndex;
            return c;
        }

        public override string ToString()
        {
            return innovationNumber + ": " + from.innovationNumber + " -> " + to.innovationNumber;
        }
    }
}