using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;

namespace AutomatedNPCMod.Models
{
    /// <summary>
    /// NPC가 수행할 작업을 정의하는 클래스.
    /// </summary>
    public class WorkTask
    {
        public string Id { get; set; }
        public TaskType Type { get; set; }
        public Vector2 TargetLocation { get; set; }
        public string AssignedNPCId { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? CompletedTime { get; set; }
        public TaskPriority Priority { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public bool IsCompleted { get; set; }
        public TaskResult Result { get; set; }

        public WorkTask()
        {
            Parameters = new Dictionary<string, object>();
            IsCompleted = false;
        }
    }

    /// <summary>
    /// 작업 유형을 정의하는 열거형.
    /// </summary>
    public enum TaskType
    {
        Farming,    // 농사 (씨앗 심기, 물주기, 수확)
        Mining,     // 채굴 (광석, 돌 채굴)
        Foraging,   // 채집 (야생 식물, 과일 수집)
        Fishing,    // 낚시
        Combat,     // 전투
        Crafting,   // 제작
        Building,   // 건설
        Maintenance // 유지보수
    }

    /// <summary>
    /// 작업 우선순위를 정의하는 열거형.
    /// </summary>
    public enum TaskPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Critical = 4
    }

    /// <summary>
    /// 작업 완료 후의 결과를 나타내는 클래스.
    /// </summary>
    public class TaskResult
    {
        public bool Success { get; set; }
        public List<Item> ItemsObtained { get; set; }
        public int ExperienceGained { get; set; }
        public int GoldEarned { get; set; }
        public TimeSpan TimeTaken { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; }

        public TaskResult()
        {
            ItemsObtained = new List<Item>();
            AdditionalData = new Dictionary<string, object>();
            Success = false;
            ExperienceGained = 0;
            GoldEarned = 0;
        }
    }

    /// <summary>
    /// NPC의 능력치와 상태를 나타내는 클래스.
    /// </summary>
    public class NPCStats
    {
        public int FarmingLevel { get; set; }
        public int MiningLevel { get; set; }
        public int ForagingLevel { get; set; }
        public int FishingLevel { get; set; }
        public int CombatLevel { get; set; }
        public int Energy { get; set; }
        public int MaxEnergy { get; set; }
        public float MovementSpeed { get; set; }
        public float WorkEfficiency { get; set; }
        public Dictionary<string, int> ToolProficiency { get; set; }

        public NPCStats()
        {
            // 기본값 설정
            FarmingLevel = 1;
            MiningLevel = 1;
            ForagingLevel = 1;
            FishingLevel = 1;
            CombatLevel = 1;
            Energy = 100;
            MaxEnergy = 100;
            MovementSpeed = 2.0f;
            WorkEfficiency = 1.0f;
            ToolProficiency = new Dictionary<string, int>();
        }

        /// <summary>
        /// 특정 작업 유형에 대한 레벨을 반환합니다.
        /// </summary>
        /// <param name="taskType">작업 유형</param>
        /// <returns>해당 작업의 레벨</returns>
        public int GetLevelForTask(TaskType taskType)
        {
            return taskType switch
            {
                TaskType.Farming => FarmingLevel,
                TaskType.Mining => MiningLevel,
                TaskType.Foraging => ForagingLevel,
                TaskType.Fishing => FishingLevel,
                TaskType.Combat => CombatLevel,
                _ => 1
            };
        }

        /// <summary>
        /// 에너지를 소모합니다.
        /// </summary>
        /// <param name="amount">소모할 에너지 양</param>
        /// <returns>에너지 소모 성공 여부</returns>
        public bool ConsumeEnergy(int amount)
        {
            if (Energy >= amount)
            {
                Energy -= amount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 에너지를 회복합니다.
        /// </summary>
        /// <param name="amount">회복할 에너지 양</param>
        public void RestoreEnergy(int amount)
        {
            Energy = Math.Min(Energy + amount, MaxEnergy);
        }

        /// <summary>
        /// NPC가 작업을 수행할 수 있는 상태인지 확인합니다.
        /// </summary>
        /// <param name="requiredEnergy">필요한 에너지</param>
        /// <returns>작업 가능 여부</returns>
        public bool CanWork(int requiredEnergy = 10)
        {
            return Energy >= requiredEnergy;
        }
    }
}

