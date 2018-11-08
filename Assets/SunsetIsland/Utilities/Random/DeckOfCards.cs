using System.Collections.Generic;
using System.Linq;

namespace Assets.SunsetIsland.Utilities.Random
{
    internal class DeckOfCards<T>
    {
        protected readonly Stack<T> m_discard;

        //TODO : since this object is stateful, it will likely need a serialize function
        protected readonly FastRandom m_random;

        protected Stack<T> m_deck;

        public DeckOfCards(IEnumerable<T> items, uint seed, int iterationsPerShuffle = 1, bool autoShuffle = true)
        {
            m_random = new FastRandom(seed);
            m_discard = new Stack<T>();
            m_deck = new Stack<T>(items);
            AutoShuffle = autoShuffle;
            IterationsPerShuffle = iterationsPerShuffle;
            if (autoShuffle)
                Shuffle();
        }

        public int CardsRemaining => m_deck.Count();

        public bool AutoShuffle { get; set; }
        public int IterationsPerShuffle { get; set; }

        public T Draw()
        {
            var ret = default(T);
            if (m_deck.Count > 0)
            {
                ret = m_deck.Pop();
            }
            else if (AutoShuffle)
            {
                Shuffle();
                ret = m_deck.Pop();
            }
            m_discard.Push(ret);
            return ret;
        }

        public void Shuffle()
        {
            var list = m_deck.ToList();
            while (m_discard.Count > 0)
                list.Add(m_discard.Pop());
            for (var i = 0; i < IterationsPerShuffle; ++i)
            {
                var n = m_deck.Count;
                while (n > 1)
                {
                    n--;
                    var k = m_random.Next(n + 1);
                    var value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
            }
            m_deck = new Stack<T>(list);
        }
    }
}