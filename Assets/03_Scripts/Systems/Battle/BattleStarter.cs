using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

/// <summary>
/// InGame 씬에서 전투를 시작하는 간단한 스크립트
/// 실제 전투는 InBattle 씬의 BattleManager가 처리합니다
/// </summary>
public class BattleStarter : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private AssetReference inBattleSceneReference;

    /// <summary>
    /// InBattle 씬 참조 프로퍼티 (런타임에 설정 가능)
    /// </summary>
    public AssetReference InBattleSceneReference
    {
        get => inBattleSceneReference;
        set => inBattleSceneReference = value;
    }

    /// <summary>
    /// 전투 시작 (Enemy에서 호출)
    /// </summary>
    public void StartBattle(Enemy enemy)
    {
        if (inBattleSceneReference == null)
        {
            Logger.LogError("InBattle 씬 참조가 설정되지 않았습니다.");
            return;
        }

        if (enemy == null)
        {
            Logger.LogError("Enemy가 null입니다. 전투를 시작할 수 없습니다.");
            return;
        }

        // Enemy와 Player 데이터 저장
        BattleDataTransfer.StoreEnemyData(enemy);

        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            BattleDataTransfer.StorePlayerData(player);
        }
        else
        {
            Logger.LogWarning("Player를 찾을 수 없습니다. 전투 데이터가 불완전할 수 있습니다.");
        }

        BattleDataTransfer.SetBattleActive(true);

        // InBattle 씬으로 전환
        inBattleSceneReference.LoadSceneAsync(LoadSceneMode.Single);
    }
}

