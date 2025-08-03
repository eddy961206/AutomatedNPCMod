# 스타듀밸리 자동화 NPC 모드 개발 종합 보고서

**작성자:** Manus AI  
**작성일:** 2025년 8월 3일  
**버전:** 1.0

---

## 목차

1. [개요](#개요)
2. [프로젝트 목표 및 요구사항](#프로젝트-목표-및-요구사항)
3. [기존 모드 및 모딩 가이드 조사](#기존-모드-및-모딩-가이드-조사)
4. [기술 스택 및 아키텍처 분석](#기술-스택-및-아키텍처-분석)
5. [시스템 설계](#시스템-설계)
6. [프로토타입 구현](#프로토타입-구현)
7. [향후 개발 계획](#향후-개발-계획)
8. [애로사항 및 개선점](#애로사항-및-개선점)
9. [결론](#결론)
10. [참고문헌](#참고문헌)

---

## 개요

본 보고서는 스타듀밸리(Stardew Valley) 게임에서 플레이어가 직접 컨트롤할 수 있는 신규 NPC를 생성하고, 해당 NPC에게 농사, 채집, 광석 채굴 등의 다양한 노동 작업을 할당하여 자동으로 수행하게 하는 SMAPI 기반 C# 모드 개발에 대한 종합적인 조사, 설계, 그리고 프로토타입 구현 결과를 다룹니다.

이 프로젝트는 스타듀밸리의 기존 게임플레이를 확장하여 플레이어가 보다 효율적으로 농장을 관리하고 수익을 창출할 수 있도록 하는 자동화 시스템을 구축하는 것을 목표로 합니다. 개발된 모드는 컴패니언 동반자처럼 각각 따로 일을 할 수 있고 구경도 가능하게 하는 기능을 제공하며, 게임의 밸런스를 해치지 않으면서도 플레이어에게 새로운 게임 경험을 제공합니다.

본 보고서는 삼성 수준의 상세한 문서화를 목표로 하여, 모드 개발의 전 과정에서 수행된 조사, 분석, 설계, 그리고 구현 작업의 결과를 체계적으로 정리하였습니다. 또한 향후 개발 과정에서 참고할 수 있는 구체적인 개발 계획과 예상되는 기술적 도전 과제들을 포함하고 있습니다.

## 프로젝트 목표 및 요구사항

### 주요 목표

이 프로젝트의 핵심 목표는 스타듀밸리 게임 내에서 플레이어가 생성하고 제어할 수 있는 자동화된 NPC 시스템을 구축하는 것입니다. 이 시스템은 플레이어가 직접 수행해야 하는 반복적인 작업들을 NPC에게 위임함으로써 게임의 효율성을 높이고, 동시에 새로운 전략적 요소를 게임에 도입하는 것을 목표로 합니다.

구체적으로, 개발될 모드는 플레이어가 가상의 농장 직원을 고용하는 것과 같은 경험을 제공합니다. 이러한 NPC들은 단순히 자동화된 기계가 아니라, 각자의 능력치와 특성을 가진 개별적인 캐릭터로서 게임 세계에 통합됩니다. 플레이어는 이들에게 다양한 작업을 할당하고, 그 결과로 얻어지는 수익을 통해 농장을 확장하고 발전시킬 수 있습니다.

### 기능적 요구사항

#### 신규 NPC 생성 및 관리
모드는 플레이어가 새로운 NPC를 생성할 수 있는 기능을 제공해야 합니다. 이는 NPC의 이름, 외형, 그리고 기본 능력치를 설정할 수 있는 사용자 친화적인 인터페이스를 포함합니다. 생성된 NPC는 게임 세계에 자연스럽게 통합되어야 하며, 기존 NPC들과 구별되는 시각적 표시나 상호작용 방식을 가져야 합니다.

NPC 관리 시스템은 여러 NPC를 동시에 운용할 수 있도록 설계되어야 하며, 각 NPC의 상태, 현재 작업, 그리고 성과를 추적할 수 있는 기능을 포함해야 합니다. 또한 필요에 따라 NPC를 해고하거나 재배치할 수 있는 유연성을 제공해야 합니다.

#### 스프라이트 및 동작 애니메이션
각 NPC는 고유한 스프라이트와 애니메이션을 가져야 합니다. 이는 기존 게임의 아트 스타일과 일관성을 유지하면서도 커스텀 NPC임을 명확히 구별할 수 있도록 해야 합니다. 애니메이션은 걷기, 작업 수행, 대기 등의 기본적인 동작을 포함해야 하며, 각 작업 유형에 따른 특화된 애니메이션도 제공해야 합니다.

스프라이트 시스템은 확장 가능하도록 설계되어야 하며, 향후 새로운 NPC 타입이나 커스터마이제이션 옵션을 추가할 때 기존 시스템을 크게 수정하지 않고도 통합할 수 있어야 합니다.

#### 작업 할당 시스템
플레이어는 직관적인 UI 또는 명령 시스템을 통해 NPC에게 특정 작업을 할당할 수 있어야 합니다. 이 시스템은 다음과 같은 작업 유형을 지원해야 합니다:

농사 작업에는 씨앗 심기, 물주기, 그리고 수확이 포함됩니다. NPC는 계절과 작물의 성장 단계를 고려하여 적절한 시기에 각 작업을 수행해야 합니다. 또한 비료 사용이나 품질 향상을 위한 추가적인 관리 작업도 수행할 수 있어야 합니다.

채집 작업은 야생 식물, 과일, 그리고 기타 자연에서 얻을 수 있는 자원들을 수집하는 것을 포함합니다. NPC는 계절별로 사용 가능한 채집 아이템을 인식하고, 효율적인 경로로 이동하여 최대한 많은 자원을 수집해야 합니다.

광석 채굴 작업은 광산에서 돌과 광석을 채굴하는 것을 포함합니다. 이는 더 복잡한 환경 탐색과 위험 요소 관리를 필요로 하며, NPC의 전투 능력이나 생존 기술도 고려되어야 합니다.

#### 실제 행동 구현
할당된 작업을 NPC가 실제로 수행할 수 있도록 하는 구현이 필요합니다. 이는 적절한 도구 사용, 목표 지점으로의 이동, 작업 수행 애니메이션, 그리고 결과물 처리를 포함합니다. NPC는 작업 중 발생할 수 있는 다양한 상황에 대응할 수 있어야 하며, 예를 들어 도구가 부족하거나 에너지가 떨어졌을 때 적절한 행동을 취해야 합니다.

작업 실행 시스템은 게임의 기존 메커니즘과 완전히 호환되어야 하며, 플레이어가 직접 수행하는 작업과 동일한 결과를 생성해야 합니다. 이는 게임의 밸런스를 유지하고 공정성을 보장하기 위해 중요합니다.

#### 수익 정산 시스템
NPC가 완료한 작업에서 발생하는 수익은 플레이어에게 정산되어야 합니다. 이는 아이템, 경험치, 그리고 골드를 포함하며, 실시간으로 이루어지거나 일정 주기로 정산될 수 있습니다. 수익 정산 시스템은 투명하고 추적 가능해야 하며, 플레이어가 각 NPC의 성과를 명확히 파악할 수 있어야 합니다.

또한 수익 정산은 NPC의 능력치, 사용된 도구의 품질, 그리고 작업 환경 등의 요소를 고려하여 계산되어야 합니다. 이를 통해 플레이어가 NPC의 능력 향상이나 더 나은 장비 제공에 투자할 동기를 제공할 수 있습니다.

#### AI 및 경로 탐색
NPC는 지능적으로 게임 세계를 탐색하고 이동할 수 있어야 합니다. 이는 효율적인 경로 계산, 장애물 회피, 그리고 동적인 환경 변화에 대한 적응을 포함합니다. AI 시스템은 다른 NPC나 플레이어와의 충돌을 피하고, 게임의 물리 법칙을 준수해야 합니다.

경로 탐색 알고리즘은 성능과 정확성의 균형을 맞춰야 하며, 복잡한 지형이나 다층 구조의 건물에서도 올바르게 작동해야 합니다. 또한 NPC는 작업 우선순위에 따라 경로를 동적으로 조정할 수 있어야 합니다.

### 비기능적 요구사항

#### 성능 최적화
모드는 게임의 전반적인 성능에 부정적인 영향을 미치지 않아야 합니다. 여러 NPC가 동시에 작업을 수행하더라도 프레임 드롭이나 지연이 발생하지 않아야 하며, 메모리 사용량도 합리적인 수준을 유지해야 합니다. 이를 위해 AI 연산과 경로 탐색은 효율적으로 최적화되어야 하며, 불필요한 계산을 최소화해야 합니다.

성능 최적화는 특히 다수의 NPC가 동시에 활동할 때 중요해집니다. 시스템은 NPC의 수가 증가하더라도 선형적으로 성능이 저하되지 않도록 설계되어야 하며, 필요에 따라 작업 부하를 분산하거나 우선순위를 조정할 수 있어야 합니다.

#### 밸런스 고려사항
자동화 시스템이 게임의 기존 밸런스를 해치지 않도록 신중하게 설계되어야 합니다. NPC의 작업 효율성, 수익률, 그리고 유지 비용은 게임의 전체적인 경제 시스템과 조화를 이루어야 합니다. 이는 플레이어가 여전히 직접적인 게임플레이에 참여할 동기를 유지하면서도, 자동화의 이점을 누릴 수 있도록 하는 것을 목표로 합니다.

밸런스 조정은 지속적인 테스트와 피드백을 통해 이루어져야 하며, 필요에 따라 설정 파일을 통해 조정 가능한 매개변수를 제공해야 합니다. 이를 통해 다양한 플레이 스타일과 선호도에 맞춰 모드를 커스터마이징할 수 있습니다.

#### 확장성 및 유지보수성
시스템은 새로운 작업 유형이나 NPC 기능을 쉽게 추가할 수 있도록 설계되어야 합니다. 모듈화된 구조를 통해 개별 기능을 독립적으로 개발하고 테스트할 수 있어야 하며, 다른 모드와의 호환성도 고려해야 합니다. 코드는 명확하고 일관된 스타일로 작성되어야 하며, 충분한 문서화를 통해 향후 개발자들이 쉽게 이해하고 수정할 수 있어야 합니다.

확장성은 특히 커뮤니티 기여를 고려할 때 중요합니다. 다른 개발자들이 새로운 NPC 타입이나 작업 유형을 추가할 수 있도록 하는 API나 플러그인 시스템을 제공하는 것도 고려해볼 수 있습니다.

### 기술적 제약사항

#### SMAPI 호환성
모드는 SMAPI(Stardew Modding API)의 최신 버전과 완전히 호환되어야 하며, SMAPI의 모범 사례와 가이드라인을 준수해야 합니다. 이는 모드의 안정성과 다른 모드와의 호환성을 보장하기 위해 중요합니다.

#### 크로스 플랫폼 지원
모드는 Windows, macOS, 그리고 Linux에서 모두 작동해야 하며, 각 플랫폼의 특성을 고려한 최적화가 필요할 수 있습니다. SMAPI의 크로스 플랫폼 기능을 활용하여 플랫폼별 차이점을 최소화해야 합니다.

#### 게임 버전 호환성
모드는 스타듀밸리의 현재 버전뿐만 아니라 향후 업데이트에도 대응할 수 있도록 설계되어야 합니다. 게임의 내부 API 변경에 대비하여 추상화 계층을 제공하고, 버전별 호환성을 관리할 수 있는 메커니즘을 구축해야 합니다.



## 기존 모드 및 모딩 가이드 조사

### SMAPI 모딩 프레임워크 분석

SMAPI(Stardew Modding API)는 스타듀밸리의 공식 모딩 프레임워크로, 2016년부터 개발되어 현재까지 지속적으로 업데이트되고 있습니다[1]. SMAPI는 게임의 내부 구조에 안전하게 접근할 수 있는 API를 제공하며, 모드 간의 호환성과 안정성을 보장하는 역할을 합니다.

SMAPI의 핵심 기능은 게임의 이벤트 시스템을 통한 안전한 게임 상태 접근입니다. 이는 게임의 업데이트 루프, 저장/로드 이벤트, 플레이어 행동, 그리고 게임 세계의 변화를 모니터링하고 이에 반응할 수 있는 메커니즘을 제공합니다. 특히 중요한 것은 SMAPI가 제공하는 Harmony 패칭 시스템으로, 이를 통해 게임의 기존 메서드를 안전하게 수정하거나 확장할 수 있습니다[2].

SMAPI의 모드 로딩 시스템은 각 모드를 독립적인 어셈블리로 관리하며, 의존성 해결과 버전 호환성 검사를 자동으로 수행합니다. 이는 복잡한 모드 생태계에서 안정성을 보장하는 핵심 요소입니다. 또한 SMAPI는 크로스 플랫폼 지원을 제공하여 Windows, macOS, Linux에서 동일한 모드가 작동할 수 있도록 합니다.

모드 개발자 관점에서 SMAPI는 풍부한 API를 제공합니다. 게임 데이터 접근을 위한 Helper 클래스들, 설정 파일 관리, 다국어 지원, 그리고 에셋 로딩 시스템 등이 포함됩니다. 특히 에셋 로딩 시스템은 게임의 이미지, 맵, 데이터 파일을 동적으로 수정하거나 추가할 수 있게 해주어, 시각적 변경이나 새로운 콘텐츠 추가에 필수적입니다.

### Automate 모드 구조 분석

Automate 모드는 스타듀밸리에서 가장 인기 있는 자동화 모드 중 하나로, 기계들을 연결하여 자동화된 생산 라인을 구축할 수 있게 해줍니다[3]. 이 모드의 구조 분석은 우리가 개발하고자 하는 NPC 자동화 시스템에 중요한 통찰을 제공합니다.

Automate 모드의 핵심 아키텍처는 기계 네트워크 관리 시스템을 중심으로 구성됩니다. 각 기계는 입력과 출력을 가진 노드로 모델링되며, 인접한 기계들과 연결되어 네트워크를 형성합니다. 이 네트워크는 아이템의 흐름을 관리하고, 각 기계의 상태를 모니터링하여 최적의 처리 순서를 결정합니다.

모드의 업데이트 시스템은 게임의 틱 이벤트를 활용하여 주기적으로 모든 기계 네트워크를 검사합니다. 이 과정에서 성능 최적화를 위해 여러 기법이 사용됩니다. 예를 들어, 변경이 없는 네트워크는 건너뛰고, 복잡한 계산은 여러 틱에 걸쳐 분산 처리됩니다. 이러한 접근 방식은 다수의 NPC가 동시에 작업을 수행하는 우리 시스템에서도 적용할 수 있는 중요한 패턴입니다.

Automate 모드의 설정 시스템도 주목할 만합니다. 모드는 JSON 기반의 설정 파일을 통해 사용자가 자동화 규칙을 커스터마이징할 수 있게 합니다. 이는 기계별 우선순위, 아이템 필터링, 그리고 네트워크 동작 방식을 세밀하게 조정할 수 있게 해줍니다. 우리의 NPC 시스템에서도 유사한 설정 시스템을 통해 NPC의 행동 패턴이나 작업 우선순위를 조정할 수 있을 것입니다.

특히 흥미로운 것은 Automate 모드가 다른 모드와의 호환성을 어떻게 관리하는지입니다. 모드는 확장 가능한 인터페이스를 제공하여 다른 모드가 새로운 기계 타입을 추가할 수 있게 합니다. 이는 플러그인 아키텍처의 좋은 예시로, 우리 시스템에서도 새로운 작업 유형이나 NPC 능력을 추가할 때 참고할 수 있습니다.

### NPC 관련 모드 사례 연구

스타듀밸리의 NPC 관련 모드들을 분석하면 NPC 생성과 관리에 대한 다양한 접근 방식을 확인할 수 있습니다. Custom NPC 모드들은 주로 새로운 캐릭터를 게임에 추가하는 데 중점을 두며, 이들은 대화, 선물 주고받기, 그리고 이벤트 참여 등의 사회적 상호작용을 제공합니다[4].

이러한 모드들의 공통적인 구조는 NPC 데이터 정의, 스프라이트 관리, 그리고 행동 스케줄링으로 구성됩니다. NPC 데이터는 일반적으로 JSON 파일로 정의되며, 이름, 생일, 좋아하는 선물, 대화 내용 등의 정보를 포함합니다. 스프라이트 관리는 SMAPI의 에셋 로딩 시스템을 활용하여 커스텀 이미지를 게임에 통합합니다.

행동 스케줄링은 NPC가 하루 동안 어떤 활동을 할지 결정하는 시스템입니다. 기존 모드들은 주로 정적인 스케줄을 사용하여 특정 시간에 특정 위치로 이동하도록 프로그래밍됩니다. 그러나 우리가 개발하고자 하는 자동화 NPC는 동적인 작업 할당이 필요하므로, 더 유연하고 지능적인 스케줄링 시스템이 필요합니다.

NPC의 경로 탐색과 관련해서는 기존 모드들이 게임의 내장 경로 탐색 시스템을 활용하는 것을 확인할 수 있습니다. 스타듀밸리는 A* 알고리즘 기반의 경로 탐색을 제공하지만, 복잡한 환경이나 동적인 장애물에 대한 처리는 제한적입니다. 따라서 우리 시스템에서는 추가적인 경로 최적화나 충돌 회피 로직이 필요할 수 있습니다.

### 모딩 커뮤니티 모범 사례

스타듀밸리 모딩 커뮤니티는 오랜 기간에 걸쳐 형성된 다양한 모범 사례들을 가지고 있습니다[5]. 이러한 관례들을 따르는 것은 모드의 품질과 호환성을 보장하는 데 중요합니다.

코드 구조 측면에서, 커뮤니티는 명확한 네임스페이스 사용, 의존성 주입 패턴, 그리고 이벤트 기반 아키텍처를 권장합니다. 특히 SMAPI의 이벤트 시스템을 적극 활용하여 게임 상태 변화에 반응하는 것이 안정성과 성능 면에서 유리합니다. 또한 Harmony 패치를 사용할 때는 최소한의 변경만을 가하고, 가능한 한 비침습적인 방식을 선택하는 것이 권장됩니다.

성능 최적화 관점에서는 불필요한 계산을 피하고, 캐싱을 적극 활용하며, 무거운 작업은 백그라운드 스레드에서 처리하는 것이 일반적입니다. 특히 게임의 메인 스레드를 블로킹하지 않도록 주의해야 하며, 프레임당 처리량을 제한하여 일정한 성능을 유지해야 합니다.

호환성 관리 측면에서는 다른 모드와의 충돌을 최소화하기 위해 고유한 식별자 사용, 공유 리소스에 대한 신중한 접근, 그리고 버전 호환성 검사가 중요합니다. 또한 모드 간 통신이 필요한 경우에는 표준화된 API나 이벤트 시스템을 활용하는 것이 권장됩니다.

### 기존 자동화 솔루션의 한계점

현재 스타듀밸리에서 사용 가능한 자동화 솔루션들을 분석하면 몇 가지 공통적인 한계점을 발견할 수 있습니다. 첫째, 대부분의 자동화 모드는 기계나 건물 중심의 자동화에 초점을 맞추고 있어, 실제 농장 작업의 많은 부분이 여전히 수동으로 이루어져야 합니다.

둘째, 기존 솔루션들은 정적인 자동화 규칙에 의존하는 경우가 많아, 계절 변화나 작물 성장 단계와 같은 동적인 요소에 대한 적응력이 부족합니다. 이는 플레이어가 지속적으로 설정을 조정해야 하는 번거로움을 야기합니다.

셋째, 대부분의 자동화 시스템은 단일 작업에 특화되어 있어, 복합적인 농장 관리나 다양한 작업 간의 우선순위 조정이 어렵습니다. 예를 들어, 농사와 채집을 동시에 관리하거나, 계절별로 작업 우선순위를 자동으로 조정하는 기능은 제한적입니다.

넷째, 기존 솔루션들은 플레이어와의 상호작용이나 피드백 메커니즘이 부족하여, 자동화 시스템의 상태나 성과를 파악하기 어려운 경우가 많습니다. 이는 플레이어가 시스템을 효과적으로 관리하고 최적화하는 데 장애가 됩니다.

이러한 한계점들은 우리가 개발하고자 하는 NPC 기반 자동화 시스템이 해결해야 할 주요 과제들을 제시합니다. 지능적이고 적응적인 NPC, 다양한 작업을 통합적으로 관리할 수 있는 시스템, 그리고 플레이어와의 효과적인 상호작용 메커니즘이 핵심 요구사항이 됩니다.

### 모딩 도구 및 개발 환경

스타듀밸리 모드 개발을 위한 도구 생태계는 상당히 성숙한 상태입니다. Visual Studio나 Visual Studio Code와 같은 IDE에서 SMAPI 모드를 개발할 수 있으며, 다양한 템플릿과 예제 코드가 제공됩니다[6].

개발 과정에서 가장 중요한 도구는 SMAPI 자체에 포함된 디버깅 기능입니다. 이는 실시간 로그 모니터링, 게임 상태 검사, 그리고 모드 간 상호작용 추적을 제공합니다. 또한 SMAPI는 모드 호환성 검사와 성능 프로파일링 기능도 제공하여 개발자가 품질 높은 모드를 만들 수 있도록 지원합니다.

에셋 관리를 위해서는 Content Patcher와 같은 도구가 널리 사용됩니다. 이는 게임의 이미지, 맵, 데이터 파일을 코드 없이도 수정할 수 있게 해주어, 아티스트나 디자이너도 모드 개발에 참여할 수 있게 합니다.

테스팅 환경 구축도 중요한 요소입니다. 모드 개발자들은 일반적으로 별도의 게임 인스턴스를 사용하여 테스트를 진행하며, 자동화된 테스트 스크립트를 통해 다양한 시나리오를 검증합니다. 특히 멀티플레이어 호환성이나 다른 모드와의 상호작용을 테스트하는 것은 복잡하지만 필수적인 과정입니다.

### 라이선스 및 법적 고려사항

스타듀밸리 모드 개발에는 몇 가지 법적 고려사항이 있습니다. 게임 자체는 상업적 제품이므로, 모드는 게임의 지적 재산권을 침해하지 않는 범위 내에서 개발되어야 합니다[7]. 다행히 ConcernedApe(게임 개발자)는 모딩 커뮤니티를 공식적으로 지원하고 있으며, SMAPI를 통한 모드 개발을 허용하고 있습니다.

모드 배포 시에는 적절한 라이선스를 선택하는 것이 중요합니다. 대부분의 스타듀밸리 모드는 MIT, GPL, 또는 Creative Commons 라이선스를 사용하며, 이는 커뮤니티의 협력과 기여를 촉진합니다. 특히 오픈소스 라이선스를 선택하면 다른 개발자들이 모드를 개선하거나 확장할 수 있어 생태계 발전에 기여할 수 있습니다.

상업적 이용에 대해서는 신중한 접근이 필요합니다. 모드 자체를 판매하는 것은 일반적으로 허용되지 않지만, 후원이나 기부를 받는 것은 커뮤니티에서 일반적으로 받아들여지는 관행입니다. 다만 이러한 활동도 게임의 이용약관과 관련 법규를 준수해야 합니다.




## 기술 스택 및 아키텍처 분석

### 핵심 기술 스택

본 자동화 NPC 모드 개발을 위해 다음과 같은 핵심 기술 스택이 선정되었습니다.

**1. C# 언어:** 스타듀밸리 모딩의 표준 언어이자 SMAPI의 기반 언어입니다. 객체 지향 프로그래밍(OOP) 패러다임을 지원하며, 강력한 타입 시스템과 풍부한 라이브러리를 제공하여 복잡한 게임 로직 구현에 적합합니다. 특히 .NET 프레임워크의 다양한 기능을 활용할 수 있다는 장점이 있습니다.

**2. SMAPI (Stardew Modding API):** 스타듀밸리 모드 개발의 필수 프레임워크입니다. 게임의 내부 API에 안전하게 접근하고, 게임 이벤트를 후킹하며, 모드 간의 호환성을 관리하는 핵심적인 역할을 수행합니다. SMAPI는 모드 개발에 필요한 다양한 헬퍼 함수와 유틸리티를 제공하여 개발 효율성을 높입니다.

**3. Harmony 라이브러리:** SMAPI를 통해 간접적으로 사용되는 라이브러리로, 런타임에 메서드를 패치(Patch)하여 게임의 기존 로직을 수정하거나 확장할 수 있게 해줍니다. 이를 통해 게임의 핵심 기능을 변경하지 않고도 새로운 동작을 추가하거나 기존 동작을 오버라이드할 수 있습니다. 예를 들어, NPC의 기본 AI 루틴을 수정하거나 새로운 상호작용을 추가하는 데 활용될 수 있습니다.

**4. MonoGame Framework (Microsoft XNA Framework):** 스타듀밸리 게임 자체가 MonoGame (과거 XNA) 기반으로 개발되었기 때문에, 그래픽 처리, 스프라이트 애니메이션, 입력 처리 등 게임의 저수준 요소들을 다룰 때 필요합니다. 특히 `Microsoft.Xna.Framework.Vector2`, `Microsoft.Xna.Framework.GameTime` 등은 게임 내 위치, 시간 등을 다루는 데 필수적인 타입입니다.

**5. JSON (JavaScript Object Notation):** 모드의 설정 파일, NPC 데이터, 작업 스케줄 등 구조화된 데이터를 저장하고 로드하는 데 사용됩니다. C#의 `Newtonsoft.Json` 라이브러리 등을 활용하여 객체를 JSON으로 직렬화/역직렬화할 수 있습니다. 이는 모드의 유연성과 확장성을 높이는 데 기여합니다.

**6. Git (버전 관리 시스템):** 소스 코드의 변경 이력을 관리하고, 여러 개발자 간의 협업을 용이하게 합니다. 안정적인 개발 환경을 유지하고, 문제 발생 시 이전 버전으로 쉽게 롤백할 수 있도록 합니다.

### 아키텍처 개요

제안하는 자동화 NPC 모드의 아키텍처는 **모듈화된 계층형 구조**를 채택하여 확장성과 유지보수성을 극대화합니다. 각 계층은 명확한 책임과 역할을 가지며, 느슨하게 결합되어 있어 개별 모듈의 변경이 전체 시스템에 미치는 영향을 최소화합니다.

**1. Presentation Layer (UI/UX):**
   - **UIManager:** 플레이어의 입력(키보드, 마우스)을 처리하고, 게임 내 UI(HUD 메시지, 메뉴 등)를 통해 정보를 표시하거나 상호작용을 유도합니다. NPC에게 작업을 할당하거나, NPC의 상태를 조회하는 등의 명령을 받아서 Core Layer로 전달합니다. SMAPI의 이벤트 시스템(`GameEvents`, `InputEvents` 등)을 활용하여 게임 이벤트를 구독하고 반응합니다.
   - **Custom UI Elements:** 필요에 따라 커스텀 메뉴, 대화 상자, 정보 패널 등을 구현하여 플레이어에게 풍부한 상호작용 경험을 제공합니다. 이는 `StardewValley.Menus` 네임스페이스의 클래스들을 확장하여 구현될 수 있습니다.

**2. Core Layer (Business Logic):**
   - **ModEntry:** SMAPI 모드의 진입점입니다. 모드의 초기화, 주요 구성 요소(Manager 클래스들)의 생성 및 의존성 주입, 그리고 게임 이벤트 구독을 담당합니다. 모드의 생명주기(로드, 언로드, 저장 등)를 관리합니다.
   - **NPCManager:** 게임 내 모든 커스텀 NPC의 생성, 등록, 조회, 삭제를 중앙에서 관리합니다. NPC 객체의 생명주기를 제어하고, 게임 저장/로드 시 NPC 데이터를 직렬화/역직렬화합니다. `StardewValley.NPC` 클래스를 상속받는 `CustomNPC` 객체들을 관리합니다.
   - **TaskManager:** NPC에게 할당될 작업(WorkTask)의 생성, 할당, 우선순위 관리, 그리고 완료 처리를 담당합니다. 작업 큐를 관리하고, 사용 가능한 NPC에게 작업을 분배하는 스케줄링 로직을 포함합니다. 작업 완료 시 수익 정산 로직을 트리거합니다.
   - **ConfigManager:** 모드의 설정(예: NPC 생성 비용, 작업 효율성 배율, 단축키 등)을 관리합니다. JSON 파일을 통해 설정을 로드하고 저장하며, 게임 내에서 설정 변경 UI를 제공할 수 있습니다.

**3. Domain Layer (Models & AI):**
   - **CustomNPC:** `StardewValley.NPC` 클래스를 상속받아 자동화된 NPC의 기본 속성(이름, 위치, 스프라이트 등)과 추가적인 속성(현재 작업, 능력치, AI 상태 등)을 정의합니다. `AIController`와 `WorkExecutor`를 포함하여 NPC의 행동을 제어합니다.
   - **WorkTask:** NPC가 수행할 작업의 추상화된 모델입니다. 작업 유형(농사, 채굴 등), 목표 위치, 할당된 NPC, 진행 상태, 결과 등의 정보를 포함합니다. `TaskType`, `TaskPriority` 등의 열거형을 정의합니다.
   - **TaskResult:** 작업 완료 후의 결과(획득 아이템, 골드, 경험치, 성공 여부 등)를 캡슐화하는 클래스입니다.
   - **NPCStats:** 각 NPC의 능력치(농사 레벨, 채굴 레벨, 에너지 등)를 정의하고 관리합니다. 작업 수행에 필요한 에너지 소모 및 회복 로직을 포함합니다.
   - **AIController:** `CustomNPC`의 지능적인 행동을 제어하는 핵심 모듈입니다. 경로 탐색(`Pathfinder`), 상태 관리(`AIState`), 그리고 목표 지점 도달 여부 판단 등의 로직을 포함합니다. NPC의 이동, 대기, 작업 수행 등의 상태 전이를 관리합니다.
   - **Pathfinder:** 게임 맵 내에서 NPC의 효율적인 이동 경로를 계산하는 알고리즘(예: A* 알고리즘)을 구현합니다. 장애물 회피 및 최단 경로 탐색 기능을 제공합니다. `StardewValley.GameLocation` 및 `xTile` 라이브러리를 활용하여 맵 정보를 분석합니다.
   - **WorkExecutor:** `CustomNPC`에 할당된 `WorkTask`를 실제로 게임 내에서 수행하는 로직을 담당합니다. 각 `TaskType`에 해당하는 `IWorkHandler` 인터페이스를 구현한 클래스들을 포함합니다. 예를 들어, `FarmingHandler`, `MiningHandler`, `ForagingHandler` 등이 있습니다.

**4. Infrastructure Layer (Data & Utilities):**
   - **Data Persistence:** SMAPI의 `IModHelper.Data` 인터페이스를 사용하여 NPC 데이터, 작업 큐, 설정 등을 게임 저장 파일에 저장하고 로드합니다. 이는 게임 세션 간에 모드 상태를 유지하는 데 필수적입니다.
   - **Logging:** SMAPI의 `IMonitor` 인터페이스를 사용하여 모드의 동작 상태, 오류, 경고 등을 콘솔에 출력합니다. 디버깅 및 문제 해결에 중요한 역할을 합니다.
   - **Asset Management:** SMAPI의 에셋 로딩 기능을 활용하여 커스텀 NPC 스프라이트, 애니메이션, UI 이미지 등을 게임에 로드합니다. `Content Patcher`와의 연동을 통해 유연한 에셋 관리가 가능합니다.

### 모듈 간 상호작용 다이어그램

```mermaid
graph TD
    subgraph Player Interaction
        Player[플레이어]
    end

    subgraph Presentation Layer
        UIManager
    end

    subgraph Core Layer
        ModEntry
        NPCManager
        TaskManager
        ConfigManager
    end

    subgraph Domain Layer
        CustomNPC
        WorkTask
        TaskResult
        NPCStats
        AIController
        Pathfinder
        WorkExecutor
        IWorkHandler
        FarmingHandler
        MiningHandler
        ForagingHandler
    end

    subgraph Infrastructure Layer
        SMAPI[SMAPI API]
        GameAPI[Stardew Valley Game API]
        Harmony[Harmony Library]
        MonoGame[MonoGame Framework]
        Json[JSON Serialization]
    end

    Player -- 입력 --> UIManager
    UIManager -- 명령/정보 요청 --> TaskManager
    UIManager -- 명령/정보 요청 --> NPCManager
    UIManager -- 설정 변경 --> ConfigManager

    ModEntry -- 초기화/의존성 주입 --> NPCManager
    ModEntry -- 초기화/의존성 주입 --> TaskManager
    ModEntry -- 초기화/의존성 주입 --> UIManager
    ModEntry -- 초기화/의존성 주입 --> ConfigManager
    ModEntry -- 게임 이벤트 구독 --> SMAPI

    NPCManager -- NPC 생성/관리 --> CustomNPC
    NPCManager -- 데이터 저장/로드 --> Json
    NPCManager -- 게임 캐릭터 추가/제거 --> GameAPI

    TaskManager -- 작업 할당/조회 --> CustomNPC
    TaskManager -- 작업 생성/관리 --> WorkTask
    TaskManager -- 작업 결과 처리 --> TaskResult
    TaskManager -- 데이터 저장/로드 --> Json
    TaskManager -- 수익 정산 --> GameAPI

    CustomNPC -- AI 제어 --> AIController
    CustomNPC -- 작업 실행 --> WorkExecutor
    CustomNPC -- 능력치 조회/업데이트 --> NPCStats
    CustomNPC -- 게임 객체 상호작용 --> GameAPI

    AIController -- 경로 탐색 --> Pathfinder
    AIController -- NPC 이동 제어 --> CustomNPC
    Pathfinder -- 맵 정보 조회 --> GameAPI

    WorkExecutor -- 작업 유형별 처리 --> IWorkHandler
    IWorkHandler -->|inherits| FarmingHandler
    IWorkHandler -->|inherits| MiningHandler
    IWorkHandler -->|inherits| ForagingHandler
    FarmingHandler -- 농사 작업 수행 --> GameAPI
    MiningHandler -- 채굴 작업 수행 --> GameAPI
    ForagingHandler -- 채집 작업 수행 --> GameAPI

    SMAPI -- 게임 이벤트 제공 --> ModEntry
    SMAPI -- API 제공 --> GameAPI
    GameAPI -- 게임 데이터/함수 --> CustomNPC
    GameAPI -- 게임 데이터/함수 --> Pathfinder
    GameAPI -- 게임 데이터/함수 --> WorkExecutor
    Harmony -- 메서드 패치 --> GameAPI
    MonoGame -- 그래픽/입력 --> CustomNPC
    Json -- 데이터 직렬화/역직렬화 --> NPCManager
    Json -- 데이터 직렬화/역직렬화 --> TaskManager

    style Player fill:#f9f,stroke:#333,stroke-width:2px
    style UIManager fill:#bbf,stroke:#333,stroke-width:2px
    style ModEntry fill:#bbf,stroke:#333,stroke-width:2px
    style NPCManager fill:#bbf,stroke:#333,stroke-width:2px
    style TaskManager fill:#bbf,stroke:#333,stroke-width:2px
    style ConfigManager fill:#bbf,stroke:#333,stroke-width:2px
    style CustomNPC fill:#ccf,stroke:#333,stroke-width:2px
    style WorkTask fill:#ccf,stroke:#333,stroke-width:2px
    style TaskResult fill:#ccf,stroke:#333,stroke-width:2px
    style NPCStats fill:#ccf,stroke:#333,stroke-width:2px
    style AIController fill:#ccf,stroke:#333,stroke-width:2px
    style Pathfinder fill:#ccf,stroke:#333,stroke-width:2px
    style WorkExecutor fill:#ccf,stroke:#333,stroke-width:2px
    style IWorkHandler fill:#ddf,stroke:#333,stroke-width:2px
    style FarmingHandler fill:#ddf,stroke:#333,stroke-width:2px
    style MiningHandler fill:#ddf,stroke:#333,stroke-width:2px
    style ForagingHandler fill:#ddf,stroke:#333,stroke-width:2px
    style SMAPI fill:#eef,stroke:#333,stroke-width:2px
    style GameAPI fill:#eef,stroke:#333,stroke-width:2px
    style Harmony fill:#eef,stroke:#333,stroke-width:2px
    style MonoGame fill:#eef,stroke:#333,stroke-width:2px
    style Json fill:#eef,stroke:#333,stroke-width:2px

```

### 데이터 흐름 및 상호작용

**1. 모드 초기화:**
   - `ModEntry`가 SMAPI에 의해 로드됩니다.
   - `ModEntry`는 `NPCManager`, `TaskManager`, `UIManager`, `ConfigManager` 인스턴스를 생성하고 필요한 의존성을 주입합니다.
   - `ModEntry`는 SMAPI의 `GameEvents`, `InputEvents` 등을 구독하여 게임의 주요 이벤트(예: 게임 로드, 업데이트 틱, 키 입력)를 수신할 준비를 합니다.
   - `ConfigManager`는 설정 파일을 로드하여 모드의 초기 동작을 구성합니다.
   - `NPCManager`와 `TaskManager`는 저장된 NPC 및 작업 데이터를 로드합니다.

**2. NPC 생성 및 관리:**
   - 플레이어가 `UIManager`를 통해 NPC 생성 명령을 내립니다.
   - `UIManager`는 `NPCManager`에 NPC 생성을 요청합니다.
   - `NPCManager`는 `CustomNPC` 인스턴스를 생성하고, 게임의 현재 위치에 추가합니다. 이 과정에서 `CustomNPC`는 자신의 `AIController`와 `WorkExecutor`를 초기화합니다.
   - `NPCManager`는 생성된 NPC 정보를 내부 목록에 추가하고, 게임 저장 시 이 데이터를 직렬화하여 보존합니다.

**3. 작업 할당 및 스케줄링:**
   - 플레이어가 `UIManager`를 통해 특정 NPC에게 특정 작업(예: 농사)을 할당합니다.
   - `UIManager`는 `TaskManager`에 작업 생성을 요청하고, 생성된 `WorkTask`를 반환받습니다.
   - `TaskManager`는 이 작업을 특정 NPC에게 할당하거나, 대기 큐에 추가합니다.
   - `TaskManager`는 주기적으로 `ProcessTasks()` 메서드를 호출하여 대기 중인 작업을 사용 가능한 NPC에게 분배합니다.
   - 작업이 할당되면, `TaskManager`는 해당 `CustomNPC`의 `AssignTask()` 메서드를 호출하여 작업을 전달합니다.

**4. NPC 행동 및 작업 수행:**
   - 게임의 매 업데이트 틱마다 `ModEntry`는 `NPCManager`의 `UpdateAllNPCs()`를 호출하고, 각 `CustomNPC`의 `update()` 메서드가 호출됩니다.
   - `CustomNPC`의 `update()` 메서드 내에서 `AIController`의 `Update()`가 호출되어 NPC의 AI 상태(Idle, Moving, Working 등)를 관리합니다.
   - `AIController`는 할당된 작업의 `TargetLocation`으로 이동하기 위해 `Pathfinder`를 사용하여 경로를 계산하고, NPC를 이동시킵니다.
   - NPC가 목표 위치에 도달하면 `AIController`는 상태를 `Working`으로 변경하고, `CustomNPC`는 `WorkExecutor`의 `ExecuteTask()` 메서드를 호출하여 실제 작업을 수행합니다.
   - `WorkExecutor`는 `WorkTask`의 `Type`에 따라 적절한 `IWorkHandler`(예: `FarmingHandler`)를 찾아 작업을 실행합니다. 각 핸들러는 게임 API를 직접 호출하여 작물 수확, 광석 채굴 등의 게임 내 상호작용을 수행합니다.
   - 작업이 완료되면 `WorkExecutor`는 `TaskResult`를 생성하고, `CustomNPC`는 이 결과를 `TaskManager`에 전달하여 작업 완료를 알립니다.

**5. 수익 정산:**
   - `TaskManager`는 작업 완료 알림을 받으면 `TaskResult`에 포함된 정보(획득 아이템, 골드, 경험치)를 기반으로 `DistributeProfits()` 메서드를 호출합니다.
   - `DistributeProfits()`는 `StardewValley.Game1.player` 객체를 통해 플레이어의 인벤토리에 아이템을 추가하거나, 골드를 증가시키고, 경험치를 부여합니다.

**6. 게임 저장/로드:**
   - 게임이 저장될 때, `ModEntry`는 `NPCManager`와 `TaskManager`에 저장 요청을 보냅니다.
   - 각 Manager는 내부적으로 관리하는 NPC 및 작업 데이터를 JSON 형식으로 직렬화하여 SMAPI의 저장 데이터 시스템을 통해 저장합니다.
   - 게임이 로드될 때, 반대로 저장된 데이터를 역직렬화하여 NPC와 작업 상태를 복원합니다.

### 성능 및 밸런스 고려사항

**1. AI 경로/스케줄 관리:**
   - **경로 탐색 최적화:** `Pathfinder`는 A*와 같은 효율적인 알고리즘을 사용하여 최단 경로를 계산합니다. 복잡한 맵에서는 경로 계산 비용이 높아질 수 있으므로, 캐싱 메커니즘을 도입하여 자주 사용되는 경로를 저장하고 재사용할 수 있습니다. 또한, NPC의 이동 가능 영역을 미리 계산하여 탐색 공간을 줄이는 방법도 고려할 수 있습니다.
   - **스케줄링 우선순위:** `TaskManager`는 `TaskPriority` 열거형을 사용하여 작업의 우선순위를 관리합니다. 긴급한 작업(예: 수확)은 높은 우선순위를 부여하여 즉시 처리되도록 하고, 중요도가 낮은 작업은 대기 큐에서 대기하도록 합니다. 이는 게임의 밸런스를 유지하면서도 플레이어의 긴급한 요구에 반응할 수 있게 합니다.
   - **작업 부하 분산:** 다수의 NPC가 동시에 작업을 수행할 때, 특정 지역이나 자원에 병목 현상이 발생하지 않도록 작업 할당 시 부하를 분산하는 로직을 추가할 수 있습니다. 예를 들어, 한 지역에 너무 많은 NPC가 몰리지 않도록 제한하거나, 특정 자원이 고갈되면 다른 자원으로 작업을 전환하도록 지시할 수 있습니다.
   - **비동기 처리:** 복잡한 AI 계산이나 대량의 데이터 처리 작업은 게임의 메인 스레드를 블로킹하지 않도록 비동기적으로 처리하는 것을 고려할 수 있습니다. C#의 `async/await` 패턴이나 별도의 스레드를 활용하여 게임의 프레임 속도에 영향을 주지 않으면서 백그라운드 작업을 수행합니다.

**2. 밸런스 조정:**
   - **수익률 조정:** NPC가 생성하는 수익(골드, 아이템)은 게임의 기존 경제 시스템과 조화를 이루도록 신중하게 조정되어야 합니다. 너무 높은 수익은 게임의 난이도를 낮추고 플레이어의 동기를 감소시킬 수 있으며, 너무 낮은 수익은 NPC 활용의 매력을 떨어뜨릴 수 있습니다. `ConfigManager`를 통해 수익 배율을 설정할 수 있도록 하여 플레이어가 직접 조정할 수 있게 합니다.
   - **에너지 및 휴식 시스템:** NPC에게 에너지 시스템을 도입하여 무한정 작업하는 것을 방지합니다. 작업 수행 시 에너지를 소모하고, 에너지가 부족하면 휴식을 취하도록 합니다. 이는 NPC의 현실성을 높이고, 플레이어가 NPC의 컨디션을 관리하는 전략적 요소를 추가합니다.
   - **도구 및 장비:** NPC가 작업을 수행할 때 도구의 내구도를 소모하거나, 더 좋은 도구를 사용하면 작업 효율성이 증가하도록 설계합니다. 이는 플레이어가 NPC에게 투자할 동기를 부여하고, 게임의 진행에 따라 NPC의 능력을 향상시킬 수 있는 경로를 제공합니다.
   - **작업 시간 및 효율성:** 각 작업 유형별로 소요되는 시간과 NPC의 효율성을 조정하여 밸런스를 맞춥니다. 예를 들어, 채굴은 농사보다 더 많은 시간이 소요되도록 설정하거나, NPC의 레벨이 높을수록 작업 속도가 빨라지도록 합니다.
   - **NPC 유지 비용:** NPC를 고용하고 유지하는 데 비용(예: 일일 급여, 식량)을 부과하여 플레이어가 NPC의 효율성을 고려하도록 합니다. 이는 게임의 경제적 도전 과제를 추가하고, 플레이어가 NPC를 전략적으로 활용하도록 유도합니다.

**3. 성능 저하 방지:**
   - **불필요한 업데이트 최소화:** NPC의 AI나 작업 로직은 필요한 경우에만 업데이트되도록 설계합니다. 예를 들어, NPC가 대기 상태일 때는 AI 계산을 최소화하고, 작업 중일 때만 상세한 로직을 실행합니다.
   - **객체 풀링:** 자주 생성되고 파괴되는 객체(예: 경로 노드, 임시 작업 객체)에 대해 객체 풀링(Object Pooling) 기법을 사용하여 가비지 컬렉션 부하를 줄이고 성능을 향상시킵니다.
   - **거리 기반 로딩:** 플레이어로부터 멀리 떨어진 NPC나 지역은 업데이트 빈도를 낮추거나, 간략화된 AI 로직을 적용하여 성능을 최적화합니다. 이는 게임의 시야 밖에서 발생하는 불필요한 계산을 줄이는 데 효과적입니다.
   - **SMAPI 이벤트 최적화:** SMAPI 이벤트 핸들러 내에서 복잡한 계산을 수행할 때는 주의해야 합니다. 가능한 한 이벤트 핸들러는 가볍게 유지하고, 무거운 작업은 별도의 스레드나 지연된 실행 큐로 오프로드합니다.

이러한 성능 및 밸런스 고려사항들은 모드 개발 전반에 걸쳐 지속적으로 검토되고 적용되어야 합니다. 특히 초기 프로토타입 단계에서는 기능 구현에 집중하되, 이후 최적화 단계에서 이러한 요소들을 적극적으로 반영하여 최종 모드의 품질을 높일 것입니다.




## 시스템 설계

본 자동화 NPC 모드는 모듈화되고 계층적인 아키텍처를 기반으로 설계되었습니다. 이는 각 구성 요소의 독립성을 보장하고, 향후 기능 확장 및 유지보수를 용이하게 합니다. 다음은 주요 클래스 및 파일 단위의 구조 설계도와 각 구성 요소의 상세 설명입니다.

### 1. 클래스/파일 단위의 주요 구조 설계도

```
StardewValleyAutomatedNPCMod/
├── AutomatedNPCMod/                 # 모드 프로젝트 루트 디렉토리
│   ├── AutomatedNPCMod.csproj       # C# 프로젝트 파일
│   ├── manifest.json                # SMAPI 모드 매니페스트 파일
│   ├── ModEntry.cs                  # 모드 진입점 (SMAPI IMod 인터페이스 구현)
│   │
│   ├── Core/                        # 핵심 로직 및 관리자 클래스
│   │   ├── ConfigManager.cs         # 모드 설정 관리
│   │   ├── NPCManager.cs            # CustomNPC 생성, 관리, 저장/로드
│   │   ├── TaskManager.cs           # 작업 생성, 할당, 스케줄링, 결과 처리
│   │   └── UIManager.cs             # 사용자 인터페이스 및 입력 처리
│   │
│   └── Models/                      # 데이터 모델 및 AI/작업 관련 클래스
│       ├── CustomNPC.cs             # 게임 내 Custom NPC 객체 (StardewValley.NPC 상속)
│       ├── WorkTask.cs              # NPC가 수행할 작업 정의 (TaskType, TaskPriority, TaskResult 포함)
│       ├── NPCStats.cs              # NPC의 능력치 및 상태 관리
│       ├── AIController.cs          # NPC의 AI 행동 제어 (경로 탐색, 상태 관리)
│       ├── Pathfinder.cs            # A* 기반 경로 탐색 알고리즘 구현
│       └── WorkExecutor.cs          # 실제 게임 내 작업 실행 로직 (IWorkHandler 인터페이스 및 구현체 포함)
│           ├── IWorkHandler.cs      # 작업 처리기 인터페이스
│           ├── FarmingHandler.cs    # 농사 작업 처리 로직
│           ├── MiningHandler.cs     # 채굴 작업 처리 로직
│           └── ForagingHandler.cs   # 채집 작업 처리 로직
│
└── assets/                          # 모드 에셋 (스프라이트, 사운드 등)
    ├── sprites/                     # NPC 스프라이트 및 애니메이션
    │   └── custom_npc_sprite.png
    └── config.json.default          # 기본 설정 파일 예시
```

### 2. 주요 구성 요소 상세 설명

#### 2.1. ModEntry.cs

`ModEntry` 클래스는 SMAPI 모드의 진입점이며, `IMod` 인터페이스를 구현합니다. 이 클래스는 모드의 생명주기(초기화, 저장, 로드 등)를 관리하고, 다른 핵심 관리자 클래스(`NPCManager`, `TaskManager`, `UIManager`, `ConfigManager`)의 인스턴스를 생성하고 의존성을 주입하는 역할을 합니다. 또한, SMAPI가 제공하는 게임 이벤트(예: `GameLoop.UpdateTicked`, `GameLoop.Saving`, `GameLoop.Saved`, `Input.ButtonPressed`)를 구독하여 모드의 핵심 로직이 게임과 상호작용할 수 있도록 합니다. `ModEntry`는 모드의 전역적인 상태와 로거(`IMonitor`)에 대한 접근을 제공하는 싱글톤 인스턴스(`Instance`)를 가집니다.

#### 2.2. Core/ 디렉토리 (핵심 관리자 클래스)

**2.2.1. ConfigManager.cs**

`ConfigManager`는 모드의 설정(예: NPC 생성 비용, 작업 효율성 배율, 단축키 설정 등)을 관리합니다. SMAPI의 `IModHelper.ReadConfig<T>()` 및 `WriteConfig<T>()` 메서드를 사용하여 JSON 형식의 설정 파일을 읽고 씁니다. 이를 통해 플레이어가 모드의 동작 방식을 유연하게 커스터마이징할 수 있도록 지원합니다. 설정 변경 시 게임 내에 즉시 반영되도록 로직을 구현할 수 있습니다.

**2.2.2. NPCManager.cs**

`NPCManager`는 게임 내에서 생성되고 관리되는 모든 `CustomNPC` 인스턴스를 중앙에서 제어합니다. 주요 기능은 다음과 같습니다:
- **NPC 생성 및 등록:** 새로운 `CustomNPC` 객체를 생성하고 게임 세계에 추가하며, 내부적으로 활성 NPC 목록을 관리합니다.
- **NPC 조회 및 제거:** 이름 또는 ID를 통해 특정 NPC를 조회하거나 게임에서 제거합니다.
- **데이터 저장 및 로드:** 게임 저장 시 현재 활성 NPC들의 상태와 위치를 직렬화하여 저장하고, 게임 로드 시 이를 역직렬화하여 복원합니다. 이는 `NPCSaveData` 및 `NPCData` 모델을 통해 이루어집니다.
- **전역 업데이트:** 게임 틱마다 모든 활성 NPC의 `update()` 메서드를 호출하여 AI 및 작업 로직이 실행되도록 합니다.

**2.2.3. TaskManager.cs**

`TaskManager`는 NPC에게 할당될 작업(`WorkTask`)의 생명주기를 관리합니다. 이는 작업의 생성, 할당, 스케줄링, 진행 상황 추적, 그리고 완료 처리를 포함합니다. 주요 기능은 다음과 같습니다:
- **작업 생성:** 플레이어의 요청에 따라 새로운 `WorkTask` 객체를 생성하고, 대기 큐에 추가합니다.
- **작업 할당:** 대기 큐에 있는 작업을 검사하여, 현재 사용 가능하고 해당 작업을 수행할 수 있는 `CustomNPC`에게 작업을 할당합니다. 이 과정에서 NPC의 능력치와 현재 상태를 고려합니다.
- **작업 완료 처리:** NPC가 작업을 완료하면 `TaskManager`에 결과를 보고하고, `TaskManager`는 이를 기반으로 플레이어에게 수익을 정산합니다(골드, 아이템, 경험치 등).
- **작업 취소:** 할당되거나 대기 중인 작업을 취소할 수 있는 기능을 제공합니다.
- **데이터 저장 및 로드:** 현재 대기 중이거나 활성 상태인 작업 목록을 게임 저장 파일에 저장하고 로드하여 게임 세션 간에 작업 상태를 유지합니다.

**2.2.4. UIManager.cs**

`UIManager`는 플레이어와 모드 간의 상호작용을 담당하는 사용자 인터페이스 로직을 처리합니다. 이는 키 입력 감지, 게임 내 HUD 메시지 표시, 그리고 향후 구현될 커스텀 메뉴 시스템을 포함합니다. 주요 기능은 다음과 같습니다:
- **입력 처리:** SMAPI의 `Input.ButtonPressed` 이벤트를 구독하여 플레이어의 키 입력을 감지하고, 정의된 단축키에 따라 적절한 모드 기능을 호출합니다 (예: NPC 생성, 작업 할당, NPC 정보 표시).
- **피드백 제공:** 게임 내 HUD 메시지를 통해 플레이어에게 모드의 상태, 작업 완료 알림, 오류 메시지 등을 시각적으로 전달합니다.
- **커스텀 UI:** (향후 구현) NPC 관리, 작업 스케줄링, 설정 변경 등을 위한 전용 UI 메뉴를 제공하여 플레이어가 보다 편리하게 모드를 사용할 수 있도록 합니다.

#### 2.3. Models/ 디렉토리 (데이터 모델 및 AI/작업 로직)

**2.3.1. CustomNPC.cs**

`CustomNPC` 클래스는 스타듀밸리 게임의 `StardewValley.NPC` 클래스를 상속받아 구현됩니다. 이는 게임의 기존 NPC 시스템과의 호환성을 유지하면서도, 자동화된 작업을 수행하는 데 필요한 추가적인 속성과 행동 로직을 포함합니다. 주요 특징은 다음과 같습니다:
- **기본 NPC 속성:** 이름, 위치, 스프라이트, 애니메이션 등 `StardewValley.NPC`의 기본 속성을 활용합니다.
- **AI 및 작업 제어:** `AIController`와 `WorkExecutor` 인스턴스를 내부에 포함하여 NPC의 지능적인 행동과 실제 작업 수행을 위임합니다.
- **작업 할당 및 상태:** 현재 할당된 `WorkTask`를 관리하고, NPC가 작업 중인지(`IsBusy()`) 여부를 나타내는 상태를 가집니다.
- **능력치 관리:** `NPCStats` 객체를 통해 농사, 채굴, 채집 등 각 작업 유형에 대한 NPC의 능력치와 에너지 상태를 관리합니다.
- **게임 업데이트 루프:** 게임 틱마다 `update()` 메서드가 호출되어 `AIController`와 `WorkExecutor`를 업데이트하고, NPC의 시각적 상태(애니메이션, 방향)를 갱신합니다.

**2.3.2. WorkTask.cs, TaskResult.cs, NPCStats.cs**

이 파일들은 모드의 핵심 데이터 모델을 정의합니다:
- **WorkTask:** NPC가 수행할 단일 작업을 나타내는 데이터 구조입니다. 작업 유형(`TaskType`), 목표 위치(`TargetLocation`), 할당된 NPC, 생성 시간, 시작 시간, 완료 시간, 우선순위(`TaskPriority`), 그리고 작업에 필요한 추가 매개변수(`Parameters`)를 포함합니다. 작업의 완료 여부(`IsCompleted`)와 결과(`Result`)도 관리합니다.
- **TaskResult:** `WorkTask`가 완료된 후의 결과를 캡슐화하는 클래스입니다. 작업의 성공 여부(`Success`), 획득한 아이템 목록(`ItemsObtained`), 얻은 경험치(`ExperienceGained`), 벌어들인 골드(`GoldEarned`), 소요 시간(`TimeTaken`), 그리고 발생한 오류 메시지(`ErrorMessage`) 등을 포함합니다.
- **NPCStats:** `CustomNPC`의 다양한 능력치(농사 레벨, 채굴 레벨, 에너지 등)를 정의하고 관리합니다. 각 작업 유형에 따른 레벨 조회, 에너지 소모 및 회복 로직을 포함하여 NPC의 작업 효율성과 지속 가능성을 시뮬레이션합니다.

**2.3.3. AIController.cs**

`AIController`는 `CustomNPC`의 지능적인 행동을 제어하는 핵심 로직을 담당합니다. 이는 NPC의 이동, 상태 관리, 그리고 목표 지점 도달 여부 판단을 포함합니다. 주요 기능은 다음과 같습니다:
- **상태 관리:** `AIState` 열거형(Idle, Moving, Working, Returning, Resting)을 사용하여 NPC의 현재 행동 상태를 관리하고, 상태 전이를 제어합니다.
- **목표 설정 및 이동:** `SetDestination()` 메서드를 통해 NPC의 목표 타일 위치를 설정하고, `Pathfinder`를 사용하여 해당 위치까지의 경로를 계산합니다. 계산된 경로를 따라 NPC를 이동시킵니다.
- **경로 추적 및 도달 확인:** NPC가 경로를 따라 이동하는 과정을 관리하고, 목표 지점에 성공적으로 도달했는지 여부를 확인합니다.
- **애니메이션 제어:** NPC의 이동 방향에 따라 적절한 스프라이트 애니메이션을 재생하도록 `CustomNPC`의 시각적 상태를 업데이트합니다.

**2.3.4. Pathfinder.cs**

`Pathfinder` 클래스는 게임 맵 내에서 NPC의 효율적인 이동 경로를 계산하는 역할을 합니다. 현재 프로토타입에서는 간단한 직선 경로 탐색 로직이 구현되어 있지만, 최종 구현에서는 A*와 같은 고급 경로 탐색 알고리즘을 사용하여 복잡한 지형과 장애물을 회피하며 최단 경로를 찾을 수 있도록 확장될 예정입니다. `StardewValley.GameLocation`의 `isTilePassable()` 메서드를 활용하여 이동 가능한 타일인지 여부를 판단합니다.

**2.3.5. WorkExecutor.cs 및 IWorkHandler.cs, FarmingHandler.cs, MiningHandler.cs, ForagingHandler.cs**

`WorkExecutor`는 `CustomNPC`에 할당된 `WorkTask`를 실제로 게임 내에서 수행하는 로직을 담당하는 핵심 모듈입니다. 이 클래스는 `IWorkHandler` 인터페이스와 이를 구현하는 여러 핸들러 클래스(예: `FarmingHandler`, `MiningHandler`, `ForagingHandler`)를 사용하여 다양한 작업 유형을 처리하는 전략 패턴을 따릅니다.
- **IWorkHandler:** `Execute()`, `CanHandle()`, `GetResult()` 등의 메서드를 정의하는 인터페이스로, 각 작업 유형별 처리기가 구현해야 할 계약을 명시합니다.
- **FarmingHandler:** 농사 작업(씨앗 심기, 물주기, 수확)을 처리하는 구체적인 로직을 구현합니다. `StardewValley.TerrainFeatures.HoeDirt` 및 `StardewValley.Crop` 클래스와 상호작용하여 작물 상태를 확인하고 변경합니다.
- **MiningHandler:** 채굴 작업(돌, 광석 채굴)을 처리하는 로직을 구현합니다. `StardewValley.Object` 클래스와 상호작용하여 채굴 가능한 객체를 식별하고 제거하며, 채굴된 아이템을 생성합니다.
- **ForagingHandler:** 채집 작업(야생 식물, 과일 수집)을 처리하는 로직을 구현합니다. 게임 내 채집 가능한 아이템을 식별하고 수집하며, 플레이어에게 수익을 정산합니다.

각 핸들러는 작업 수행에 필요한 시간(`WORK_DURATION`)을 시뮬레이션하고, 작업 완료 시 `TaskResult`를 생성하여 `WorkExecutor`를 통해 `TaskManager`에 보고합니다. 이 모듈화된 접근 방식은 새로운 작업 유형을 추가할 때 기존 코드를 수정하지 않고 새로운 핸들러 클래스만 추가하면 되므로 확장성이 매우 높습니다.

### 3. 데이터 저장 및 관리

모드의 모든 영구 데이터(NPC 정보, 작업 상태, 설정 등)는 SMAPI의 `IModHelper.Data` 인터페이스를 통해 게임 저장 파일에 통합됩니다. 이는 JSON 직렬화를 사용하여 C# 객체를 텍스트 형식으로 변환하고, 게임 로드 시 다시 객체로 복원하는 방식으로 이루어집니다. 이 방식은 모드 데이터의 무결성을 보장하고, 게임 세션 간에 모드 상태를 유지하는 데 필수적입니다.

- `NPCManager`는 `NPCSaveData` 객체를 사용하여 모든 `CustomNPC`의 핵심 정보(이름, 위치, 스프라이트 시트 등)를 저장합니다.
- `TaskManager`는 `TaskSaveData` 객체를 사용하여 대기 중이거나 활성 상태인 `WorkTask` 목록을 저장합니다.
- `ConfigManager`는 `Config` 객체를 사용하여 모드 설정을 저장합니다.

이러한 데이터 저장 전략은 모드가 게임의 저장/로드 시스템과 원활하게 통합되도록 하며, 플레이어가 게임을 종료했다가 다시 시작해도 모드의 상태가 유지되도록 합니다.

### 4. 성능 및 밸런스 고려사항 (설계 관점)

시스템 설계 단계에서부터 성능과 밸런스를 최우선으로 고려했습니다:
- **모듈화 및 책임 분리:** 각 클래스가 명확한 책임을 가지도록 설계하여, 특정 기능의 변경이 다른 부분에 미치는 영향을 최소화하고, 성능 병목 지점을 쉽게 식별할 수 있도록 합니다.
- **이벤트 기반 아키텍처:** SMAPI의 이벤트 시스템을 적극 활용하여 필요한 시점에만 로직이 실행되도록 합니다. 불필요한 폴링이나 반복적인 계산을 피하여 CPU 사용량을 줄입니다.
- **AI 상태 머신:** `AIController`의 상태 머신은 NPC가 불필요한 계산을 수행하지 않도록 합니다. 예를 들어, `Idle` 상태에서는 최소한의 업데이트만 수행하고, `Moving` 상태에서만 경로 탐색 및 이동 로직을 활성화합니다.
- **작업 우선순위:** `TaskManager`의 작업 우선순위 시스템은 중요한 작업이 먼저 처리되도록 하여 게임 플레이의 흐름을 방해하지 않도록 합니다.
- **확장 가능한 작업 처리기:** `WorkExecutor`의 `IWorkHandler` 패턴은 새로운 작업 유형을 추가할 때 기존 코드를 수정하지 않고 새로운 핸들러만 추가하면 되므로, 코드의 복잡성을 낮추고 유지보수성을 높입니다.
- **데이터 직렬화 최적화:** JSON 직렬화는 비교적 가볍고 빠르지만, 대량의 데이터를 처리할 때는 성능 저하가 발생할 수 있습니다. 이를 위해 필요한 최소한의 데이터만 저장하고, 복잡한 객체 그래프는 참조 대신 ID를 사용하여 저장하는 방식을 고려할 수 있습니다.

이러한 설계 원칙들은 프로토타입 구현에 반영되었으며, 향후 개발 단계에서 지속적인 성능 프로파일링과 밸런스 테스트를 통해 더욱 최적화될 예정입니다.




## 프로토타입 구현

본 프로젝트의 핵심 목표 중 하나는 제안된 아키텍처와 기술 스택을 기반으로 최소 기능 프로토타입을 구현하는 것입니다. 이 프로토타입은 자동화 NPC 모드의 핵심 구성 요소들이 어떻게 상호작용하는지 보여주고, 기본적인 NPC 생성, 작업 할당, 그리고 AI 동작을 시뮬레이션합니다.

### 1. 프로토타입 코드 구조

프로토타입은 다음의 파일 및 디렉토리 구조로 구성됩니다:

```
StardewValleyAutomatedNPCMod/
├── AutomatedNPCMod/                 # 모드 프로젝트 루트 디렉토리
│   ├── AutomatedNPCMod.csproj       # C# 프로젝트 파일
│   ├── manifest.json                # SMAPI 모드 매니페스트 파일
│   ├── ModEntry.cs                  # 모드 진입점
│   │
│   ├── Core/                        # 핵심 로직 및 관리자 클래스
│   │   ├── ConfigManager.cs         # 모드 설정 관리 (더미 구현)
│   │   ├── NPCManager.cs            # CustomNPC 생성, 관리, 저장/로드
│   │   ├── TaskManager.cs           # 작업 생성, 할당, 스케줄링, 결과 처리
│   │   └── UIManager.cs             # 사용자 인터페이스 및 입력 처리
│   │
│   └── Models/                      # 데이터 모델 및 AI/작업 관련 클래스
│       ├── CustomNPC.cs             # 게임 내 Custom NPC 객체
│       ├── WorkTask.cs              # NPC가 수행할 작업 정의
│       ├── NPCStats.cs              # NPC의 능력치 및 상태 관리
│       ├── AIController.cs          # NPC의 AI 행동 제어
│       ├── Pathfinder.cs            # 간단한 경로 탐색 알고리즘
│       └── WorkExecutor.cs          # 실제 게임 내 작업 실행 로직
│           ├── IWorkHandler.cs      # 작업 처리기 인터페이스
│           ├── FarmingHandler.cs    # 농사 작업 처리 로직
│           ├── MiningHandler.cs     # 채굴 작업 처리 로직
│           └── ForagingHandler.cs   # 채집 작업 처리 로직
```

### 2. 주요 클래스별 코드 설명

#### 2.2.1. ModEntry.cs

`ModEntry`는 SMAPI 모드의 진입점입니다. `IMod` 인터페이스를 구현하며, 모드 초기화 시 `Entry` 메서드가 호출됩니다. 이 메서드에서는 `ConfigManager`, `NPCManager`, `TaskManager`, `UIManager` 인스턴스를 생성하고, 필요한 SMAPI 이벤트(예: `GameLoop.UpdateTicked`, `Input.ButtonPressed`)를 구독합니다. 또한, `Instance` 속성을 통해 다른 클래스에서 `ModEntry`의 인스턴스에 접근할 수 있도록 싱글톤 패턴을 적용했습니다.

```csharp
using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using AutomatedNPCMod.Core;

namespace AutomatedNPCMod
{
    /// <summary>
    /// Automated NPC Mod의 메인 클래스.
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
                configManager = new ConfigManager(helper, Monitor);
                npcManager = new NPCManager(helper, Monitor);
                taskManager = new TaskManager(npcManager, helper, Monitor);
                uiManager = new UIManager(npcManager, taskManager, helper, Monitor);

                // 이벤트 구독
                helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
                helper.Events.Input.ButtonPressed += OnButtonPressed;
                helper.Events.GameLoop.Saving += OnSaving;
                helper.Events.GameLoop.Saved += OnSaved;
                helper.Events.Player.LocationChanged += OnLocationChanged;

                Monitor.Log("Automated NPC Mod 초기화 완료", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Automated NPC Mod 초기화 중 오류 발생: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 게임 틱마다 호출됩니다.
        /// </summary>
        /// <param name="sender">이벤트 발신자</param>
        /// <param name="e">이벤트 인수</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady) return; // 게임 로드 전에는 처리하지 않음

            // NPC 업데이트
            npcManager.UpdateAllNPCs(StardewValley.Game1.currentGameTime);

            // 작업 처리
            taskManager.ProcessTasks();
        }

        /// <summary>
        /// 버튼이 눌렸을 때 호출됩니다.
        /// </summary>
        /// <param name="sender">이벤트 발신자</param>
        /// <param name="e">이벤트 인수</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree) return; // 플레이어가 자유롭지 않으면 처리하지 않음

            uiManager.HandleInput(e.Button);
        }

        /// <summary>
        /// 게임 저장 시 호출됩니다.
        /// </summary>
        /// <param name="sender">이벤트 발신자</param>
        /// <param name="e">이벤트 인수</param>
        private void OnSaving(object sender, SavingEventArgs e)
        {
            npcManager.SaveNPCData();
            taskManager.SaveTaskData();
            Monitor.Log("모드 데이터 저장 완료.", LogLevel.Info);
        }

        /// <summary>
        /// 게임 저장 완료 후 호출됩니다.
        /// </summary>
        /// <param name="sender">이벤트 발신자</param>
        /// <param name="e">이벤트 인수</param>
        private void OnSaved(object sender, SavedEventArgs e)
        {
            // 저장 완료 후 필요한 작업 (예: 메시지 표시)
        }

        /// <summary>
        /// 플레이어 위치 변경 시 호출됩니다.
        /// </summary>
        /// <param name="sender">이벤트 발신자</param>
        /// <param name="e">이벤트 인수</param>
        private void OnLocationChanged(object sender, LocationChangedEventArgs e)
        {
            npcManager.OnLocationChanged(e.NewLocation);
        }

        // 다른 클래스에서 접근할 수 있도록 Manager 인스턴스 제공
        public NPCManager NPCManager => npcManager;
        public TaskManager TaskManager => taskManager;
        public UIManager UIManager => uiManager;
        public ConfigManager ConfigManager => configManager;
    }
}
```

#### 2.2.2. Core/NPCManager.cs

`NPCManager`는 `CustomNPC` 객체들을 생성, 관리, 저장/로드하는 역할을 합니다. `CreateNPC` 메서드는 새로운 NPC를 게임에 추가하고, `RemoveNPC`는 제거합니다. `UpdateAllNPCs`는 매 틱마다 모든 활성 NPC의 `update` 메서드를 호출하여 AI 및 작업 로직이 실행되도록 합니다. `SaveNPCData`와 `LoadNPCData`는 SMAPI의 데이터 저장 기능을 활용하여 NPC 상태를 영구적으로 저장하고 복원합니다.

```csharp
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
        public bool CreateNPC(string name, Vector2 position, string spriteSheet = null)
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

                _monitor.Log($"NPC 	'{name}'	 생성 완료. 위치: 	{position}	", LogLevel.Info);
                return true;
            }
            catch (Exception ex)
            {
                _monitor.Log($"NPC 생성 중 오류 발생: 	{ex.Message}	", LogLevel.Error);
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
                    _monitor.Log($"존재하지 않는 NPC: 	{name}	", LogLevel.Warn);
                    return false;
                }

                // 게임에서 NPC 제거
                npc.currentLocation?.characters.Remove(npc);

                // 활성 NPC 목록에서 제거
                _activeNPCs.Remove(name);

                _monitor.Log($"NPC 	'{name}'	 제거 완료", LogLevel.Info);
                return true;
            }
            catch (Exception ex)
            {
                _monitor.Log($"NPC 제거 중 오류 발생: 	{ex.Message}	", LogLevel.Error);
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
                        npc.Update(gameTime, npc.currentLocation);
                    }
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"NPC 업데이트 중 오류 발생: 	{ex.Message}	", LogLevel.Error);
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
                _monitor.Log($"NPC 데이터 저장 중 오류 발생: 	{ex.Message}	", LogLevel.Error);
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
                _monitor.Log($"NPC 데이터 로드 중 오류 발생: 	{ex.Message}	", LogLevel.Error);
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
            _monitor.Log($"위치 	'{newLocation.Name}'	에 	{npcsInLocation.Count}	개의 NPC가 있습니다", LogLevel.Trace);
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
```

#### 2.2.3. Core/TaskManager.cs

`TaskManager`는 `WorkTask`의 생성, 할당, 처리, 완료를 관리합니다. `AssignTask`는 NPC에게 작업을 할당하고, `CreateTask`는 새로운 작업을 생성하여 대기 큐에 추가합니다. `ProcessTasks`는 대기 중인 작업을 사용 가능한 NPC에게 분배하며, `CompleteTask`는 작업 완료 후 수익을 정산합니다. `SaveTaskData`와 `LoadTaskData`는 작업 상태를 저장하고 로드합니다.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using AutomatedNPCMod.Models;

namespace AutomatedNPCMod.Core
{
    /// <summary>
    /// 작업의 생성, 할당, 실행, 완료를 관리하는 클래스.
    /// </summary>
    public class TaskManager
    {
        private readonly NPCManager _npcManager;
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly Queue<WorkTask> _pendingTasks;
        private readonly Dictionary<string, WorkTask> _activeTasks;

        public TaskManager(NPCManager npcManager, IModHelper helper, IMonitor monitor)
        {
            _npcManager = npcManager;
            _helper = helper;
            _monitor = monitor;
            _pendingTasks = new Queue<WorkTask>();
            _activeTasks = new Dictionary<string, WorkTask>();
        }

        /// <summary>
        /// NPC에게 작업을 할당합니다.
        /// </summary>
        /// <param name="npcName">NPC 이름</param>
        /// <param name="task">할당할 작업</param>
        /// <returns>할당 성공 여부</returns>
        public bool AssignTask(string npcName, WorkTask task)
        {
            try
            {
                var npc = _npcManager.GetNPC(npcName);
                if (npc == null)
                {
                    _monitor.Log($"존재하지 않는 NPC: 	{npcName}	", LogLevel.Warn);
                    return false;
                }

                // NPC가 해당 작업을 수행할 수 있는지 확인
                if (!npc.CanPerformTask(task.Type))
                {
                    _monitor.Log($"NPC 	'{npcName}'	는 	{task.Type}	 작업을 수행할 수 없습니다", LogLevel.Warn);
                    return false;
                }

                // 작업 할당
                task.AssignedNPCId = npcName;
                task.StartTime = DateTime.Now;

                if (npc.AssignTask(task))
                {
                    _activeTasks[task.Id] = task;
                    _monitor.Log($"NPC 	'{npcName}'	에게 	{task.Type}	 작업 할당 완료", LogLevel.Info);
                    return true;
                }
                else
                {
                    _monitor.Log($"NPC 	'{npcName}'	에게 작업 할당 실패", LogLevel.Warn);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"작업 할당 중 오류 발생: 	{ex.Message}	", LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// 새로운 작업을 생성합니다.
        /// </summary>
        /// <param name="taskType">작업 유형</param>
        /// <param name="targetLocation">목표 위치</param>
        /// <param name="priority">우선순위</param>
        /// <returns>생성된 작업</returns>
        public WorkTask CreateTask(TaskType taskType, Vector2 targetLocation, TaskPriority priority = TaskPriority.Normal)
        {
            var task = new WorkTask
            {
                Id = Guid.NewGuid().ToString(),
                Type = taskType,
                TargetLocation = targetLocation,
                Priority = priority,
                CreatedTime = DateTime.Now,
                Parameters = new Dictionary<string, object>()
            };

            _pendingTasks.Enqueue(task);
            _monitor.Log($"새로운 	{taskType}	 작업 생성: 	{task.Id}	", LogLevel.Debug);

            return task;
        }

        /// <summary>
        /// 대기 중인 작업들을 처리합니다.
        /// </summary>
        public void ProcessTasks()
        {
            try
            {
                // 대기 중인 작업을 사용 가능한 NPC에게 할당
                while (_pendingTasks.Count > 0)
                {
                    var task = _pendingTasks.Peek();
                    var availableNPC = FindAvailableNPC(task.Type);

                    if (availableNPC != null)
                    {
                        _pendingTasks.Dequeue();
                        AssignTask(availableNPC.Name, task);
                    }
                    else
                    {
                        // 사용 가능한 NPC가 없으면 대기
                        break;
                    }
                }

                // 완료된 작업 정리
                CleanupCompletedTasks();
            }
            catch (Exception ex)
            {
                _monitor.Log($"작업 처리 중 오류 발생: 	{ex.Message}	", LogLevel.Error);
            }
        }

        /// <summary>
        /// 작업 완료를 처리하고 수익을 정산합니다.
        /// </summary>
        /// <param name="taskId">완료된 작업 ID</param>
        /// <param name="result">작업 결과</param>
        public void CompleteTask(string taskId, TaskResult result)
        {
            try
            {
                if (!_activeTasks.TryGetValue(taskId, out WorkTask task))
                {
                    _monitor.Log($"존재하지 않는 작업 ID: 	{taskId}	", LogLevel.Warn);
                    return;
                }

                task.IsCompleted = true;
                task.CompletedTime = DateTime.Now;
                task.Result = result;

                // 수익 분배
                if (result.Success)
                {
                    DistributeProfits(result);
                    _monitor.Log($"작업 	'{taskId}'	 완료. 수익: 	{result.GoldEarned}	G", LogLevel.Info);
                }
                else
                {
                    _monitor.Log($"작업 	'{taskId}'	 실패: 	{result.ErrorMessage}	", LogLevel.Warn);
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"작업 완료 처리 중 오류 발생: 	{ex.Message}	", LogLevel.Error);
            }
        }

        /// <summary>
        /// 특정 NPC의 작업 목록을 반환합니다.
        /// </summary>
        /// <param name="npcName">NPC 이름</param>
        /// <returns>작업 목록</returns>
        public List<WorkTask> GetTasksForNPC(string npcName)
        {
            return _activeTasks.Values
                .Where(task => task.AssignedNPCId == npcName)
                .ToList();
        }

        /// <summary>
        /// 작업을 취소합니다.
        /// </summary>
        /// <param name="taskId">취소할 작업 ID</param>
        public void CancelTask(string taskId)
        {
            try
            {
                if (_activeTasks.TryGetValue(taskId, out WorkTask task))
                {
                    var npc = _npcManager.GetNPC(task.AssignedNPCId);
                    npc?.CancelCurrentTask();

                    _activeTasks.Remove(taskId);
                    _monitor.Log($"작업 	'{taskId}'	 취소됨", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"작업 취소 중 오류 발생: 	{ex.Message}	", LogLevel.Error);
            }
        }

        /// <summary>
        /// 특정 작업 유형을 수행할 수 있는 사용 가능한 NPC를 찾습니다.
        /// </summary>
        /// <param name="taskType">작업 유형</param>
        /// <returns>사용 가능한 NPC 또는 null</returns>
        private CustomNPC FindAvailableNPC(TaskType taskType)
        {
            return _npcManager.GetAllNPCs()
                .Where(npc => npc.CanPerformTask(taskType) && !npc.IsBusy())
                .FirstOrDefault();
        }

        /// <summary>
        /// 완료된 작업들을 정리합니다.
        /// </summary>
        private void CleanupCompletedTasks()
        {
            var completedTasks = _activeTasks.Values
                .Where(task => task.IsCompleted)
                .ToList();

            foreach (var task in completedTasks)
            {
                _activeTasks.Remove(task.Id);
            }
        }

        /// <summary>
        /// 수익을 플레이어에게 분배합니다.
        /// </summary>
        /// <param name="result">작업 결과</param>
        private void DistributeProfits(TaskResult result)
        {
            try
            {
                // 골드 추가
                if (result.GoldEarned > 0)
                {
                    StardewValley.Game1.player.Money += result.GoldEarned;
                }

                // 아이템 추가
                if (result.ItemsObtained?.Count > 0)
                {
                    foreach (var item in result.ItemsObtained)
                    {
                        StardewValley.Game1.player.addItemToInventory(item);
                    }
                }

                // 경험치 추가 (필요시 구현)
                if (result.ExperienceGained > 0)
                {
                    // 경험치 추가 로직
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"수익 분배 중 오류 발생: 	{ex.Message}	", LogLevel.Error);
            }
        }

        /// <summary>
        /// 작업 데이터를 저장합니다.
        /// </summary>
        public void SaveTaskData()
        {
            try
            {
                var saveData = new TaskSaveData
                {
                    PendingTasks = _pendingTasks.ToList(),
                    ActiveTasks = _activeTasks.Values.ToList()
                };

                _helper.Data.WriteSaveData("task-data", saveData);
                _monitor.Log($"작업 데이터 저장 완료", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor.Log($"작업 데이터 저장 중 오류 발생: 	{ex.Message}	", LogLevel.Error);
            }
        }

        /// <summary>
        /// 작업 데이터를 로드합니다.
        /// </summary>
        public void LoadTaskData()
        {
            try
            {
                var saveData = _helper.Data.ReadSaveData<TaskSaveData>("task-data");
                if (saveData == null)
                {
                    _monitor.Log("로드할 작업 데이터가 없습니다", LogLevel.Debug);
                    return;
                }

                // 대기 중인 작업 복원
                _pendingTasks.Clear();
                if (saveData.PendingTasks != null)
                {
                    foreach (var task in saveData.PendingTasks)
                    {
                        _pendingTasks.Enqueue(task);
                    }
                }

                // 활성 작업 복원
                _activeTasks.Clear();
                if (saveData.ActiveTasks != null)
                {
                    foreach (var task in saveData.ActiveTasks)
                    {
                        _activeTasks[task.Id] = task;
                    }
                }

                _monitor.Log($"작업 데이터 로드 완료", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"작업 데이터 로드 중 오류 발생: 	{ex.Message}	", LogLevel.Error);
            }
        }
    }

    /// <summary>
    /// 작업 저장 데이터 구조
    /// </summary>
    public class TaskSaveData
    {
        public List<WorkTask> PendingTasks { get; set; } = new List<WorkTask>();
        public List<WorkTask> ActiveTasks { get; set; } = new List<WorkTask>();
    }
}
```

#### 2.2.4. Core/UIManager.cs

`UIManager`는 플레이어의 입력을 처리하고 게임 내 HUD 메시지를 통해 피드백을 제공합니다. 프로토타입에서는 F9 키로 NPC 생성, F10 키로 농사 작업 할당, F11 키로 NPC 정보 표시, F12 키로 모든 NPC 제거 기능을 구현했습니다. 이는 모드의 기본적인 상호작용 방식을 보여줍니다.

```csharp
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
                _monitor.Log($"입력 처리 중 오류 발생: 	{ex.Message}	", LogLevel.Error);
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
                    Game1.addHUDMessage(new HUDMessage($"NPC 	'{npcName}'	 생성됨!", HUDMessage.newQuest_type));
                    _monitor.Log($"테스트 NPC 	'{npcName}'	 생성 완료", LogLevel.Info);
                }
                else
                {
                    Game1.addHUDMessage(new HUDMessage("NPC 생성 실패!", HUDMessage.error_type));
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"테스트 NPC 생성 중 오류 발생: 	{ex.Message}	", LogLevel.Error);
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
                    Game1.addHUDMessage(new HUDMessage($"'	{targetNPC.Name}'	에게 농사 작업 할당됨!", HUDMessage.newQuest_type));
                    _monitor.Log($"농사 작업이 	'{targetNPC.Name}'	에게 할당됨", LogLevel.Info);
                }
                else
                {
                    Game1.addHUDMessage(new HUDMessage("작업 할당 실패!", HUDMessage.error_type));
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"농사 작업 할당 중 오류 발생: 	{ex.Message}	", LogLevel.Error);
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

                var infoMessage = $"활성 NPC: 	{npcs.Count}	개";
                foreach (var npc in npcs)
                {
                    var position = new Vector2(npc.getTileX(), npc.getTileY());
                    var status = npc.IsBusy() ? "작업 중" : "대기 중";
                    infoMessage += $"\n- 	{npc.Name}	: 	{status}	 (	{position.X}	, 	{position.Y}	)";
                }

                Game1.addHUDMessage(new HUDMessage(infoMessage, HUDMessage.newQuest_type));
                _monitor.Log($"NPC 정보 표시: 	{npcs.Count}	개의 NPC", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"NPC 정보 표시 중 오류 발생: 	{ex.Message}	", LogLevel.Error);
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

                Game1.addHUDMessage(new HUDMessage($"	{count}	개의 NPC가 제거됨!", HUDMessage.newQuest_type));
                _monitor.Log($"	{count}	개의 NPC 제거 완료", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"NPC 제거 중 오류 발생: 	{ex.Message}	", LogLevel.Error);
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
```

#### 2.3.1. Models/CustomNPC.cs

`CustomNPC`는 `StardewValley.NPC`를 상속받아 게임 내에서 실제 NPC로 동작합니다. `AIController`와 `WorkExecutor`를 내부에 포함하여 AI 행동과 작업 실행을 위임합니다. `update` 메서드에서 `AIController`를 통해 이동 및 상태를 관리하고, `WorkExecutor`를 통해 할당된 작업을 수행합니다. `AssignTask` 메서드는 NPC에게 새로운 작업을 할당하고, `IsBusy`는 현재 작업 중인지 여부를 반환합니다.

```csharp
using System;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
using StardewModdingAPI;
using AutomatedNPCMod.Core;

namespace AutomatedNPCMod.Models
{
    /// <summary>
    /// 플레이어가 컨트롤할 수 있는 자동화된 NPC를 나타내는 클래스.
    /// </summary>
    public class CustomNPC : NPC
    {
        private AIController _aiController;
        private WorkExecutor _workExecutor;
        private WorkTask _currentTask;
        private NPCStats _stats;
        private string _uniqueId;
        private IMonitor _monitor;

        /// <summary>
        /// 새로운 CustomNPC 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="name">NPC 이름</param>
        /// <param name="position">초기 위치</param>
        /// <param name="spriteSheet">스프라이트 시트 경로</param>
        public CustomNPC(string name, Vector2 position, string spriteSheet) 
            : base(new AnimatedSprite(spriteSheet, 0, 16, 32), position * 64f, 2, name)
        {
            _uniqueId = Guid.NewGuid().ToString();
            _stats = new NPCStats();
            _aiController = new AIController(this);
            _workExecutor = new WorkExecutor(this);
            _monitor = ModEntry.Instance.Monitor; // ModEntry 인스턴스에서 Monitor 가져오기

            // 기본값 설정
            this.displayName = name;
            this.currentLocation = Game1.currentLocation ?? Game1.getFarm();
            this.speed = 2; // 기본 이동 속도
            this.collidesWith = StardewValley.Farmer.initialPassable; // 플레이어와 충돌하지 않도록 설정
            this.drawOffset.Y = -16; // 스프라이트 오프셋 조정
        }

        /// <summary>
        /// 매 게임 틱마다 호출되어 NPC의 AI와 애니메이션을 업데이트합니다.
        /// </summary>
        /// <param name="time">게임 시간</param>
        /// <param name="location">현재 위치</param>
        public override void update(GameTime time, GameLocation location)
        {
            base.update(time, location);

            // 현재 위치 업데이트 (base.update에서 처리되지만 명시적으로)
            this.currentLocation = location;

            // AI 업데이트
            _aiController.Update(time);

            // 현재 작업 실행
            if (_currentTask != null && !_currentTask.IsCompleted)
            {
                _workExecutor.ExecuteTask(_currentTask, time);
            }
            else if (_currentTask != null && _currentTask.IsCompleted)
            {
                // 작업 완료 후 처리 (예: TaskManager에 알림)
                _monitor.Log($"NPC 	{this.Name}	 작업 	{_currentTask.Id}	 완료.", LogLevel.Debug);
                ModEntry.Instance.TaskManager.CompleteTask(_currentTask.Id, _currentTask.Result);
                _currentTask = null; // 작업 초기화
            }
        }

        /// <summary>
        /// NPC에게 작업을 할당합니다.
        /// </summary>
        /// <param name="task">할당할 작업</param>
        /// <returns>작업 할당 성공 여부</returns>
        public bool AssignTask(WorkTask task)
        {
            if (_currentTask != null && !_currentTask.IsCompleted)
            {
                _monitor.Log($"NPC 	{this.Name}	 이미 작업 중입니다: 	{_currentTask.Type}	", LogLevel.Warn);
                return false;
            }

            _currentTask = task;
            _monitor.Log($"NPC 	{this.Name}	에게 	{task.Type}	 작업 할당됨.", LogLevel.Info);
            _aiController.SetDestination(task.TargetLocation); // 작업 위치로 이동 시작
            return true;
        }

        /// <summary>
        /// 현재 작업을 취소합니다.
        /// </summary>
        public void CancelCurrentTask()
        {
            if (_currentTask != null)
            {
                _monitor.Log($"NPC 	{this.Name}	의 현재 작업 	{_currentTask.Type}	을 취소합니다.", LogLevel.Info);
                _currentTask = null;
                _aiController.StopMoving();
            }
        }

        /// <summary>
        /// NPC가 현재 작업 중인지 여부를 반환합니다.
        /// </summary>
        public bool IsBusy()
        {
            return _currentTask != null && !_currentTask.IsCompleted;
        }

        /// <summary>
        /// NPC가 특정 유형의 작업을 수행할 수 있는지 여부를 확인합니다.
        /// </summary>
        /// <param name="taskType">확인할 작업 유형</param>
        /// <returns>수행 가능 여부</returns>
        public bool CanPerformTask(TaskType taskType)
        {
            // TODO: NPC의 능력치나 특성에 따라 작업 가능 여부 판단 로직 추가
            return _workExecutor.CanExecuteTask(taskType);
        }

        /// <summary>
        /// NPC의 현재 위치를 타일 좌표로 반환합니다.
        /// </summary>
        public Vector2 GetCurrentTilePosition()
        {
            return new Vector2(getTileX(), getTileY());
        }

        /// <summary>
        /// NPC의 현재 작업을 반환합니다.
        /// </summary>
        public WorkTask GetCurrentTask()
        {
            return _currentTask;
        }

        /// <summary>
        /// NPC의 통계 정보를 반환합니다.
        /// </summary>
        public NPCStats GetStats()
        {
            return _stats;
        }

        /// <summary>
        /// NPC의 AI 컨트롤러를 반환합니다.
        /// </summary>
        public AIController GetAIController()
        {
            return _aiController;
        }

        /// <summary>
        /// NPC의 작업 실행기를 반환합니다.
        /// </summary>
        public WorkExecutor GetWorkExecutor()
        {
            return _workExecutor;
        }
    }
}
```

#### 2.3.2. Models/WorkTask.cs, TaskResult.cs, NPCStats.cs

이 파일들은 모드의 핵심 데이터 모델을 정의합니다. `WorkTask`는 작업의 유형, 목표, 상태 등을 정의하며, `TaskResult`는 작업 완료 후의 결과(수익, 아이템 등)를 캡슐화합니다. `NPCStats`는 NPC의 능력치와 에너지 상태를 관리합니다.

```csharp
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;

namespace AutomatedNPCMod.Models
{
    /// <summary>
    /// NPC가 수행할 작업을 정의하는 클래스.
    /// </summary>
    public class WorkTask
    {
        public string Id { get; set; }
        public TaskType Type { get; set; }
        public Vector2 TargetLocation { get; set; }
        public string AssignedNPCId { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? CompletedTime { get; set; }
        public TaskPriority Priority { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public bool IsCompleted { get; set; }
        public TaskResult Result { get; set; }

        public WorkTask()
        {
            Parameters = new Dictionary<string, object>();
            IsCompleted = false;
        }
    }

    /// <summary>
    /// 작업 유형을 정의하는 열거형.
    /// </summary>
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

    /// <summary>
    /// 작업 우선순위를 정의하는 열거형.
    /// </summary>
    public enum TaskPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Critical = 4
    }

    /// <summary>
    /// 작업 완료 후의 결과를 나타내는 클래스.
    /// </summary>
    public class TaskResult
    {
        public bool Success { get; set; }
        public List<Item> ItemsObtained { get; set; }
        public int ExperienceGained { get; set; }
        public int GoldEarned { get; set; }
        public TimeSpan TimeTaken { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; }

        public TaskResult()
        {
            ItemsObtained = new List<Item>();
            AdditionalData = new Dictionary<string, object>();
            Success = false;
            ExperienceGained = 0;
            GoldEarned = 0;
        }
    }

    /// <summary>
    /// NPC의 능력치와 상태를 나타내는 클래스.
    /// </summary>
    public class NPCStats
    {
        public int FarmingLevel { get; set; }
        public int MiningLevel { get; set; }
        public int ForagingLevel { get; set; }
        public int FishingLevel { get; set; }
        public int CombatLevel { get; set; }
        public int Energy { get; set; }
        public int MaxEnergy { get; set; }
        public float MovementSpeed { get; set; }
        public float WorkEfficiency { get; set; }
        public Dictionary<string, int> ToolProficiency { get; set; }

        public NPCStats()
        {
            // 기본값 설정
            FarmingLevel = 1;
            MiningLevel = 1;
            ForagingLevel = 1;
            FishingLevel = 1;
            CombatLevel = 1;
            Energy = 100;
            MaxEnergy = 100;
            MovementSpeed = 2.0f;
            WorkEfficiency = 1.0f;
            ToolProficiency = new Dictionary<string, int>();
        }

        /// <summary>
        /// 특정 작업 유형에 대한 레벨을 반환합니다.
        /// </summary>
        /// <param name="taskType">작업 유형</param>
        /// <returns>해당 작업의 레벨</returns>
        public int GetLevelForTask(TaskType taskType)
        {
            return taskType switch
            {
                TaskType.Farming => FarmingLevel,
                TaskType.Mining => MiningLevel,
                TaskType.Foraging => ForagingLevel,
                TaskType.Fishing => FishingLevel,
                TaskType.Combat => CombatLevel,
                _ => 1
            };
        }

        /// <summary>
        /// 에너지를 소모합니다.
        /// </summary>
        /// <param name="amount">소모할 에너지 양</param>
        /// <returns>에너지 소모 성공 여부</returns>
        public bool ConsumeEnergy(int amount)
        {
            if (Energy >= amount)
            {
                Energy -= amount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 에너지를 회복합니다.
        /// </summary>
        /// <param name="amount">회복할 에너지 양</param>
        public void RestoreEnergy(int amount)
        {
            Energy = Math.Min(Energy + amount, MaxEnergy);
        }

        /// <summary>
        /// NPC가 작업을 수행할 수 있는 상태인지 확인합니다.
        /// </summary>
        /// <param name="requiredEnergy">필요한 에너지</param>
        /// <returns>작업 가능 여부</returns>
        public bool CanWork(int requiredEnergy = 10)
        {
            return Energy >= requiredEnergy;
        }
    }
}
```

#### 2.3.3. Models/AIController.cs

`AIController`는 `CustomNPC`의 AI 행동을 제어합니다. `Update` 메서드에서 NPC의 현재 상태(`AIState`)에 따라 이동(`Moving`) 또는 작업(`Working`) 로직을 실행합니다. `SetDestination`은 목표 위치를 설정하고 `Pathfinder`를 통해 경로를 계산합니다. 현재 `Pathfinder`는 간단한 직선 경로를 제공하며, 실제 게임 환경에서는 A* 알고리즘을 사용하여 복잡한 경로를 탐색해야 합니다.

```csharp
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace AutomatedNPCMod.Models
{
    /// <summary>
    /// NPC의 지능적인 행동을 담당하는 AI 제어기 클래스.
    /// </summary>
    public class AIController
    {
        private readonly CustomNPC _npc;
        private readonly Pathfinder _pathfinder;
        private Vector2? _destination;
        private Queue<Vector2> _currentPath;
        private AIState _currentState;
        private Vector2 _nextTile;
        private float _moveTimer;
        private const float MOVE_INTERVAL = 0.5f; // 타일 간 이동 시간 (초)

        public AIController(CustomNPC npc)
        {
            _npc = npc;
            _pathfinder = new Pathfinder();
            _currentState = AIState.Idle;
            _currentPath = new Queue<Vector2>();
            _moveTimer = 0f;
        }

        /// <summary>
        /// AI를 업데이트합니다.
        /// </summary>
        /// <param name="gameTime">게임 시간</param>
        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _moveTimer += deltaTime;

            switch (_currentState)
            {
                case AIState.Idle:
                    HandleIdleState();
                    break;
                case AIState.Moving:
                    HandleMovingState(deltaTime);
                    break;
                case AIState.Working:
                    HandleWorkingState();
                    break;
            }
        }

        /// <summary>
        /// 목표 지점을 설정하고 경로를 계산합니다.
        /// </summary>
        /// <param name="target">목표 위치</param>
        public void SetDestination(Vector2 target)
        {
            _destination = target;
            var currentPos = _npc.GetCurrentTilePosition();
            
            // 이미 목표 지점에 있는 경우
            if (Vector2.Distance(currentPos, target) < 1.0f)
            {
                _currentState = AIState.Working;
                return;
            }

            // 경로 계산
            _currentPath = _pathfinder.FindPath(currentPos, target, _npc.currentLocation);
            
            if (_currentPath.Count > 0)
            {
                _currentState = AIState.Moving;
                _nextTile = _currentPath.Dequeue();
                _moveTimer = 0f;
            }
            else
            {
                // 경로를 찾을 수 없는 경우
                _currentState = AIState.Idle;
            }
        }

        /// <summary>
        /// 이동을 중지합니다.
        /// </summary>
        public void StopMoving()
        {
            _destination = null;
            _currentPath.Clear();
            _currentState = AIState.Idle;
        }

        /// <summary>
        /// 대기 상태를 처리합니다.
        /// </summary>
        private void HandleIdleState()
        {
            // 대기 중일 때의 행동 (예: 주변 둘러보기, 애니메이션 등)
            // 현재는 단순히 대기
        }

        /// <summary>
        /// 이동 상태를 처리합니다.
        /// </summary>
        /// <param name="deltaTime">프레임 간 시간</param>
        private void HandleMovingState(float deltaTime)
        {
            if (_moveTimer >= MOVE_INTERVAL)
            {
                // 다음 타일로 이동
                var targetPixelPos = _nextTile * 64f; // 타일 좌표를 픽셀 좌표로 변환
                _npc.Position = targetPixelPos;
                _npc.setTileLocation(_nextTile);

                _moveTimer = 0f;

                // 다음 경로가 있는지 확인
                if (_currentPath.Count > 0)
                {
                    _nextTile = _currentPath.Dequeue();
                }
                else
                {
                    // 목표 지점에 도달
                    if (HasReachedDestination())
                    {
                        _currentState = AIState.Working;
                    }
                    else
                    {
                        _currentState = AIState.Idle;
                    }
                }
            }
        }

        /// <summary>
        /// 작업 상태를 처리합니다.
        /// </summary>
        private void HandleWorkingState()
        {
            // 작업 중일 때의 행동
            // 실제 작업은 WorkExecutor에서 처리되므로 여기서는 상태만 유지
        }

        /// <summary>
        /// 목표 지점에 도달했는지 확인합니다.
        /// </summary>
        /// <returns>도달 여부</returns>
        public bool HasReachedDestination()
        {
            if (!_destination.HasValue) return false;
            
            var currentPos = _npc.GetCurrentTilePosition();
            return Vector2.Distance(currentPos, _destination.Value) < 1.0f;
        }

        /// <summary>
        /// 현재 AI 상태를 반환합니다.
        /// </summary>
        public AIState GetCurrentState()
        {
            return _currentState;
        }

        /// <summary>
        /// AI 상태를 설정합니다.
        /// </summary>
        /// <param name="state">설정할 상태</param>
        public void SetState(AIState state)
        {
            _currentState = state;
        }
    }

    /// <summary>
    /// AI 상태를 정의하는 열거형.
    /// </summary>
    public enum AIState
    {
        Idle,       // 대기
        Moving,     // 이동 중
        Working,    // 작업 중
        Returning,  // 복귀 중
        Resting     // 휴식 중
    }

    /// <summary>
    /// 경로 탐색을 담당하는 클래스.
    /// </summary>
    public class Pathfinder
    {
        /// <summary>
        /// A* 알고리즘을 사용하여 경로를 찾습니다.
        /// </summary>
        /// <param name="start">시작 위치</param>
        /// <param name="end">목표 위치</param>
        /// <param name="location">게임 위치</param>
        /// <returns>경로 (타일 좌표 큐)</returns>
        public Queue<Vector2> FindPath(Vector2 start, Vector2 end, GameLocation location)
        {
            var path = new Queue<Vector2>();

            // 간단한 직선 경로 (실제 구현에서는 A* 알고리즘 사용)
            // 현재는 프로토타입이므로 단순한 경로 계산
            var current = start;
            var target = end;

            while (Vector2.Distance(current, target) > 1.0f)
            {
                var direction = target - current;
                direction.Normalize();

                // 가장 가까운 방향으로 한 타일씩 이동
                Vector2 nextTile;
                if (Math.Abs(direction.X) > Math.Abs(direction.Y))
                {
                    nextTile = new Vector2(current.X + Math.Sign(direction.X), current.Y);
                }
                else
                {
                    nextTile = new Vector2(current.X, current.Y + Math.Sign(direction.Y));
                }

                // 이동 가능한 타일인지 확인
                if (IsValidTile(nextTile, location))
                {
                    path.Enqueue(nextTile);
                    current = nextTile;
                }
                else
                {
                    // 장애물이 있는 경우 우회 경로 시도 (간단한 구현)
                    var alternativeTile = new Vector2(current.X + Math.Sign(direction.Y), current.Y + Math.Sign(direction.X));
                    if (IsValidTile(alternativeTile, location))
                    {
                        path.Enqueue(alternativeTile);
                        current = alternativeTile;
                    }
                    else
                    {
                        // 경로를 찾을 수 없는 경우 중단
                        break;
                    }
                }

                // 무한 루프 방지
                if (path.Count > 100)
                {
                    break;
                }
            }

            return path;
        }

        /// <summary>
        /// 지정된 타일이 이동 가능한지 확인합니다.
        /// </summary>
        /// <param name="tile">확인할 타일</param>
        /// <param name="location">게임 위치</param>
        /// <returns>이동 가능 여부</returns>
        private bool IsValidTile(Vector2 tile, GameLocation location)
        {
            if (location == null) return false;

            // 맵 경계 확인
            if (tile.X < 0 || tile.Y < 0 || tile.X >= location.Map.Layers[0].LayerWidth || tile.Y >= location.Map.Layers[0].LayerHeight)
            {
                return false;
            }

            // 충돌 확인 (간단한 구현)
            return location.isTilePassable(new xTile.Dimensions.Location((int)tile.X, (int)tile.Y), Game1.viewport);
        }
    }
}
```

#### 2.3.4. Models/WorkExecutor.cs 및 IWorkHandler.cs, FarmingHandler.cs, MiningHandler.cs, ForagingHandler.cs

`WorkExecutor`는 `IWorkHandler` 인터페이스를 통해 다양한 작업 유형을 처리합니다. `FarmingHandler`, `MiningHandler`, `ForagingHandler`는 각각 농사, 채굴, 채집 작업을 시뮬레이션합니다. 각 핸들러는 작업 완료까지 일정 시간을 소요하며, 완료 시 `TaskResult`를 생성하여 수익을 정산합니다. 현재는 간단한 수익 정산 로직이 포함되어 있습니다.

```csharp
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Objects;

namespace AutomatedNPCMod.Models
{
    /// <summary>
    /// 실제 게임 내 작업을 수행하는 로직을 담당하는 클래스.
    /// </summary>
    public class WorkExecutor
    {
        private readonly CustomNPC _npc;
        private readonly Dictionary<TaskType, IWorkHandler> _workHandlers;

        public WorkExecutor(CustomNPC npc)
        {
            _npc = npc;
            InitializeWorkHandlers();
        }

        /// <summary>
        /// 작업 처리기들을 초기화합니다.
        /// </summary>
        private void InitializeWorkHandlers()
        {
            _workHandlers = new Dictionary<TaskType, IWorkHandler>
            {
                { TaskType.Farming, new FarmingHandler() },
                { TaskType.Mining, new MiningHandler() },
                { TaskType.Foraging, new ForagingHandler() }
            };
        }

        /// <summary>
        /// 지정된 작업을 실행합니다.
        /// </summary>
        /// <param name="task">실행할 작업</param>
        /// <param name="gameTime">게임 시간</param>
        public void ExecuteTask(WorkTask task, GameTime gameTime)
        {
            if (_workHandlers.TryGetValue(task.Type, out IWorkHandler handler))
            {
                handler.Execute(_npc, task, gameTime);
            }
        }

        /// <summary>
        /// 지정된 작업 유형을 실행할 수 있는지 확인합니다.
        /// </summary>
        /// <param name="taskType">작업 유형</param>
        /// <returns>실행 가능 여부</returns>
        public bool CanExecuteTask(TaskType taskType)
        {
            return _workHandlers.ContainsKey(taskType);
        }
    }

    /// <summary>
    /// 작업 처리기 인터페이스.
    /// </summary>
    public interface IWorkHandler
    {
        void Execute(CustomNPC npc, WorkTask task, GameTime gameTime);
        bool CanHandle(WorkTask task);
        TaskResult GetResult();
        void Initialize(CustomNPC npc);
        void Cleanup();
    }

    /// <summary>
    /// 농사 작업을 처리하는 클래스.
    /// </summary>
    public class FarmingHandler : IWorkHandler
    {
        private TaskResult _result;
        private float _workTimer;
        private const float WORK_DURATION = 3.0f; // 작업 완료까지 3초

        public void Execute(CustomNPC npc, WorkTask task, GameTime gameTime)
        {
            if (task.IsCompleted) return;

            // 목표 위치에 도달했는지 확인
            if (!npc.GetAIController().HasReachedDestination())
            {
                return; // 아직 이동 중
            }

            // 작업 시작
            if (task.StartTime == null)
            {
                task.StartTime = DateTime.Now;
                _workTimer = 0f;
                _result = new TaskResult();
                npc.GetAIController().SetState(AIState.Working);
            }

            _workTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 작업 완료 확인
            if (_workTimer >= WORK_DURATION)
            {
                PerformFarmingAction(npc, task);
                task.IsCompleted = true;
                task.CompletedTime = DateTime.Now;
                task.Result = _result;
            }
        }

        /// <summary>
        /// 실제 농사 작업을 수행합니다.
        /// </summary>
        private void PerformFarmingAction(CustomNPC npc, WorkTask task)
        {
            var location = npc.currentLocation;
            var targetTile = task.TargetLocation;

            try
            {
                // 해당 위치에 작물이 있는지 확인
                if (location.terrainFeatures.TryGetValue(targetTile, out TerrainFeature feature))
                {
                    if (feature is HoeDirt dirt)
                    {
                        if (dirt.crop != null && dirt.crop.fullyGrown.Value)
                        {
                            // 수확
                            HarvestCrop(dirt, targetTile, location);
                        }
                        else if (dirt.crop == null)
                        {
                            // 씨앗 심기 (간단한 예시로 파스닙 씨앗)
                            PlantSeed(dirt, 24); // 파스닙 씨앗 ID
                        }
                        else
                        {
                            // 물주기
                            WaterCrop(dirt);
                        }
                    }
                }
                else
                {
                    // 땅 갈기
                    TillSoil(location, targetTile);
                }

                _result.Success = true;
                _result.GoldEarned = Game1.random.Next(10, 50); // 랜덤 수익
                _result.ExperienceGained = 5;
            }
            catch (Exception ex)
            {
                _result.Success = false;
                _result.ErrorMessage = $"농사 작업 중 오류: 	{ex.Message}	";
            }
        }

        private void HarvestCrop(HoeDirt dirt, Vector2 tile, GameLocation location)
        {
            if (dirt.crop != null)
            {
                var crop = dirt.crop;
                var harvestItem = new StardewValley.Object(crop.indexOfHarvest.Value, 1);
                _result.ItemsObtained.Add(harvestItem);
                _result.GoldEarned += harvestItem.Price;
                
                // 작물 제거
                dirt.crop = null;
            }
        }

        private void PlantSeed(HoeDirt dirt, int seedId)
        {
            dirt.plant(seedId, (int)Game1.player.getTileX(), (int)Game1.player.getTileY(), Game1.player, false, Game1.currentLocation);
            _result.GoldEarned += 5; // 심기 보상
        }

        private void WaterCrop(HoeDirt dirt)
        {
            dirt.state.Value = HoeDirt.watered;
            _result.GoldEarned += 2; // 물주기 보상
        }

        private void TillSoil(GameLocation location, Vector2 tile)
        {
            var dirt = new HoeDirt();
            location.terrainFeatures[tile] = dirt;
            _result.GoldEarned += 3; // 갈기 보상
        }

        public bool CanHandle(WorkTask task) => task.Type == TaskType.Farming;
        public TaskResult GetResult() => _result;
        public void Initialize(CustomNPC npc) { }
        public void Cleanup() { }
    }

    /// <summary>
    /// 채굴 작업을 처리하는 클래스.
    /// </summary>
    public class MiningHandler : IWorkHandler
    {
        private TaskResult _result;
        private float _workTimer;
        private const float WORK_DURATION = 4.0f; // 채굴은 4초

        public void Execute(CustomNPC npc, WorkTask task, GameTime gameTime)
        {
            if (task.IsCompleted) return;

            if (!npc.GetAIController().HasReachedDestination())
            {
                return;
            }

            if (task.StartTime == null)
            {
                task.StartTime = DateTime.Now;
                _workTimer = 0f;
                _result = new TaskResult();
                npc.GetAIController().SetState(AIState.Working);
            }

            _workTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_workTimer >= WORK_DURATION)
            {
                PerformMiningAction(npc, task);
                task.IsCompleted = true;
                task.CompletedTime = DateTime.Now;
                task.Result = _result;
            }
        }

        private void PerformMiningAction(CustomNPC npc, WorkTask task)
        {
            var location = npc.currentLocation;
            var targetTile = task.TargetLocation;

            try
            {
                // 해당 위치에 돌이나 광석이 있는지 확인
                if (location.objects.TryGetValue(targetTile, out StardewValley.Object obj))
                {
                    if (obj.Name.Contains("Stone") || obj.Name.Contains("Ore"))
                    {
                        // 광석/돌 채굴
                        var miningResult = GetMiningResult();
                        _result.ItemsObtained.Add(miningResult);
                        _result.GoldEarned = miningResult.Price;
                        
                        // 객체 제거
                        location.objects.Remove(targetTile);
                    }
                }
                else
                {
                    // 기본 돌 생성 및 채굴 (시뮬레이션)
                    var stone = new StardewValley.Object(390, 1); // 돌
                    _result.ItemsObtained.Add(stone);
                    _result.GoldEarned = stone.Price;
                }

                _result.Success = true;
                _result.ExperienceGained = 8;
            }
            catch (Exception ex)
            {
                _result.Success = false;
                _result.ErrorMessage = $"채굴 작업 중 오류: 	{ex.Message}	";
            }
        }

        private StardewValley.Object GetMiningResult()
        {
            // 랜덤하게 채굴 결과 결정
            var random = Game1.random.Next(1, 100);
            if (random <= 10) // 10% 확률로 구리 광석
                return new StardewValley.Object(378, 1);
            else if (random <= 20) // 10% 확률로 철 광석
                return new StardewValley.Object(380, 1);
            else if (random <= 25) // 5% 확률로 금 광석
                return new StardewValley.Object(384, 1);
            else // 나머지는 돌
                return new StardewValley.Object(390, 1);
        }

        public bool CanHandle(WorkTask task) => task.Type == TaskType.Mining;
        public TaskResult GetResult() => _result;
        public void Initialize(CustomNPC npc) { }
        public void Cleanup() { }
    }

    /// <summary>
    /// 채집 작업을 처리하는 클래스.
    /// </summary>
    public class ForagingHandler : IWorkHandler
    {
        private TaskResult _result;
        private float _workTimer;
        private const float WORK_DURATION = 2.0f; // 채집은 2초

        public void Execute(CustomNPC npc, WorkTask task, GameTime gameTime)
        {
            if (task.IsCompleted) return;

            if (!npc.GetAIController().HasReachedDestination())
            {
                return;
            }

            if (task.StartTime == null)
            {
                task.StartTime = DateTime.Now;
                _workTimer = 0f;
                _result = new TaskResult();
                npc.GetAIController().SetState(AIState.Working);
            }

            _workTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_workTimer >= WORK_DURATION)
            {
                PerformForagingAction(npc, task);
                task.IsCompleted = true;
                task.CompletedTime = DateTime.Now;
                task.Result = _result;
            }
        }

        private void PerformForagingAction(CustomNPC npc, WorkTask task)
        {
            var location = npc.currentLocation;
            var targetTile = task.TargetLocation;

            try
            {
                // 해당 위치에 채집 가능한 아이템이 있는지 확인
                if (location.objects.TryGetValue(targetTile, out StardewValley.Object obj))
                {
                    if (obj.isForage(location))
                    {
                        // 채집 아이템 수집
                        _result.ItemsObtained.Add(obj);
                        _result.GoldEarned = obj.Price;
                        
                        // 객체 제거
                        location.objects.Remove(targetTile);
                    }
                }
                else
                {
                    // 기본 채집 아이템 생성 (시뮬레이션)
                    var foragingResult = GetForagingResult();
                    _result.ItemsObtained.Add(foragingResult);
                    _result.GoldEarned = foragingResult.Price;
                }

                _result.Success = true;
                _result.ExperienceGained = 3;
            }
            catch (Exception ex)
            {
                _result.Success = false;
                _result.ErrorMessage = $"채집 작업 중 오류: 	{ex.Message}	";
            }
        }

        private StardewValley.Object GetForagingResult()
        {
            // 랜덤하게 채집 결과 결정
            var random = Game1.random.Next(1, 100);
            if (random <= 30) // 30% 확률로 야생 양파
                return new StardewValley.Object(399, 1);
            else if (random <= 50) // 20% 확률로 민들레
                return new StardewValley.Object(18, 1);
            else if (random <= 70) // 20% 확률로 리크
                return new StardewValley.Object(20, 1);
            else if (random <= 85) // 15% 확률로 야생 마늘
                return new StardewValley.Object(22, 1);
            else // 나머지는 나무 가지
                return new StardewValley.Object(388, 1);
        }

        public bool CanHandle(WorkTask task) => task.Type == TaskType.Foraging;
        public TaskResult GetResult() => _result;
        public void Initialize(CustomNPC npc) { }
        public void Cleanup() { }
    }
}
```

### 3. 프로토타입 빌드 및 실행 환경

현재 프로토타입은 샌드박스 환경에서 .NET SDK 6.0을 설치하여 빌드를 시도했습니다. 그러나 실제 스타듀밸리 게임이 설치되어 있지 않아 게임 어셈블리(`StardewValley.dll`, `StardewModdingAPI.dll`, `Microsoft.Xna.Framework.dll` 등)에 대한 참조가 없어 빌드 오류가 발생했습니다. 이는 샌드박스 환경의 제약으로 인한 것으로, 실제 개발 환경에서는 이러한 참조를 추가해야 합니다.

**빌드 오류 메시지 예시:**
```
/home/ubuntu/StardewValleyAutomatedNPCMod/AutomatedNPCMod/Models/AIController.cs(204,40): error CS0246: The type or namespace name 'Vector2' could not be found (are you missing a using directive or an assembly reference?)
```

이러한 오류는 `Microsoft.Xna.Framework.Vector2`와 같은 게임 관련 타입들을 찾을 수 없기 때문에 발생합니다. 실제 개발 환경에서는 `AutomatedNPCMod.csproj` 파일에 다음과 같은 참조를 추가해야 합니다:

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
  <!-- 기타 필요한 참조 -->
</ItemGroup>
```

또한, `Pathoschild.Stardew.ModBuildConfig` 패키지를 사용하여 빌드 시 게임 경로를 자동으로 찾아주도록 설정할 수 있습니다. 현재 프로토타입에서는 이 패키지를 제거하여 빌드 오류를 회피했지만, 실제 개발에서는 이 패키지를 활용하는 것이 편리합니다.

### 4. 최소 기능 프로토타입의 동작 시나리오

실제 게임 환경에서 프로토타입이 빌드 및 실행된다고 가정했을 때, 다음과 같은 최소 기능 동작 시나리오를 예상할 수 있습니다:

1. **모드 로드:** SMAPI를 통해 `AutomatedNPCMod`가 게임에 로드됩니다. `ModEntry`의 `Entry` 메서드가 실행되어 모든 관리자 클래스들이 초기화되고 이벤트가 구독됩니다.
2. **NPC 생성:** 플레이어가 게임 내에서 F9 키를 누르면, `UIManager`가 이를 감지하고 `NPCManager`에 새로운 `CustomNPC` 생성을 요청합니다. `NPCManager`는 플레이어 근처에 `Abigail` 스프라이트를 가진 새로운 NPC를 생성하고 게임 세계에 추가합니다. HUD 메시지를 통해 NPC 생성 성공 여부가 표시됩니다.
3. **NPC 정보 확인:** 플레이어가 F11 키를 누르면, `UIManager`가 `NPCManager`로부터 현재 활성 NPC 목록을 받아와 각 NPC의 이름, 위치, 현재 상태(작업 중/대기 중)를 HUD 메시지로 표시합니다.
4. **농사 작업 할당:** 플레이어가 F10 키를 누르면, `UIManager`가 `TaskManager`에 농사 작업 생성을 요청합니다. `TaskManager`는 생성된 작업을 현재 사용 가능한 NPC(예: 방금 생성된 NPC)에게 할당합니다. HUD 메시지를 통해 작업 할당 성공 여부가 표시됩니다.
5. **NPC 작업 수행:** 할당된 농사 작업을 받은 `CustomNPC`는 `AIController`를 통해 작업 목표 위치(플레이어 근처의 타일)로 이동하기 시작합니다. 이동이 완료되면 `WorkExecutor`의 `FarmingHandler`가 해당 타일에서 농사 작업을 시뮬레이션합니다. 이는 작물 수확, 씨앗 심기, 물주기, 또는 땅 갈기 중 하나가 될 수 있습니다. 작업이 완료되면 `TaskManager`에 결과가 보고되고, 플레이어에게 골드나 아이템이 정산됩니다.
6. **NPC 제거:** 플레이어가 F12 키를 누르면, `UIManager`가 `NPCManager`에 모든 활성 NPC 제거를 요청합니다. `NPCManager`는 게임에서 모든 `CustomNPC`를 제거하고 내부 목록에서도 삭제합니다. HUD 메시지를 통해 제거된 NPC 수가 표시됩니다.
7. **데이터 저장 및 로드:** 게임이 저장될 때, `NPCManager`와 `TaskManager`는 현재 상태의 NPC 및 작업 데이터를 게임 저장 파일에 기록합니다. 게임 로드 시, 이 데이터가 다시 로드되어 이전 세션의 NPC와 작업 상태가 복원됩니다.

이 시나리오는 프로토타입이 핵심 요구사항인 NPC 생성, 작업 할당, AI 동작, 그리고 수익 정산의 기본적인 흐름을 성공적으로 시뮬레이션할 수 있음을 보여줍니다. 비록 실제 게임과의 완벽한 통합은 아니지만, 모드의 핵심 로직과 아키텍처의 유효성을 검증하는 데 충분합니다.




## 향후 개발 계획 및 애로사항, 개선점

본 프로젝트는 스타듀밸리 자동화 NPC 모드 개발을 위한 초기 단계의 조사, 설계 및 최소 기능 프로토타입 구현을 완료했습니다. 성공적인 모드 개발을 위해서는 향후 다음과 같은 개발 계획을 수립하고, 예상되는 애로사항을 극복하며, 지속적인 개선을 이루어 나가야 합니다.

### 1. 향후 개발 계획

**1.1. 핵심 기능 고도화**

- **고급 AI 및 경로 탐색:** 현재의 간단한 `Pathfinder`를 A* 알고리즘 기반의 정교한 경로 탐색 시스템으로 대체하여, NPC가 복잡한 맵 구조와 장애물을 효율적으로 회피하고 최단 경로로 이동할 수 있도록 합니다. 게임 내 지형, 건물, 오브젝트 등을 고려한 충돌 감지 및 회피 로직을 강화합니다.
- **작업 유형 확장 및 세분화:** 농사, 채굴, 채집 외에 낚시, 전투, 제작, 건설, 유지보수 등 다양한 작업 유형을 추가하고, 각 작업에 대한 상세한 행동 로직(`IWorkHandler` 구현체)을 구현합니다. 예를 들어, 농사 작업은 씨앗 종류, 비료 사용, 작물 성장 단계 등을 고려하도록 세분화합니다.
- **NPC 능력치 및 성장 시스템:** `NPCStats`를 기반으로 NPC의 작업 숙련도, 에너지, 체력 등을 관리하고, 작업 수행을 통해 경험치를 얻어 레벨업하고 능력치가 향상되는 성장 시스템을 구현합니다. 이는 NPC의 효율성과 가치를 높여 플레이어에게 동기 부여를 제공합니다.
- **수익 정산 및 경제 시스템 연동:** 작업 완료 시 플레이어에게 골드, 아이템, 경험치 등을 정확하게 정산하고, 게임 내 경제 시스템(상점, 판매 가격 등)과 연동하여 밸런스를 유지합니다. NPC 유지 비용(식량, 급여 등)을 추가하여 전략적인 요소를 도입할 수 있습니다.

**1.2. 사용자 인터페이스 (UI) 강화**

- **커스텀 메뉴 시스템:** SMAPI의 `GenericModConfigMenu` 또는 자체 UI 프레임워크를 활용하여 NPC 관리(생성, 이름 변경, 능력치 확인), 작업 할당(작업 유형, 목표 위치 지정), 스케줄 설정, 모드 설정 변경 등을 위한 직관적인 인게임 UI를 개발합니다.
- **시각적 피드백:** NPC의 현재 상태(이동 중, 작업 중, 휴식 중), 작업 진행 상황, 수익 정산 내역 등을 게임 내에서 시각적으로 명확하게 표시하여 플레이어가 모드의 동작을 쉽게 이해할 수 있도록 합니다.
- **명령 UI:** 채팅 명령 또는 콘솔 명령을 통해 NPC에게 작업을 할당하거나 상태를 조회할 수 있는 기능을 제공하여, 고급 사용자나 디버깅 시 유용하게 활용할 수 있도록 합니다.

**1.3. 확장성 및 유지보수성 강화**

- **모듈화된 스프라이트 및 애니메이션 시스템:** NPC의 스프라이트와 애니메이션을 쉽게 추가하고 변경할 수 있는 시스템을 구축하여, 다양한 외형의 NPC를 지원하고 커스터마이징의 폭을 넓힙니다.
- **설정 파일 및 현지화:** 모드의 모든 설정(단축키, 작업 효율 등)을 외부 설정 파일로 분리하고, 다국어 지원을 위한 현지화(i18n) 시스템을 적용하여 전 세계 사용자들이 편리하게 이용할 수 있도록 합니다.
- **API 및 이벤트 노출:** 다른 모드 개발자들이 본 모드의 기능을 활용하거나 연동할 수 있도록 공개 API 및 이벤트를 제공하여 모드 생태계에 기여합니다.

**1.4. 성능 및 밸런스 최적화**

- **성능 프로파일링:** 실제 게임 환경에서 모드의 성능을 지속적으로 프로파일링하여 CPU, 메모리 사용량 등을 모니터링하고, 병목 현상이 발생하는 부분을 최적화합니다.
- **AI 스케줄링 및 자원 관리:** 여러 NPC가 동시에 작업을 수행할 때 발생할 수 있는 자원 경합(예: 동일한 작물 수확)을 방지하고, NPC의 작업 스케줄을 효율적으로 관리하여 게임 밸런스를 해치지 않도록 합니다.
- **게임 밸런스 조정:** NPC의 작업 효율성, 수익, 유지 비용 등을 지속적으로 조정하여 게임 플레이 경험을 풍부하게 하면서도, 너무 강력하거나 약하지 않도록 밸런스를 맞춥니다.

### 2. 예상되는 애로사항

- **게임 업데이트 호환성:** 스타듀밸리 게임 및 SMAPI의 업데이트는 모드의 동작에 영향을 미칠 수 있습니다. 지속적인 업데이트 모니터링과 코드 수정이 필요합니다.
- **복잡한 AI 구현:** 스타듀밸리의 복잡한 맵 구조와 다양한 오브젝트를 고려한 정교한 AI 경로 탐색 및 행동 로직 구현은 상당한 개발 노력과 테스트를 요구합니다.
- **성능 최적화:** 다수의 NPC가 동시에 활동하고 복잡한 작업을 수행할 경우, 게임 성능 저하가 발생할 수 있습니다. 효율적인 코드 작성과 최적화 기법 적용이 필수적입니다.
- **스프라이트 및 애니메이션 제작:** 커스텀 NPC의 자연스러운 동작을 위한 스프라이트 및 애니메이션 제작은 전문적인 디자인 역량을 요구하며, 시간과 비용이 많이 소요될 수 있습니다.
- **멀티플레이어 호환성:** 멀티플레이어 환경에서 모드가 안정적으로 동작하도록 구현하는 것은 동기화 문제 등으로 인해 매우 복잡하며, 추가적인 설계와 개발이 필요합니다.
- **버그 및 디버깅:** 게임 내에서 발생하는 다양한 상황과 상호작용으로 인해 예상치 못한 버그가 발생할 수 있으며, 이를 찾아내고 수정하는 과정이 어려울 수 있습니다.

### 3. 개선점

- **테스트 자동화:** 단위 테스트 및 통합 테스트를 자동화하여 코드 변경 시 발생할 수 있는 회귀 버그를 조기에 발견하고 개발 효율성을 높입니다.
- **커뮤니티 피드백 활용:** 모드 출시 후 사용자들의 피드백을 적극적으로 수렴하여 기능 개선 및 버그 수정을 진행하고, 커뮤니티와 소통하며 모드를 발전시킵니다.
- **문서화 강화:** 개발 과정에서 작성된 모든 설계 문서, 코드 주석, 사용자 가이드 등을 지속적으로 업데이트하고 상세화하여, 향후 유지보수 및 협업을 용이하게 합니다.
- **버전 관리 시스템 활용:** Git과 같은 버전 관리 시스템을 체계적으로 활용하여 코드 변경 이력을 관리하고, 여러 개발자가 협업할 수 있는 환경을 구축합니다.

이러한 계획과 고려사항들을 바탕으로, 스타듀밸리 자동화 NPC 모드는 플레이어에게 새로운 게임 경험을 제공하는 성공적인 프로젝트로 발전할 수 있을 것입니다.


