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
            if (_moveTimer >= MOVE_INTERVAL)
            {
                // 다음 타일로 이동
                var targetPixelPos = _nextTile * 64f; // 타일 좌표를 픽셀 좌표로 변환
                _npc.Position = targetPixelPos;
                _npc.setTileLocation(_nextTile);

                _moveTimer = 0f;

                // 다음 경로가 있는지 확인
                if (_currentPath.Count > 0)
                {
                    _nextTile = _currentPath.Dequeue();
                }
                else
                {
                    // 목표 지점에 도달
                    if (HasReachedDestination())
                    {
                        _currentState = AIState.Working;
                    }
                    else
                    {
                        _currentState = AIState.Idle;
                    }
                }
            }
        }

        /// <summary>
        /// 작업 상태를 처리합니다.
        /// </summary>
        private void HandleWorkingState()
        {
            // 작업 중일 때의 행동
            // 실제 작업은 WorkExecutor에서 처리되므로 여기서는 상태만 유지
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
    }

    /// <summary>
    /// AI 상태를 정의하는 열거형.
    /// </summary>
    public enum AIState
    {
        Idle,       // 대기
        Moving,     // 이동 중
        Working,    // 작업 중
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

