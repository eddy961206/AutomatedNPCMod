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
                    this.Monitor.Log("F11 - NPC 상태 확인 테스트", LogLevel.Info);
                    break;

                case SButton.F12:
                    this.Monitor.Log("F12 - NPC 제거 테스트", LogLevel.Info);
                    break;
            }
        }

        private void CreateTestNPC()
        {
            // Game1과 NPC 타입 가져오기
            var game1Type = Type.GetType("StardewValley.Game1, Stardew Valley");
            var npcType = Type.GetType("StardewValley.NPC, Stardew Valley");
            var vector2Type = Type.GetType("Microsoft.Xna.Framework.Vector2, MonoGame.Framework");
            
            if (game1Type == null || npcType == null || vector2Type == null)
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

            // 플레이어 위치 가져오기 (다른 방법으로 시도)
            var positionField = player.GetType().GetField("Position");
            var positionProp = player.GetType().GetProperty("Position");
            
            float x = 10f; // 기본값
            float y = 10f; // 기본값
            
            // 여러 방법으로 위치 가져오기 시도
            try
            {
                object? pos = null;
                
                // 디버깅: 플레이어 객체의 모든 프로퍼티 출력
                this.Monitor.Log($"플레이어 타입: {player.GetType().Name}", LogLevel.Debug);
                var allProps = player.GetType().GetProperties();
                var allFields = player.GetType().GetFields();
                
                this.Monitor.Log($"사용 가능한 프로퍼티: {string.Join(", ", allProps.Take(10).Select(p => p.Name))}", LogLevel.Debug);
                this.Monitor.Log($"사용 가능한 필드: {string.Join(", ", allFields.Take(10).Select(f => f.Name))}", LogLevel.Debug);
                
                // 방법 1: Position 필드 접근
                if (positionField != null)
                {
                    pos = positionField.GetValue(player);
                    this.Monitor.Log("Position 필드로 위치 가져옴", LogLevel.Debug);
                }
                // 방법 2: Position 프로퍼티 접근
                else if (positionProp != null)
                {
                    pos = positionProp.GetValue(player);
                    this.Monitor.Log("Position 프로퍼티로 위치 가져옴", LogLevel.Debug);
                }
                else
                {
                    // 방법 3: 다른 위치 관련 프로퍼티 시도
                    var tileLocationProp = player.GetType().GetProperty("TileLocation");
                    var standingPixelProp = player.GetType().GetProperty("StandingPixel");
                    
                    if (tileLocationProp != null)
                    {
                        pos = tileLocationProp.GetValue(player);
                        this.Monitor.Log("TileLocation 프로퍼티로 위치 가져옴", LogLevel.Debug);
                    }
                    else if (standingPixelProp != null)
                    {
                        pos = standingPixelProp.GetValue(player);
                        this.Monitor.Log("StandingPixel 프로퍼티로 위치 가져옴", LogLevel.Debug);
                    }
                }
                
                if (pos != null)
                {
                    this.Monitor.Log($"Position 객체 타입: {pos.GetType().Name}", LogLevel.Debug);
                    var posProps = pos.GetType().GetProperties();
                    this.Monitor.Log($"Position 프로퍼티들: {string.Join(", ", posProps.Select(p => p.Name))}", LogLevel.Debug);
                    
                    // Vector2는 X, Y가 필드임
                    var xField = pos.GetType().GetField("X");
                    var yField = pos.GetType().GetField("Y");
                    
                    if (xField != null && yField != null)
                    {
                        var rawX = (float)xField.GetValue(pos)!;
                        var rawY = (float)yField.GetValue(pos)!;
                        
                        // 픽셀 단위인지 타일 단위인지 확인
                        if (rawX > 100) // 픽셀 단위로 추정
                        {
                            x = rawX / 64f + 1;
                            y = rawY / 64f;
                        }
                        else // 타일 단위로 추정
                        {
                            x = rawX + 1;
                            y = rawY;
                        }
                        
                        this.Monitor.Log($"원본 위치: ({rawX}, {rawY}), 변환된 위치: ({x}, {y})", LogLevel.Debug);
                    }
                    else
                    {
                        this.Monitor.Log($"X 또는 Y 필드를 찾을 수 없음. X: {xField != null}, Y: {yField != null}", LogLevel.Debug);
                    }
                }
                else
                {
                    this.Monitor.Log("Position 객체가 null입니다.", LogLevel.Debug);
                }
                
                this.Monitor.Log($"플레이어 위치 계산 완료: ({x}, {y})", LogLevel.Info);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"플레이어 위치 계산 중 오류, 기본값 사용: {ex.Message}", LogLevel.Warn);
            }
                
            // Vector2 생성
            var npcPosition = Activator.CreateInstance(vector2Type, x * 64f, y * 64f); // 타일을 픽셀로 변환
            
            // NPC 생성자 디버깅
            var constructors = npcType.GetConstructors();
            this.Monitor.Log($"사용 가능한 NPC 생성자 개수: {constructors.Length}", LogLevel.Debug);
            
            foreach (var ctor in constructors.Take(3))
            {
                var paramTypes = ctor.GetParameters().Select(p => p.ParameterType.Name).ToArray();
                this.Monitor.Log($"생성자 매개변수: [{string.Join(", ", paramTypes)}]", LogLevel.Debug);
            }
            
            // 여러 생성자 시도
            object? testNPC = null;
            
            // 시도 1: (string name, Vector2 position, string spriteName)
            var constructor1 = npcType.GetConstructor(new[] { typeof(string), vector2Type, typeof(string) });
            if (constructor1 != null)
            {
                try
                {
                    testNPC = constructor1.Invoke(new object[] { "TestWorker", npcPosition!, "Abigail" });
                    this.Monitor.Log("생성자 1로 NPC 생성 성공", LogLevel.Debug);
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"생성자 1 실패: {ex.Message}", LogLevel.Debug);
                }
            }
            
            // 시도 2: 기본 생성자 + 프로퍼티 설정
            if (testNPC == null)
            {
                var defaultConstructor = npcType.GetConstructor(Type.EmptyTypes);
                if (defaultConstructor != null)
                {
                    try
                    {
                        testNPC = defaultConstructor.Invoke(null);
                        
                        // 이름과 위치 설정
                        var nameProp = testNPC.GetType().GetProperty("Name");
                        var npcPositionProp = testNPC.GetType().GetProperty("Position");
                        
                        nameProp?.SetValue(testNPC, "TestWorker");
                        npcPositionProp?.SetValue(testNPC, npcPosition);
                        
                        this.Monitor.Log("기본 생성자로 NPC 생성 성공", LogLevel.Debug);
                    }
                    catch (Exception ex)
                    {
                        this.Monitor.Log($"기본 생성자 실패: {ex.Message}", LogLevel.Debug);
                    }
                }
            }
            
            if (testNPC != null)
            {
                // 현재 위치에 NPC 추가
                var charactersField = currentLocation.GetType().GetField("characters");
                var charactersProp = currentLocation.GetType().GetProperty("characters");
                
                object? characters = charactersField?.GetValue(currentLocation) ?? charactersProp?.GetValue(currentLocation);
                
                if (characters != null)
                {
                    var addMethod = characters.GetType().GetMethod("Add");
                    addMethod?.Invoke(characters, new[] { testNPC });
                    
                    this.Monitor.Log($"테스트 NPC 'TestWorker' 생성 완료! 위치: ({x}, {y})", LogLevel.Info);
                }
                else
                {
                    this.Monitor.Log("characters 컬렉션을 찾을 수 없습니다.", LogLevel.Error);
                }
            }
            else
            {
                this.Monitor.Log("모든 NPC 생성 방법이 실패했습니다.", LogLevel.Error);
            }
        }
    }
}