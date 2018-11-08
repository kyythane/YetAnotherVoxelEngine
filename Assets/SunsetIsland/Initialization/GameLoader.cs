using Assets.SunsetIsland.Config;
using Assets.SunsetIsland.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.SunsetIsland.Initialization
{
    public class GameLoader : MonoBehaviour
    {
        // Use this for initialization
        private void Start()
        {
            var initPipeline = InitPipeline.Create(graph =>
                                                   {
                                                       var configInitializer =
                                                           InitLambda.Action("InitializeConfig", InitializeConfig);
                                                       graph.AddItem(configInitializer);
                                                       var blockInitializer =
                                                           InitLambda.Action("InitializeBlockFactory",
                                                                             BlockFactory.Initialize);
                                                       var blockNode = graph.AddItem(blockInitializer);
                                                       blockNode.DependsOn(configInitializer);
                                                       var startGame = InitLambda.Action("Start Game", StartGame);
                                                       var startNode = graph.AddItem(startGame);
                                                       startNode.DependsOn(configInitializer, blockInitializer);
                                                   });
            initPipeline.OnStepCompleted += (sender, args) =>
                                            {
                                                if (args.CompletedStepName != null)
                                                    Debug
                                                        .Log($"{args.CompletedStepName} {args.CompletedSteps}/{args.TotalSteps}");
                                            };
            initPipeline.Run();
        }

        private void StartGame()
        {
            SceneManager.LoadScene("MainMenu");
        }

        private static void InitializeConfig()
        {
            var unityProperties = FindObjectOfType<UnityProperties>();
            DontDestroyOnLoad(unityProperties);
            ConfigManager.Initialize(unityProperties);
        }
    }
}