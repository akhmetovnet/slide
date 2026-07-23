using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TableEntryPool : MonoBehaviour
{
    private const int FakePlayersCount = 5000;
    private const int ScoreStep = 50;

    [SerializeField] private int poolCount = 20;
    [SerializeField] private bool autoExpand = true;
    [SerializeField] private PlayerData cellPrefab;
    [SerializeField] private ScrollRect _scrollRect;

    private readonly string[] _enemyNames =
    {
        "Север", "Искра", "Вектор", "Тайга", "Орбита",
        "Рубин", "Пульсар", "Гранит", "Ладога", "Спутник",
        "Байкал", "Волна", "Кедр", "Буран", "Зенит",
        "Сокол", "Нева", "Маяк", "Кварц", "Ангара"
    };

    private PoolMono<PlayerData> _pool;

    public int _gainedPoints;
    public List<PlayerData> entryList = new List<PlayerData>();
    public PlayerData playerCell;

    private void Awake()
    {
        _pool = new PoolMono<PlayerData>(cellPrefab, poolCount, transform);
        _pool.autoExpand = autoExpand;
        entryList.Clear();

        for (var i = 0; i < poolCount; i++)
            entryList.Add(_pool.GetFreeElement());
    }

    public void CreateRandomTable(int currentScore)
    {
        _gainedPoints = PlayerPrefs.GetInt("GainedPoints", 0) + Mathf.Max(0, currentScore);
        PlayerPrefs.SetInt("GainedPoints", _gainedPoints);

        var playerRank = CalculatePlayerRank(_gainedPoints);
        PlayerPrefs.SetInt("PlayerRank", playerRank);

        var totalRows = FakePlayersCount + 1;
        var firstRank = Mathf.Clamp(playerRank - poolCount / 2, 1, Mathf.Max(1, totalRows - poolCount + 1));

        playerCell = null;
        for (var i = 0; i < entryList.Count; i++)
        {
            var visibleRank = firstRank + i;
            var cell = entryList[i];
            var isPlayer = visibleRank == playerRank;

            if (isPlayer)
            {
                cell.playerName = "ВЫ";
                cell.score = _gainedPoints;
                playerCell = cell;
            }
            else
            {
                var fakeRank = visibleRank < playerRank ? visibleRank : visibleRank - 1;
                cell.playerName = FakeName(fakeRank);
                cell.score = FakeScore(fakeRank);
            }

            cell.rank = visibleRank;
            cell.ChangeColor(isPlayer ? new Color32(44, 190, 116, 255) : Color.white);
            cell.Render();
        }

        if (_scrollRect != null)
            _scrollRect.verticalNormalizedPosition = 0.5f;
    }

    private static int CalculatePlayerRank(int score)
    {
        var betterFakePlayers = Mathf.Clamp(FakePlayersCount - score / ScoreStep, 0, FakePlayersCount);
        return betterFakePlayers + 1;
    }

    private static int FakeScore(int fakeRank)
    {
        fakeRank = Mathf.Clamp(fakeRank, 1, FakePlayersCount);
        return (FakePlayersCount - fakeRank + 1) * ScoreStep;
    }

    private string FakeName(int fakeRank)
    {
        var index = Mathf.Abs(fakeRank * 37 + 11) % _enemyNames.Length;
        return $"{_enemyNames[index]}-{fakeRank:0000}";
    }
}
