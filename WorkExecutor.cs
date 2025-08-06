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
                { TaskType.Farming, new FarmingHandler() },
                { TaskType.Mining, new MiningHandler() },
                { TaskType.Foraging, new ForagingHandler() },
                { TaskType.Woodcutting, new WoodcuttingHandler() }
            };
        }

        /// <summary>
        /// 지정된 작업을 실행합니다.
        /// </summary>
        /// <param name="task">실행할 작업</param>
        /// <param name="gameTime">게임 시간</param>
        public void ExecuteTask(WorkTask task, GameTime gameTime)
        {
            if (_workHandlers.TryGetValue(task.Type, out IWorkHandler handler))
            {
                handler.Execute(_npc, task, gameTime);
            }
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
        private TaskResult _result;
        private float _workTimer;
        private const float WORK_DURATION = 3.0f; // 작업 완료까지 3초

        public void Execute(CustomNPC npc, WorkTask task, GameTime gameTime)
        {
            if (task.IsCompleted) return;

            // 목표 위치에 도달했는지 확인
            if (!npc.GetAIController().HasReachedDestination())
            {
                return; // 아직 이동 중
            }

            // 작업 시작
            if (task.StartTime == null)
            {
                task.StartTime = DateTime.Now;
                _workTimer = 0f;
                _result = new TaskResult();
                npc.GetAIController().SetState(AIState.Working);
            }

            _workTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 작업 완료 확인
            if (_workTimer >= WORK_DURATION)
            {
                PerformFarmingAction(npc, task);
                task.IsCompleted = true;
                task.CompletedTime = DateTime.Now;
                task.Result = _result;
            }
        }

        /// <summary>
        /// 실제 농사 작업을 수행합니다.
        /// </summary>
        private void PerformFarmingAction(CustomNPC npc, WorkTask task)
        {
            var location = npc.currentLocation;
            var targetTile = task.TargetLocation;

            try
            {
                // 해당 위치에 작물이 있는지 확인
                if (location.terrainFeatures.TryGetValue(targetTile, out TerrainFeature feature))
                {
                    if (feature is HoeDirt dirt)
                    {
                        if (dirt.crop != null && dirt.crop.fullyGrown.Value)
                        {
                            // 수확
                            HarvestCrop(dirt, targetTile, location);
                        }
                        else if (dirt.crop == null)
                        {
                            // 씨앗 심기 (간단한 예시로 파스닙 씨앗)
                            PlantSeed(dirt, 24); // 파스닙 씨앗 ID
                        }
                        else
                        {
                            // 물주기
                            WaterCrop(dirt);
                        }
                    }
                }
                else
                {
                    // 땅 갈기
                    TillSoil(location, targetTile);
                }

                _result.Success = true;
                _result.GoldEarned = Game1.random.Next(10, 50); // 랜덤 수익
                _result.ExperienceGained = 5;
            }
            catch (Exception ex)
            {
                _result.Success = false;
                _result.ErrorMessage = $"농사 작업 중 오류: {ex.Message}";
            }
        }

        private void HarvestCrop(HoeDirt dirt, Vector2 tile, GameLocation location)
        {
            if (dirt.crop != null)
            {
                var crop = dirt.crop;
                var harvestItem = new StardewValley.Object(crop.indexOfHarvest.Value.ToString(), 1);
                _result.ItemsObtained.Add(harvestItem);
                _result.GoldEarned += harvestItem.Price;
                
                // 작물 제거
                dirt.crop = null;
            }
        }

        private void PlantSeed(HoeDirt dirt, int seedId)
        {
            var playerTile = new Vector2((int)(Game1.player.position.X / 64f), (int)(Game1.player.position.Y / 64f));
            dirt.plant(seedId.ToString(), Game1.player, false);
            _result.GoldEarned += 5; // 심기 보상
        }

        private void WaterCrop(HoeDirt dirt)
        {
            dirt.state.Value = HoeDirt.watered;
            _result.GoldEarned += 2; // 물주기 보상
        }

        private void TillSoil(GameLocation location, Vector2 tile)
        {
            var dirt = new HoeDirt();
            location.terrainFeatures[tile] = dirt;
            _result.GoldEarned += 3; // 갈기 보상
        }

        public bool CanHandle(WorkTask task) => task.Type == TaskType.Farming;
        public TaskResult GetResult() => _result;
        public void Initialize(CustomNPC npc) { }
        public void Cleanup() { }
    }

    /// <summary>
    /// 채굴 작업을 처리하는 클래스.
    /// </summary>
    public class MiningHandler : IWorkHandler
    {
        private TaskResult _result;
        private float _workTimer;
        private const float WORK_DURATION = 4.0f; // 채굴은 4초

        public void Execute(CustomNPC npc, WorkTask task, GameTime gameTime)
        {
            if (task.IsCompleted) return;

            if (!npc.GetAIController().HasReachedDestination())
            {
                return;
            }

            if (task.StartTime == null)
            {
                task.StartTime = DateTime.Now;
                _workTimer = 0f;
                _result = new TaskResult();
                npc.GetAIController().SetState(AIState.Working);
            }

            _workTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_workTimer >= WORK_DURATION)
            {
                PerformMiningAction(npc, task);
                task.IsCompleted = true;
                task.CompletedTime = DateTime.Now;
                task.Result = _result;
            }
        }

        private void PerformMiningAction(CustomNPC npc, WorkTask task)
        {
            var location = npc.currentLocation;
            var targetTile = task.TargetLocation;

            try
            {
                // 해당 위치에 돌이나 광석이 있는지 확인
                if (location.objects.TryGetValue(targetTile, out StardewValley.Object obj))
                {
                    if (obj.Name.Contains("Stone") || obj.Name.Contains("Ore"))
                    {
                        // 광석/돌 채굴
                        var miningResult = GetMiningResult();
                        _result.ItemsObtained.Add(miningResult);
                        _result.GoldEarned = miningResult.Price;
                        
                        // 객체 제거
                        location.objects.Remove(targetTile);
                    }
                }
                else
                {
                    // 기본 돌 생성 및 채굴 (시뮬레이션)
                    var stone = new StardewValley.Object("390", 1); // 돌
                    _result.ItemsObtained.Add(stone);
                    _result.GoldEarned = stone.Price;
                }

                _result.Success = true;
                _result.ExperienceGained = 8;
            }
            catch (Exception ex)
            {
                _result.Success = false;
                _result.ErrorMessage = $"채굴 작업 중 오류: {ex.Message}";
            }
        }

        private StardewValley.Object GetMiningResult()
        {
            // 랜덤하게 채굴 결과 결정
            var random = Game1.random.Next(1, 100);
            if (random <= 10) // 10% 확률로 구리 광석
                return new StardewValley.Object("378", 1);
            else if (random <= 20) // 10% 확률로 철 광석
                return new StardewValley.Object("380", 1);
            else if (random <= 25) // 5% 확률로 금 광석
                return new StardewValley.Object("384", 1);
            else // 나머지는 돌
                return new StardewValley.Object("390", 1);
        }

        public bool CanHandle(WorkTask task) => task.Type == TaskType.Mining;
        public TaskResult GetResult() => _result;
        public void Initialize(CustomNPC npc) { }
        public void Cleanup() { }
    }

    /// <summary>
    /// 채집 작업을 처리하는 클래스.
    /// </summary>
    public class ForagingHandler : IWorkHandler
    {
        private TaskResult _result;
        private float _workTimer;
        private const float WORK_DURATION = 2.0f; // 채집은 2초

        public void Execute(CustomNPC npc, WorkTask task, GameTime gameTime)
        {
            if (task.IsCompleted) return;

            if (!npc.GetAIController().HasReachedDestination())
            {
                return;
            }

            if (task.StartTime == null)
            {
                task.StartTime = DateTime.Now;
                _workTimer = 0f;
                _result = new TaskResult();
                npc.GetAIController().SetState(AIState.Working);
            }

            _workTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_workTimer >= WORK_DURATION)
            {
                PerformForagingAction(npc, task);
                task.IsCompleted = true;
                task.CompletedTime = DateTime.Now;
                task.Result = _result;
            }
        }

        private void PerformForagingAction(CustomNPC npc, WorkTask task)
        {
            var location = npc.currentLocation;
            var targetTile = task.TargetLocation;

            try
            {
                // 해당 위치에 채집 가능한 아이템이 있는지 확인
                if (location.objects.TryGetValue(targetTile, out StardewValley.Object obj))
                {
                    if (obj.isForage())
                    {
                        // 채집 아이템 수집
                        _result.ItemsObtained.Add(obj);
                        _result.GoldEarned = obj.Price;
                        
                        // 객체 제거
                        location.objects.Remove(targetTile);
                    }
                }
                else
                {
                    // 기본 채집 아이템 생성 (시뮬레이션)
                    var foragingResult = GetForagingResult();
                    _result.ItemsObtained.Add(foragingResult);
                    _result.GoldEarned = foragingResult.Price;
                }

                _result.Success = true;
                _result.ExperienceGained = 3;
            }
            catch (Exception ex)
            {
                _result.Success = false;
                _result.ErrorMessage = $"채집 작업 중 오류: {ex.Message}";
            }
        }

        private StardewValley.Object GetForagingResult()
        {
            // 랜덤하게 채집 결과 결정
            var random = Game1.random.Next(1, 100);
            if (random <= 30) // 30% 확률로 야생 양파
                return new StardewValley.Object("399", 1);
            else if (random <= 50) // 20% 확률로 민들레
                return new StardewValley.Object("18", 1);
            else if (random <= 70) // 20% 확률로 리크
                return new StardewValley.Object("20", 1);
            else if (random <= 85) // 15% 확률로 야생 마늘
                return new StardewValley.Object("22", 1);
            else // 나머지는 나무 가지
                return new StardewValley.Object("388", 1);
        }

        public bool CanHandle(WorkTask task) => task.Type == TaskType.Foraging;
        public TaskResult GetResult() => _result;
        public void Initialize(CustomNPC npc) { }
        public void Cleanup() { }
    }

    /// <summary>
    /// 나무 베기 작업을 처리하는 클래스.
    /// </summary>
    public class WoodcuttingHandler : IWorkHandler
    {
        private TaskResult _result;
        private float _workTimer;
        private const float WORK_DURATION = 3.5f; // 나무 베기는 3.5초

        public void Execute(CustomNPC npc, WorkTask task, GameTime gameTime)
        {
            if (task.IsCompleted) return;

            if (!npc.GetAIController().HasReachedDestination())
            {
                return;
            }

            if (task.StartTime == null)
            {
                task.StartTime = DateTime.Now;
                _workTimer = 0f;
                _result = new TaskResult();
                npc.GetAIController().SetState(AIState.Working);
            }

            _workTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_workTimer >= WORK_DURATION)
            {
                PerformWoodcuttingAction(npc, task);
                task.IsCompleted = true;
                task.CompletedTime = DateTime.Now;
                task.Result = _result;
            }
        }

        private void PerformWoodcuttingAction(CustomNPC npc, WorkTask task)
        {
            var location = npc.currentLocation;
            var targetTile = task.TargetLocation;

            try
            {
                // 해당 위치에 나무가 있는지 확인
                if (location.terrainFeatures.TryGetValue(targetTile, out TerrainFeature feature))
                {
                    if (feature is Tree tree && tree.growthStage.Value >= 5)
                    {
                        // 성장한 나무 베기
                        var woodAmount = Game1.random.Next(8, 15); // 8-14개의 나무
                        var wood = new StardewValley.Object("388", woodAmount); // 나무 ID: 388
                        _result.ItemsObtained.Add(wood);
                        _result.GoldEarned = wood.Price * woodAmount;
                        
                        // 나무 제거
                        location.terrainFeatures.Remove(targetTile);
                        
                        // 씨앗 떨어트리기 (확률적)
                        if (Game1.random.NextDouble() < 0.05) // 5% 확률
                        {
                            var seed = GetTreeSeed(tree);
                            if (seed != null)
                            {
                                _result.ItemsObtained.Add(seed);
                                _result.GoldEarned += seed.Price;
                            }
                        }
                    }
                }
                else
                {
                    // 나무가 없는 경우 시뮬레이션
                    var wood = new StardewValley.Object("388", 10);
                    _result.ItemsObtained.Add(wood);
                    _result.GoldEarned = wood.Price * 10;
                }

                _result.Success = true;
                _result.ExperienceGained = 12;
            }
            catch (Exception ex)
            {
                _result.Success = false;
                _result.ErrorMessage = $"나무 베기 작업 중 오류: {ex.Message}";
            }
        }

        private StardewValley.Object GetTreeSeed(Tree tree)
        {
            // 나무 타입에 따라 해당하는 씨앗 반환
            return tree.treeType.Value.ToString() switch
            {
                "1" => new StardewValley.Object("309", 1), // 떡갈나무 씨앗
                "2" => new StardewValley.Object("310", 1), // 단풍나무 씨앗
                "3" => new StardewValley.Object("311", 1), // 소나무 씨앗
                "6" => new StardewValley.Object("292", 1), // 마호가니 씨앗
                _ => new StardewValley.Object("309", 1)  // 기본값
            };
        }

        public bool CanHandle(WorkTask task) => task.Type == TaskType.Woodcutting;
        public TaskResult GetResult() => _result;
        public void Initialize(CustomNPC npc) { }
        public void Cleanup() { }
    }
}

