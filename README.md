\# \[ 주요 코드 구조 ]

\### ActionQueue

카드 게임 특성상 게임플레이 플로우가 고정되어 있지않기 때문에 모든 게임플레이를 IBattleAction으로 만들어 큐가 하나씩 await하여 순차 처리하도록 구현했습니다. Prepend(앞) / Append(뒤) 함수를 통해 액션을 추가하여 게임플레이 플로우를 명시적으로 제어할 수 있습니다.

예를 들어, 카드를 사용하여 적을 공격하면 턴 종료 액션이 큐에 Append되는데, 반격·죽음 액션은 턴 종료보다 선행되어야 하므로 큐에 Prepend됩니다.



\### Model / View

Board, BoardSide, CardInstance는 MonoBehaviour를 상속하지 않는 데이터(Model)로 구현해 렌더링을 담당하는 View와 분리했습니다. PlayerInputController처럼 렌더링과 직접 상호작용하는 클래스는 View를, View는 Model을 참조하도록 하여 의존성을 단방향으로 유지했습니다. 덕분에 표현을 바꿔도 데이터 클래스는 수정할 필요가 없고, 버그 발생 시 원인이 데이터에 있는지 렌더링에 있는지 쉽게 특정할 수 있습니다.



\### Data

카드, 덱은 인스펙터에서 설정이 가능하도록 ScriptableObject를 상속받아 구현했습니다.

특히 카드는 OnDeployed/OnTurnStart/OnTurnEnd/OnUsed 훅을 정의하고, 각 카드가 override하여 효과를 구현할 수 있도록 했습니다.

예를 들어, OnUsed를 override하여 공격 시 인접 카드 추가 타격, OnTurnStart를 override하여 턴 시작 시 아군 회복을 구현합니다.

