using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace AutomatedNPCMod
{
    public enum TaskType
    {
        Farming,    // 농사 (씨앗 심기, 물주기, 수확)
        Mining,     // 채굴 (광석, 돌 채굴)
        Foraging,   // 채집 (야생 식물, 과일 수집)
        Fishing,    // 낚시
        Combat,     // 전투
        Crafting,   // 제작
        Building,   // 건설
        Maintenance // 유지보수
    }

    public enum TaskPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Critical = 4
    }

    public class WorkTask
    {
        public string Id { get; set; }
        public TaskType Type { get; set; }
        public object TargetLocation { get; set; } // Vector2
        public string AssignedNPCId { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? CompletedTime { get; set; }
        public TaskPriority Priority { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public bool IsCompleted { get; set; }

        public WorkTask()
        {
            Id = Guid.NewGuid().ToString();
            Parameters = new Dictionary<string, object>();
            IsCompleted = false;
            CreatedTime = DateTime.Now;
        }
    }

    public class SimpleModEntry : Mod
    {
        private List<object> createdNPCs = new List<object>();
        private List<WorkTask> activeTasks = new List<WorkTask>();
        
        public override void Entry(IModHelper helper)
        {
            this.Monitor.Log("Automated NPC Mod 로드됨!", LogLevel.Info);
            
            helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady) return;

            switch (e.Button)
            {
                case SButton.F9:
                    this.Monitor.Log("F9 - NPC 생성 시도 중...", LogLevel.Info);
                    try
                    {
                        CreateTestNPC();
                        this.Monitor.Log("NPC 생성 완료!", LogLevel.Info);
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
                        AssignFarmingTask();
                    }
                    catch (Exception ex)
                    {
                        this.Monitor.Log($"작업 할당 실패: {ex.Message}", LogLevel.Error);
                    }
                    break;

                case SButton.F11:
                    this.Monitor.Log("F11 - NPC 상태 확인 중...", LogLevel.Info);
                    ShowNPCStatus();
                    break;

                case SButton.F12:
                    this.Monitor.Log("F12 - 모든 NPC 제거 중...", LogLevel.Info);
                    try
                    {
                        RemoveAllNPCs();
                    }
                    catch (Exception ex)
                    {
                        this.Monitor.Log($"NPC 제거 실패: {ex.Message}", LogLevel.Error);
                    }
                    break;
            }
        }

        private void CreateTestNPC()
        {
            try
            {
                // Game1과 필요한 타입들 가져오기
                var game1Type = Type.GetType("StardewValley.Game1, Stardew Valley");
                var npcType = Type.GetType("StardewValley.NPC, Stardew Valley");
                var animatedSpriteType = Type.GetType("StardewValley.AnimatedSprite, Stardew Valley");
                var vector2Type = Type.GetType("Microsoft.Xna.Framework.Vector2, MonoGame.Framework");
                
                if (game1Type == null || npcType == null || animatedSpriteType == null || vector2Type == null)
                {
                    this.Monitor.Log("필요한 타입을 찾을 수 없습니다.", LogLevel.Error);
                    return;
                }

                // 현재 위치와 플레이어 가져오기
                var currentLocationProp = game1Type.GetProperty("currentLocation", BindingFlags.Public | BindingFlags.Static);
                var playerProp = game1Type.GetProperty("player", BindingFlags.Public | BindingFlags.Static);
                
                var currentLocation = currentLocationProp?.GetValue(null);
                var player = playerProp?.GetValue(null);
                
                if (currentLocation == null || player == null)
                {
                    this.Monitor.Log("위치나 플레이어를 찾을 수 없습니다.", LogLevel.Error);
                    return;
                }

                // 플레이어 위치 계산 (타일 단위)
                float tileX = 10f, tileY = 10f; // 기본값
                
                try
                {
                    var positionField = player.GetType().GetField("Position");
                    var positionProp = player.GetType().GetProperty("Position");
                    object? pos = positionField?.GetValue(player) ?? positionProp?.GetValue(player);
                    
                    if (pos != null)
                    {
                        var xField = pos.GetType().GetField("X");
                        var yField = pos.GetType().GetField("Y");
                        
                        if (xField != null && yField != null)
                        {
                            var rawX = (float)xField.GetValue(pos)!;
                            var rawY = (float)yField.GetValue(pos)!;
                            
                            // 픽셀을 타일로 변환
                            tileX = rawX / 64f + 1; // 플레이어 오른쪽에 생성
                            tileY = rawY / 64f;
                            
                            this.Monitor.Log($"플레이어 위치: ({rawX/64f:F1}, {rawY/64f:F1}), NPC 위치: ({tileX:F1}, {tileY:F1})", LogLevel.Info);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"위치 계산 오류, 기본값 사용: {ex.Message}", LogLevel.Warn);
                }

                // 백업 코드 방식으로 AnimatedSprite 생성 (spriteSheet, currentFrame, spriteWidth, spriteHeight)
                var animatedSprite = Activator.CreateInstance(animatedSpriteType, "Characters\\Abigail", 0, 16, 32);
                this.Monitor.Log("AnimatedSprite 생성 성공 (백업 방식)", LogLevel.Debug);
                
                // Vector2 위치 생성 (타일 * 64 = 픽셀)
                var pixelPosition = Activator.CreateInstance(vector2Type, tileX * 64f, tileY * 64f);
                
                // NPC 생성자들 디버깅
                var constructors = npcType.GetConstructors();
                this.Monitor.Log($"사용 가능한 NPC 생성자 개수: {constructors.Length}", LogLevel.Debug);
                
                foreach (var ctor in constructors.Take(5))
                {
                    var paramTypes = ctor.GetParameters().Select(p => p.ParameterType.Name).ToArray();
                    this.Monitor.Log($"생성자 매개변수: [{string.Join(", ", paramTypes)}]", LogLevel.Debug);
                }
                
                // 여러 생성자 시도
                var npcConstructor = npcType.GetConstructor(new[] { animatedSpriteType, vector2Type, typeof(int), typeof(string) });
                
                if (npcConstructor == null)
                {
                    this.Monitor.Log("기본 생성자 시도...", LogLevel.Debug);
                    npcConstructor = npcType.GetConstructor(Type.EmptyTypes);
                }
                
                if (npcConstructor == null)
                {
                    this.Monitor.Log("모든 NPC 생성자를 찾을 수 없습니다.", LogLevel.Error);
                    return;
                }
                
                object? testNPC = null;
                
                // 매개변수가 있는 생성자 사용
                if (npcConstructor.GetParameters().Length > 0)
                {
                    testNPC = npcConstructor.Invoke(new object[] { animatedSprite, pixelPosition!, 2, "TestWorker" });
                    this.Monitor.Log("매개변수 생성자로 NPC 생성 성공!", LogLevel.Debug);
                }
                else
                {
                    // 기본 생성자 사용 후 속성 설정
                    testNPC = npcConstructor.Invoke(null);
                    this.Monitor.Log("기본 생성자로 NPC 생성 성공!", LogLevel.Debug);
                    
                    // 수동으로 속성 설정
                    var spriteProp = testNPC.GetType().GetProperty("Sprite");
                    var positionProp = testNPC.GetType().GetProperty("Position");
                    var nameProp = testNPC.GetType().GetProperty("Name");
                    var facingDirectionField = testNPC.GetType().GetField("FacingDirection");
                    
                    spriteProp?.SetValue(testNPC, animatedSprite);
                    positionProp?.SetValue(testNPC, pixelPosition);
                    nameProp?.SetValue(testNPC, "TestWorker");
                    facingDirectionField?.SetValue(testNPC, 2);
                }
                
                // NPC 기본 설정
                var displayNameProp = testNPC.GetType().GetProperty("displayName");
                var currentLocationField = testNPC.GetType().GetField("currentLocation");
                var speedField = testNPC.GetType().GetField("speed");
                
                displayNameProp?.SetValue(testNPC, "TestWorker");
                currentLocationField?.SetValue(testNPC, currentLocation);
                speedField?.SetValue(testNPC, 2);
                
                this.Monitor.Log("NPC 속성 설정 완료", LogLevel.Debug);
                
                // 현재 위치의 characters 컬렉션에 추가
                var charactersField = currentLocation.GetType().GetField("characters");
                var charactersProp = currentLocation.GetType().GetProperty("characters");
                
                object? characters = charactersField?.GetValue(currentLocation) ?? charactersProp?.GetValue(currentLocation);
                
                if (characters != null)
                {
                    var addMethod = characters.GetType().GetMethod("Add");
                    addMethod?.Invoke(characters, new[] { testNPC });
                    
                    // 생성된 NPC를 리스트에 추가
                    createdNPCs.Add(testNPC);
                    
                    // 게임 내 HUD 메시지 표시
                    ShowHUDMessage($"NPC 'TestWorker' 생성됨! 위치: ({tileX:F1}, {tileY:F1})");
                    this.Monitor.Log($"테스트 NPC 'TestWorker' 생성 완료! 위치: ({tileX:F1}, {tileY:F1})", LogLevel.Info);
                }
                else
                {
                    this.Monitor.Log("characters 컬렉션을 찾을 수 없습니다.", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"NPC 생성 중 오류 발생: {ex.Message}", LogLevel.Error);
                this.Monitor.Log($"스택 트레이스: {ex.StackTrace}", LogLevel.Debug);
            }
        }

        private void CheckAllNPCs()
        {
            try
            {
                var game1Type = Type.GetType("StardewValley.Game1, Stardew Valley");
                var currentLocationProp = game1Type?.GetProperty("currentLocation", BindingFlags.Public | BindingFlags.Static);
                var currentLocation = currentLocationProp?.GetValue(null);
                
                if (currentLocation == null)
                {
                    this.Monitor.Log("현재 위치를 가져올 수 없습니다.", LogLevel.Error);
                    return;
                }

                this.Monitor.Log($"현재 위치: {currentLocation.GetType().GetProperty("Name")?.GetValue(currentLocation) ?? "Unknown"}", LogLevel.Info);

                // characters 컬렉션 가져오기
                var charactersField = currentLocation.GetType().GetField("characters");
                var charactersProp = currentLocation.GetType().GetProperty("characters");
                
                object? characters = charactersField?.GetValue(currentLocation) ?? charactersProp?.GetValue(currentLocation);
                
                if (characters != null)
                {
                    var countProp = characters.GetType().GetProperty("Count");
                    var count = (int)(countProp?.GetValue(characters) ?? 0);
                    
                    this.Monitor.Log($"현재 위치의 총 NPC 수: {count}", LogLevel.Info);
                    
                    // 각 NPC 정보 출력
                    var getEnumeratorMethod = characters.GetType().GetMethod("GetEnumerator");
                    if (getEnumeratorMethod != null)
                    {
                        var enumerator = getEnumeratorMethod.Invoke(characters, null);
                        var moveNextMethod = enumerator?.GetType().GetMethod("MoveNext");
                        var currentProp = enumerator?.GetType().GetProperty("Current");
                        
                        int index = 0;
                        while ((bool)(moveNextMethod?.Invoke(enumerator, null) ?? false))
                        {
                            var npc = currentProp?.GetValue(enumerator);
                            if (npc != null)
                            {
                                var name = npc.GetType().GetProperty("Name")?.GetValue(npc) ?? "Unknown";
                                var position = npc.GetType().GetProperty("Position")?.GetValue(npc);
                                
                                string posStr = "Unknown";
                                if (position != null)
                                {
                                    var xField = position.GetType().GetField("X");
                                    var yField = position.GetType().GetField("Y");
                                    if (xField != null && yField != null)
                                    {
                                        var x = (float)xField.GetValue(position)!;
                                        var y = (float)yField.GetValue(position)!;
                                        posStr = $"({x/64f:F1}, {y/64f:F1})";
                                    }
                                }
                                
                                this.Monitor.Log($"NPC #{index}: 이름='{name}', 위치={posStr}", LogLevel.Info);
                                index++;
                            }
                        }
                    }
                }
                else
                {
                    this.Monitor.Log("characters 컬렉션을 찾을 수 없습니다.", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"NPC 확인 중 오류 발생: {ex.Message}", LogLevel.Error);
            }
        }

        private void AssignFarmingTask()
        {
            if (createdNPCs.Count == 0)
            {
                ShowHUDMessage("작업을 할당할 NPC가 없습니다!");
                this.Monitor.Log("작업 할당 실패: NPC가 없음", LogLevel.Warn);
                return;
            }

            try
            {
                // 플레이어 위치 근처에 농사 작업 생성
                var game1Type = Type.GetType("StardewValley.Game1, Stardew Valley");
                var playerProp = game1Type?.GetProperty("player", BindingFlags.Public | BindingFlags.Static);
                var player = playerProp?.GetValue(null);
                
                if (player != null)
                {
                    var positionField = player.GetType().GetField("Position");
                    var positionProp = player.GetType().GetProperty("Position");
                    object? pos = positionField?.GetValue(player) ?? positionProp?.GetValue(player);
                    
                    if (pos != null)
                    {
                        var xField = pos.GetType().GetField("X");
                        var yField = pos.GetType().GetField("Y");
                        
                        if (xField != null && yField != null)
                        {
                            var rawX = (float)xField.GetValue(pos)!;
                            var rawY = (float)yField.GetValue(pos)!;
                            
                            // 플레이어 앞쪽에 작업 위치 생성
                            var vector2Type = Type.GetType("Microsoft.Xna.Framework.Vector2, MonoGame.Framework");
                            var taskLocation = Activator.CreateInstance(vector2Type, (rawX / 64f) + 2, rawY / 64f);
                            
                            // 새 작업 생성
                            var task = new WorkTask
                            {
                                Type = TaskType.Farming,
                                TargetLocation = taskLocation,
                                Priority = TaskPriority.Normal,
                                AssignedNPCId = $"Worker_{createdNPCs.Count - 1}"
                            };
                            
                            activeTasks.Add(task);
                            
                            ShowHUDMessage($"농사 작업이 '{task.AssignedNPCId}'에게 할당됨!");
                            this.Monitor.Log($"농사 작업 할당 완료: {task.Id}", LogLevel.Info);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"농사 작업 할당 중 오류: {ex.Message}", LogLevel.Error);
                ShowHUDMessage("작업 할당 중 오류 발생!");
            }
        }

        private void ShowNPCStatus()
        {
            if (createdNPCs.Count == 0)
            {
                ShowHUDMessage("활성 NPC가 없습니다.");
                this.Monitor.Log("NPC 상태 확인: 활성 NPC 없음", LogLevel.Info);
                return;
            }

            try
            {
                var statusMessage = $"활성 NPC: {createdNPCs.Count}개";
                var completedTasks = activeTasks.Count(t => t.IsCompleted);
                var pendingTasks = activeTasks.Count - completedTasks;
                
                statusMessage += $"\n작업 상태: 완료 {completedTasks}개, 대기 {pendingTasks}개";
                
                // 각 NPC의 위치 정보 추가
                for (int i = 0; i < Math.Min(createdNPCs.Count, 3); i++) // 최대 3개만 표시
                {
                    var npc = createdNPCs[i];
                    var positionProp = npc.GetType().GetProperty("Position");
                    var nameProp = npc.GetType().GetProperty("Name");
                    
                    var position = positionProp?.GetValue(npc);
                    var name = nameProp?.GetValue(npc) ?? $"Worker_{i}";
                    
                    if (position != null)
                    {
                        var xField = position.GetType().GetField("X");
                        var yField = position.GetType().GetField("Y");
                        
                        if (xField != null && yField != null)
                        {
                            var x = (float)xField.GetValue(position)! / 64f;
                            var y = (float)yField.GetValue(position)! / 64f;
                            statusMessage += $"\n- {name}: ({x:F1}, {y:F1})";
                        }
                    }
                }
                
                ShowHUDMessage(statusMessage);
                this.Monitor.Log($"NPC 상태 표시: {createdNPCs.Count}개 NPC, {activeTasks.Count}개 작업", LogLevel.Info);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"NPC 상태 확인 중 오류: {ex.Message}", LogLevel.Error);
                ShowHUDMessage("NPC 상태 확인 중 오류 발생!");
            }
        }

        private void RemoveAllNPCs()
        {
            if (createdNPCs.Count == 0)
            {
                ShowHUDMessage("제거할 NPC가 없습니다.");
                this.Monitor.Log("NPC 제거: 제거할 NPC 없음", LogLevel.Info);
                return;
            }

            try
            {
                var game1Type = Type.GetType("StardewValley.Game1, Stardew Valley");
                var currentLocationProp = game1Type?.GetProperty("currentLocation", BindingFlags.Public | BindingFlags.Static);
                var currentLocation = currentLocationProp?.GetValue(null);
                
                if (currentLocation != null)
                {
                    var charactersField = currentLocation.GetType().GetField("characters");
                    var charactersProp = currentLocation.GetType().GetProperty("characters");
                    object? characters = charactersField?.GetValue(currentLocation) ?? charactersProp?.GetValue(currentLocation);
                    
                    if (characters != null)
                    {
                        // NPC 타입을 얻어서 구체적인 Remove 메서드 찾기
                        var npcType = Type.GetType("StardewValley.NPC, Stardew Valley");
                        var removeMethod = characters.GetType().GetMethod("Remove", new[] { npcType });
                        
                        if (removeMethod == null)
                        {
                            // 대안: Contains + Remove 조합 사용
                            var containsMethod = characters.GetType().GetMethod("Contains");
                            var removeAtMethod = characters.GetType().GetMethod("RemoveAt");
                            var indexOfMethod = characters.GetType().GetMethod("IndexOf");
                            removeMethod = characters.GetType().GetMethod("Remove", new[] { typeof(object) });
                        }
                        
                        int removedCount = 0;
                        
                        foreach (var npc in createdNPCs.ToList())
                        {
                            try
                            {
                                if (removeMethod != null)
                                {
                                    var result = (bool)(removeMethod.Invoke(characters, new[] { npc }) ?? false);
                                    if (result) removedCount++;
                                }
                                else
                                {
                                    // 대안: List에서 직접 제거 시도
                                    var containsMethod = characters.GetType().GetMethod("Contains");
                                    var indexOfMethod = characters.GetType().GetMethod("IndexOf");
                                    var removeAtMethod = characters.GetType().GetMethod("RemoveAt");
                                    
                                    if (containsMethod != null && indexOfMethod != null && removeAtMethod != null)
                                    {
                                        var contains = (bool)(containsMethod.Invoke(characters, new[] { npc }) ?? false);
                                        if (contains)
                                        {
                                            var index = (int)(indexOfMethod.Invoke(characters, new[] { npc }) ?? -1);
                                            if (index >= 0)
                                            {
                                                removeAtMethod.Invoke(characters, new object[] { index });
                                                removedCount++;
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                this.Monitor.Log($"개별 NPC 제거 실패: {ex.Message}", LogLevel.Debug);
                            }
                        }
                        
                        var totalCount = createdNPCs.Count;
                        createdNPCs.Clear();
                        activeTasks.Clear();
                        
                        ShowHUDMessage($"{removedCount}/{totalCount}개의 NPC가 제거됨!");
                        this.Monitor.Log($"NPC 제거 완료: {removedCount}/{totalCount}개", LogLevel.Info);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"NPC 제거 중 오류: {ex.Message}", LogLevel.Error);
                ShowHUDMessage("NPC 제거 중 오류 발생!");
            }
        }

        private void ShowHUDMessage(string message)
        {
            try
            {
                var game1Type = Type.GetType("StardewValley.Game1, Stardew Valley");
                var hudMessageType = Type.GetType("StardewValley.HUDMessage, Stardew Valley");
                var addHUDMessageMethod = game1Type?.GetMethod("addHUDMessage", BindingFlags.Public | BindingFlags.Static);
                
                if (addHUDMessageMethod != null && hudMessageType != null)
                {
                    // HUDMessage 생성자 (string text, int whatType)
                    var hudMessageConstructor = hudMessageType.GetConstructor(new[] { typeof(string), typeof(int) });
                    if (hudMessageConstructor != null)
                    {
                        var hudMessage = hudMessageConstructor.Invoke(new object[] { message, 1 }); // 1 = newQuest_type
                        addHUDMessageMethod.Invoke(null, new[] { hudMessage });
                    }
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"HUD 메시지 표시 실패: {ex.Message}", LogLevel.Debug);
            }
        }
    }
}