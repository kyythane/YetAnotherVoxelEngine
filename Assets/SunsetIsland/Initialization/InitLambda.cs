using System;
using System.Collections;

namespace Assets.SunsetIsland.Initialization
{
    public class InitLambda : IInitStep
    {
        private readonly IEnumerator _routine;

        private InitLambda(string name, IEnumerator routine)
        {
            Name = name;
            _routine = routine;
        }

        public string Name { get; }

        public IEnumerator Execute()
        {
            return _routine;
        }

        public static IInitStep Action(string name, Action action)
        {
            if (null == action)
                throw new ArgumentException("action cannot be null");

            return new InitLambda(name, WithAction(action));
        }

        private static IEnumerator WithAction(Action action)
        {
            action();
            yield return null;
        }
    }
}