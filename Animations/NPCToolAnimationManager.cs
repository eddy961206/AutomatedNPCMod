using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Tools;
using AutomatedNPCMod.Models;

namespace AutomatedNPCMod.Animations
{
    /// <summary>
    /// NPC의 도구 사용 애니메이션을 관리하는 클래스
    /// </summary>
    public class NPCToolAnimationManager
    {
        private readonly CustomNPC _npc;
        private Tool _currentTool;
        private ToolType _currentToolType;
        private bool _isAnimating;
        private float _animationTimer;
        private int _currentFrame;
        private Vector2 _targetTile;
        private ToolAnimationPattern _currentPattern;
        
        // 애니메이션 상수들
        private const float FRAME_DURATION = 0.15f; // 각 프레임 지속시간
        private const int ANIMATION_CYCLES = 3; // 애니메이션 반복 횟수
        
        public NPCToolAnimationManager(CustomNPC npc)
        {
            _npc = npc;
            _isAnimating = false;
            _animationTimer = 0f;
            _currentFrame = 0;
        }

        /// <summary>
        /// 도구와 도구 타입을 설정합니다.
        /// </summary>
        public void SetTool(Tool tool, ToolType toolType)
        {
            _currentTool = tool;
            _currentToolType = toolType;
            _currentPattern = GetAnimationPattern(toolType);
        }

        /// <summary>
        /// 도구 애니메이션을 시작합니다.
        /// </summary>
        public void StartToolAnimation(Vector2 targetTile)
        {
            if (_currentTool == null || _currentPattern == null) return;

            _isAnimating = true;
            _animationTimer = 0f;
            _currentFrame = 0;
            _targetTile = targetTile;

            // NPC 스프라이트를 도구 사용 애니메이션으로 설정
            UpdateNPCSprite();
        }

        /// <summary>
        /// 도구 애니메이션을 중지합니다.
        /// </summary>
        public void StopToolAnimation()
        {
            if (!_isAnimating) return;

            _isAnimating = false;
            _animationTimer = 0f;
            _currentFrame = 0;

            // NPC를 기본 스프라이트로 되돌림
            ResetNPCSprite();
        }

        /// <summary>
        /// 매 프레임마다 애니메이션을 업데이트합니다.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (!_isAnimating || _currentPattern == null) return;

            _animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 프레임 전환 체크
            if (_animationTimer >= FRAME_DURATION)
            {
                _animationTimer = 0f;
                _currentFrame++;

                // 애니메이션 사이클 완료 체크
                int totalFrames = _currentPattern.Frames.Count * ANIMATION_CYCLES;
                if (_currentFrame >= totalFrames)
                {
                    // 애니메이션 완료
                    StopToolAnimation();
                    return;
                }

                // 스프라이트 업데이트
                UpdateNPCSprite();
            }
        }

        /// <summary>
        /// 현재 애니메이션 중인지 여부를 반환합니다.
        /// </summary>
        public bool IsAnimating()
        {
            return _isAnimating;
        }

        /// <summary>
        /// 애니메이션 진행률을 반환합니다 (0.0 ~ 1.0).
        /// </summary>
        public float GetAnimationProgress()
        {
            if (!_isAnimating || _currentPattern == null) return 0f;

            int totalFrames = _currentPattern.Frames.Count * ANIMATION_CYCLES;
            return Math.Min((float)_currentFrame / totalFrames, 1f);
        }

        /// <summary>
        /// NPC 스프라이트를 현재 애니메이션 프레임으로 업데이트합니다.
        /// </summary>
        private void UpdateNPCSprite()
        {
            if (_currentPattern == null || !_isAnimating) return;

            int frameIndex = _currentFrame % _currentPattern.Frames.Count;
            var frameData = _currentPattern.Frames[frameIndex];

            // NPC 방향에 따른 스프라이트 인덱스 계산
            int spriteIndex = CalculateSpriteIndex(frameData, _npc.FacingDirection);
            
            // NPC 스프라이트 업데이트
            _npc.Sprite.CurrentFrame = spriteIndex;
            
            // 도구 사용 효과음 재생 (타이밍에 맞게)
            if (frameIndex == _currentPattern.SoundEffectFrame && _currentFrame % _currentPattern.Frames.Count == _currentPattern.SoundEffectFrame)
            {
                PlayToolSound();
            }
        }

        /// <summary>
        /// NPC를 기본 대기 스프라이트로 되돌립니다.
        /// </summary>
        private void ResetNPCSprite()
        {
            // 방향에 따른 기본 스프라이트 인덱스
            int baseFrame = _npc.FacingDirection switch
            {
                0 => 12, // 위쪽
                1 => 6,  // 오른쪽
                2 => 0,  // 아래쪽
                3 => 18, // 왼쪽
                _ => 0
            };

            _npc.Sprite.CurrentFrame = baseFrame;
        }

        /// <summary>
        /// 프레임 데이터와 방향에 따른 스프라이트 인덱스를 계산합니다.
        /// </summary>
        private int CalculateSpriteIndex(AnimationFrameData frameData, int facingDirection)
        {
            return facingDirection switch
            {
                0 => frameData.UpFrame,    // 위쪽
                1 => frameData.RightFrame, // 오른쪽
                2 => frameData.DownFrame,  // 아래쪽
                3 => frameData.LeftFrame,  // 왼쪽
                _ => frameData.DownFrame
            };
        }

        /// <summary>
        /// 도구 타입에 따른 애니메이션 패턴을 반환합니다.
        /// </summary>
        private ToolAnimationPattern GetAnimationPattern(ToolType toolType)
        {
            return toolType switch
            {
                ToolType.Hoe => GetHoeAnimationPattern(),
                ToolType.Axe => GetAxeAnimationPattern(),
                ToolType.WateringCan => GetWateringCanAnimationPattern(),
                ToolType.Pickaxe => GetPickaxeAnimationPattern(),
                _ => GetDefaultAnimationPattern()
            };
        }

        /// <summary>
        /// 호미 애니메이션 패턴을 생성합니다.
        /// </summary>
        private ToolAnimationPattern GetHoeAnimationPattern()
        {
            return new ToolAnimationPattern
            {
                Frames = new List<AnimationFrameData>
                {
                    new AnimationFrameData { UpFrame = 12, RightFrame = 6, DownFrame = 0, LeftFrame = 18 }, // 준비
                    new AnimationFrameData { UpFrame = 13, RightFrame = 7, DownFrame = 1, LeftFrame = 19 }, // 올리기
                    new AnimationFrameData { UpFrame = 14, RightFrame = 8, DownFrame = 2, LeftFrame = 20 }, // 내리치기
                    new AnimationFrameData { UpFrame = 15, RightFrame = 9, DownFrame = 3, LeftFrame = 21 }, // 완료
                },
                SoundEffectFrame = 2, // 내리치는 순간 소리
                TotalDuration = 1.5f
            };
        }

        /// <summary>
        /// 도끼 애니메이션 패턴을 생성합니다.
        /// </summary>
        private ToolAnimationPattern GetAxeAnimationPattern()
        {
            return new ToolAnimationPattern
            {
                Frames = new List<AnimationFrameData>
                {
                    new AnimationFrameData { UpFrame = 12, RightFrame = 6, DownFrame = 0, LeftFrame = 18 }, // 준비
                    new AnimationFrameData { UpFrame = 13, RightFrame = 7, DownFrame = 1, LeftFrame = 19 }, // 뒤로 빼기
                    new AnimationFrameData { UpFrame = 14, RightFrame = 8, DownFrame = 2, LeftFrame = 20 }, // 휘두르기
                    new AnimationFrameData { UpFrame = 15, RightFrame = 9, DownFrame = 3, LeftFrame = 21 }, // 완료
                    new AnimationFrameData { UpFrame = 12, RightFrame = 6, DownFrame = 0, LeftFrame = 18 }, // 되돌리기
                },
                SoundEffectFrame = 2, // 휘두르는 순간 소리
                TotalDuration = 2.5f
            };
        }

        /// <summary>
        /// 물뿌리개 애니메이션 패턴을 생성합니다.
        /// </summary>
        private ToolAnimationPattern GetWateringCanAnimationPattern()
        {
            return new ToolAnimationPattern
            {
                Frames = new List<AnimationFrameData>
                {
                    new AnimationFrameData { UpFrame = 12, RightFrame = 6, DownFrame = 0, LeftFrame = 18 }, // 준비
                    new AnimationFrameData { UpFrame = 13, RightFrame = 7, DownFrame = 1, LeftFrame = 19 }, // 올리기
                    new AnimationFrameData { UpFrame = 14, RightFrame = 8, DownFrame = 2, LeftFrame = 20 }, // 기울이기
                },
                SoundEffectFrame = 2, // 물 뿌리는 소리
                TotalDuration = 1.0f
            };
        }

        /// <summary>
        /// 곡괭이 애니메이션 패턴을 생성합니다.
        /// </summary>
        private ToolAnimationPattern GetPickaxeAnimationPattern()
        {
            return new ToolAnimationPattern
            {
                Frames = new List<AnimationFrameData>
                {
                    new AnimationFrameData { UpFrame = 12, RightFrame = 6, DownFrame = 0, LeftFrame = 18 }, // 준비
                    new AnimationFrameData { UpFrame = 13, RightFrame = 7, DownFrame = 1, LeftFrame = 19 }, // 뒤로 빼기
                    new AnimationFrameData { UpFrame = 14, RightFrame = 8, DownFrame = 2, LeftFrame = 20 }, // 내리치기
                    new AnimationFrameData { UpFrame = 15, RightFrame = 9, DownFrame = 3, LeftFrame = 21 }, // 완료
                },
                SoundEffectFrame = 2, // 내리치는 순간 소리
                TotalDuration = 2.0f
            };
        }

        /// <summary>
        /// 기본 애니메이션 패턴을 생성합니다.
        /// </summary>
        private ToolAnimationPattern GetDefaultAnimationPattern()
        {
            return new ToolAnimationPattern
            {
                Frames = new List<AnimationFrameData>
                {
                    new AnimationFrameData { UpFrame = 12, RightFrame = 6, DownFrame = 0, LeftFrame = 18 }, // 기본
                },
                SoundEffectFrame = 0,
                TotalDuration = 1.0f
            };
        }

        /// <summary>
        /// 도구 사용 효과음을 재생합니다.
        /// </summary>
        private void PlayToolSound()
        {
            try
            {
                string soundName = _currentToolType switch
                {
                    ToolType.Hoe => "hoeHit",
                    ToolType.Axe => "axchop",
                    ToolType.WateringCan => "wateringCan",
                    ToolType.Pickaxe => "hammer",
                    _ => "toolSwap"
                };

                Game1.playSound(soundName);
            }
            catch (Exception)
            {
                // 사운드 재생 실패 시 무시
            }
        }
    }

    /// <summary>
    /// 도구 애니메이션 패턴을 정의하는 클래스
    /// </summary>
    public class ToolAnimationPattern
    {
        public List<AnimationFrameData> Frames { get; set; } = new List<AnimationFrameData>();
        public int SoundEffectFrame { get; set; } = 0; // 효과음이 재생될 프레임
        public float TotalDuration { get; set; } = 1.0f; // 총 지속시간
    }

    /// <summary>
    /// 애니메이션 프레임 데이터를 정의하는 클래스
    /// </summary>
    public class AnimationFrameData
    {
        public int UpFrame { get; set; }    // 위쪽 방향 프레임
        public int RightFrame { get; set; } // 오른쪽 방향 프레임
        public int DownFrame { get; set; }  // 아래쪽 방향 프레임
        public int LeftFrame { get; set; }  // 왼쪽 방향 프레임
    }
}