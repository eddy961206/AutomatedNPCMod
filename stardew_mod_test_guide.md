# 스타듀밸리 자동화 NPC 모드 프로토타입 테스트 가이드 (Windows PC)

**작성자:** Manus AI  
**작성일:** 2025년 8월 3일  
**버전:** 1.0

---

## 1. 개발 환경 설정

이 가이드는 사용자의 Windows PC에서 스타듀밸리 자동화 NPC 모드 프로토타입을 성공적으로 빌드하고 테스트하기 위한 단계별 지침을 제공합니다. 시작하기 전에 다음 필수 구성 요소들이 설치되어 있는지 확인해야 합니다.

### 1.1. 필수 구성 요소 확인

모드 개발 및 테스트를 위해서는 다음 소프트웨어들이 사용자의 PC에 설치되어 있어야 합니다.

*   **Windows 운영체제:** 본 가이드는 Windows 환경을 기준으로 작성되었습니다.
*   **스타듀밸리 (Steam 버전):** 스팀을 통해 설치된 스타듀밸리 게임이 필요합니다. 모드는 게임 파일에 직접 접근하여 빌드되므로, 게임이 정상적으로 설치되어 있어야 합니다.
*   **SMAPI (Stardew Modding API):** 스타듀밸리 모드를 실행하기 위한 필수 모딩 API입니다. SMAPI는 게임과 모드 간의 상호작용을 가능하게 하며, 모드의 안정적인 로딩과 실행을 보장합니다. SMAPI는 [공식 웹사이트](https://smapi.io/)에서 다운로드하여 설치할 수 있습니다. 설치 가이드를 따라 게임 폴더에 SMAPI를 설치해야 합니다.
*   **.NET SDK 6.0 이상:** 본 프로토타입은 .NET 6.0 환경에서 개발되었습니다. C# 프로젝트를 빌드하기 위해서는 해당 버전의 .NET SDK가 필요합니다. .NET SDK는 [Microsoft 공식 웹사이트](https://dotnet.microsoft.com/download/dotnet/6.0)에서 다운로드하여 설치할 수 있습니다. `dotnet --list-sdks` 명령어를 명령 프롬프트(CMD) 또는 PowerShell에서 실행하여 설치된 SDK 버전을 확인할 수 있습니다.
*   **Visual Studio Code (VS Code) 또는 Visual Studio:** C# 프로젝트를 열고 편집하며 빌드하기 위한 통합 개발 환경(IDE)입니다. VS Code는 가볍고 확장성이 뛰어나며, Visual Studio는 더 강력한 기능을 제공합니다. 둘 중 하나를 선택하여 설치하면 됩니다. VS Code를 사용하는 경우, C# 확장을 설치해야 합니다.

### 1.2. SMAPI 설치 확인

SMAPI가 올바르게 설치되었는지 확인하는 것이 중요합니다. 일반적으로 SMAPI 설치 관리자는 스타듀밸리 게임 폴더에 `StardewModdingAPI.exe` 파일을 생성하고, 게임 실행 시 SMAPI를 통해 게임이 시작되도록 설정합니다. 게임을 실행했을 때 콘솔 창이 뜨면서 SMAPI가 로드되는 메시지가 보인다면 정상적으로 설치된 것입니다.

SMAPI가 설치된 게임 폴더의 경로는 다음과 유사할 것입니다:

`C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley`

이 경로는 `.csproj` 파일에서 게임 어셈블리를 참조하는 데 사용될 수 있습니다. 만약 SMAPI가 설치되지 않았다면, [SMAPI 공식 웹사이트](https://smapi.io/)의 지침에 따라 설치를 완료해야 합니다.

### 1.3. .NET SDK 설치 확인

명령 프롬프트 또는 PowerShell을 열고 다음 명령어를 입력하여 .NET SDK가 올바르게 설치되었는지 확인합니다:

```bash
dotnet --list-sdks
```

이 명령어를 실행했을 때, `6.0.x` 버전의 SDK가 목록에 포함되어 있어야 합니다. 만약 6.0 버전이 없거나 설치되어 있지 않다면, [Microsoft .NET 공식 다운로드 페이지](https://dotnet.microsoft.com/download/dotnet/6.0)에서 `.NET SDK 6.0` 버전을 다운로드하여 설치해야 합니다. 설치 후에는 PC를 재부팅하거나 새 명령 프롬프트 창을 열어 환경 변수가 적용되었는지 확인하는 것이 좋습니다.

### 1.4. Visual Studio Code 설정 (선택 사항)

Visual Studio Code를 사용하는 경우, 다음 확장을 설치하여 C# 개발 환경을 최적화할 수 있습니다:

*   **C# (ms-dotnettools.csharp):** C# 언어 지원, IntelliSense, 디버깅 기능 등을 제공합니다.
*   **NuGet Package Manager (jmrog.vscode-nuget-package-manager):** NuGet 패키지를 관리하는 데 도움을 줍니다.

이러한 확장을 설치하면 코드 편집, 자동 완성, 오류 검사, 디버깅 등의 작업이 훨씬 수월해집니다.




## 2. 프로젝트 파일 준비 및 수정

이제 개발 환경이 준비되었으니, 제공된 프로토타입 코드를 사용자의 PC로 가져와 스타듀밸리 모드 프로젝트로 설정해야 합니다.

### 2.1. 프로토타입 코드 다운로드 및 배치

이 가이드에서는 이전에 생성된 프로토타입 코드(`StardewValleyAutomatedNPCMod` 디렉토리)를 사용합니다. 이 디렉토리에는 `AutomatedNPCMod`라는 서브 디렉토리가 포함되어 있으며, 이 안에 실제 C# 프로젝트 파일들이 있습니다.

1.  **프로토타입 코드 다운로드:** 이 보고서와 함께 제공된 프로토타입 코드 압축 파일(예: `StardewValleyAutomatedNPCMod.zip`)을 다운로드합니다.
2.  **압축 해제:** 다운로드한 압축 파일을 원하는 위치에 압축 해제합니다. 예를 들어, `C:\Users\YourUser\Documents\StardewValleyMods`와 같은 경로에 압축을 해제할 수 있습니다. 압축을 해제하면 `StardewValleyAutomatedNPCMod`라는 폴더가 생성되고, 그 안에 `AutomatedNPCMod` 폴더가 있을 것입니다.
3.  **모드 폴더로 이동:** `AutomatedNPCMod` 폴더 전체를 스타듀밸리 게임의 `Mods` 폴더 안으로 이동시킵니다. `Mods` 폴더는 일반적으로 스타듀밸리 게임이 설치된 경로(`C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley`) 안에 있습니다.

    예시 경로:
    `C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\AutomatedNPCMod`

    이 `AutomatedNPCMod` 폴더 안에는 `AutomatedNPCMod.csproj`, `manifest.json`, `ModEntry.cs` 등의 파일들이 직접적으로 위치해야 합니다.

### 2.2. `.csproj` 파일 수정

프로토타입 코드는 샌드박스 환경에서 빌드되었기 때문에, 실제 스타듀밸리 게임 어셈블리에 대한 참조 경로가 설정되어 있지 않습니다. 사용자의 PC에서 올바르게 빌드되려면 `AutomatedNPCMod.csproj` 파일을 수정하여 게임 어셈블리 경로를 지정해야 합니다.

1.  **`AutomatedNPCMod.csproj` 파일 열기:** `C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\AutomatedNPCMod` 경로로 이동하여 `AutomatedNPCMod.csproj` 파일을 Visual Studio Code 또는 메모장과 같은 텍스트 편집기로 엽니다.

2.  **`GamePath` 속성 추가:** `<PropertyGroup>` 태그 안에 `<GamePath>` 속성을 추가하여 스타듀밸리 게임이 설치된 경로를 지정합니다. 이 경로는 SMAPI가 설치된 게임의 루트 폴더여야 합니다.

    ```xml
    <PropertyGroup>
        <GamePath>C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley</GamePath>
        <!-- 기존 속성들 -->
        <TargetFramework>net6.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
    </PropertyGroup>
    ```

    **주의:** `GamePath`는 사용자의 실제 스타듀밸리 설치 경로와 일치해야 합니다. 위 경로는 일반적인 예시이며, 사용자의 PC 환경에 따라 다를 수 있습니다.

3.  **참조 추가:** `<ItemGroup>` 태그 안에 다음 참조들을 추가하거나, 이미 있다면 `HintPath`가 올바른지 확인합니다. 이 참조들은 게임의 핵심 라이브러리들입니다.

    ```xml
    <ItemGroup>
        <Reference Include="StardewValley">
            <HintPath>$(GamePath)\Stardew Valley.exe</HintPath>
        </Reference>
        <Reference Include="StardewModdingAPI">
            <HintPath>$(GamePath)\StardewModdingAPI.dll</HintPath>
        </Reference>
        <Reference Include="Microsoft.Xna.Framework">
            <HintPath>$(GamePath)\Microsoft.Xna.Framework.dll</HintPath>
        </Reference>
        <Reference Include="Microsoft.Xna.Framework.Game">
            <HintPath>$(GamePath)\Microsoft.Xna.Framework.Game.dll</HintPath>
        </Reference>
        <Reference Include="Microsoft.Xna.Framework.Graphics">
            <HintPath>$(GamePath)\Microsoft.Xna.Framework.Graphics.dll</HintPath>
        </Reference>
        <!-- 기타 필요한 참조 (예: xTile.dll, Netcode.dll 등) -->
        <Reference Include="xTile">
            <HintPath>$(GamePath)\xTile.dll</HintPath>
        </Reference>
        <Reference Include="Netcode">
            <HintPath>$(GamePath)\Netcode.dll</HintPath>
        </Reference>
    </ItemGroup>
    ```

    **참고:** `Pathoschild.Stardew.ModBuildConfig` NuGet 패키지를 사용하는 경우, 위와 같은 수동 `GamePath` 설정이나 `HintPath` 지정 없이도 자동으로 게임 경로를 찾아주므로 더 편리할 수 있습니다. 하지만 프로토타입에서는 수동 설정을 기준으로 설명합니다.

4.  **파일 저장:** `AutomatedNPCMod.csproj` 파일을 저장합니다.

이제 프로젝트 파일이 빌드 준비를 마쳤습니다. 다음 단계에서는 실제로 모드를 빌드하고 게임에서 테스트하는 방법을 설명합니다.




## 3. 모드 빌드 및 게임 내 테스트

`AutomatedNPCMod.csproj` 파일을 수정하여 게임 어셈블리 참조를 올바르게 설정했다면, 이제 모드를 빌드하고 스타듀밸리 게임 내에서 테스트할 준비가 되었습니다.

### 3.1. 모드 빌드하기

모드를 빌드하는 방법은 Visual Studio Code 또는 명령 프롬프트/PowerShell을 사용하는 두 가지 주요 방법이 있습니다.

#### 3.1.1. Visual Studio Code에서 빌드

1.  **VS Code 열기:** Visual Studio Code를 실행합니다.
2.  **폴더 열기:** `파일(File)` 메뉴에서 `폴더 열기(Open Folder...)`를 선택하고, `C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\AutomatedNPCMod` 경로를 선택하여 엽니다.
3.  **터미널 열기:** VS Code 상단 메뉴에서 `터미널(Terminal)` -> `새 터미널(New Terminal)`을 선택하여 터미널 창을 엽니다. 터미널의 현재 디렉토리가 `AutomatedNPCMod` 폴더인지 확인합니다.
4.  **빌드 명령 실행:** 터미널에 다음 명령어를 입력하고 Enter 키를 누릅니다:

    ```bash
    dotnet build
    ```

    이 명령은 프로젝트를 컴파일하고, 필요한 모든 종속성을 해결하여 모드 파일을 생성합니다. 빌드가 성공하면 `AutomatedNPCMod\bin\Debug\net6.0` (또는 `net5.0` 등) 폴더 안에 `AutomatedNPCMod.dll` 파일이 생성될 것입니다. 이 `AutomatedNPCMod.dll` 파일이 바로 게임에서 로드될 모드 파일입니다.

    **성공적인 빌드 메시지 예시:**
    ```
    Microsoft (R) Build Engine version 17.x.x for .NET
    Copyright (C) Microsoft Corporation. All rights reserved.

      Determining projects to restore...
      All projects are up-to-date for restore.
      AutomatedNPCMod -> C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\AutomatedNPCMod\bin\Debug\net6.0\AutomatedNPCMod.dll

    Build succeeded.
        0 Warning(s)
        0 Error(s)

    Time Elapsed 00:00:0X.XX
    ```

    만약 빌드 오류가 발생한다면, 대부분 `.csproj` 파일의 참조 경로가 잘못되었거나, .NET SDK가 올바르게 설치되지 않았을 가능성이 높습니다. `1. 개발 환경 설정` 및 `2. 프로젝트 파일 준비 및 수정` 섹션을 다시 확인해주세요.

#### 3.1.2. 명령 프롬프트/PowerShell에서 빌드

1.  **명령 프롬프트/PowerShell 열기:** Windows 검색창에 `cmd` 또는 `powershell`을 입력하여 실행합니다.
2.  **디렉토리 이동:** 다음 명령어를 입력하여 `AutomatedNPCMod` 프로젝트 폴더로 이동합니다:

    ```bash
    cd 


C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\AutomatedNPCMod
    ```

    **주의:** 이 경로는 사용자의 실제 설치 경로와 일치해야 합니다.

3.  **빌드 명령 실행:** 다음 명령어를 입력하고 Enter 키를 누릅니다:

    ```bash
    dotnet build
    ```

    빌드 결과는 VS Code에서 빌드했을 때와 동일하게 출력됩니다. 성공적으로 빌드되면 `AutomatedNPCMod\bin\Debug\net6.0` 폴더 안에 `AutomatedNPCMod.dll` 파일이 생성됩니다.

### 3.2. 게임 내 테스트

모드가 성공적으로 빌드되었다면, 이제 스타듀밸리 게임을 실행하여 모드의 기능을 테스트할 수 있습니다.

1.  **스타듀밸리 실행:** SMAPI를 통해 스타듀밸리를 실행합니다. 일반적으로 Steam에서 'Stardew Valley'를 실행하면 SMAPI가 자동으로 게임을 로드합니다. SMAPI 콘솔 창이 뜨면서 모드 로딩 로그가 표시될 것입니다.

2.  **모드 로딩 확인:** SMAPI 콘솔 창에서 `AutomatedNPCMod`가 성공적으로 로드되었는지 확인합니다. 다음과 유사한 메시지를 찾을 수 있습니다:

    ```
    [SMAPI] Loaded mod: AutomatedNPCMod 1.0.0 by Manus AI
    ```

    만약 모드 로딩에 실패했다면, SMAPI 콘솔에 오류 메시지가 표시될 것입니다. 이 경우 오류 메시지를 확인하여 문제 해결을 시도해야 합니다.

3.  **게임 플레이 및 기능 테스트:** 게임을 시작하거나 기존 저장 파일을 로드합니다. 게임 내에서 다음 단축키를 사용하여 프로토타입의 기능을 테스트할 수 있습니다.

    *   **F9 키:** 현재 플레이어 위치 근처에 새로운 테스트 NPC를 생성합니다. NPC가 생성되면 게임 화면 하단에 HUD 메시지가 표시됩니다.
    *   **F10 키:** 가장 최근에 생성된 NPC에게 농사 작업을 할당합니다. NPC가 작업 목표 위치로 이동하고 작업을 시뮬레이션합니다. 작업 완료 시 수익이 플레이어에게 정산됩니다.
    *   **F11 키:** 현재 활성화된 모든 NPC의 이름, 위치, 상태(작업 중/대기 중)를 HUD 메시지로 표시합니다.
    *   **F12 키:** 현재 활성화된 모든 NPC를 게임에서 제거합니다.

    **참고:** 프로토타입의 농사, 채굴, 채집 작업은 실제 게임 내 오브젝트와 상호작용하지만, 복잡한 AI와 경로 탐색은 아직 완벽하게 구현되지 않았습니다. NPC는 목표 위치로 이동한 후 해당 위치에서 작업을 시뮬레이션하며, 실제 게임 플레이에 큰 영향을 미치지 않을 수 있습니다. 이는 최소 기능 프로토타입의 한계이며, 향후 개발 계획에서 개선될 부분입니다.

4.  **SMAPI 콘솔 로그 확인:** 게임을 플레이하면서 SMAPI 콘솔 창을 주시합니다. 모드의 동작과 관련된 로그 메시지(NPC 생성, 작업 할당, 작업 완료 등)가 실시간으로 출력될 것입니다. 오류 메시지가 발생하면 이를 기록하여 문제 해결에 활용합니다.

5.  **게임 저장 및 로드 테스트:** 게임을 저장하고 종료한 후 다시 로드하여, 이전에 생성된 NPC와 할당된 작업 데이터가 올바르게 저장되고 복원되는지 확인합니다. 이는 모드의 데이터 영속성 기능을 검증하는 중요한 단계입니다.

이 단계를 통해 사용자는 자동화 NPC 모드 프로토타입의 기본적인 기능을 직접 경험하고, 빌드 및 실행 환경에 대한 이해를 높일 수 있습니다. 문제가 발생하면 SMAPI 콘솔의 로그 메시지를 통해 원인을 파악하고 해결할 수 있습니다.




## 4. 결론

이 가이드를 통해 스타듀밸리 자동화 NPC 모드 프로토타입을 사용자의 Windows PC에서 성공적으로 빌드하고 게임 내에서 테스트하는 방법을 익혔을 것입니다. 이 프로토타입은 모드의 핵심 개념과 기본적인 기능을 보여주며, 향후 개발될 모드의 기반이 됩니다.

프로토타입 테스트 과정에서 발생할 수 있는 문제들은 대부분 `.csproj` 파일의 참조 경로 설정 오류나 .NET SDK 버전 문제에서 비롯됩니다. 이 가이드의 지침을 꼼꼼히 따르고, SMAPI 콘솔의 로그 메시지를 주의 깊게 확인한다면 대부분의 문제를 해결할 수 있을 것입니다.

이 모드는 아직 초기 프로토타입 단계이므로, 실제 게임 플레이에 완벽하게 통합되거나 복잡한 시나리오를 처리하지 못할 수 있습니다. 하지만 이 가이드를 통해 모드 개발의 기본적인 흐름과 테스트 방법을 이해하고, 향후 모드 개발에 기여하거나 자신만의 아이디어를 구현하는 데 도움이 되기를 바랍니다.

궁금한 점이나 추가적인 도움이 필요하면 언제든지 문의해주세요.


