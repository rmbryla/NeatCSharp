using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NeatRider.Model
{
    public class Species
    {
        public List<Client> clients;
        public Client representative;

        public int staleness;

        public double averageScore = 0d;
        public int id;
        
        public Species(Client representative, int id)
        {
            this.id = id;
            this.staleness = 0;
            this.representative = representative;
            representative.species = this;
            this.clients = new List<Client>();
            clients.Add(representative);
        }

        public double evaluateSpecies()
        {
            var total = 0d;
            foreach (var client in clients)
            {
                if (client.Genome != null) total += client.score;
            }

            if ((total / clients.Count) < averageScore) staleness++;
            else staleness = 0;
            
            averageScore = total / clients.Count;
            return averageScore;
        }

        public void goExtinct()
        {
            //this.representative.species = null;
            clients.ForEach(client => client.species = null);
            //clients.Clear();
        }

        public void add(Client client)
        {
            this.clients.Add(client);
            client.species = this;
        }

        public void reset()
        {
            clients.Sort();
            representative = clients[^1];
            clients.ForEach(client => client.species = null);
            clients.Clear();
            
            clients.Add(representative);
            representative.species = this;
            //averageScore = -1;
        }
        public bool inSpecies(Client client)
        {
            if (client.distance(representative) < Config.IN_SPECIES_THRESHOLD)
            {
                clients.Add(client);
                client.species = this;
                if (client.score > representative.score) representative = client;
                return true;
            }

            return false;
        }


        public void kill(double killPercent)
        {
            clients.Sort();
            List<Client> removed = clients.Where(((client, i) => i < clients.Count * (killPercent))).ToList();
            clients = clients.Where((client, i) => i >= clients.Count * (killPercent)).ToList();
            removed.ForEach(client => client.species = null);
            clients.Sort();
            if (clients.Count >= 1) this.representative = clients[^1];
        }

        public int size()
        {
            return this.clients.Count;
        }

        public void adjustFitness()
        {
            foreach (var cl in clients)
            {
                cl.score /= clients.Count;
            }
        }
        
    }

    public static class extSp
    {
        public static Species getRandomWeighted(this List<Species> list)
        {
            var totalScore = 0d;
            foreach (Species s in list)
            {
                totalScore += s.averageScore;
            }

            var threshold = (new Random()).NextDouble() * totalScore;
            var runningTotal = 0d;
            foreach (Species s in list)
            {
                runningTotal += s.averageScore;
                if (runningTotal >= threshold) return s;
            }

            return null;
        }
        
        public static Client getRandomWeighted(this List<Client> list)
        {
            var totalScore = 0d;
            foreach (Client s in list)
            {
                totalScore += s.score;
            }

            var threshold = (new Random()).NextDouble() * totalScore;
            var runningTotal = 0d;
            foreach (Client s in list)
            {
                runningTotal += s.score;
                if (runningTotal >= threshold) return s;
            }

            return null;
        }
    }
}