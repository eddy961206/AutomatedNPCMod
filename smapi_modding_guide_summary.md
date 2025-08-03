# SMAPI 모딩 가이드 요약

## 1. SMAPI 모드란?
SMAPI 모드는 SMAPI 모딩 API를 사용하여 게임 로직을 확장합니다. 게임 내에서 어떤 일이 발생할 때(예: 개체가 배치될 때) 응답하거나, 주기적으로 코드를 실행하거나, 게임의 자산 및 데이터를 변경할 수 있습니다. SMAPI 모드는 C#으로 작성되며 .NET을 사용합니다. 스타듀밸리는 MonoGame을 사용하여 게임 로직을 처리합니다.

## 2. SMAPI를 사용하는 이유
SMAPI는 다음과 같은 다양한 기능을 제공하여 모드 개발을 용이하게 합니다:
* 모드 로드: SMAPI 없이는 코드 모드를 로드할 수 없습니다.
* API 및 이벤트 제공: 게임과 상호 작용할 수 있는 간소화된 API를 제공합니다.
* 크로스 플랫폼 호환성: Linux/Mac/Windows 버전 간의 차이점을 걱정할 필요 없이 모드 코드를 작성할 수 있습니다.
* 모드 업데이트: 게임 업데이트로 인해 손상된 모드 코드를 감지하고 수정합니다.
* 오류 가로채기: 모드 충돌 시 오류를 가로채고, 오류 세부 정보를 콘솔 창에 표시하며, 대부분의 경우 게임을 자동으로 복구합니다.
* 업데이트 확인: 모드의 새 버전이 사용 가능할 때 플레이어에게 자동으로 알립니다.
* 호환성 확인: 모드가 호환되지 않을 때 자동으로 감지하고 비활성화하여 문제를 방지합니다.

## 3. 모드 개발 시작하기
### 3.1 C# 학습
모드는 C#으로 작성되므로 C#의 기본 사항(필드, 메서드, 변수, 클래스 등)을 익히는 것이 좋습니다.

### 3.2 요구 사항
* 스타듀밸리 설치
* SMAPI 설치
* .NET 6 SDK 설치 (게임에서 사용하는 버전)
* IDE 설치 (Visual Studio Community, JetBrains Rider, Visual Studio Code 등)

### 3.3 기본 모드 생성
1. Visual Studio 또는 MonoDevelop에서 Class Library 프로젝트를 생성합니다.
2. .NET 6을 대상으로 설정합니다.
3. Pathoschild.Stardew.ModBuildConfig NuGet 패키지를 참조합니다.
4. ModEntry.cs 파일을 추가하고 다음 코드를 포함합니다:

```csharp
using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace YourProjectName
{
    internal sealed class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady) { return; }

            this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);
        }
    }
}
```

### 3.4 매니페스트 추가
모드 폴더에 `manifest.json` 파일을 추가합니다. 이 파일은 모드의 이름, 작성자, 버전, 설명 등 메타데이터를 정의합니다.

### 3.5 모드 테스트
모드를 빌드하고 스타듀밸리 `Mods` 폴더에 복사한 후 게임을 실행하여 테스트합니다.

## 4. FAQ
* SMAPI 문서는 SMAPI GitHub 저장소의 `StardewModdingAPI` 프로젝트에 있습니다.
* 다른 모드의 코드를 참고하는 것은 좋은 학습 방법입니다.
* 모드를 크로스 플랫폼으로 작동시키려면 SMAPI가 자동으로 처리해줍니다.
* 게임 코드를 디컴파일하려면 dnSpy와 같은 도구를 사용할 수 있습니다.
* .NET 6.0을 대상으로 하는 이유는 게임에서 해당 버전을 사용하기 때문입니다.

