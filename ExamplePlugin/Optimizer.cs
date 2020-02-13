using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using static System.Math;

namespace ExamplePlugin
{
    class Optimizer
    {
        public Polyline Path { get; private set; }
        public string Info { get; private set; } = "";
        public event EventHandler UpdatedCandidate;
        public event EventHandler Finished;

        readonly Random _random = new Random(42);
        readonly Point3d[] _locations;
        readonly int[] _candidate;
        readonly int _timeLimit;
        double _currentFitness;
        int _selectedCount;
        int _iterationCount;

        public Optimizer(int cityCount, int seconds)
        {
            _timeLimit = seconds * 1000;
            _locations = RandomCities(cityCount).ToArray();
            _candidate = Enumerable.Range(0, _locations.Length).ToArray();
            _currentFitness = Distance(_candidate);
            Path = new Polyline(_locations);
        }

        IEnumerable<Point3d> RandomCities(int count)
        {
            double size = 10;

            while (count-- > 0)
            {
                var point = new Point3d(_random.NextDouble() * size, _random.NextDouble() * size, 0);
                yield return point;
            }
        }

        void Update()
        {
            Path = new Polyline(_candidate.Select(i => _locations[i]));            
            Info = $"Distance: {_currentFitness:0.00} - Iteration: {_selectedCount++}/{_iterationCount}";
            UpdatedCandidate?.Invoke(this, EventArgs.Empty);
            //System.Threading.Thread.Sleep(8);
        }

        void Completed()
        {
            Info = $"Distance: {_currentFitness:0.00} - Iteration: {_selectedCount}/{_iterationCount}";
            Finished?.Invoke(this, EventArgs.Empty);
        }

        double Distance(int[] candidate)
        {
            double distance = 0;

            for (int i = 0; i < candidate.Length - 1; i++)
            {
                int a = candidate[i];
                int b = candidate[i + 1];
                distance += _locations[a].DistanceTo(_locations[b]);
            }

            return distance;
        }

        void Swap<T>(ref T a, ref T b)
        {
            var temp = a;
            a = b;
            b = temp;
           // (a, b) = (b, a);
        }

        void Shuffle(int[] list)
        {
            for (int i = list.Length - 1; i >= 1; i--)
            {
                int j = _random.Next(i + 1);
                Swap(ref list[i], ref list[j]);
            }
        }

        public void RandomSearch()
        {
            var watch = Stopwatch.StartNew();
            while (watch.ElapsedMilliseconds < _timeLimit)
            {
                _iterationCount++;
                Shuffle(_candidate);

                var fitness = Distance(_candidate);

                if (fitness < _currentFitness)
                {
                    _currentFitness = fitness;
                    Update();
                }
            }

            Completed();
        }

        public void HillClimbing()
        {
            int length = _locations.Length;

            var watch = Stopwatch.StartNew();
            while (watch.ElapsedMilliseconds < _timeLimit)
            {
                _iterationCount++;

                int a = _random.Next(0, length);
                int b = _random.Next(0, length);
                Swap(ref _candidate[a], ref _candidate[b]);

                var fitness = Distance(_candidate);

                if (fitness < _currentFitness)
                {
                    _currentFitness = fitness;
                    Update();
                }
                else
                {
                    Swap(ref _candidate[a], ref _candidate[b]);
                }
            }

            Completed();
        }

        public void SimulatedAnnealing()
        {
            int length = _locations.Length;

            const double s = 50;    //start temperature
            const double f = 0.001; //final temperature

            var l = (double)_timeLimit;

            var watch = Stopwatch.StartNew();
            long t;
            while ((t = watch.ElapsedMilliseconds) < _timeLimit)
            {
                _iterationCount++;

                int a = _random.Next(0, length);
                int b = _random.Next(0, length);
                Swap(ref _candidate[a], ref _candidate[b]);

                var fitness = Distance(_candidate);
                bool shouldAccept = fitness < _currentFitness;

                if (!shouldAccept)
                {
                    var d = _currentFitness - fitness;
                    var p = Exp(d * Pow(f/s, -t/l)/s);

                    shouldAccept = p > _random.NextDouble();
                }

                if (shouldAccept)
                {
                    _currentFitness = fitness;
                    Update();
                }
                else
                {
                    Swap(ref _candidate[a], ref _candidate[b]);
                }
            }

            Completed();
        }
    }
}
