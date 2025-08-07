using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;
using StardewModdingAPI;
using AutomatedNPCMod.Core;

namespace AutomatedNPCMod.Models
{
    /// <summary>
    /// 플레이어가 컨트롤할 수 있는 자동화된 NPC를 나타내는 클래스.
    /// </summary>
    public class CustomNPC : NPC
    {
        private AIController _aiController;
        private WorkExecutor _workExecutor;
        private WorkTask _currentTask;
        private NPCStats _stats;
        private string _uniqueId;
        private IMonitor _monitor;

        /// <summary>
        /// 새로운 CustomNPC 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="name">NPC 이름</param>
        /// <param name="position">초기 위치</param>
        /// <param name="spriteSheet">스프라이트 시트 경로</param>
        public CustomNPC(string name, Vector2 position, string spriteSheet)
            : base(new AnimatedSprite(spriteSheet, 0, 16, 32), position * 64f, 2, name)
        {
            _uniqueId = Guid.NewGuid().ToString();
            _stats = new NPCStats();
            _aiController = new AIController(this);
            _workExecutor = new WorkExecutor(this);
            _monitor = ModEntry.Instance.Monitor;

            this.displayName = name;
            this.currentLocation = Game1.currentLocation ?? Game1.getFarm();
            this.speed = 4; // Junimo-like speed
            this.drawOffset.Y = -16;
        }

        /// <summary>
        /// 매 게임 틱마다 호출되어 NPC의 AI와 애니메이션을 업데이트합니다.
        /// </summary>
        /// <param name="time">게임 시간</param>
        /// <param name="location">현재 위치</param>
        public override void update(GameTime time, GameLocation location)
        {
            base.update(time, location);
            this.currentLocation = location;

            _aiController.Update(time);

            if (_currentTask != null && !_currentTask.IsCompleted)
            {
                if (_aiController.GetCurrentState() == AIState.Working)
                {
                    // 목표 위치에 도달하면 작업 수행
                    PerformTask(time);
                }
            }
            else if (_currentTask != null && _currentTask.IsCompleted)
            {
                _monitor.Log($"NPC {this.Name} 작업 {_currentTask.Id} 완료.", LogLevel.Debug);
                ModEntry.Instance.TaskManager.CompleteTask(_currentTask.Id, _currentTask.Result);
                _currentTask = null;
                _aiController.SetState(AIState.Idle);
            }
        }

        private void PerformTask(GameTime time)
        {
            if (_currentTask.Type == TaskType.Farming)
            {
                bool harvested = TryToHarvestHere();
                if (harvested)
                {
                    // 수확 후 점프 애니메이션
                    this.jump();
                }
            }

            // 작업을 즉시 완료 처리
            _currentTask.IsCompleted = true;
            _currentTask.CompletedTime = DateTime.Now;
            _currentTask.Result = new TaskResult { Success = true };
        }

        private bool TryToHarvestHere()
        {
            var tile = this.getTileLocation();
            if (this.currentLocation.terrainFeatures.TryGetValue(tile, out var feature) && feature is HoeDirt dirt)
            {
                if (dirt.crop != null && dirt.crop.harvest((int)tile.X, (int)tile.Y, dirt))
                {
                    // 수확 성공
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// NPC에게 작업을 할당합니다.
        /// </summary>
        /// <param name="task">할당할 작업</param>
        /// <returns>작업 할당 성공 여부</returns>
        public bool AssignTask(WorkTask task)
        {
            if (_currentTask != null && !_currentTask.IsCompleted)
            {
                _monitor.Log($"NPC {this.Name} 이미 작업 중입니다: {_currentTask.Type}", LogLevel.Warn);
                return false;
            }

            _currentTask = task;
            _monitor.Log($"NPC {this.Name}에게 {task.Type} 작업 할당됨.", LogLevel.Info);
            _aiController.SetDestination(task.TargetLocation);
            return true;
        }

        /// <summary>
        /// 현재 작업을 취소합니다.
        /// </summary>
        public void CancelCurrentTask()
        {
            if (_currentTask != null)
            {
                _monitor.Log($"NPC {this.Name}의 현재 작업 {_currentTask.Type}을 취소합니다.", LogLevel.Info);
                _currentTask = null;
                _aiController.StopMoving();
            }
        }

        public bool IsBusy()
        {
            return _currentTask != null && !_currentTask.IsCompleted;
        }

        public bool IsIdle()
        {
            return !IsBusy();
        }

        public bool CanPerformTask(TaskType taskType)
        {
            return _workExecutor.CanExecuteTask(taskType);
        }

        public int getTileX()
        {
            return (int)(this.Position.X / 64f);
        }

        public int getTileY()
        {
            return (int)(this.Position.Y / 64f);
        }

        public Vector2 getTileLocation()
        {
            return new Vector2(getTileX(), getTileY());
        }

        public Vector2 GetCurrentTilePosition()
        {
            return getTileLocation();
        }

        public Point getTileLocationPoint()
        {
            return new Point(getTileX(), getTileY());
        }

        public WorkTask GetCurrentTask()
        {
            return _currentTask;
        }

        public NPCStats GetStats()
        {
            return _stats;
        }

        public AIController GetAIController()
        {
            return _aiController;
        }

        public WorkExecutor GetWorkExecutor()
        {
            return _workExecutor;
        }

        // 도구 관련 메서드 제거됨

        public bool IsUsingTool()
        {
            return false; // 도구 시스템 제거
        }

        public void StartUsingTool(Vector2 targetTile)
        {
            // 도구 시스템 제거
        }

        public void StopUsingTool()
        {
            // 도구 시스템 제거
        }
    }

    /// <summary>
    /// 도구 타입 열거형
    /// </summary>
    public enum ToolType
    {
        None,
        Hoe,
        Axe,
        WateringCan,
        Pickaxe
    }
}

