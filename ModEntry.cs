using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using AutomatedNPCMod.Core;

namespace AutomatedNPCMod
{
    /// <summary>
    /// 자동화 NPC 모드의 진입점 클래스
    /// SMAPI 모드의 생명주기를 관리하고 핵심 구성 요소들을 초기화합니다.
    /// </summary>
    public class ModEntry : Mod
    {
        private NPCManager npcManager;
        private TaskManager taskManager;
        private UIManager uiManager;
        private ConfigManager configManager;

        public static ModEntry Instance { get; private set; }

        /// <summary>
        /// 모드 진입점. SMAPI에 의해 호출됩니다.
        /// </summary>
        /// <param name="helper">SMAPI 헬퍼 인터페이스</param>
        public override void Entry(IModHelper helper)
        {
            Instance = this;
            try
            {
                // 로깅
                this.Monitor.Log("Automated NPC Mod 초기화 시작", LogLevel.Info);

                // 구성 요소 초기화
                InitializeComponents();

                // SMAPI 이벤트 구독
                SubscribeToEvents(helper);

                this.Monitor.Log("Automated NPC Mod 초기화 완료", LogLevel.Info);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"모드 초기화 중 오류 발생: {ex.Message}", LogLevel.Error);
                this.Monitor.Log($"스택 트레이스: {ex.StackTrace}", LogLevel.Trace);
            }
        }

        /// <summary>
        /// 핵심 구성 요소들을 초기화합니다.
        /// </summary>
        private void InitializeComponents()
        {
            // 설정 관리자 초기화
            configManager = new ConfigManager(this.Helper, this.Monitor);

            // NPC 관리자 초기화
            npcManager = new NPCManager(this.Helper, this.Monitor);

            // 작업 관리자 초기화
            taskManager = new TaskManager(npcManager, this.Helper, this.Monitor);

            // UI 관리자 초기화
            uiManager = new UIManager(npcManager, taskManager, this.Helper, this.Monitor);
        }

        /// <summary>
        /// SMAPI 이벤트를 구독합니다.
        /// </summary>
        /// <param name="helper">SMAPI 헬퍼 인터페이스</param>
        private void SubscribeToEvents(IModHelper helper)
        {
            // 게임 생명주기 이벤트
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.Saving += OnSaving;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;

            // 입력 이벤트
            helper.Events.Input.ButtonPressed += OnButtonPressed;

            // 플레이어 이벤트
            helper.Events.Player.Warped += OnPlayerWarped;
        }

        /// <summary>
        /// 게임이 시작될 때 호출됩니다.
        /// </summary>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this.Monitor.Log("게임 시작됨", LogLevel.Debug);
            
            // 추가 초기화 작업 수행
            configManager.LoadConfig();
        }

        /// <summary>
        /// 세이브 파일이 로드될 때 호출됩니다.
        /// </summary>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            this.Monitor.Log("세이브 파일 로드됨", LogLevel.Debug);
            
            // NPC 데이터 복원
            npcManager.LoadNPCData();
            
            // 작업 데이터 복원
            taskManager.LoadTaskData();
        }

        /// <summary>
        /// 게임을 저장할 때 호출됩니다.
        /// </summary>
        private void OnSaving(object sender, SavingEventArgs e)
        {
            this.Monitor.Log("게임 저장 중", LogLevel.Debug);
            
            // NPC 데이터 저장
            npcManager.SaveNPCData();
            
            // 작업 데이터 저장
            taskManager.SaveTaskData();
        }

        /// <summary>
        /// 매 게임 틱마다 호출됩니다.
        /// </summary>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // 성능을 위해 매 틱마다 실행하지 않고 주기적으로 실행
            if (e.IsMultipleOf(15)) // 약 4틱에 한 번 (60FPS 기준 약 0.25초)
            {
                try
                {
                    // NPC 업데이트
                    npcManager.UpdateAllNPCs(Game1.currentGameTime);
                    
                    // 작업 처리
                    taskManager.ProcessTasks();
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"업데이트 중 오류 발생: {ex.Message}", LogLevel.Error);
                }
            }
        }

        /// <summary>
        /// 버튼이 눌렸을 때 호출됩니다.
        /// </summary>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // 게임이 준비되지 않았으면 무시
            if (!Context.IsWorldReady)
                return;

            try
            {
                // UI 관리자에게 입력 전달
                uiManager.HandleInput(e.Button);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"입력 처리 중 오류 발생: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 플레이어가 다른 위치로 이동했을 때 호출됩니다.
        /// </summary>
        private void OnPlayerWarped(object sender, WarpedEventArgs e)
        {
            this.Monitor.Log($"플레이어가 {e.NewLocation.Name}으로 이동함", LogLevel.Trace);
            
            // 새 위치의 NPC들을 업데이트
            npcManager.OnLocationChanged(e.NewLocation);
        }
    }
}

