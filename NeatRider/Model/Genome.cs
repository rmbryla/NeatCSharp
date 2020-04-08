using System;
using System.Collections.Generic;
using System.Linq;

namespace NeatRider.Model
{
    public class Genome
    {
        public readonly Neat neat;
        public List<Connection> Connections;

        public Dictionary<int, Node> Nodes;

        private readonly Random random = new Random();

        public double score = 0;

        public int inputSize;
        public int outputSize;

        public Genome(Neat neat, int inputSize, int outputSize)
        {
            this.inputSize = inputSize;
            this.outputSize = outputSize;
            this.neat = neat;
            Nodes = new Dictionary<int, Node>();
            Connections = new List<Connection>();
        }


        public static Genome crossover(Genome g1, Genome g2)
        {
            if (g2.score > g1.score) // make genome with better score g1
            {
                var temp = g1;
                g1 = g2;
                g2 = temp;
            }

            var random = new Random();
            var genome = g1.neat.emptyGenome();
            var p1 = 0;
            var p2 = 0;

            while (p1 < g1.Connections.Count && p2 < g2.Connections.Count)
            {
                var innovation1 = g1.Connections[p1].innovationNumber;
                var innovation2 = g2.Connections[p2].innovationNumber;
                if (innovation1 == innovation2)
                {
                    genome.Connections.Add(random.NextDouble() < .5
                        ? g1.Connections[p1].clone()
                        : g2.Connections[p2].clone());
                    p1++;
                    p2++;
                }
                else if (innovation2 > innovation1)
                {
                    genome.Connections.Add(g1.Connections[p1].clone());
                    p1++;
                }
                else // g2 has gene that g1 doesnt
                {
                    p2++;
                }
            }

            while (p1 < g1.Connections.Count) genome.Connections.Add(g1.Connections[p1++]);

            foreach (var con in genome.Connections)
            {
                if (!genome.Nodes.ContainsKey(con.from.innovationNumber))
                    genome.Nodes.Add(con.from.innovationNumber, con.from);
                if (!genome.Nodes.ContainsKey(con.to.innovationNumber))
                    genome.Nodes.Add(con.to.innovationNumber, con.to);
            }

            return genome;
        }

        public double Distance(Genome other)
        {
            Connections.Sort();
            other.Connections.Sort();

            var g1 = this;
            var g2 = other;

            var highestG1num = (this.Connections.Count > 0) ? Connections[^1].innovationNumber : 0;
            var highestG2num = (other.Connections.Count > 0) ? other.Connections[^1].innovationNumber : 0;
            if (highestG2num > highestG1num)
            {
                g2 = this;
                g1 = other;
            }

            var disjoint = 0d;
            var weightDiff = 0d;
            var similar = 0d;

            var p1 = 0;
            var p2 = 0;

            while (p1 < g1.Connections.Count && p2 < g2.Connections.Count)
            {
                var innovation1 = g1.Connections[p1].innovationNumber;
                var innovation2 = g2.Connections[p2].innovationNumber;
                if (innovation1 > innovation2)
                {
                    disjoint++;
                    p1++;
                }
                else if (innovation2 > innovation1)
                {
                    disjoint++;
                    p2++;
                }
                else // equal case
                {
                    weightDiff += Math.Abs(g1.Connections[p1].weight - g2.Connections[p2].weight);
                    p1++;
                    p2++;
                }
            }

            var extra = (double) g1.Connections.Count - p1;

            var N = g1.Connections.Count < 20 ? 1 : g1.Connections.Count;

            var temp1 = Config.C1 * extra / N;
            var temp2 = Config.C2 * disjoint / N;
            var temp3 = Config.C3 * weightDiff;
            return temp1 + temp2 + temp3;
        }

        public double[] calculate(params double[] inputs)
        {
            
            updateConnectionReferences();
            foreach (var node in Nodes.Values)
            {
                node.incomingConnections.Clear();
            }

            Connections.ForEach(con => { Nodes[con.to.innovationNumber].incomingConnections.Add(con); });

            if (inputs.Length + 1 != inputSize) throw new Exception("Inputs given do not match input size");
            for (int i = 0; i < inputs.Length; i++)
            {
                Nodes[i + 1].outputSum = inputs[i];
            }

            Nodes[inputs.Length + 1].outputSum = 1;

            for (int i = inputSize; i < inputSize + outputSize; i++)
            {
                Nodes[i + 1].calculate();
            }

            double[] output = new double[outputSize];

            for (int i = 0; i < outputSize; i++)
            {
                output[i] = Nodes[inputSize + i + 1].outputSum;
            }

            resetOutputs();

            return output;
        }

        private void updateConnectionReferences()
        {
            foreach (var connection in Connections)
            {
                connection.from = Nodes[connection.from.innovationNumber];
                connection.to = Nodes[connection.to.innovationNumber];
            }
        }

        public void mutate()
        {
            if (Connections.Count == 0) mutateLink();
            if (random.NextDouble() < Config.MUTATE_LINK_PROB) mutateLink();
            if (random.NextDouble() < Config.MUTATE_NODE_PROB) mutateNode();
            if (random.NextDouble() < Config.SHIFT_WEIGHT_PROB) shiftWeight();
            if (random.NextDouble() < Config.RANDOM_WEIGHT_PROB) mutateRandomWeight();
            if (random.NextDouble() < Config.TOGGLE_PROB) toggleConnection();
        }

        public void mutateLink()
        {
            var nodeIndexes = new List<int>();
            foreach (var key in Nodes.Keys) nodeIndexes.Add(key);
            for (var i = 0; i < 100; i++)
            {
                var a = Nodes[nodeIndexes.getRandom()];
                var b = Nodes[nodeIndexes.getRandom()];
                if (a.X == b.X) continue;
                Connection connection;
                if (a.X < b.X) connection = neat.getConnection(a, b);
                else connection = neat.getConnection(b, a);
                if (Connections.Contains(connection)) continue;

                connection = connection.clone();

                connection.weight = (random.NextDouble() * 2 - 1) * Config.RANDOM_WEIGHT_STRENGTH;
                Connections.Add(connection);
                updateConnectionReferences();
                break;
            }
        }

        public void mutateNode()
        {
            var c = Connections.getRandom();
            if (c == null) return;
            var splitIndex = neat.getSplitIndex(c.from, c.to);
            if (Nodes.ContainsKey(splitIndex)) return;
            Node middle;
            if (splitIndex == 0)
            {
                middle = neat.getNode();
                neat.setSplitIndex(c.from, c.to, middle.innovationNumber);
            }
            else
            {
                middle = neat.getNode(splitIndex);
            }


            var c1 = neat.getConnection(c.from, middle).clone();
            var c2 = neat.getConnection(middle, c.to).clone();
            c2.weight = c.weight;
            middle.X = (c.from.X + c.to.X) / 2;
            middle.Y = (c.from.Y + c.to.Y) / 2; // + random.NextDouble() * .4 - .2;

            middle = middle.clone();


            // middle.incomingConnections.Add(c1);
            // c.to.incomingConnections.Add(c2);

            Nodes.Add(middle.innovationNumber, middle);
            Connections.Add(c1);
            Connections.Add(c2);
            updateConnectionReferences();
        }

        public void shiftWeight()
        {
            var c = Connections.getRandom();
            if (c == null) return;
            c.weight += (new Random().NextDouble() - .5) * Config.SHIFT_WEIGHT_STRENGTH;
        }

        public void mutateRandomWeight()
        {
            var c = Connections.getRandom();
            if (c == null) return;
            c.weight = new Random().NextDouble();
        }

        public void toggleConnection()
        {
            var c = Connections.getRandom();
            if (c == null) return;
            c.enabled = !c.enabled;
        }

        public void resetOutputs()
        {
            foreach (var node in Nodes.Values)
            {
                node.outputSum = 0;
            }
        }

        public override string ToString()
        {
            return string.Format("(N: {0}, C:{1})", Nodes.Count, Connections.Count);
        }

        public Genome clone()
        {
            var g = neat.emptyGenome();
            foreach (var con in Connections)
            {
                g.Connections.Add(con.clone());
            }

            foreach (var node in Nodes.Values)
            {
                if (!g.Nodes.ContainsKey(node.innovationNumber)) g.Nodes.Add(node.innovationNumber, node.clone());
            }

            return g;
        }
}


    public static class listExt
    {
        public static T getRandom<T>(this List<T> list)
        {
            if (list.Count == 0) return default;
            var random = new Random();
            var index = (int) (random.NextDouble() * list.Count);
            return list[index];
        }
    }
}