using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using AutomatedNPCMod.Models;

namespace AutomatedNPCMod.Core
{
    /// <summary>
    /// 플레이어와 모드 간의 상호작용을 담당하는 사용자 인터페이스를 관리하는 클래스.
    /// </summary>
    public class UIManager
    {
        private readonly NPCManager _npcManager;
        private readonly TaskManager _taskManager;
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;

        public UIManager(NPCManager npcManager, TaskManager taskManager, IModHelper helper, IMonitor monitor)
        {
            _npcManager = npcManager;
            _taskManager = taskManager;
            _helper = helper;
            _monitor = monitor;
        }

        /// <summary>
        /// 플레이어 입력을 처리합니다.
        /// </summary>
        /// <param name="button">눌린 버튼</param>
        public void HandleInput(SButton button)
        {
            try
            {
                // F9 키: NPC 생성 (테스트용)
                if (button == SButton.F9)
                {
                    CreateTestNPC();
                }
                // F10 키: 농사 작업 할당 (테스트용)
                else if (button == SButton.F10)
                {
                    AssignFarmingTask();
                }
                // F11 키: NPC 정보 표시
                else if (button == SButton.F11)
                {
                    ShowNPCInfo();
                }
                // F12 키: 모든 NPC 제거 (테스트용)
                else if (button == SButton.F12)
                {
                    RemoveAllNPCs();
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"입력 처리 중 오류 발생: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 테스트용 NPC를 생성합니다.
        /// </summary>
        private void CreateTestNPC()
        {
            try
            {
                var success = _npcManager.CreateTestNPC();
                if (!success)
                {
                    Game1.addHUDMessage(new HUDMessage("NPC 생성 실패!", HUDMessage.error_type));
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"테스트 NPC 생성 중 오류 발생: {ex.Message}", LogLevel.Error);
                Game1.addHUDMessage(new HUDMessage("NPC 생성 중 오류 발생!", HUDMessage.error_type));
            }
        }

        /// <summary>
        /// 농사 작업을 할당합니다.
        /// </summary>
        private void AssignFarmingTask()
        {
            try
            {
                _taskManager.AssignTaskToAvailableNPC(_npcManager);
            }
            catch (Exception ex)
            {
                _monitor.Log($"작업 할당 중 오류: {ex.Message}", LogLevel.Error);
                Game1.addHUDMessage(new HUDMessage("작업 할당 중 오류 발생!", HUDMessage.error_type));
            }
        }

        /// <summary>
        /// 현재 활성 NPC들의 정보를 표시합니다.
        /// </summary>
        private void ShowNPCInfo()
        {
            ShowNPCStatus(_npcManager, _taskManager);
        }

        /// <summary>
        /// NPC 상태 정보를 표시합니다.
        /// </summary>
        /// <param name="npcManager">NPC 매니저</param>
        /// <param name="taskManager">작업 매니저</param>
        public void ShowNPCStatus(NPCManager npcManager, TaskManager taskManager)
        {
            try
            {
                var npcs = npcManager.GetAllNPCs();
                if (npcs.Count == 0)
                {
                    Game1.addHUDMessage(new HUDMessage("활성 NPC가 없습니다.", HUDMessage.newQuest_type));
                    _monitor.Log("NPC 상태 확인: 활성 NPC 없음", LogLevel.Info);
                    return;
                }

                var statusMessage = $"활성 NPC: {npcs.Count}개";
                var completedTasks = taskManager.GetCompletedTaskCount();
                var activeTasks = taskManager.GetActiveTaskCount();
                var pendingTasks = taskManager.GetPendingTaskCount();
                var workingNPCs = npcs.Count(n => n.IsBusy());
                
                statusMessage += $"\n작업 상태: 완료 {completedTasks}개, 활성 {activeTasks}개, 대기 {pendingTasks}개";
                statusMessage += $"\n활동 중인 NPC: {workingNPCs}개";
                
                // 각 NPC의 상태 정보 추가 (최대 3개만 표시)
                for (int i = 0; i < Math.Min(npcs.Count, 3); i++)
                {
                    var npc = npcs[i];
                    var stateText = npc.IsBusy() ? "작업중" : "대기";
                    var position = npc.getTileLocation();
                    statusMessage += $"\n- {npc.Name}: {stateText} ({position.X:F1}, {position.Y:F1})";
                }
                
                Game1.addHUDMessage(new HUDMessage(statusMessage, HUDMessage.newQuest_type));
                _monitor.Log($"NPC 상태 표시: {npcs.Count}개 NPC, {activeTasks}개 작업", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"NPC 상태 확인 중 오류: {ex.Message}", LogLevel.Error);
                Game1.addHUDMessage(new HUDMessage("NPC 상태 확인 중 오류 발생!", HUDMessage.error_type));
            }
        }

        /// <summary>
        /// 모든 NPC를 제거합니다.
        /// </summary>
        private void RemoveAllNPCs()
        {
            try
            {
                var removedCount = _npcManager.RemoveAllNPCs();
                _taskManager.ClearAllTasks();
                _monitor.Log($"NPC 제거 완료: {removedCount}개", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"NPC 제거 실패: {ex.Message}", LogLevel.Error);
                Game1.addHUDMessage(new HUDMessage("NPC 제거 중 오류 발생!", HUDMessage.error_type));
            }
        }

        /// <summary>
        /// 도움말 메시지를 표시합니다.
        /// </summary>
        public void ShowHelp()
        {
            var helpMessage = "자동화 NPC 모드 단축키:\n" +
                             "F9: 테스트 NPC 생성\n" +
                             "F10: 농사 작업 할당\n" +
                             "F11: NPC 정보 표시\n" +
                             "F12: 모든 NPC 제거";

            Game1.addHUDMessage(new HUDMessage(helpMessage, HUDMessage.newQuest_type));
        }
    }
}

