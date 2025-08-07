using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace AutomatedNPCMod.Models
{
    /// <summary>
    /// NPC의 지능적인 행동을 담당하는 AI 제어기 클래스.
    /// </summary>
    public class AIController
    {
        private readonly CustomNPC _npc;
        private readonly Pathfinder _pathfinder;
        private Vector2? _destination;
        private Queue<Vector2> _currentPath;
        private AIState _currentState;
        private Vector2 _nextTile;
        private float _moveTimer;
        private const float MOVE_INTERVAL = 0.5f; // 타일 간 이동 시간 (초)

        public AIController(CustomNPC npc)
        {
            _npc = npc;
            _pathfinder = new Pathfinder();
            _currentState = AIState.Idle;
            _currentPath = new Queue<Vector2>();
            _moveTimer = 0f;
        }

        /// <summary>
        /// AI를 업데이트합니다.
        /// </summary>
        /// <param name="gameTime">게임 시간</param>
        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _moveTimer += deltaTime;

            switch (_currentState)
            {
                case AIState.Idle:
                    HandleIdleState();
                    break;
                case AIState.Moving:
                    HandleMovingState(deltaTime);
                    break;
                case AIState.Working:
                    HandleWorkingState();
                    break;
                case AIState.UsingTool:
                    HandleUsingToolState();
                    break;
            }
        }

        /// <summary>
        /// 목표 지점을 설정하고 경로를 계산합니다.
        /// </summary>
        /// <param name="target">목표 위치</param>
        public void SetDestination(Vector2 target)
        {
            _destination = target;
            var currentPos = _npc.GetCurrentTilePosition();
            
            // 이미 목표 지점에 있는 경우
            if (Vector2.Distance(currentPos, target) < 1.0f)
            {
                _currentState = AIState.Working;
                return;
            }

            // 경로 계산
            _currentPath = _pathfinder.FindPath(currentPos, target, _npc.currentLocation);
            
            if (_currentPath.Count > 0)
            {
                _currentState = AIState.Moving;
                _nextTile = _currentPath.Dequeue();
                _moveTimer = 0f;
            }
            else
            {
                // 경로를 찾을 수 없는 경우
                _currentState = AIState.Idle;
            }
        }

        /// <summary>
        /// 이동을 중지합니다.
        /// </summary>
        public void StopMoving()
        {
            _destination = null;
            _currentPath.Clear();
            _currentState = AIState.Idle;
        }

        /// <summary>
        /// 대기 상태를 처리합니다.
        /// </summary>
        private void HandleIdleState()
        {
            // 대기 중일 때의 행동 (예: 주변 둘러보기, 애니메이션 등)
            // 현재는 단순히 대기
        }

        /// <summary>
        /// 이동 상태를 처리합니다.
        /// </summary>
        /// <param name="deltaTime">프레임 간 시간</param>
        private void HandleMovingState(float deltaTime)
        {
            if (!_destination.HasValue) 
            {
                _currentState = AIState.Idle;
                return;
            }

            var currentPos = _npc.GetCurrentTilePosition();
            var destX = _destination.Value.X;
            var destY = _destination.Value.Y;

            // 거리 계산
            var distance = Math.Sqrt(Math.Pow(destX - currentPos.X, 2) + Math.Pow(destY - currentPos.Y, 2));
            
            // 목표 지점 근처에 도착했으면 작업 시작
            if (distance < 1.5f)
            {
                _currentState = AIState.Working;
                _moveTimer = 0f;
                return;
            }

            // 자연스러운 이동을 위해 더 작은 단위로 이동
            _moveTimer += deltaTime;
            
            if (_moveTimer >= 0.3f) // 0.3초마다 이동 (더 자주)
            {
                // 방향 계산 (더 정밀하게)
                var deltaX = destX - currentPos.X;
                var deltaY = destY - currentPos.Y;
                
                // 거리가 멀면 더 큰 스텝, 가까우면 작은 스텝
                var stepSize = distance > 3.0f ? 0.5f : 0.25f;
                
                var moveX = 0f;
                var moveY = 0f;
                
                if (Math.Abs(deltaX) > 0.1f)
                    moveX = deltaX > 0 ? stepSize : -stepSize;
                if (Math.Abs(deltaY) > 0.1f)
                    moveY = deltaY > 0 ? stepSize : -stepSize;
                
                // Stardew Valley NPC 이동 시스템 사용
                if (TryMoveNPCInGame(_npc, (int)Math.Sign(moveX), (int)Math.Sign(moveY)))
                {
                    // 게임의 이동 시스템 사용 성공
                }
                else
                {
                    // 대안: 부드러운 위치 업데이트
                    var newX = currentPos.X + moveX;
                    var newY = currentPos.Y + moveY;
                    var newPosition = new Vector2(newX * 64f, newY * 64f);
                    _npc.Position = newPosition;
                }
                
                _moveTimer = 0f;
            }
        }

        /// <summary>
        /// 작업 상태를 처리합니다.
        /// </summary>
        private void HandleWorkingState()
        {
            // 작업 중일 때의 행동
            // 실제 작업은 WorkExecutor에서 처리되므로 여기서는 상태만 유지
            // 도구가 필요한 작업인 경우 UsingTool 상태로 전환
            if (ShouldStartUsingTool())
            {
                _currentState = AIState.UsingTool;
            }
        }

        /// <summary>
        /// 도구 사용 상태를 처리합니다.
        /// </summary>
        private void HandleUsingToolState()
        {
            // 도구 사용 중일 때의 행동
            // CustomNPC에서 도구 사용이 완료되면 Working 상태로 돌아감
            if (!IsNPCUsingTool())
            {
                _currentState = AIState.Working;
            }
        }

        /// <summary>
        /// 목표 지점에 도달했는지 확인합니다.
        /// </summary>
        /// <returns>도달 여부</returns>
        public bool HasReachedDestination()
        {
            if (!_destination.HasValue) return false;
            
            var currentPos = _npc.GetCurrentTilePosition();
            return Vector2.Distance(currentPos, _destination.Value) < 1.0f;
        }

        /// <summary>
        /// 현재 AI 상태를 반환합니다.
        /// </summary>
        public AIState GetCurrentState()
        {
            return _currentState;
        }

        /// <summary>
        /// AI 상태를 설정합니다.
        /// </summary>
        /// <param name="state">설정할 상태</param>
        public void SetState(AIState state)
        {
            _currentState = state;
        }

        /// <summary>
        /// 도구 사용을 시작해야 하는지 확인합니다.
        /// </summary>
        /// <returns>도구 사용 시작 여부</returns>
        private bool ShouldStartUsingTool()
        {
            // CustomNPC의 현재 작업이 도구가 필요한 작업인지 확인
            var currentTask = _npc.GetCurrentTask();
            if (currentTask == null) return false;

            // 작업 타입에 따라 도구 필요 여부 확인
            return currentTask.Type switch
            {
                TaskType.Farming => true,    // 농사는 호미/물뿌리개 필요
                TaskType.Woodcutting => true, // 나무 베기는 도끼 필요
                TaskType.Mining => true,      // 채굴은 곡괭이 필요
                TaskType.Foraging => false,   // 채집은 도구 불필요
                _ => false
            };
        }

        /// <summary>
        /// NPC가 현재 도구를 사용 중인지 확인합니다.
        /// </summary>
        /// <returns>도구 사용 중 여부</returns>
        private bool IsNPCUsingTool()
        {
            return _npc.IsUsingTool();
        }

        /// <summary>
        /// 도구 사용 상태인지 확인합니다.
        /// </summary>
        /// <returns>도구 사용 상태 여부</returns>
        public bool IsUsingTool()
        {
            return _currentState == AIState.UsingTool;
        }

        /// <summary>
        /// 도구 사용 상태로 전환합니다.
        /// </summary>
        /// <param name="targetTile">도구 사용 대상 타일</param>
        public void StartUsingTool(Vector2 targetTile)
        {
            _currentState = AIState.UsingTool;
            // NPC에게 도구 사용 시작을 알림
            _npc.StartUsingTool(targetTile);
        }

        /// <summary>
        /// 도구 사용을 중지하고 이전 상태로 돌아갑니다.
        /// </summary>
        public void StopUsingTool()
        {
            if (_currentState == AIState.UsingTool)
            {
                _currentState = AIState.Working;
                _npc.StopUsingTool();
            }
        }

        /// <summary>
        /// Stardew Valley의 NPC 이동 시스템을 사용해서 NPC를 이동시킵니다.
        /// </summary>
        /// <param name="npc">이동시킬 NPC</param>
        /// <param name="moveX">X 방향 이동 (-1, 0, 1)</param>
        /// <param name="moveY">Y 방향 이동 (-1, 0, 1)</param>
        /// <returns>이동 성공 여부</returns>
        private bool TryMoveNPCInGame(NPC npc, int moveX, int moveY)
        {
            try
            {
                // 여러 Stardew Valley NPC 이동 방법 시도
                
                // 방법 1: moveTowardDirection 메서드 사용
                var moveTowardMethod = npc.GetType().GetMethod("moveTowardDirection");
                if (moveTowardMethod != null)
                {
                    // 방향 계산 (0=up, 1=right, 2=down, 3=left)
                    int direction = -1;
                    if (moveY < 0) direction = 0; // up
                    else if (moveX > 0) direction = 1; // right
                    else if (moveY > 0) direction = 2; // down
                    else if (moveX < 0) direction = 3; // left
                    
                    if (direction >= 0)
                    {
                        moveTowardMethod.Invoke(npc, new object[] { direction, false });
                        return true;
                    }
                }

                // 방법 2: velocity 설정
                var velocityField = npc.GetType().GetField("velocity");
                var velocityProp = npc.GetType().GetProperty("velocity");
                
                if (velocityField != null || velocityProp != null)
                {
                    var velocity = new Vector2((float)moveX * 2f, (float)moveY * 2f); // 속도 조정
                    
                    if (velocityField != null)
                    {
                        velocityField.SetValue(npc, velocity);
                    }
                    else
                    {
                        velocityProp?.SetValue(npc, velocity);
                    }
                    
                    return true;
                }

                // 방법 3: speed와 방향 설정
                var speedField = npc.GetType().GetField("speed");
                var facingDirectionField = npc.GetType().GetField("facingDirection");
                
                if (speedField != null && facingDirectionField != null)
                {
                    speedField.SetValue(npc, 2); // 이동 속도
                    
                    // 방향 설정
                    int direction = 2; // 기본값 (down)
                    if (moveY < 0) direction = 0; // up
                    else if (moveX > 0) direction = 1; // right
                    else if (moveY > 0) direction = 2; // down
                    else if (moveX < 0) direction = 3; // left
                    
                    facingDirectionField.SetValue(npc, direction);
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// AI 상태를 정의하는 열거형.
    /// </summary>
    public enum AIState
    {
        Idle,       // 대기
        Moving,     // 이동 중
        Working,    // 작업 중
        UsingTool,  // 도구 사용 중
        Returning,  // 복귀 중
        Resting     // 휴식 중
    }

    /// <summary>
    /// 경로 탐색을 담당하는 클래스.
    /// </summary>
    public class Pathfinder
    {
        /// <summary>
        /// A* 알고리즘을 사용하여 경로를 찾습니다.
        /// </summary>
        /// <param name="start">시작 위치</param>
        /// <param name="end">목표 위치</param>
        /// <param name="location">게임 위치</param>
        /// <returns>경로 (타일 좌표 큐)</returns>
        public Queue<Vector2> FindPath(Vector2 start, Vector2 end, GameLocation location)
        {
            var path = new Queue<Vector2>();

            // 간단한 직선 경로 (실제 구현에서는 A* 알고리즘 사용)
            // 현재는 프로토타입이므로 단순한 경로 계산
            var current = start;
            var target = end;

            while (Vector2.Distance(current, target) > 1.0f)
            {
                var direction = target - current;
                direction.Normalize();

                // 가장 가까운 방향으로 한 타일씩 이동
                Vector2 nextTile;
                if (Math.Abs(direction.X) > Math.Abs(direction.Y))
                {
                    nextTile = new Vector2(current.X + Math.Sign(direction.X), current.Y);
                }
                else
                {
                    nextTile = new Vector2(current.X, current.Y + Math.Sign(direction.Y));
                }

                // 이동 가능한 타일인지 확인
                if (IsValidTile(nextTile, location))
                {
                    path.Enqueue(nextTile);
                    current = nextTile;
                }
                else
                {
                    // 장애물이 있는 경우 우회 경로 시도 (간단한 구현)
                    var alternativeTile = new Vector2(current.X + Math.Sign(direction.Y), current.Y + Math.Sign(direction.X));
                    if (IsValidTile(alternativeTile, location))
                    {
                        path.Enqueue(alternativeTile);
                        current = alternativeTile;
                    }
                    else
                    {
                        // 경로를 찾을 수 없는 경우 중단
                        break;
                    }
                }

                // 무한 루프 방지
                if (path.Count > 100)
                {
                    break;
                }
            }

            return path;
        }

        /// <summary>
        /// 지정된 타일이 이동 가능한지 확인합니다.
        /// </summary>
        /// <param name="tile">확인할 타일</param>
        /// <param name="location">게임 위치</param>
        /// <returns>이동 가능 여부</returns>
        private bool IsValidTile(Vector2 tile, GameLocation location)
        {
            if (location == null) return false;

            // 맵 경계 확인
            if (tile.X < 0 || tile.Y < 0 || tile.X >= location.Map.Layers[0].LayerWidth || tile.Y >= location.Map.Layers[0].LayerHeight)
            {
                return false;
            }

            // 충돌 확인 (간단한 구현)
            return location.isTilePassable(new xTile.Dimensions.Location((int)tile.X, (int)tile.Y), Game1.viewport);
        }
    }
}

