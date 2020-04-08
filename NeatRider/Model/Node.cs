using System;
using System.Collections.Generic;

namespace NeatRider.Model
{
    public class Node : Gene
    {
        public Node(int innovationNumber)
        {
            this.innovationNumber = innovationNumber;
            this.incomingConnections = new List<Connection>();
        }

        public double X { get; set; }
        public double Y { get; set; }

        public List<Connection> incomingConnections;

        public double outputSum;


        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            return innovationNumber == ((Node) obj).innovationNumber;
        }

        public override int GetHashCode()
        {
            return innovationNumber;
        }

        public Node clone()
        {
            var n = new Node(innovationNumber);
            n.X = X;
            n.Y = Y;
            n.incomingConnections = new List<Connection>(this.incomingConnections);
            return n;
        }

        public double calculate()
        {
            if (this.X > .1)
            {
                var runningSum = 0d;
                foreach (var con in incomingConnections)
                {
                    if (!con.enabled) continue;
                    runningSum += con.from.calculate() * con.weight;
                }

                outputSum = Math.Tanh(runningSum);
            }
            //outputSum = (X == .9) ? sigmoid(outputSum) : outputSum;
            return outputSum;

        }

        public double sigmoid(double x)
        {
            return 1d / (1d + Math.Pow(Math.E, -x));
        }

        public override string ToString()
        {
            return "Node: " + innovationNumber + string.Format(" ( {0:0.0}, {1:0.0})", X, Y);
        }
    }
}