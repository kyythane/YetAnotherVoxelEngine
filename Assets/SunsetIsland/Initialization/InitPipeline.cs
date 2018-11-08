using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.SunsetIsland.Initialization
{
    public class InitPipeline
    {
        public delegate void StepCompleteHandler(object sender, PipelineStatusArgs e);

        private readonly List<IInitStep> _completedSteps = new List<IInitStep>();
        private readonly DependencyGraph<IInitStep> _dependencyGraph;

        private readonly Runner _runner;
        private bool _hasStarted;

        private InitPipeline(DependencyGraph<IInitStep> dependencyGraph, Runner runner)
        {
            _dependencyGraph = dependencyGraph;
            _runner = runner;
        }

        public event StepCompleteHandler OnStepCompleted;

        public static InitPipeline Create(Action<DependencyGraph<IInitStep>> configure)
        {
            var graph = new DependencyGraph<IInitStep>();
            configure(graph);

            var gameObject = new GameObject("InitPipelineRunner");
            var runner = gameObject.AddComponent<Runner>();

            return new InitPipeline(graph, runner);
        }

        public void Run()
        {
            if (_hasStarted)
                throw new InvalidOperationException("Initialization already started and cannot be restarted");
            _hasStarted = true;
            RunSteps(_dependencyGraph.Roots);
        }

        private void RunSteps(IEnumerable<IInitStep> steps)
        {
            // If the step is not ready to execute now, it means that there
            // is another dependency chain that will get to this point with
            // all dependencies statisfied.
            foreach (var step in steps)
                if (IsReadyToExecute(step))
                    _runner.RunInCoroutine(RunStep(step));
        }

        private IEnumerator RunStep(IInitStep step)
        {
            yield return _runner.RunInCoroutine(step.Execute());

            MarkComplete(step);

            var nextSteps = _dependencyGraph.GetDependents(step);
            RunSteps(nextSteps);
        }

        private void MarkComplete(IInitStep step)
        {
            _completedSteps.Add(step);

            OnStepCompleted?.Invoke(this, GetStatusArgs(step));
        }

        private bool IsReadyToExecute(IInitStep step)
        {
            var dependencies = _dependencyGraph.GetDependencies(step);
            return dependencies.All(dep => _completedSteps.Contains(dep));
        }

        private PipelineStatusArgs GetStatusArgs(IInitStep step)
        {
            return new PipelineStatusArgs
            {
                CompletedStepName = step.Name,
                CompletedSteps = _completedSteps.Count,
                TotalSteps = _dependencyGraph.Size
            };
        }

        private class Runner : MonoBehaviour
        {
            public Coroutine RunInCoroutine(IEnumerator routine)
            {
                return StartCoroutine(routine);
            }
        }
    }
}