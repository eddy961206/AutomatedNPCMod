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
                var playerPosition = Game1.player.getTileLocation();
                var npcPosition = new Vector2(playerPosition.X + 2, playerPosition.Y);
                
                var npcName = $"Worker_{DateTime.Now.Ticks % 1000}";
                
                if (_npcManager.CreateNPC(npcName, npcPosition))
                {
                    Game1.addHUDMessage(new HUDMessage($"NPC '{npcName}' 생성됨!", HUDMessage.newQuest_type));
                    _monitor.Log($"테스트 NPC '{npcName}' 생성 완료", LogLevel.Info);
                }
                else
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
                var npcs = _npcManager.GetAllNPCs();
                if (npcs.Count == 0)
                {
                    Game1.addHUDMessage(new HUDMessage("작업을 할당할 NPC가 없습니다!", HUDMessage.error_type));
                    return;
                }

                var playerPosition = Game1.player.getTileLocation();
                var taskPosition = new Vector2(playerPosition.X + 1, playerPosition.Y + 1);

                var task = _taskManager.CreateTask(TaskType.Farming, taskPosition, TaskPriority.Normal);
                
                // 첫 번째 사용 가능한 NPC에게 작업 할당
                var targetNPC = npcs[0];
                if (_taskManager.AssignTask(targetNPC.Name, task))
                {
                    Game1.addHUDMessage(new HUDMessage($"'{targetNPC.Name}'에게 농사 작업 할당됨!", HUDMessage.newQuest_type));
                    _monitor.Log($"농사 작업이 '{targetNPC.Name}'에게 할당됨", LogLevel.Info);
                }
                else
                {
                    Game1.addHUDMessage(new HUDMessage("작업 할당 실패!", HUDMessage.error_type));
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"농사 작업 할당 중 오류 발생: {ex.Message}", LogLevel.Error);
                Game1.addHUDMessage(new HUDMessage("작업 할당 중 오류 발생!", HUDMessage.error_type));
            }
        }

        /// <summary>
        /// 현재 활성 NPC들의 정보를 표시합니다.
        /// </summary>
        private void ShowNPCInfo()
        {
            try
            {
                var npcs = _npcManager.GetAllNPCs();
                if (npcs.Count == 0)
                {
                    Game1.addHUDMessage(new HUDMessage("활성 NPC가 없습니다.", HUDMessage.newQuest_type));
                    return;
                }

                var infoMessage = $"활성 NPC: {npcs.Count}개";
                foreach (var npc in npcs)
                {
                    var position = new Vector2(npc.getTileX(), npc.getTileY());
                    var status = npc.IsBusy() ? "작업 중" : "대기 중";
                    infoMessage += $"\n- {npc.Name}: {status} ({position.X}, {position.Y})";
                }

                Game1.addHUDMessage(new HUDMessage(infoMessage, HUDMessage.newQuest_type));
                _monitor.Log($"NPC 정보 표시: {npcs.Count}개의 NPC", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"NPC 정보 표시 중 오류 발생: {ex.Message}", LogLevel.Error);
                Game1.addHUDMessage(new HUDMessage("NPC 정보 표시 중 오류 발생!", HUDMessage.error_type));
            }
        }

        /// <summary>
        /// 모든 NPC를 제거합니다.
        /// </summary>
        private void RemoveAllNPCs()
        {
            try
            {
                var npcs = _npcManager.GetAllNPCs();
                var count = npcs.Count;

                foreach (var npc in npcs)
                {
                    _npcManager.RemoveNPC(npc.Name);
                }

                Game1.addHUDMessage(new HUDMessage($"{count}개의 NPC가 제거됨!", HUDMessage.newQuest_type));
                _monitor.Log($"{count}개의 NPC 제거 완료", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"NPC 제거 중 오류 발생: {ex.Message}", LogLevel.Error);
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

