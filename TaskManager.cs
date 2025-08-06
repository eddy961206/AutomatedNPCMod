using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
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
        
        private const int WorkScanRadius = 10;
        private const int ExploreIntervalTicks = 60;

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
        /// 사용 가능한 NPC에게 농사 작업을 할당합니다.
        /// </summary>
        public void AssignTaskToAvailableNPC(NPCManager npcManager)
        {
            try
            {
                var availableNPC = FindIdleNPC();
                if (availableNPC == null)
                {
                    Game1.addHUDMessage(new HUDMessage("사용 가능한 NPC가 없습니다!", HUDMessage.newQuest_type));
                    _monitor.Log("작업 할당 실패: 모든 NPC가 작업 중", LogLevel.Warn);
                    return;
                }

                // NPC 주변에서 작업 찾기
                var task = FindAndAssignNewTask(availableNPC);
                
                if (task != null)
                {
                    Game1.addHUDMessage(new HUDMessage($"{task.Type} 작업이 '{availableNPC.Name}'에게 할당됨!", HUDMessage.newQuest_type));
                    _monitor.Log($"작업 할당 완료: {availableNPC.Name} -> {task.Type}", LogLevel.Info);
                }
                else
                {
                    Game1.addHUDMessage(new HUDMessage("주변에 할당할 작업이 없습니다!", HUDMessage.newQuest_type));
                    _monitor.Log("작업 할당 실패: 주변에 작업 없음", LogLevel.Warn);
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"작업 할당 중 오류: {ex.Message}", LogLevel.Error);
                Game1.addHUDMessage(new HUDMessage("작업 할당 중 오류 발생!", HUDMessage.newQuest_type));
            }
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
        /// NPC를 위한 새 작업을 찾고 할당합니다.
        /// </summary>
        /// <param name="npc">작업을 할당할 NPC</param>
        /// <returns>할당된 작업 또는 null</returns>
        public WorkTask FindAndAssignNewTask(CustomNPC npc)
        {
            try
            {
                _monitor.Log($"{npc.Name} 작업 탐색 시작", LogLevel.Debug);

                var currentLocation = Game1.currentLocation;
                if (currentLocation == null) return null;

                // NPC 현재 위치
                var npcTilePosition = npc.getTileLocation();
                int npcX = (int)npcTilePosition.X;
                int npcY = (int)npcTilePosition.Y;

                // 주변 지역에서 작업 찾기
                var bestTask = ScanAreaForWork(currentLocation, npcX, npcY, WorkScanRadius);
                
                if (bestTask != null)
                {
                    // 새 작업 할당
                    bestTask.AssignedNPCId = npc.Name;
                    bestTask.StartTime = DateTime.Now;
                    
                    if (npc.AssignTask(bestTask))
                    {
                        _activeTasks[bestTask.Id] = bestTask;
                        _monitor.Log($"{npc.Name} 자동 작업 발견: {bestTask.Type}", LogLevel.Info);
                        Game1.addHUDMessage(new HUDMessage($"{npc.Name}이(가) 새 작업을 찾았습니다!", HUDMessage.newQuest_type));
                        return bestTask;
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _monitor.Log($"자동 작업 탐색 오류 ({npc.Name}): {ex.Message}", LogLevel.Error);
                return null;
            }
        }

        /// <summary>
        /// 지정된 지역을 스캔해서 작업을 찾습니다.
        /// </summary>
        /// <param name="location">게임 위치</param>
        /// <param name="centerX">중심 X 좌표</param>
        /// <param name="centerY">중심 Y 좌표</param>
        /// <param name="radius">스캔 반경</param>
        /// <returns>가장 우선순위가 높은 작업</returns>
        private WorkTask ScanAreaForWork(GameLocation location, int centerX, int centerY, int radius)
        {
            try
            {
                WorkTask bestTask = null;
                int bestPriority = 0;

                // 주변 지역 스캔
                for (int x = centerX - radius; x <= centerX + radius; x++)
                {
                    for (int y = centerY - radius; y <= centerY + radius; y++)
                    {
                        var task = EvaluateTileForWork(location, x, y);
                        if (task != null)
                        {
                            int priority = GetTaskPriority(task, centerX, centerY, x, y);
                            if (priority > bestPriority)
                            {
                                bestTask = task;
                                bestPriority = priority;
                            }
                        }
                    }
                }

                return bestTask;
            }
            catch (Exception ex)
            {
                _monitor.Log($"지역 스캔 오류: {ex.Message}", LogLevel.Error);
                return null;
            }
        }

        /// <summary>
        /// 특정 타일을 평가해서 작업을 생성합니다.
        /// </summary>
        /// <param name="location">게임 위치</param>
        /// <param name="x">X 좌표</param>
        /// <param name="y">Y 좌표</param>
        /// <returns>생성된 작업 또는 null</returns>
        private WorkTask EvaluateTileForWork(GameLocation location, int x, int y)
        {
            try
            {
                var tilePosition = new Vector2(x, y);

                if (location.terrainFeatures.TryGetValue(tilePosition, out TerrainFeature terrainFeature))
                {
                    // 나무 베기 우선순위 (높음)
                    if (terrainFeature is Tree tree)
                    {
                        // 성장한 나무만 베기 (stage 5 이상)
                        if (tree.growthStage.Value >= 5)
                        {
                            return new WorkTask
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = TaskType.Woodcutting,
                                TargetLocation = tilePosition,
                                Priority = TaskPriority.High,
                                CreatedTime = DateTime.Now,
                                Parameters = new Dictionary<string, object> 
                                { 
                                    ["treeType"] = "Tree", 
                                    ["growthStage"] = tree.growthStage.Value 
                                }
                            };
                        }
                    }
                    else if (terrainFeature is HoeDirt hoeDirt)
                    {
                        // 작물 상태 확인
                        if (hoeDirt.crop != null)
                        {
                            // 작물이 있음 - 물주기 또는 수확 작업
                            bool isFullyGrown = hoeDirt.crop.fullyGrown.Value;
                            
                            return new WorkTask
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = TaskType.Farming,
                                TargetLocation = tilePosition,
                                Priority = isFullyGrown ? TaskPriority.Normal : TaskPriority.Normal,
                                CreatedTime = DateTime.Now,
                                Parameters = new Dictionary<string, object>()
                            };
                        }
                        else
                        {
                            // 갈린 땅에 작물 없음 - 씨앗 심기
                            return new WorkTask
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = TaskType.Farming,
                                TargetLocation = tilePosition,
                                Priority = TaskPriority.Low,
                                CreatedTime = DateTime.Now,
                                Parameters = new Dictionary<string, object>()
                            };
                        }
                    }
                }
                else
                {
                    // 빈 땅 - 갈기 작업 (가장 낮은 우선순위)
                    return new WorkTask
                    {
                        Id = Guid.NewGuid().ToString(),
                        Type = TaskType.Farming,
                        TargetLocation = tilePosition,
                        Priority = TaskPriority.Low,
                        CreatedTime = DateTime.Now,
                        Parameters = new Dictionary<string, object>()
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _monitor.Log($"타일 평가 오류 ({x}, {y}): {ex.Message}", LogLevel.Debug);
                return null;
            }
        }

        /// <summary>
        /// 작업의 우선순위를 계산합니다.
        /// </summary>
        /// <param name="task">작업</param>
        /// <param name="npcX">NPC X 좌표</param>
        /// <param name="npcY">NPC Y 좌표</param>
        /// <param name="taskX">작업 X 좌표</param>
        /// <param name="taskY">작업 Y 좌표</param>
        /// <returns>우선순위 점수</returns>
        private int GetTaskPriority(WorkTask task, int npcX, int npcY, int taskX, int taskY)
        {
            int basePriority = (int)task.Priority * 100;
            
            // 거리 페널티 (가까울수록 높은 우선순위)
            int distance = Math.Abs(taskX - npcX) + Math.Abs(taskY - npcY);
            int distancePenalty = distance * 2;
            
            return Math.Max(1, basePriority - distancePenalty);
        }

        /// <summary>
        /// 대기 중인 NPC를 찾습니다.
        /// </summary>
        /// <returns>대기 중인 NPC 또는 null</returns>
        private CustomNPC FindIdleNPC()
        {
            var allNPCs = _npcManager.GetAllNPCs();
            return allNPCs.FirstOrDefault(npc => npc.IsIdle());
        }

        /// <summary>
        /// 모든 작업을 초기화합니다.
        /// </summary>
        public void ClearAllTasks()
        {
            _pendingTasks.Clear();
            _activeTasks.Clear();
            _monitor.Log("모든 작업이 초기화됨", LogLevel.Info);
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
        /// 현재 활성 작업 수를 반환합니다.
        /// </summary>
        /// <returns>활성 작업 수</returns>
        public int GetActiveTaskCount()
        {
            return _activeTasks.Count;
        }

        /// <summary>
        /// 완료된 작업 수를 반환합니다.
        /// </summary>
        /// <returns>완료된 작업 수</returns>
        public int GetCompletedTaskCount()
        {
            return _activeTasks.Values.Count(t => t.IsCompleted);
        }

        /// <summary>
        /// 대기 중인 작업 수를 반환합니다.
        /// </summary>
        /// <returns>대기 중인 작업 수</returns>
        public int GetPendingTaskCount()
        {
            return _pendingTasks.Count;
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
        /// 작업 업데이트를 처리합니다.
        /// </summary>
        public void UpdateTasks()
        {
            try
            {
                ProcessTasks();
                CleanupCompletedTasks();
            }
            catch (Exception ex)
            {
                _monitor.Log($"작업 업데이트 중 오류: {ex.Message}", LogLevel.Error);
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

