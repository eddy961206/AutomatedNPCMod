using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using AutomatedNPCMod.Models;

namespace AutomatedNPCMod.Core
{
    /// <summary>
    /// 모든 커스텀 NPC의 생성, 삭제, 업데이트를 중앙에서 관리하는 클래스.
    /// </summary>
    public class NPCManager
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly Dictionary<string, CustomNPC> _activeNPCs;

        public NPCManager(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _activeNPCs = new Dictionary<string, CustomNPC>();
        }

        /// <summary>
        /// 새로운 NPC를 생성하고 게임에 추가합니다.
        /// </summary>
        /// <param name="name">NPC 이름</param>
        /// <param name="position">초기 위치</param>
        /// <param name="spriteSheet">스프라이트 시트 경로</param>
        /// <returns>생성 성공 여부</returns>
        public bool CreateNPC(string name, Vector2 position, string? spriteSheet = null)
        {
            try
            {
                // 중복 이름 확인
                if (_activeNPCs.ContainsKey(name))
                {
                    _monitor.Log($"이미 존재하는 NPC 이름: {name}", LogLevel.Warn);
                    return false;
                }

                // 기본 스프라이트 시트 설정
                if (string.IsNullOrEmpty(spriteSheet))
                {
                    spriteSheet = "Characters\\Abigail"; // 기본 스프라이트로 Abigail 사용
                }

                // CustomNPC 인스턴스 생성
                var customNPC = new CustomNPC(name, position, spriteSheet);

                // 현재 위치에 NPC 추가
                var currentLocation = Game1.currentLocation ?? Game1.getFarm();
                currentLocation.addCharacter(customNPC);

                // 활성 NPC 목록에 추가
                _activeNPCs[name] = customNPC;

                _monitor.Log($"NPC '{name}' 생성 완료. 위치: {position}", LogLevel.Info);
                return true;
            }
            catch (Exception ex)
            {
                _monitor.Log($"NPC 생성 중 오류 발생: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// 플레이어 근처에 테스트 NPC를 생성합니다.
        /// </summary>
        /// <returns>생성 성공 여부</returns>
        public bool CreateTestNPC()
        {
            try
            {
                // 플레이어 위치 근처에 NPC 생성
                var playerPosition = new Vector2((int)(Game1.player.position.X / 64f), (int)(Game1.player.position.Y / 64f));
                var npcPosition = new Vector2(playerPosition.X + 2, playerPosition.Y);
                
                // 고유한 이름 생성
                var npcName = $"Worker_{DateTime.Now.Ticks % 1000}";
                
                if (CreateNPC(npcName, npcPosition))
                {
                    // HUD 메시지 표시
                    Game1.addHUDMessage(new HUDMessage($"NPC '{npcName}' 생성됨! 위치: ({npcPosition.X:F1}, {npcPosition.Y:F1})", HUDMessage.newQuest_type));
                    _monitor.Log($"테스트 NPC '{npcName}' 생성 완료!", LogLevel.Info);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _monitor.Log($"테스트 NPC 생성 중 오류: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// 지정된 NPC를 제거합니다.
        /// </summary>
        /// <param name="name">제거할 NPC 이름</param>
        /// <returns>제거 성공 여부</returns>
        public bool RemoveNPC(string name)
        {
            try
            {
                if (!_activeNPCs.TryGetValue(name, out CustomNPC npc))
                {
                    _monitor.Log($"존재하지 않는 NPC: {name}", LogLevel.Warn);
                    return false;
                }

                // 게임에서 NPC 제거
                npc.currentLocation?.characters.Remove(npc);

                // 활성 NPC 목록에서 제거
                _activeNPCs.Remove(name);

                _monitor.Log($"NPC '{name}' 제거 완료", LogLevel.Info);
                return true;
            }
            catch (Exception ex)
            {
                _monitor.Log($"NPC 제거 중 오류 발생: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// 지정된 이름의 NPC를 조회합니다.
        /// </summary>
        /// <param name="name">NPC 이름</param>
        /// <returns>CustomNPC 인스턴스 또는 null</returns>
        public CustomNPC GetNPC(string name)
        {
            _activeNPCs.TryGetValue(name, out CustomNPC npc);
            return npc;
        }

        /// <summary>
        /// 모든 활성 NPC를 업데이트합니다.
        /// </summary>
        /// <param name="gameTime">게임 시간</param>
        public void UpdateAllNPCs(GameTime gameTime)
        {
            try
            {
                foreach (var npc in _activeNPCs.Values.ToList())
                {
                    if (npc.currentLocation != null)
                    {
                        npc.update(gameTime, npc.currentLocation);
                    }
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"NPC 업데이트 중 오류 발생: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 특정 위치의 NPC 목록을 반환합니다.
        /// </summary>
        /// <param name="location">게임 위치</param>
        /// <returns>해당 위치의 CustomNPC 목록</returns>
        public List<CustomNPC> GetNPCsInLocation(GameLocation location)
        {
            return _activeNPCs.Values
                .Where(npc => npc.currentLocation == location)
                .ToList();
        }

        /// <summary>
        /// 모든 활성 NPC의 목록을 반환합니다.
        /// </summary>
        /// <returns>활성 NPC 목록</returns>
        public List<CustomNPC> GetAllNPCs()
        {
            return _activeNPCs.Values.ToList();
        }

        /// <summary>
        /// 모든 NPC를 제거합니다.
        /// </summary>
        /// <returns>제거된 NPC 개수</returns>
        public int RemoveAllNPCs()
        {
            try
            {
                int removedCount = 0;
                var npcsList = _activeNPCs.Values.ToList();
                
                foreach (var npc in npcsList)
                {
                    try
                    {
                        // 게임에서 NPC 제거
                        npc.currentLocation?.characters.Remove(npc);
                        removedCount++;
                    }
                    catch (Exception ex)
                    {
                        _monitor.Log($"개별 NPC 제거 실패: {ex.Message}", LogLevel.Debug);
                    }
                }
                
                // 활성 NPC 목록 초기화
                _activeNPCs.Clear();
                
                if (removedCount > 0)
                {
                    Game1.addHUDMessage(new HUDMessage($"{removedCount}개의 NPC가 제거됨!", HUDMessage.newQuest_type));
                }
                
                _monitor.Log($"NPC 제거 완료: {removedCount}개", LogLevel.Info);
                return removedCount;
            }
            catch (Exception ex)
            {
                _monitor.Log($"NPC 제거 중 오류: {ex.Message}", LogLevel.Error);
                return 0;
            }
        }

        /// <summary>
        /// 현재 관리 중인 NPC 개수를 반환합니다.
        /// </summary>
        /// <returns>NPC 개수</returns>
        public int GetNPCCount()
        {
            return _activeNPCs.Count;
        }

        /// <summary>
        /// NPC 데이터를 저장합니다.
        /// </summary>
        public void SaveNPCData()
        {
            try
            {
                var saveData = new NPCSaveData
                {
                    NPCs = _activeNPCs.Values.Select(npc => new NPCData
                    {
                        Name = npc.Name,
                        Position = new Vector2(npc.getTileX(), npc.getTileY()),
                        LocationName = npc.currentLocation?.Name ?? "Farm",
                        SpriteSheet = npc.Sprite?.textureName?.Value ?? "Characters\\Abigail"
                    }).ToList()
                };

                _helper.Data.WriteSaveData("npc-data", saveData);
                _monitor.Log($"{_activeNPCs.Count}개의 NPC 데이터 저장 완료", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor.Log($"NPC 데이터 저장 중 오류 발생: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// NPC 데이터를 로드합니다.
        /// </summary>
        public void LoadNPCData()
        {
            try
            {
                var saveData = _helper.Data.ReadSaveData<NPCSaveData>("npc-data");
                if (saveData?.NPCs == null)
                {
                    _monitor.Log("로드할 NPC 데이터가 없습니다", LogLevel.Debug);
                    return;
                }

                foreach (var npcData in saveData.NPCs)
                {
                    CreateNPC(npcData.Name, npcData.Position, npcData.SpriteSheet);
                }

                _monitor.Log($"{saveData.NPCs.Count}개의 NPC 데이터 로드 완료", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"NPC 데이터 로드 중 오류 발생: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 플레이어가 위치를 변경했을 때 호출됩니다.
        /// </summary>
        /// <param name="newLocation">새로운 위치</param>
        public void OnLocationChanged(GameLocation newLocation)
        {
            // 새 위치의 NPC들을 활성화하거나 필요한 처리 수행
            var npcsInLocation = GetNPCsInLocation(newLocation);
            _monitor.Log($"위치 '{newLocation.Name}'에 {npcsInLocation.Count}개의 NPC가 있습니다", LogLevel.Trace);
        }
    }

    /// <summary>
    /// NPC 저장 데이터 구조
    /// </summary>
    public class NPCSaveData
    {
        public List<NPCData> NPCs { get; set; } = new List<NPCData>();
    }

    /// <summary>
    /// 개별 NPC 데이터 구조
    /// </summary>
    public class NPCData
    {
        public string Name { get; set; }
        public Vector2 Position { get; set; }
        public string LocationName { get; set; }
        public string SpriteSheet { get; set; }
    }
}

