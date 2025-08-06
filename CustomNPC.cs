using System;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
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
            _monitor = ModEntry.Instance.Monitor; // ModEntry 인스턴스에서 Monitor 가져오기

            // 기본값 설정
            this.displayName = name;
            this.currentLocation = Game1.currentLocation ?? Game1.getFarm();
            this.speed = 2; // 기본 이동 속도
            // this.collidesWith = StardewValley.Farmer.initialPassable; // 플레이어와 충돌하지 않도록 설정 - API 변경으로 주석 처리
            this.drawOffset.Y = -16; // 스프라이트 오프셋 조정
        }

        /// <summary>
        /// 매 게임 틱마다 호출되어 NPC의 AI와 애니메이션을 업데이트합니다.
        /// </summary>
        /// <param name="time">게임 시간</param>
        /// <param name="location">현재 위치</param>
        public override void update(GameTime time, GameLocation location)
        {
            base.update(time, location);

            // 현재 위치 업데이트 (base.update에서 처리되지만 명시적으로)
            this.currentLocation = location;

            // AI 업데이트
            _aiController.Update(time);

            // 현재 작업 실행
            if (_currentTask != null && !_currentTask.IsCompleted)
            {
                _workExecutor.ExecuteTask(_currentTask, time);
            }
            else if (_currentTask != null && _currentTask.IsCompleted)
            {
                // 작업 완료 후 처리 (예: TaskManager에 알림)
                _monitor.Log($"NPC {this.Name} 작업 {_currentTask.Id} 완료.", LogLevel.Debug);
                ModEntry.Instance.TaskManager.CompleteTask(_currentTask.Id, _currentTask.Result);
                _currentTask = null; // 작업 초기화
            }
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
            _aiController.SetDestination(task.TargetLocation); // 작업 위치로 이동 시작
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

        /// <summary>
        /// NPC가 현재 작업 중인지 여부를 반환합니다.
        /// </summary>
        public bool IsBusy()
        {
            return _currentTask != null && !_currentTask.IsCompleted;
        }

        /// <summary>
        /// NPC가 현재 대기 중인지 여부를 반환합니다.
        /// </summary>
        public bool IsIdle()
        {
            return !IsBusy();
        }

        /// <summary>
        /// NPC가 특정 유형의 작업을 수행할 수 있는지 여부를 확인합니다.
        /// </summary>
        /// <param name="taskType">확인할 작업 유형</param>
        /// <returns>수행 가능 여부</returns>
        public bool CanPerformTask(TaskType taskType)
        {
            // TODO: NPC의 능력치나 특성에 따라 작업 가능 여부 판단 로직 추가
            return _workExecutor.CanExecuteTask(taskType);
        }

        /// <summary>
        /// NPC의 X 타일 좌표를 반환합니다.
        /// </summary>
        public int getTileX()
        {
            return (int)(this.Position.X / 64f);
        }

        /// <summary>
        /// NPC의 Y 타일 좌표를 반환합니다.
        /// </summary>
        public int getTileY()
        {
            return (int)(this.Position.Y / 64f);
        }

        /// <summary>
        /// NPC의 현재 위치를 타일 좌표로 반환합니다.
        /// </summary>
        public Vector2 getTileLocation()
        {
            return new Vector2(getTileX(), getTileY());
        }

        /// <summary>
        /// NPC의 현재 위치를 타일 좌표로 반환합니다.
        /// </summary>
        public Vector2 GetCurrentTilePosition()
        {
            return getTileLocation();
        }

        /// <summary>
        /// NPC의 현재 작업을 반환합니다.
        /// </summary>
        public WorkTask GetCurrentTask()
        {
            return _currentTask;
        }

        /// <summary>
        /// NPC의 통계 정보를 반환합니다.
        /// </summary>
        public NPCStats GetStats()
        {
            return _stats;
        }

        /// <summary>
        /// NPC의 AI 컨트롤러를 반환합니다.
        /// </summary>
        public AIController GetAIController()
        {
            return _aiController;
        }

        /// <summary>
        /// NPC의 워크 실행기를 반환합니다.
        /// </summary>
        public WorkExecutor GetWorkExecutor()
        {
            return _workExecutor;
        }
    }
}

