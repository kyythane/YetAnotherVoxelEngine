using System;
using Assets.SunsetIsland.Chunks;
using Assets.SunsetIsland.Game.Entities;
using Assets.SunsetIsland.Managers;
using UnityEngine;

namespace Assets.SunsetIsland.Simulation
{
    public class Simulation : MonoBehaviour
    {
        private enum SimulationState
        {
            Created,
            Started,
            Initializing,
            Running
        }
        private DateTime _lastUpdate;
        private Player _player;
        private World _world;
        private SimulationState _state;
        private Material _mat;
        private void Update()
        {
            switch (_state)
            {
                case SimulationState.Created:
                    _player = Instantiate(ConfigManager.UnityProperties.PlayerPrefab);
                    var spawnPosition = GenerationManager.GetHeight(0, 0) + 1;
                    _player.Initialize(new Vector3(0, spawnPosition, 0));
                    _world = new World(ConfigManager.Properties.BlockWorldScale, transform.position);
                    _state = SimulationState.Started;
                    break;
                case SimulationState.Started:
                    _world.Initialize(_player, 4);
                    _state = SimulationState.Initializing;
                    break;
                case SimulationState.Initializing:
                    _mat = BlockFactory.BlockMaterialInstance;
                    _state = SimulationState.Running;
                    break;
                case SimulationState.Running:
                    if (_world.CanEnterChunk(_player.Position))
                    {
                        //TODO : enable player
                    }
                    var delta = DateTime.Now - _lastUpdate;
                    _world.Update((float) delta.TotalSeconds);
                    if (Input.GetKeyDown(KeyCode.O))
                        DebugManager.ShowChunkBounds = !DebugManager.ShowChunkBounds;
                    if (Input.GetKeyDown(KeyCode.I))
                        DebugManager.ShowVisibility = !DebugManager.ShowVisibility;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _lastUpdate = DateTime.Now;
        }

        private void OnRenderObject()
        {
            if(_mat == null || _state != SimulationState.Running)
                return;
            _mat.SetPass(0);
            _world.Draw();
        }
    }
}