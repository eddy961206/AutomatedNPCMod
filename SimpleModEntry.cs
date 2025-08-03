using System;
using System.Linq;
using System.Reflection;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace AutomatedNPCMod
{
    public class SimpleModEntry : Mod
    {
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
                    this.Monitor.Log("F10 - 작업 할당 테스트", LogLevel.Info);
                    break;

                case SButton.F11:
                    this.Monitor.Log("F11 - NPC 상태 확인 중...", LogLevel.Info);
                    CheckAllNPCs();
                    break;

                case SButton.F12:
                    this.Monitor.Log("F12 - NPC 제거 테스트", LogLevel.Info);
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
    }
}