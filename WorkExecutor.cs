using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Objects;

namespace AutomatedNPCMod.Models
{
    /// <summary>
    /// 실제 게임 내 작업을 수행하는 로직을 담당하는 클래스.
    /// </summary>
    public class WorkExecutor
    {
        private readonly CustomNPC _npc;
        private Dictionary<TaskType, IWorkHandler> _workHandlers;

        public WorkExecutor(CustomNPC npc)
        {
            _npc = npc;
            InitializeWorkHandlers();
        }

        /// <summary>
        /// 작업 처리기들을 초기화합니다.
        /// </summary>
        private void InitializeWorkHandlers()
        {
            _workHandlers = new Dictionary<TaskType, IWorkHandler>
            {
                { TaskType.Farming, new FarmingHandler() }
                // Other handlers removed to focus on farming
            };
        }

        /// <summary>
        /// 지정된 작업을 실행합니다.
        /// </summary>
        /// <param name="task">실행할 작업</param>
        /// <param name="gameTime">게임 시간</param>
        public void ExecuteTask(WorkTask task, GameTime gameTime)
        {
            // This is now handled by CustomNPC.cs for Junimo-like behavior
        }

        /// <summary>
        /// 지정된 작업 유형을 실행할 수 있는지 확인합니다.
        /// </summary>
        /// <param name="taskType">작업 유형</param>
        /// <returns>실행 가능 여부</returns>
        public bool CanExecuteTask(TaskType taskType)
        {
            return _workHandlers.ContainsKey(taskType);
        }
    }

    /// <summary>
    /// 작업 처리기 인터페이스.
    /// </summary>
    public interface IWorkHandler
    {
        void Execute(CustomNPC npc, WorkTask task, GameTime gameTime);
        bool CanHandle(WorkTask task);
        TaskResult GetResult();
        void Initialize(CustomNPC npc);
        void Cleanup();
    }

    /// <summary>
    /// 농사 작업을 처리하는 클래스.
    /// </summary>
    public class FarmingHandler : IWorkHandler
    {
        public void Execute(CustomNPC npc, WorkTask task, GameTime gameTime)
        {
            // Logic is now in CustomNPC.cs to be more Junimo-like.
            // This handler exists only to show that farming tasks are supported.
        }

        public bool CanHandle(WorkTask task) => task.Type == TaskType.Farming;
        public TaskResult GetResult() => new TaskResult { Success = true };
        public void Initialize(CustomNPC npc) { }
        public void Cleanup() { }
    }

    /// <summary>
    /// 농사 작업 단계를 정의하는 열거형
    /// </summary>
    public enum FarmingStep
    {
        Till,
        Plant,
        Water,
        Harvest
    }
}

