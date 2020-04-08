using System;

namespace NeatRider.Model
{
    public class Client : IComparable<Client>
    {
        public Species species = null;

        public Genome Genome { get; set; }
        public double score { get; set; }

        public int id;

        public Client(int id)
        {
            this.id = id;
        }

        public double getFitness()
        {
            // Random random = new Random();
            // this.score = Genome.calculate(random.NextDouble(), random.NextDouble())[0];
            // return this.score;
            var fitness = 0d;
            //if (Genome.Connections.Count > 2) Console.WriteLine("he");

            var output = this.Genome.calculate(0, 0)[0];
            fitness += (1 - (output + 1) / 2);
            
            output = this.Genome.calculate(0, 1)[0];
            fitness += output;
            
            output = this.Genome.calculate(1, 0)[0];
            fitness += output;
            
            output = this.Genome.calculate(1, 1)[0];
            fitness += (1 - (output + 1) / 2);

            //if (fitness < 2) fitness = 0;
            //else
            {
                //fitness -= 2;
                fitness /= 4;
            }


            this.score = fitness;
            return fitness;
        }

        public void printGuesses()
        {
            Console.WriteLine("0, 0: {0:0.0000}", this.Genome.calculate(0, 0)[0]);
            Console.WriteLine("0, 1: {0:0.0000}", this.Genome.calculate(0, 1)[0]);
            Console.WriteLine("1, 0: {0:0.0000}", this.Genome.calculate(1, 0)[0]);
            Console.WriteLine("1, 1: {0:0.0000}", this.Genome.calculate(1, 1)[0]);

        }


        public double distance(Client other)
        {
            return other.Genome.Distance(this.Genome);
        }

        public int CompareTo(Client other)
        {
            if (this.score < other.score) return -1;
            return (this.score == other.score) ? 0 : 1;
        }

        public override string ToString()
        {
            return id + ": G: " + Genome.ToString() + String.Format("    {0:0.000000}", this.score);
        }
    }
}