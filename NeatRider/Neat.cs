using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NeatRider.Model;
using NeatRider.Visual;

namespace NeatRider
{
    public class Neat
    {

        public static void Main(string[] args)
        {
            Neat neat = new Neat(2, 1, Config.NUM_CLIENTS);
            MyWindow window = new MyWindow();
            //window.setNeat(neat);
            neat.window = window;
            neat.train(Config.ITERATIONS);
            //Application.Run(window);
        }
        public Random random = new Random();

        public bool redrawGenome = false;
        public int speciesIds = 1;
        public MyWindow window;
        
        public Dictionary<int, Connection> allConnections = new Dictionary<int, Connection>();
        public Dictionary<int, Node> allNodes = new Dictionary<int, Node>();
        
        public List<Client> clients = new List<Client>();
        public List<Species> species = new List<Species>();
        
        public int inputs { get; }
        public int outputs;

        public Neat(int inputs, int outputs, int numClients)
        {

            this.inputs = inputs + 1;
            this.outputs = outputs;


            for (int i = 0; i < this.inputs; i++)
            {
                var node = getNode();
                node.X = .1;
                node.Y = (double) (i + 1) / (this.inputs + 1);
            }

            for (int i = 0; i < this.outputs; i++)
            {
                var node = getNode();
                node.X = .9;
                node.Y = (double) (i + 1) / (this.outputs + 1);
            }

            for (int i = 0; i < numClients; i++)
            {
                var c = new Client(i+1);
                c.Genome = emptyGenome();
                clients.Add(c);
            }
            
            clients.ForEach(client => client.Genome.mutate());
            
        }

        public void resetOutputs()
        {
            foreach (var client in clients)
            {
                client.Genome.resetOutputs();
            }
        }

        public Client getBest()
        {
            clients.Sort();
            return clients[^1];
        }
        
        public void generateSpecies()
        {
            species.ForEach(species1 => species1.reset());
            
            foreach (var client in clients)
            {
                if (client.species != null) continue;
                bool found = false;
                foreach (var s in species)
                {
                    if (s.inSpecies(client))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    species.Add(new Species(client, this.speciesIds++));
                }
            }
        }

        public void kill()
        {
            foreach (var s in species)
            {
                s.kill(1 - Config.SURVIVORS);
                s.adjustFitness();
                s.evaluateSpecies();
            }
        }

        public void breed()
        {
            foreach (var client in clients)
            {
                if (client.species != null) continue;
                Species s = this.species.getRandomWeighted();
                var c1 = s.clients.getRandomWeighted();
                var c2 = s.clients.getRandomWeighted();
                if (random.NextDouble() < .2) client.Genome = (c1.score > c2.score) ? c1.Genome.clone() : c2.Genome.clone();
                else if (c1.score > c2.score) client.Genome = Genome.crossover(c1.Genome, c2.Genome);
                else client.Genome = Genome.crossover(c2.Genome, c1.Genome);
                client.Genome.mutate();
                s.add(client);
            }
        }

        public void printSpecies(int i)
        {
            Console.WriteLine("##########################: " + i);
            foreach (var s in species)
            {
                Console.WriteLine(s + ": " + s.id + "\t" + s.averageScore + "\t" + s.size());
            }

            var best = getBest();
            var rep = best.species.representative;
            Console.WriteLine("Best: " + best.id  + "\tSpecies: " + best.species.id + "\t" + best.score);
            //Console.WriteLine("Best species representative: " + rep.id + "\t" + rep.score);
        }

        public void removeExtinct()
        {
            List<Species> removed = species.Where((species1 => species1.size() <= 1)).ToList();
            species = species.Where(species1 => species1.size() > 1).ToList();
            foreach (var s in removed)
            {
                //Console.WriteLine("removing species: " + s.id);
                if (s.size() <= 1) s.goExtinct();
            }
        }

        public void killStaleSpecies()
        {
            var toRemove = species.Where(species1 => species1.staleness >= 15).ToList();
            species = species.Where(species1 => species1.staleness < 15).ToList();
            foreach (var speciese in toRemove)
            {
                speciese.goExtinct();
                Console.WriteLine("Stale: " + speciese.id);
            }
        }

        public void train(int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                foreach (var client in clients) client.getFitness();
                
                generateSpecies();
                //species.ForEach(species1 => species1.evaluateSpecies());
                kill();
                killStaleSpecies();
                removeExtinct();
                breed();
                clients.ForEach(client => client.Genome.mutate());
                printSpecies(i);

                var best = getBest();
                best.getFitness();
                if (best.score > 0) best.printGuesses();
                if (best.score > .9)
                {
                    best.getFitness();
                    Console.WriteLine("Best: " + best.id);
                    best.printGuesses();
                    break;
                }
                window.genome = best.Genome;
                window.drawGenome();
                this.redrawGenome = false;
                
            }

            window.genome = getBest().Genome;
            
            Application.Run(window);
        }
        
        public Node getNode()
        {
            var node = new Node(allNodes.Count+1);
            allNodes.Add(allNodes.Count+1, node);
            return node;
        }
        public Node getNode(int innovationNumber)
        {
            if (allNodes.ContainsKey(innovationNumber))
            {
                return allNodes[innovationNumber].clone();
            }
            else
            {
                return getNode();
            }
            
        }

        public Connection getConnection(Node from, Node to)
        {
            Connection c = new Connection(from, to);
            if (allConnections.ContainsKey(c.GetHashCode()))
            {
                c.weight = 1;
                c.innovationNumber = allConnections[c.GetHashCode()].innovationNumber;
                c.enabled = true;
                return c;
            }
            c.innovationNumber = allConnections.Count + 1;
            allConnections.Add(c.GetHashCode(), c);
            return c;
        }
        

        public int getSplitIndex(Node from, Node to)
        {
            Connection c = new Connection(from, to);
            if (allConnections.ContainsKey(c.GetHashCode()))
            {
                return allConnections[c.GetHashCode()].SplitIndex;
            }
            throw new Exception("Connection does not exists");
        }

        public void setSplitIndex(Node from, Node to, int index)
        {
            Connection c = new Connection(from, to);
            if (allConnections.ContainsKey(c.GetHashCode()))
            {
                allConnections[c.GetHashCode()].SplitIndex = index;
                return;
            }
            throw new Exception("Connection does not exits");
        }

        public Genome emptyGenome()
        {
            var genome = new Genome(this, inputs, outputs);
            
            for (int i = 0; i < inputs + outputs; i++)
            {
                genome.Nodes.Add(i+1, getNode(i+1).clone());
            }
            
            return genome;
        }

    }
}