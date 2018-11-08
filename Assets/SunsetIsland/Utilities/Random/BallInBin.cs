using System;
using System.Collections.Generic;

namespace Assets.SunsetIsland.Utilities.Random
{
    internal class BallInBin<T>
    {
        private readonly List<T> m_bins;

        private readonly Func<T, float> m_probabilityFunction;

        //TODO : since this object is stateful, it will likely need a serialize function
        private readonly FastRandom m_random;

        private Dictionary<T, float> m_probabilities;

        public BallInBin(IEnumerable<T> items, Func<T, float> probabilityFunction, uint seed)
            : this(items, probabilityFunction, new FastRandom(seed))
        {
        }

        public BallInBin(IEnumerable<T> items, Func<T, float> probabilityFunction, FastRandom random)
        {
            m_random = random;
            m_bins = new List<T>(items);
            m_probabilityFunction = probabilityFunction;
            UpdateProbabilities();
        }

        public T Toss()
        {
            var randomRoll = m_random.NextDouble();
            double cumulativePercentages = 0;
            foreach (var element in m_bins)
            {
                cumulativePercentages += m_probabilities[element];
                if (cumulativePercentages >= randomRoll)
                    return element;
            }
            return m_bins[m_bins.Count - 1];
        }

        public void UpdateProbabilities()
        {
            m_probabilities = new Dictionary<T, float>();
            float total = 0;
            foreach (var element in m_bins)
            {
                var prob = m_probabilityFunction(element);
                total += prob;
                m_probabilities.Add(element, prob);
            }
            foreach (var element in m_bins)
                m_probabilities[element] = m_probabilities[element] / total;
            m_bins.Sort((x, y) => (int) (m_probabilities[y] - m_probabilities[x]));
        }
    }
}