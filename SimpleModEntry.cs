using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;

namespace AutomatedNPCMod
{
    // 모드 설정 및 상수
    internal static class ModConfig
    {
        public const int UpdateIntervalTicks = 15; // 4 times per second
        public const int WorkScanRadius = 10;
        public const int ExploreIntervalTicks = 60; // 15 seconds
        public const float ArrivalDistance = 1.5f;
        public const float WorkDuration = 3.0f;
        public const int MapBoundaryMin = 5;
        public const int MapBoundaryMax = 95;
        public const int TileSize = 64;
    }

    public class SimpleModEntry : Mod
    {
        private Core.NPCManager npcManager;
        private Core.TaskManager taskManager;
        private Core.UIManager uiManager;
        private int updateCounter = 0;
        
        public override void Entry(IModHelper helper)
        {
            this.Monitor.Log("Automated NPC Mod 로드됨!", LogLevel.Info);
            
            // 매니저 클래스들 초기화
            npcManager = new Core.NPCManager(helper, this.Monitor);
            taskManager = new Core.TaskManager(npcManager, helper, this.Monitor);
            uiManager = new Core.UIManager(npcManager, taskManager, helper, this.Monitor);
            
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.Saving += OnSaving;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            try
            {
                npcManager.LoadNPCData();
                taskManager.LoadTaskData();
                this.Monitor.Log("NPC 및 작업 데이터 로드 완료", LogLevel.Info);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"데이터 로드 중 오류: {ex.Message}", LogLevel.Error);
            }
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            try
            {
                npcManager.SaveNPCData();
                taskManager.SaveTaskData();
                this.Monitor.Log("NPC 및 작업 데이터 저장 완료", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"데이터 저장 중 오류: {ex.Message}", LogLevel.Error);
            }
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady) return;
            
            // 성능을 위해 매 15틱마다 업데이트 (약 4fps)
            if (e.IsMultipleOf(ModConfig.UpdateIntervalTicks))
            {
                updateCounter++;
                
                try
                {
                    npcManager.UpdateAllNPCs(new GameTime());
                    taskManager.UpdateTasks();
                    
                    if (updateCounter % 20 == 0 && npcManager.GetNPCCount() > 0)
                    {
                        this.Monitor.Log($"업데이트 틱: {updateCounter}, 관리 중인 NPC: {npcManager.GetNPCCount()}개", LogLevel.Trace);
                    }
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"업데이트 중 오류: {ex.Message}", LogLevel.Error);
                }
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady) return;

            switch (e.Button)
            {
                case SButton.F9:
                    this.Monitor.Log("F9 - NPC 생성 시도 중...", LogLevel.Info);
                    try
                    {
                        var success = npcManager.CreateTestNPC();
                        var message = success ? "NPC 생성 완료!" : "NPC 생성 실패";
                        this.Monitor.Log(message, success ? LogLevel.Info : LogLevel.Warn);
                    }
                    catch (Exception ex)
                    {
                        this.Monitor.Log($"NPC 생성 실패: {ex.Message}", LogLevel.Error);
                    }
                    break;

                case SButton.F10:
                    this.Monitor.Log("F10 - 농사 작업 할당 중...", LogLevel.Info);
                    try
                    {
                        taskManager.AssignTaskToAvailableNPC(npcManager);
                    }
                    catch (Exception ex)
                    {
                        this.Monitor.Log($"작업 할당 실패: {ex.Message}", LogLevel.Error);
                    }
                    break;

                case SButton.F11:
                    this.Monitor.Log("F11 - NPC 상태 확인 중...", LogLevel.Info);
                    uiManager.ShowNPCStatus(npcManager, taskManager);
                    break;

                case SButton.F12:
                    this.Monitor.Log("F12 - 모든 NPC 제거 중...", LogLevel.Info);
                    try
                    {
                        var removedCount = npcManager.RemoveAllNPCs();
                        taskManager.ClearAllTasks();
                        this.Monitor.Log($"NPC 제거 완료: {removedCount}개", LogLevel.Info);
                    }
                    catch (Exception ex)
                    {
                        this.Monitor.Log($"NPC 제거 실패: {ex.Message}", LogLevel.Error);
                    }
                    break;
            }
        }
    }
}