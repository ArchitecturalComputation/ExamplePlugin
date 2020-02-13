using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using static System.Math;

namespace ExamplePlugin
{
    class OptimizerB
    {
        public Polyline Path { get; private set; }
        public string Info { get; private set; }

        Random _random = new Random(42);
        int _timeLimit;
        Point3d[] _cities;
        int[] _order;
        double _currentFitness;
        int _betterCandidateCount;
        int _iterations;
        public Action UpdateCandidate;
        public Action Finished;

        public OptimizerB(int cities, int timeLimit = 10, double size = 10.0)
        {
            _timeLimit = timeLimit * 1000;
            _cities = RandomCities(cities, size).ToArray();
            _order = Enumerable.Range(0, cities).ToArray();
            _currentFitness = Fitness(_order);
            UpdateDisplay();
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
            Info = $"Fitness: {_currentFitness:0.0} ({++_betterCandidateCount}/{_iterations})";
        }

        void UpdateCanditate()
        {
            UpdateDisplay();
            UpdateCandidate.Invoke();
        }

        double Fitness(int[] candidate)
        {
            double totalDistance = 0;

            for (int i = 0; i < candidate.Length - 1; i++)
            {
                int index = candidate[i];
                int next = candidate[i + 1];
                var distance = _cities[index].DistanceTo(_cities[next]);
                totalDistance += distance;
            }

            return totalDistance;
        }

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

        public void RandomSearch()
        {
            _currentFitness = Fitness(_order);

            var watch = Stopwatch.StartNew();

            while (watch.ElapsedMilliseconds < _timeLimit)
            {
                Shuffle(_order);
                double fitness = Fitness(_order);

                if (fitness < _currentFitness)
                {
                    _currentFitness = fitness;
                    UpdateCanditate();
                }

                _iterations++;
            }

            Finished.Invoke();
        }

        public void HillClimbing()
        {
            _currentFitness = Fitness(_order);

            var watch = Stopwatch.StartNew();

            while (watch.ElapsedMilliseconds < _timeLimit)
            {
                int a = _random.Next(0, _order.Length);
                int b = _random.Next(0, _order.Length);
                Swap(ref _order[a], ref _order[b]);

                double fitness = Fitness(_order);

                if (fitness < _currentFitness)
                {
                    _currentFitness = fitness;
                    UpdateCanditate();
                }
                else
                {
                    Swap(ref _order[a], ref _order[b]);
                }

                _iterations++;
            }

            Finished.Invoke();
        }

        public void SimulatedAnnealing()
        {
            const double s = 50;    //start temperature
            const double f = 0.001; //final temperature
            long t;
            var l = (double)_timeLimit;

            var watch = Stopwatch.StartNew();
            while ((t = watch.ElapsedMilliseconds) < _timeLimit)
            {
                int a = _random.Next(0, _order.Length);
                int b = _random.Next(0, _order.Length);
                Swap(ref _order[a], ref _order[b]);

                double fitness = Fitness(_order);

                bool acceptCandidate = fitness < _currentFitness;

                if(!acceptCandidate)
                {
                    var d = _currentFitness - fitness;
                    var p = Exp(d * Pow(f / s, -t / l) / s);
                    acceptCandidate = p > _random.NextDouble();
                }

                if (acceptCandidate)
                {
                    _currentFitness = fitness;
                    UpdateCanditate();
                }
                else
                {
                    Swap(ref _order[a], ref _order[b]);
                }

                _iterations++;
            }

            UpdateDisplay();
            Finished.Invoke();
        }
    }
}
