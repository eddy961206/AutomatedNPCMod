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
        private PathfindingController _pathfindingController;
        private Vector2? _destination;
        private AIState _currentState;

        public AIController(CustomNPC npc)
        {
            _npc = npc;
            _currentState = AIState.Idle;
        }

        /// <summary>
        /// AI를 업데이트합니다.
        /// </summary>
        /// <param name="gameTime">게임 시간</param>
        public void Update(GameTime gameTime)
        {
            if (_pathfindingController != null)
            {
                if (_pathfindingController.update(gameTime))
                {
                    _pathfindingController = null; // 경로 완료
                    _currentState = AIState.Working;
                }
            }

            switch (_currentState)
            {
                case AIState.Idle:
                    HandleIdleState();
                    break;
                case AIState.Moving:
                    // PathfindingController가 이동을 처리
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
            var currentPos = _npc.getTileLocationPoint();
            var targetPoint = new Point((int)target.X, (int)target.Y);

            if (currentPos == targetPoint)
            {
                _currentState = AIState.Working;
                return;
            }

            _pathfindingController = new PathfindingController(_npc, _npc.currentLocation, targetPoint, 2);
            if (_pathfindingController.pathToEndPoint == null || _pathfindingController.pathToEndPoint.Count == 0)
            {
                _currentState = AIState.Idle; // 경로를 찾을 수 없음
            }
            else
            {
                _currentState = AIState.Moving;
            }
        }

        /// <summary>
        /// 이동을 중지합니다.
        /// </summary>
        public void StopMoving()
        {
            _destination = null;
            if (_pathfindingController != null)
            {
                _pathfindingController = null;
            }
            _currentState = AIState.Idle;
        }

        /// <summary>
        /// 대기 상태를 처리합니다.
        /// </summary>
        private void HandleIdleState()
        {
        }

        /// <summary>
        /// 작업 상태를 처리합니다.
        /// </summary>
        private void HandleWorkingState()
        {
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
            return _pathfindingController == null && _currentState != AIState.Moving;
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

        private bool ShouldStartUsingTool()
        {
            var currentTask = _npc.GetCurrentTask();
            if (currentTask == null) return false;

            return currentTask.Type switch
            {
                TaskType.Farming => true,
                TaskType.Woodcutting => true,
                TaskType.Mining => true,
                TaskType.Foraging => false,
                _ => false
            };
        }

        private bool IsNPCUsingTool()
        {
            return _npc.IsUsingTool();
        }

        public bool IsUsingTool()
        {
            return _currentState == AIState.UsingTool;
        }

        public void StartUsingTool(Vector2 targetTile)
        {
            _currentState = AIState.UsingTool;
            _npc.StartUsingTool(targetTile);
        }

        public void StopUsingTool()
        {
            if (_currentState == AIState.UsingTool)
            {
                _currentState = AIState.Working;
                _npc.StopUsingTool();
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
}

