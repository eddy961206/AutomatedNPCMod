using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Tools;
using StardewModdingAPI;
using AutomatedNPCMod.Core;
using AutomatedNPCMod.Animations;

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
        
        // 도구 시스템
        private Tool _currentTool;
        private NPCToolAnimationManager _toolAnimationManager;
        private bool _isUsingTool;
        private float _toolUseTimer;
        private ToolType _requiredToolType;
        private Rectangle _toolHitbox;

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
            
            // 도구 시스템 초기화
            _toolAnimationManager = new NPCToolAnimationManager(this);
            _isUsingTool = false;
            _toolUseTimer = 0f;
            _requiredToolType = ToolType.None;
            _toolHitbox = new Rectangle();

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
            
            // 도구 애니메이션 업데이트
            _toolAnimationManager.Update(time);
            
            // 도구 사용 타이머 업데이트
            if (_isUsingTool)
            {
                _toolUseTimer += (float)time.ElapsedGameTime.TotalSeconds;
            }

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
                
                // 도구 사용 종료
                StopUsingTool();
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

        #region 도구 시스템 메서드

        /// <summary>
        /// NPC가 사용할 도구를 설정합니다.
        /// </summary>
        /// <param name="toolType">도구 타입</param>
        public void SetRequiredTool(ToolType toolType)
        {
            _requiredToolType = toolType;
            _currentTool = CreateTool(toolType);
            _toolAnimationManager.SetTool(_currentTool, toolType);
        }

        /// <summary>
        /// 도구 사용을 시작합니다.
        /// </summary>
        /// <param name="targetTile">대상 타일</param>
        public void StartUsingTool(Vector2 targetTile)
        {
            if (_currentTool == null || _isUsingTool) return;

            _isUsingTool = true;
            _toolUseTimer = 0f;
            _toolAnimationManager.StartToolAnimation(targetTile);
            _aiController.SetState(AIState.Working);

            // 방향 설정 (타겟을 바라보도록)
            SetFacingDirectionToTarget(targetTile);

            _monitor.Log($"NPC {this.Name} started using {_requiredToolType} tool at {targetTile}", LogLevel.Debug);
        }

        /// <summary>
        /// 도구 사용을 종료합니다.
        /// </summary>
        public void StopUsingTool()
        {
            if (!_isUsingTool) return;

            _isUsingTool = false;
            _toolUseTimer = 0f;
            _toolAnimationManager.StopToolAnimation();
            _aiController.SetState(AIState.Idle);

            _monitor.Log($"NPC {this.Name} stopped using tool", LogLevel.Debug);
        }

        /// <summary>
        /// 현재 도구 사용 중인지 여부를 반환합니다.
        /// </summary>
        public bool IsUsingTool()
        {
            return _isUsingTool;
        }

        /// <summary>
        /// 도구 사용 진행도를 반환합니다 (0.0 ~ 1.0).
        /// </summary>
        public float GetToolUseProgress()
        {
            if (!_isUsingTool) return 0f;
            
            float totalDuration = GetToolUseDuration(_requiredToolType);
            return Math.Min(_toolUseTimer / totalDuration, 1f);
        }

        /// <summary>
        /// 도구 사용이 완료되었는지 확인합니다.
        /// </summary>
        public bool IsToolUseCompleted()
        {
            return _isUsingTool && GetToolUseProgress() >= 1f;
        }

        /// <summary>
        /// 현재 도구를 반환합니다.
        /// </summary>
        public Tool GetCurrentTool()
        {
            return _currentTool;
        }

        /// <summary>
        /// 현재 필요한 도구 타입을 반환합니다.
        /// </summary>
        public ToolType GetRequiredToolType()
        {
            return _requiredToolType;
        }

        /// <summary>
        /// 도구 애니메이션 매니저를 반환합니다.
        /// </summary>
        public NPCToolAnimationManager GetToolAnimationManager()
        {
            return _toolAnimationManager;
        }

        /// <summary>
        /// 타겟을 바라보는 방향으로 설정합니다.
        /// </summary>
        private void SetFacingDirectionToTarget(Vector2 targetTile)
        {
            var currentTile = getTileLocation();
            var diff = targetTile - currentTile;

            if (Math.Abs(diff.X) > Math.Abs(diff.Y))
            {
                this.FacingDirection = diff.X > 0 ? 1 : 3; // 오른쪽 또는 왼쪽
            }
            else
            {
                this.FacingDirection = diff.Y > 0 ? 2 : 0; // 아래 또는 위
            }
        }

        /// <summary>
        /// 도구 타입에 따른 도구 인스턴스를 생성합니다.
        /// </summary>
        private Tool CreateTool(ToolType toolType)
        {
            return toolType switch
            {
                ToolType.Hoe => new Hoe(),
                ToolType.Axe => new Axe(),
                ToolType.WateringCan => new WateringCan(),
                ToolType.Pickaxe => new Pickaxe(),
                _ => null
            };
        }

        /// <summary>
        /// 도구 타입별 사용 시간을 반환합니다.
        /// </summary>
        private float GetToolUseDuration(ToolType toolType)
        {
            return toolType switch
            {
                ToolType.Hoe => 1.5f,
                ToolType.Axe => 2.5f,
                ToolType.WateringCan => 1.0f,
                ToolType.Pickaxe => 2.0f,
                _ => 1.0f
            };
        }

        #endregion

        #region 렌더링 관련 메서드

        /// <summary>
        /// NPC를 화면에 그릴 때 호출됩니다. 도구도 함께 렌더링합니다.
        /// </summary>
        public override void draw(SpriteBatch b, float alpha = 1f)
        {
            // 기본 NPC 렌더링
            base.draw(b, alpha);

            // 도구 사용 중일 때 도구도 함께 렌더링
            if (_isUsingTool && _currentTool != null && _requiredToolType != ToolType.None)
            {
                DrawTool(b, alpha);
            }
        }

        /// <summary>
        /// 도구를 렌더링합니다.
        /// </summary>
        private void DrawTool(SpriteBatch spriteBatch, float alpha)
        {
            try
            {
                // 도구 텍스처 가져오기
                var toolTexture = GetToolTexture(_requiredToolType);
                if (toolTexture == null) return;

                // NPC 위치와 방향에 따른 도구 위치 계산
                var toolPosition = CalculateToolPosition();
                var toolSourceRect = GetToolSourceRect(_requiredToolType);
                var toolRotation = GetToolRotation();
                var toolOrigin = GetToolOrigin(_requiredToolType);

                // 도구 렌더링
                spriteBatch.Draw(
                    toolTexture,
                    Game1.GlobalToLocal(Game1.viewport, toolPosition),
                    toolSourceRect,
                    Color.White * alpha,
                    toolRotation,
                    toolOrigin,
                    4f, // 스케일
                    SpriteEffects.None,
                    (float)((this.Position.Y + 64f) / 10000f + 0.001f) // 깊이
                );
            }
            catch (Exception ex)
            {
                _monitor.Log($"도구 렌더링 중 오류: {ex.Message}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 도구 타입에 따른 텍스처를 반환합니다.
        /// </summary>
        private Texture2D? GetToolTexture(ToolType toolType)
        {
            try
            {
                return toolType switch
                {
                    ToolType.Hoe => Game1.content.Load<Texture2D>("TileSheets\\tools"),
                    ToolType.Axe => Game1.content.Load<Texture2D>("TileSheets\\tools"),
                    ToolType.WateringCan => Game1.content.Load<Texture2D>("TileSheets\\tools"),
                    ToolType.Pickaxe => Game1.content.Load<Texture2D>("TileSheets\\tools"),
                    _ => null
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 도구 타입에 따른 소스 사각형을 반환합니다.
        /// </summary>
        private Rectangle GetToolSourceRect(ToolType toolType)
        {
            return toolType switch
            {
                ToolType.Hoe => new Rectangle(0, 0, 16, 16),       // 호미
                ToolType.Axe => new Rectangle(16, 0, 16, 16),      // 도끼
                ToolType.WateringCan => new Rectangle(32, 0, 16, 16), // 물뿌리개
                ToolType.Pickaxe => new Rectangle(48, 0, 16, 16),  // 곡괭이
                _ => new Rectangle(0, 0, 16, 16)
            };
        }

        /// <summary>
        /// NPC 방향과 애니메이션에 따른 도구 위치를 계산합니다.
        /// </summary>
        private Vector2 CalculateToolPosition()
        {
            var basePosition = this.Position;
            var animationProgress = GetToolUseProgress();
            
            // 방향에 따른 기본 오프셋
            var offset = this.FacingDirection switch
            {
                0 => new Vector2(8f, -16f),  // 위쪽
                1 => new Vector2(24f, 8f),   // 오른쪽
                2 => new Vector2(8f, 24f),   // 아래쪽
                3 => new Vector2(-16f, 8f),  // 왼쪽
                _ => new Vector2(8f, 8f)
            };

            // 애니메이션에 따른 추가 오프셋
            if (_isUsingTool)
            {
                var animOffset = CalculateAnimationOffset(animationProgress);
                offset += animOffset;
            }

            return basePosition + offset;
        }

        /// <summary>
        /// 애니메이션 진행도에 따른 추가 오프셋을 계산합니다.
        /// </summary>
        private Vector2 CalculateAnimationOffset(float progress)
        {
            var swingIntensity = 12f; // 휘두름 강도
            var swingOffset = (float)Math.Sin(progress * Math.PI) * swingIntensity;

            return this.FacingDirection switch
            {
                0 => new Vector2(swingOffset * 0.5f, -swingOffset),      // 위쪽
                1 => new Vector2(swingOffset, swingOffset * 0.5f),       // 오른쪽
                2 => new Vector2(swingOffset * 0.5f, swingOffset),       // 아래쪽
                3 => new Vector2(-swingOffset, swingOffset * 0.5f),      // 왼쪽
                _ => Vector2.Zero
            };
        }

        /// <summary>
        /// 도구 회전값을 계산합니다.
        /// </summary>
        private float GetToolRotation()
        {
            if (!_isUsingTool) return 0f;

            var animationProgress = GetToolUseProgress();
            var baseRotation = this.FacingDirection switch
            {
                0 => 0f,           // 위쪽
                1 => MathHelper.PiOver2,  // 오른쪽 (90도)
                2 => MathHelper.Pi,       // 아래쪽 (180도)
                3 => -MathHelper.PiOver2, // 왼쪽 (-90도)
                _ => 0f
            };

            // 애니메이션 기반 추가 회전
            var swingRotation = (float)Math.Sin(animationProgress * Math.PI) * 0.5f;
            return baseRotation + swingRotation;
        }

        /// <summary>
        /// 도구 원점을 반환합니다.
        /// </summary>
        private Vector2 GetToolOrigin(ToolType toolType)
        {
            return toolType switch
            {
                ToolType.Hoe => new Vector2(8f, 14f),
                ToolType.Axe => new Vector2(8f, 14f),
                ToolType.WateringCan => new Vector2(8f, 8f),
                ToolType.Pickaxe => new Vector2(8f, 14f),
                _ => new Vector2(8f, 8f)
            };
        }

        #endregion
    }

    /// <summary>
    /// 도구 타입 열거형
    /// </summary>
    public enum ToolType
    {
        None,
        Hoe,        // 호미
        Axe,        // 도끼
        WateringCan, // 물뿌리개
        Pickaxe     // 곡괭이
    }
}

