using Assets.SunsetIsland.Game.Data;
using Assets.SunsetIsland.Managers;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Assets.SunsetIsland.Initialization
{
    //TODO : this way of loading does not work with the Job system, Needs a rewrite
    public class SimulationLoader
    {
        private readonly WorldSettings _settings;

        public SimulationLoader(WorldSettings settings)
        {
            _settings = settings;
        }

        //TODO : this run method is burried in callbacks for UI. Stack is _ugly_
        public void Run()
        {
            var initPipeline = InitPipeline.Create(graph =>
                                                   {
                                                       var generatorInitializer =
                                                           InitLambda.Action("InitializeGenerators",
                                                                             () =>
                                                                             {
                                                                                 GenerationManager
                                                                                     .Initialize(_settings.SeedValue);
                                                                             });
                                                       graph.AddItem(generatorInitializer);
                                                       var startGame =
                                                           InitLambda.Action("Start Game",
                                                                             () => SceneManager.LoadScene("GameWorld"));
                                                       var startNode = graph.AddItem(startGame);
                                                       startNode.DependsOn(generatorInitializer);
                                                   });
            initPipeline.OnStepCompleted += (sender, args) =>
                                            {
                                                if (args.CompletedStepName != null)
                                                    Debug
                                                        .Log($"{args.CompletedStepName} {args.CompletedSteps}/{args.TotalSteps}");
                                            };
            initPipeline.Run();
        }
    }
}