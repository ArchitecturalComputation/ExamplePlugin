using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamplePlugin
{
    class OptimizerB
    {
        public Polyline Path { get; private set; }
        public string Info { get; private set; }

        Random _random = new Random(42);
        Point3d[] _cities;
        int[] _order;

        public OptimizerB(int cities, double size = 10.0)
        {
            _cities = RandomCities(cities, size).ToArray();
            _order = Enumerable.Range(0, cities).ToArray();
        }

        IEnumerable<Point3d> RandomCities(int count, double size)
        {
            while (count-- > 0)
            {
                var point = new Point3d(_random.NextDouble() * size, _random.NextDouble() * size, 0);
                yield return point;
            }
        }

        void UpdateDisplay()
        {
            Path = new Polyline(_order.Select(i => _cities[i]));
        }

        double Fitness(int[] candidate)
        {
            double totalDistance = 0;

            for (int i = 0; i < candidate.Length - 1; i++)
            {
                int index = candidate[i];
                int next = candidate[i + 1];
                var distance = _cities[index].DistanceToSquared(_cities[next]);
                totalDistance += distance;
            }

            return totalDistance;
        }

        //void Suffle(int[] candidate)
        //{
        //    Array.Sort(candidate, new Comparison<int>((i, j) => _random.Next(0, 1) * 2 - 1));
        //    var shuffled = candidate.OrderBy(i => _random.NextDouble()).ToArray();
        //}

        void Shuffle(int[] list)
        {
            for (int i = list.Length - 1; i >= 1; i--)
            {
                int j = _random.Next(i + 1);
                Swap(ref list[i], ref list[j]);
            }
        }

        void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        void RandomSearch()
        {
            int count = 1000;
            double currentFitness = Fitness(_order);

            while (count-- > 0)
            {

            }
        }
    }
}
