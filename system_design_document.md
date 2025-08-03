# 스타듀밸리 자동화 NPC 모드 시스템 설계 문서

## 1. 개요

본 문서는 스타듀밸리(Stardew Valley) 게임에서 플레이어가 직접 컨트롤할 수 있는 신규 NPC를 생성하고, 해당 NPC에게 농사, 채집, 광석 채굴 등의 다양한 노동 작업을 할당하여 자동으로 수행하게 하는 SMAPI 기반 C# 모드의 시스템 설계를 다룹니다. 이 모드는 컴패니언 동반자처럼 각각 따로 일을 할 수 있고 구경도 가능하게 하는 자동화 시스템을 목표로 합니다.

## 2. 시스템 요구사항

### 2.1. 기능적 요구사항

#### 2.1.1. NPC 생성 및 관리
시스템은 플레이어가 새로운 NPC를 생성할 수 있는 기능을 제공해야 합니다. 이는 NPC의 이름, 외형, 기본 속성 등을 설정할 수 있는 인터페이스를 포함합니다. 생성된 NPC는 게임 세계에 스폰되어 플레이어와 상호작용할 수 있어야 하며, 필요에 따라 제거하거나 수정할 수 있어야 합니다. NPC는 고유한 식별자를 가져야 하며, 여러 NPC가 동시에 존재할 수 있어야 합니다.

#### 2.1.2. 작업 할당 시스템
플레이어는 직관적인 UI 또는 명령 시스템을 통해 NPC에게 특정 작업을 할당할 수 있어야 합니다. 작업 유형은 농사(씨앗 심기, 물주기, 수확), 채집(야생 식물, 과일 등 수집), 광석 채굴(돌, 광석 채굴) 등을 포함합니다. 각 작업은 명확한 목표와 완료 조건을 가져야 하며, 플레이어는 작업의 우선순위를 설정할 수 있어야 합니다.

#### 2.1.3. 자동화된 작업 실행
할당된 작업을 NPC가 자동으로 수행해야 합니다. 이는 적절한 도구 사용, 목표 지점으로의 이동, 작업 수행 애니메이션, 그리고 결과물 처리를 포함합니다. NPC는 작업 중 발생할 수 있는 다양한 상황(장애물, 도구 부족, 에너지 부족 등)에 대응할 수 있어야 합니다.

#### 2.1.4. 수익 정산 시스템
NPC가 완료한 작업에서 발생하는 수익(아이템, 경험치, 골드 등)은 플레이어에게 정산되어야 합니다. 이는 실시간으로 이루어지거나 일정 주기로 정산될 수 있으며, 플레이어는 수익 내역을 확인할 수 있어야 합니다.

#### 2.1.5. AI 및 경로 탐색
NPC는 지능적으로 게임 세계를 탐색하고 이동할 수 있어야 합니다. 이는 효율적인 경로 계산, 장애물 회피, 그리고 동적인 환경 변화에 대한 적응을 포함합니다. NPC는 다른 NPC나 플레이어와 충돌하지 않고 이동해야 하며, 게임의 물리 법칙을 준수해야 합니다.

### 2.2. 비기능적 요구사항

#### 2.2.1. 성능
모드는 게임의 전반적인 성능에 부정적인 영향을 미치지 않아야 합니다. 여러 NPC가 동시에 작업을 수행하더라도 프레임 드롭이나 지연이 발생하지 않아야 하며, 메모리 사용량도 합리적인 수준을 유지해야 합니다. AI 연산과 경로 탐색은 효율적으로 최적화되어야 합니다.

#### 2.2.2. 확장성
시스템은 새로운 작업 유형이나 NPC 기능을 쉽게 추가할 수 있도록 설계되어야 합니다. 모듈화된 구조를 통해 개별 기능을 독립적으로 개발하고 테스트할 수 있어야 하며, 다른 모드와의 호환성도 고려해야 합니다.

#### 2.2.3. 안정성
모드는 게임 크래시나 데이터 손실을 일으키지 않아야 합니다. 예외 상황에 대한 적절한 처리와 복구 메커니즘을 제공해야 하며, SMAPI의 안전성 기능을 적극 활용해야 합니다.

#### 2.2.4. 사용성
플레이어가 모드의 기능을 쉽게 이해하고 사용할 수 있어야 합니다. 직관적인 UI, 명확한 피드백, 그리고 도움말 시스템을 제공해야 합니다.

## 3. 시스템 아키텍처

### 3.1. 전체 아키텍처 개요

자동화 NPC 모드의 시스템 아키텍처는 계층화된 구조로 설계됩니다. 최상위 계층은 SMAPI와의 인터페이스를 담당하는 ModEntry이며, 그 아래로 핵심 비즈니스 로직을 담당하는 관리자 클래스들, 그리고 실제 NPC 객체와 작업 실행 로직이 위치합니다. 이러한 구조는 관심사의 분리(Separation of Concerns) 원칙을 따르며, 각 계층이 명확한 책임을 가지도록 설계됩니다.

### 3.2. 주요 구성 요소

#### 3.2.1. ModEntry (진입점)
ModEntry는 SMAPI 모드의 표준 진입점으로, 모드의 생명주기를 관리합니다. 이 클래스는 모드 초기화, SMAPI 이벤트 구독, 그리고 다른 핵심 구성 요소들의 인스턴스화를 담당합니다. 또한 모드 종료 시 리소스 정리와 데이터 저장을 수행합니다.

#### 3.2.2. NPCManager (NPC 관리자)
NPCManager는 모든 커스텀 NPC의 생성, 삭제, 업데이트를 중앙에서 관리하는 핵심 구성 요소입니다. 이 클래스는 NPC의 생명주기를 관리하고, 게임 세계에서 NPC의 존재를 유지하며, 각 NPC의 상태를 추적합니다. 또한 NPC 간의 상호작용이나 충돌을 방지하는 역할도 수행합니다.

#### 3.2.3. TaskManager (작업 관리자)
TaskManager는 플레이어가 할당한 작업들을 관리하고, 이를 적절한 NPC에게 배분하는 역할을 담당합니다. 작업 큐 관리, 우선순위 처리, 그리고 작업 완료 후 후속 처리를 수행합니다. 이 클래스는 작업의 효율적인 스케줄링을 통해 전체 시스템의 성능을 최적화합니다.

#### 3.2.4. UIManager (사용자 인터페이스 관리자)
UIManager는 플레이어와 모드 간의 상호작용을 담당하는 사용자 인터페이스를 관리합니다. 이는 NPC 생성 UI, 작업 할당 UI, 수익 확인 UI 등을 포함하며, 게임의 기존 UI 스타일과 일관성을 유지하도록 설계됩니다.

#### 3.2.5. CustomNPC (커스텀 NPC 객체)
CustomNPC는 실제 NPC 개체를 나타내는 클래스로, 게임의 기본 NPC 클래스를 확장하여 자동화 기능을 추가합니다. 각 CustomNPC 인스턴스는 고유한 AI, 작업 실행 능력, 그리고 플레이어와의 상호작용 기능을 가집니다.

#### 3.2.6. AIController (AI 제어기)
AIController는 NPC의 지능적인 행동을 담당하는 구성 요소입니다. 경로 탐색, 의사결정, 그리고 환경에 대한 반응을 처리하며, 각 NPC가 할당된 작업을 효율적으로 수행할 수 있도록 지원합니다.

#### 3.2.7. WorkExecutor (작업 실행기)
WorkExecutor는 실제 게임 내 작업을 수행하는 로직을 담당합니다. 농사, 채집, 채굴 등 각 작업 유형에 대한 구체적인 실행 방법을 구현하며, 게임의 기존 시스템과 안전하게 상호작용합니다.

### 3.3. 데이터 흐름

시스템의 데이터 흐름은 다음과 같은 순서로 진행됩니다. 먼저 플레이어가 UIManager를 통해 NPC 생성이나 작업 할당 요청을 보냅니다. 이 요청은 해당하는 관리자(NPCManager 또는 TaskManager)로 전달되어 처리됩니다. NPCManager는 새로운 CustomNPC 인스턴스를 생성하고 게임 세계에 추가하며, TaskManager는 작업을 적절한 NPC에게 할당합니다.

할당된 작업은 CustomNPC의 AIController에 의해 분석되고, 실행 계획이 수립됩니다. AIController는 경로 탐색을 수행하고 NPC를 목표 지점으로 이동시킵니다. 목표 지점에 도달하면 WorkExecutor가 실제 작업을 수행하고, 결과를 TaskManager로 보고합니다. TaskManager는 작업 완료를 확인하고 수익을 계산하여 플레이어에게 정산합니다.

이러한 데이터 흐름은 이벤트 기반으로 설계되어, 각 구성 요소가 느슨하게 결합되도록 합니다. 이는 시스템의 유연성과 확장성을 높이며, 개별 구성 요소의 독립적인 개발과 테스트를 가능하게 합니다.

## 4. 클래스 설계

### 4.1. ModEntry 클래스

ModEntry 클래스는 SMAPI 모드의 표준 진입점으로, Mod 기본 클래스를 상속받습니다. 이 클래스의 주요 책임은 모드의 초기화와 종료, SMAPI 이벤트 구독, 그리고 핵심 구성 요소들의 생명주기 관리입니다.

```csharp
public class ModEntry : Mod
{
    private NPCManager npcManager;
    private TaskManager taskManager;
    private UIManager uiManager;
    private ConfigManager configManager;

    public override void Entry(IModHelper helper)
    {
        // 구성 요소 초기화
        // SMAPI 이벤트 구독
        // 설정 로드
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        // 게임 시작 시 초기화 작업
    }

    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        // 세이브 파일 로드 시 NPC 데이터 복원
    }

    private void OnSaving(object sender, SavingEventArgs e)
    {
        // 세이브 시 NPC 데이터 저장
    }

    private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
    {
        // 매 틱마다 NPC 업데이트
    }
}
```

Entry 메서드는 모드가 로드될 때 호출되며, 여기서 모든 핵심 구성 요소들을 초기화하고 필요한 SMAPI 이벤트를 구독합니다. OnGameLaunched 이벤트 핸들러는 게임이 완전히 로드된 후 추가적인 초기화 작업을 수행하며, OnSaveLoaded와 OnSaving 이벤트 핸들러는 NPC 데이터의 영속성을 보장합니다. OnUpdateTicked 이벤트 핸들러는 게임의 매 틱마다 호출되어 NPC들의 상태를 업데이트합니다.

### 4.2. NPCManager 클래스

NPCManager는 모든 커스텀 NPC의 중앙 관리를 담당하는 핵심 클래스입니다. 이 클래스는 NPC의 생성, 삭제, 업데이트, 그리고 게임 세계와의 통합을 관리합니다.

```csharp
public class NPCManager
{
    private Dictionary<string, CustomNPC> activeNPCs;
    private IModHelper helper;
    private IMonitor monitor;

    public NPCManager(IModHelper helper, IMonitor monitor)
    {
        this.helper = helper;
        this.monitor = monitor;
        this.activeNPCs = new Dictionary<string, CustomNPC>();
    }

    public bool CreateNPC(string name, Vector2 position, string spriteSheet)
    {
        // NPC 생성 로직
    }

    public bool RemoveNPC(string name)
    {
        // NPC 제거 로직
    }

    public CustomNPC GetNPC(string name)
    {
        // NPC 조회 로직
    }

    public void UpdateAllNPCs(GameTime gameTime)
    {
        // 모든 NPC 업데이트
    }

    public List<CustomNPC> GetNPCsInLocation(GameLocation location)
    {
        // 특정 위치의 NPC 목록 반환
    }

    public void SaveNPCData()
    {
        // NPC 데이터 저장
    }

    public void LoadNPCData()
    {
        // NPC 데이터 로드
    }
}
```

CreateNPC 메서드는 새로운 CustomNPC 인스턴스를 생성하고 게임 세계에 추가합니다. 이 과정에서 NPC의 고유성을 확인하고, 스프라이트 시트를 로드하며, 초기 위치를 설정합니다. RemoveNPC 메서드는 지정된 NPC를 게임에서 안전하게 제거하고 관련 리소스를 정리합니다. UpdateAllNPCs 메서드는 매 게임 틱마다 호출되어 모든 활성 NPC의 AI와 애니메이션을 업데이트합니다.

### 4.3. TaskManager 클래스

TaskManager는 작업의 생성, 할당, 실행, 완료를 관리하는 클래스입니다. 이 클래스는 작업 큐를 유지하고, 우선순위에 따라 작업을 스케줄링하며, 작업 완료 후 수익을 정산합니다.

```csharp
public class TaskManager
{
    private Queue<WorkTask> pendingTasks;
    private Dictionary<string, WorkTask> activeTasks;
    private NPCManager npcManager;
    private IModHelper helper;

    public TaskManager(NPCManager npcManager, IModHelper helper)
    {
        this.npcManager = npcManager;
        this.helper = helper;
        this.pendingTasks = new Queue<WorkTask>();
        this.activeTasks = new Dictionary<string, WorkTask>();
    }

    public bool AssignTask(string npcName, WorkTask task)
    {
        // NPC에게 작업 할당
    }

    public void ProcessTasks()
    {
        // 대기 중인 작업 처리
    }

    public void CompleteTask(string taskId, TaskResult result)
    {
        // 작업 완료 처리 및 수익 정산
    }

    public List<WorkTask> GetTasksForNPC(string npcName)
    {
        // 특정 NPC의 작업 목록 반환
    }

    public void CancelTask(string taskId)
    {
        // 작업 취소
    }

    private void DistributeProfits(TaskResult result)
    {
        // 수익 분배 로직
    }
}
```

AssignTask 메서드는 플레이어가 요청한 작업을 특정 NPC에게 할당합니다. 이 과정에서 NPC의 현재 상태와 능력을 확인하고, 작업의 실행 가능성을 검증합니다. ProcessTasks 메서드는 대기 중인 작업들을 검토하고, 사용 가능한 NPC에게 적절히 배분합니다. CompleteTask 메서드는 작업이 완료되었을 때 호출되어 결과를 처리하고 플레이어에게 수익을 정산합니다.

### 4.4. CustomNPC 클래스

CustomNPC 클래스는 게임의 기본 NPC 클래스를 확장하여 자동화 기능을 추가한 클래스입니다. 이 클래스는 AI 제어, 작업 실행, 그리고 플레이어와의 상호작용을 담당합니다.

```csharp
public class CustomNPC : NPC
{
    private AIController aiController;
    private WorkExecutor workExecutor;
    private WorkTask currentTask;
    private NPCStats stats;
    private string uniqueId;

    public CustomNPC(string name, Vector2 position, string spriteSheet) 
        : base(new AnimatedSprite(spriteSheet), position, 2, name)
    {
        this.uniqueId = Guid.NewGuid().ToString();
        this.aiController = new AIController(this);
        this.workExecutor = new WorkExecutor(this);
        this.stats = new NPCStats();
    }

    public override void update(GameTime time, GameLocation location)
    {
        base.update(time, location);
        
        // AI 업데이트
        aiController.Update(time);
        
        // 현재 작업 실행
        if (currentTask != null && !currentTask.IsCompleted)
        {
            workExecutor.ExecuteTask(currentTask, time);
        }
    }

    public bool AssignTask(WorkTask task)
    {
        // 작업 할당 로직
    }

    public void CompleteCurrentTask()
    {
        // 현재 작업 완료 처리
    }

    public bool CanPerformTask(TaskType taskType)
    {
        // 작업 수행 가능 여부 확인
    }

    public Vector2 GetCurrentPosition()
    {
        return new Vector2(getTileX(), getTileY());
    }

    public void MoveTo(Vector2 targetPosition)
    {
        aiController.SetDestination(targetPosition);
    }
}
```

CustomNPC 클래스는 게임의 표준 NPC 클래스를 상속받아 기본적인 NPC 기능을 유지하면서, 자동화에 필요한 추가 기능을 구현합니다. update 메서드는 매 게임 틱마다 호출되어 AI와 작업 실행을 업데이트합니다. AssignTask 메서드는 새로운 작업을 할당받을 때 호출되며, NPC의 현재 상태와 능력을 고려하여 작업 수락 여부를 결정합니다.

### 4.5. AIController 클래스

AIController는 NPC의 지능적인 행동을 담당하는 클래스입니다. 경로 탐색, 의사결정, 그리고 환경 인식 기능을 제공합니다.

```csharp
public class AIController
{
    private CustomNPC npc;
    private Pathfinder pathfinder;
    private Vector2? destination;
    private Queue<Vector2> currentPath;
    private AIState currentState;

    public AIController(CustomNPC npc)
    {
        this.npc = npc;
        this.pathfinder = new Pathfinder();
        this.currentState = AIState.Idle;
    }

    public void Update(GameTime gameTime)
    {
        switch (currentState)
        {
            case AIState.Idle:
                HandleIdleState();
                break;
            case AIState.Moving:
                HandleMovingState();
                break;
            case AIState.Working:
                HandleWorkingState();
                break;
        }
    }

    public void SetDestination(Vector2 target)
    {
        destination = target;
        currentPath = pathfinder.FindPath(npc.GetCurrentPosition(), target);
        currentState = AIState.Moving;
    }

    private void HandleMovingState()
    {
        // 이동 상태 처리 로직
    }

    private void HandleWorkingState()
    {
        // 작업 상태 처리 로직
    }

    private void HandleIdleState()
    {
        // 대기 상태 처리 로직
    }

    public bool HasReachedDestination()
    {
        return destination.HasValue && 
               Vector2.Distance(npc.GetCurrentPosition(), destination.Value) < 1.0f;
    }
}
```

AIController는 상태 기반 AI를 구현하여 NPC의 행동을 관리합니다. 각 상태(Idle, Moving, Working)에 따라 적절한 행동을 수행하며, 상태 간 전환을 통해 복잡한 행동 패턴을 구현할 수 있습니다. SetDestination 메서드는 NPC에게 새로운 목표 지점을 설정하고, 경로 탐색을 통해 최적의 이동 경로를 계산합니다.

### 4.6. WorkExecutor 클래스

WorkExecutor는 실제 게임 내 작업을 수행하는 로직을 담당하는 클래스입니다. 각 작업 유형에 대한 구체적인 실행 방법을 구현합니다.

```csharp
public class WorkExecutor
{
    private CustomNPC npc;
    private Dictionary<TaskType, IWorkHandler> workHandlers;

    public WorkExecutor(CustomNPC npc)
    {
        this.npc = npc;
        InitializeWorkHandlers();
    }

    private void InitializeWorkHandlers()
    {
        workHandlers = new Dictionary<TaskType, IWorkHandler>
        {
            { TaskType.Farming, new FarmingHandler() },
            { TaskType.Mining, new MiningHandler() },
            { TaskType.Foraging, new ForagingHandler() }
        };
    }

    public void ExecuteTask(WorkTask task, GameTime gameTime)
    {
        if (workHandlers.TryGetValue(task.Type, out IWorkHandler handler))
        {
            handler.Execute(npc, task, gameTime);
        }
    }

    public bool CanExecuteTask(TaskType taskType)
    {
        return workHandlers.ContainsKey(taskType);
    }
}
```

WorkExecutor는 전략 패턴(Strategy Pattern)을 사용하여 각 작업 유형에 대한 처리를 분리합니다. 이를 통해 새로운 작업 유형을 쉽게 추가할 수 있으며, 각 작업의 로직을 독립적으로 개발하고 테스트할 수 있습니다.

## 5. 인터페이스 설계

### 5.1. IWorkHandler 인터페이스

IWorkHandler 인터페이스는 모든 작업 처리기가 구현해야 하는 공통 인터페이스입니다. 이를 통해 다양한 작업 유형을 일관된 방식으로 처리할 수 있습니다.

```csharp
public interface IWorkHandler
{
    void Execute(CustomNPC npc, WorkTask task, GameTime gameTime);
    bool CanHandle(WorkTask task);
    TaskResult GetResult();
    void Initialize(CustomNPC npc);
    void Cleanup();
}
```

### 5.2. IPathfinder 인터페이스

IPathfinder 인터페이스는 경로 탐색 알고리즘을 추상화하여, 다양한 경로 탐색 방법을 구현할 수 있도록 합니다.

```csharp
public interface IPathfinder
{
    Queue<Vector2> FindPath(Vector2 start, Vector2 end, GameLocation location);
    bool IsValidPath(Queue<Vector2> path);
    float CalculatePathCost(Queue<Vector2> path);
}
```

### 5.3. INPCPersistence 인터페이스

INPCPersistence 인터페이스는 NPC 데이터의 저장과 로드를 담당하는 인터페이스입니다.

```csharp
public interface INPCPersistence
{
    void SaveNPC(CustomNPC npc);
    CustomNPC LoadNPC(string npcId);
    void DeleteNPC(string npcId);
    List<string> GetAllNPCIds();
}
```

## 6. 데이터 모델

### 6.1. WorkTask 클래스

WorkTask 클래스는 NPC가 수행할 작업을 정의하는 데이터 모델입니다.

```csharp
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
}
```

### 6.2. NPCStats 클래스

NPCStats 클래스는 NPC의 능력치와 상태를 나타내는 데이터 모델입니다.

```csharp
public class NPCStats
{
    public int FarmingLevel { get; set; }
    public int MiningLevel { get; set; }
    public int ForagingLevel { get; set; }
    public int Energy { get; set; }
    public int MaxEnergy { get; set; }
    public float MovementSpeed { get; set; }
    public float WorkEfficiency { get; set; }
    public Dictionary<string, int> ToolProficiency { get; set; }
}
```

### 6.3. TaskResult 클래스

TaskResult 클래스는 작업 완료 후의 결과를 나타내는 데이터 모델입니다.

```csharp
public class TaskResult
{
    public bool Success { get; set; }
    public List<Item> ItemsObtained { get; set; }
    public int ExperienceGained { get; set; }
    public int GoldEarned { get; set; }
    public TimeSpan TimeTaken { get; set; }
    public string ErrorMessage { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; }
}
```

이러한 시스템 설계는 확장성, 유지보수성, 그리고 성능을 고려하여 구성되었습니다. 각 구성 요소는 명확한 책임을 가지며, 인터페이스를 통한 추상화로 유연성을 확보했습니다. 또한 SMAPI의 모범 사례를 따라 안정성과 호환성을 보장하도록 설계되었습니다.

