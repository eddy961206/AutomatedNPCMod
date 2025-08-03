using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using AutomatedNPCMod.Models;

namespace AutomatedNPCMod.Core
{
    /// <summary>
    /// 작업의 생성, 할당, 실행, 완료를 관리하는 클래스.
    /// </summary>
    public class TaskManager
    {
        private readonly NPCManager _npcManager;
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly Queue<WorkTask> _pendingTasks;
        private readonly Dictionary<string, WorkTask> _activeTasks;

        public TaskManager(NPCManager npcManager, IModHelper helper, IMonitor monitor)
        {
            _npcManager = npcManager;
            _helper = helper;
            _monitor = monitor;
            _pendingTasks = new Queue<WorkTask>();
            _activeTasks = new Dictionary<string, WorkTask>();
        }

        /// <summary>
        /// NPC에게 작업을 할당합니다.
        /// </summary>
        /// <param name="npcName">NPC 이름</param>
        /// <param name="task">할당할 작업</param>
        /// <returns>할당 성공 여부</returns>
        public bool AssignTask(string npcName, WorkTask task)
        {
            try
            {
                var npc = _npcManager.GetNPC(npcName);
                if (npc == null)
                {
                    _monitor.Log($"존재하지 않는 NPC: {npcName}", LogLevel.Warn);
                    return false;
                }

                // NPC가 해당 작업을 수행할 수 있는지 확인
                if (!npc.CanPerformTask(task.Type))
                {
                    _monitor.Log($"NPC '{npcName}'는 {task.Type} 작업을 수행할 수 없습니다", LogLevel.Warn);
                    return false;
                }

                // 작업 할당
                task.AssignedNPCId = npcName;
                task.StartTime = DateTime.Now;

                if (npc.AssignTask(task))
                {
                    _activeTasks[task.Id] = task;
                    _monitor.Log($"NPC '{npcName}'에게 {task.Type} 작업 할당 완료", LogLevel.Info);
                    return true;
                }
                else
                {
                    _monitor.Log($"NPC '{npcName}'에게 작업 할당 실패", LogLevel.Warn);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"작업 할당 중 오류 발생: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// 새로운 작업을 생성합니다.
        /// </summary>
        /// <param name="taskType">작업 유형</param>
        /// <param name="targetLocation">목표 위치</param>
        /// <param name="priority">우선순위</param>
        /// <returns>생성된 작업</returns>
        public WorkTask CreateTask(TaskType taskType, Vector2 targetLocation, TaskPriority priority = TaskPriority.Normal)
        {
            var task = new WorkTask
            {
                Id = Guid.NewGuid().ToString(),
                Type = taskType,
                TargetLocation = targetLocation,
                Priority = priority,
                CreatedTime = DateTime.Now,
                Parameters = new Dictionary<string, object>()
            };

            _pendingTasks.Enqueue(task);
            _monitor.Log($"새로운 {taskType} 작업 생성: {task.Id}", LogLevel.Debug);

            return task;
        }

        /// <summary>
        /// 대기 중인 작업들을 처리합니다.
        /// </summary>
        public void ProcessTasks()
        {
            try
            {
                // 대기 중인 작업을 사용 가능한 NPC에게 할당
                while (_pendingTasks.Count > 0)
                {
                    var task = _pendingTasks.Peek();
                    var availableNPC = FindAvailableNPC(task.Type);

                    if (availableNPC != null)
                    {
                        _pendingTasks.Dequeue();
                        AssignTask(availableNPC.Name, task);
                    }
                    else
                    {
                        // 사용 가능한 NPC가 없으면 대기
                        break;
                    }
                }

                // 완료된 작업 정리
                CleanupCompletedTasks();
            }
            catch (Exception ex)
            {
                _monitor.Log($"작업 처리 중 오류 발생: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 작업 완료를 처리하고 수익을 정산합니다.
        /// </summary>
        /// <param name="taskId">완료된 작업 ID</param>
        /// <param name="result">작업 결과</param>
        public void CompleteTask(string taskId, TaskResult result)
        {
            try
            {
                if (!_activeTasks.TryGetValue(taskId, out WorkTask task))
                {
                    _monitor.Log($"존재하지 않는 작업 ID: {taskId}", LogLevel.Warn);
                    return;
                }

                task.IsCompleted = true;
                task.CompletedTime = DateTime.Now;
                task.Result = result;

                // 수익 분배
                if (result.Success)
                {
                    DistributeProfits(result);
                    _monitor.Log($"작업 '{taskId}' 완료. 수익: {result.GoldEarned}G", LogLevel.Info);
                }
                else
                {
                    _monitor.Log($"작업 '{taskId}' 실패: {result.ErrorMessage}", LogLevel.Warn);
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"작업 완료 처리 중 오류 발생: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 특정 NPC의 작업 목록을 반환합니다.
        /// </summary>
        /// <param name="npcName">NPC 이름</param>
        /// <returns>작업 목록</returns>
        public List<WorkTask> GetTasksForNPC(string npcName)
        {
            return _activeTasks.Values
                .Where(task => task.AssignedNPCId == npcName)
                .ToList();
        }

        /// <summary>
        /// 작업을 취소합니다.
        /// </summary>
        /// <param name="taskId">취소할 작업 ID</param>
        public void CancelTask(string taskId)
        {
            try
            {
                if (_activeTasks.TryGetValue(taskId, out WorkTask task))
                {
                    var npc = _npcManager.GetNPC(task.AssignedNPCId);
                    npc?.CancelCurrentTask();

                    _activeTasks.Remove(taskId);
                    _monitor.Log($"작업 '{taskId}' 취소됨", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"작업 취소 중 오류 발생: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 특정 작업 유형을 수행할 수 있는 사용 가능한 NPC를 찾습니다.
        /// </summary>
        /// <param name="taskType">작업 유형</param>
        /// <returns>사용 가능한 NPC 또는 null</returns>
        private CustomNPC FindAvailableNPC(TaskType taskType)
        {
            return _npcManager.GetAllNPCs()
                .Where(npc => npc.CanPerformTask(taskType) && !npc.IsBusy())
                .FirstOrDefault();
        }

        /// <summary>
        /// 완료된 작업들을 정리합니다.
        /// </summary>
        private void CleanupCompletedTasks()
        {
            var completedTasks = _activeTasks.Values
                .Where(task => task.IsCompleted)
                .ToList();

            foreach (var task in completedTasks)
            {
                _activeTasks.Remove(task.Id);
            }
        }

        /// <summary>
        /// 수익을 플레이어에게 분배합니다.
        /// </summary>
        /// <param name="result">작업 결과</param>
        private void DistributeProfits(TaskResult result)
        {
            try
            {
                // 골드 추가
                if (result.GoldEarned > 0)
                {
                    StardewValley.Game1.player.Money += result.GoldEarned;
                }

                // 아이템 추가
                if (result.ItemsObtained?.Count > 0)
                {
                    foreach (var item in result.ItemsObtained)
                    {
                        StardewValley.Game1.player.addItemToInventory(item);
                    }
                }

                // 경험치 추가 (필요시 구현)
                if (result.ExperienceGained > 0)
                {
                    // 경험치 추가 로직
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"수익 분배 중 오류 발생: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 작업 데이터를 저장합니다.
        /// </summary>
        public void SaveTaskData()
        {
            try
            {
                var saveData = new TaskSaveData
                {
                    PendingTasks = _pendingTasks.ToList(),
                    ActiveTasks = _activeTasks.Values.ToList()
                };

                _helper.Data.WriteSaveData("task-data", saveData);
                _monitor.Log($"작업 데이터 저장 완료", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor.Log($"작업 데이터 저장 중 오류 발생: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 작업 데이터를 로드합니다.
        /// </summary>
        public void LoadTaskData()
        {
            try
            {
                var saveData = _helper.Data.ReadSaveData<TaskSaveData>("task-data");
                if (saveData == null)
                {
                    _monitor.Log("로드할 작업 데이터가 없습니다", LogLevel.Debug);
                    return;
                }

                // 대기 중인 작업 복원
                _pendingTasks.Clear();
                if (saveData.PendingTasks != null)
                {
                    foreach (var task in saveData.PendingTasks)
                    {
                        _pendingTasks.Enqueue(task);
                    }
                }

                // 활성 작업 복원
                _activeTasks.Clear();
                if (saveData.ActiveTasks != null)
                {
                    foreach (var task in saveData.ActiveTasks)
                    {
                        _activeTasks[task.Id] = task;
                    }
                }

                _monitor.Log($"작업 데이터 로드 완료", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"작업 데이터 로드 중 오류 발생: {ex.Message}", LogLevel.Error);
            }
        }
    }

    /// <summary>
    /// 작업 저장 데이터 구조
    /// </summary>
    public class TaskSaveData
    {
        public List<WorkTask> PendingTasks { get; set; } = new List<WorkTask>();
        public List<WorkTask> ActiveTasks { get; set; } = new List<WorkTask>();
    }
}

