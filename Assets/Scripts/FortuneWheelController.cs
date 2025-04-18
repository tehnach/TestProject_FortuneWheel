using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FortuneWheelController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private FortuneWheel fortuneWheel;
    [SerializeField] private Button buttonRotate;
    [SerializeField] private GameObject textOnButton;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text rewardCounterText;
    [SerializeField] private GameObject rewardIcons;
    [SerializeField] private RewardObj rewardObjPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Parameters")]
    [SerializeField] private float spawnRadiusMin = 1f;
    [SerializeField] private float spawnRadiusMax = 2f;


    private int _rewardStep = 5;
    private int _minReward = 5;
    private int _maxReward = 100;
    private int _sectorsCount = 12;
    private int _maxSpawnRewardObj = 20;

    private List<int> _allRewardTypes = new List<int>() { 0, 1, 2 };
    private List<int> _allSectorRewads = new List<int>();

    private int _lastRewardType = -1;
    private List<int> sectorRewards = new List<int>();

    private float timerInterval = 10f;
    private float timerStep = 1f;

    private int winSectorIndex;
    private int winSectorValue;

    private int rewardCounterValue;

    private void Start()
    {
        rewardCounterText.text = rewardCounterValue.ToString();
        FillAllSectorRewardsList();
        SetState(FortuneWheelState.Cooldown);
    }

    private void GenerateRewards()
    {
        _lastRewardType = GetRewardType();
        GetSectorRewards();
    }

    private int GetRewardType()
    {
        List<int> rewardTypes = new List<int>(_allRewardTypes);

        if (_lastRewardType >= 0 && _lastRewardType < rewardTypes.Count)
        {
            rewardTypes.Remove(_lastRewardType);
        }

        if (rewardTypes.Count > 0)
        {
            int _rndIndex = Random.Range(0, rewardTypes.Count);
            return rewardTypes[_rndIndex];
        }
        else
        {
            Debug.LogError("GetRewardType return -1");
        }

        return -1;
    }

    private void FillAllSectorRewardsList()
    {
        int steps = (_maxReward - _minReward) / _rewardStep;
        int _sectorReward = _minReward;
        _allSectorRewads.Add(_sectorReward);

        for (int i = 0; i < steps; i++)
        {
            _sectorReward += _rewardStep;
            _allSectorRewads.Add(_sectorReward);
        }
    }

    private void GetSectorRewards()
    {
        sectorRewards.Clear();

        List<int> tmpList = new List<int>(_allSectorRewads);

        for (int i = 0; i < _sectorsCount && tmpList.Count > 0; i++)
        {
            int _rndIndex = Random.Range(0, tmpList.Count);
            sectorRewards.Add(tmpList[_rndIndex]);
            tmpList.RemoveAt(_rndIndex);
        }
    }

    private IEnumerator Timer()
    {
        float interval = 0f;

        while (interval < timerInterval)
        {
            GenerateRewards();
            UpdateFortuneWheelReward();
            timerText.text = Mathf.CeilToInt(timerInterval - interval).ToString();

            yield return new WaitForSeconds(timerStep);
            interval += timerStep;
        }

        SetState(FortuneWheelState.Active);
    }

    private void UpdateFortuneWheelReward()
    {
        fortuneWheel.Init(_lastRewardType, sectorRewards);
    }

    public void ButtonRotate_Click()
    {
        SetState(FortuneWheelState.Rotate);
        GetWinSector();
        fortuneWheel.RotateWheel(winSectorIndex, () => StartCoroutine(ShowReward()));
    }

    private void GetWinSector()
    {
        winSectorIndex = Random.Range(0, _sectorsCount);

        if (winSectorIndex >= 0 && winSectorIndex < sectorRewards.Count)
        {
            winSectorValue = sectorRewards[winSectorIndex];
            Debug.Log("Result: " + winSectorValue.ToString());
        }
        else
        {
            winSectorValue = 0;
            Debug.LogError("Result: error");
        }
    }

    private IEnumerator ShowReward()
    {
        rewardIcons.SetActive(false);
        rewardCounterText.gameObject.SetActive(true);

        if (winSectorValue > 0)
        {
            List<int> rewardDistribution = CalculateRewardDistribution(winSectorValue);

            Sequence seqRewardTotal = DOTween.Sequence().SetLink(gameObject);

            foreach (int rValue in rewardDistribution)
            {
                RewardObj rewardObj = Instantiate(rewardObjPrefab);
                rewardObj.Init(_lastRewardType, rValue);

                // Random position in ring
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                float randomRadius = Random.Range(spawnRadiusMin, spawnRadiusMax);
                Vector2 spawnPos = (Vector2)spawnPoint.position + randomDir * randomRadius;


                rewardObj.transform.position = spawnPos;

                Sequence seqReward = DOTween.Sequence().SetLink(rewardObj.gameObject);
                seqReward.Append(rewardObj.transform.DOScale(0f, 0.3f).From().SetEase(Ease.OutSine))
                    .AppendInterval(Random.Range(1f, 2.5f))
                    .Append(rewardObj.transform.DOMove(rewardCounterText.transform.position, 0.3f))
                    .OnComplete(() =>
                    {
                        AddCounter(rewardObj.RewardAmount);
                        Destroy(rewardObj.gameObject);
                    });

                seqRewardTotal.Join(seqReward);
            }

            seqRewardTotal.AppendInterval(2f);
            seqRewardTotal.Play();

            yield return seqRewardTotal.WaitForCompletion();
            SetState(FortuneWheelState.Cooldown);
        }

    }

    private List<int> CalculateRewardDistribution(int total)
    {
        List<int> distribution = new List<int>();

        if (total <= _maxSpawnRewardObj)
        {
            for (int i = 0; i < total; i++)
            {
                distribution.Add(1);
            }
        }
        else
        {
            int maxObjects = Mathf.Min(_maxSpawnRewardObj, total);
            int baseValue = total / maxObjects;
            int remainder = total % maxObjects;

            for (int i = 0; i < maxObjects; i++)
            {
                distribution.Add(baseValue);
            }

            for (int i = 0; i < remainder; i++)
            {
                distribution[i] += 1;
            }
        }

        return distribution;
    }

    private void SetState(FortuneWheelState state)
    {
        switch (state)
        {
            case FortuneWheelState.Active:
                buttonRotate.interactable = true;
                textOnButton.SetActive(true);
                timerText.gameObject.SetActive(false);
                rewardIcons.SetActive(true);
                break;
            case FortuneWheelState.Rotate:
                buttonRotate.interactable = false;
                textOnButton.SetActive(true);
                timerText.gameObject.SetActive(false);
                break;
            case FortuneWheelState.Cooldown:
                buttonRotate.interactable = false;
                textOnButton.SetActive(false);
                timerText.gameObject.SetActive(true);
                rewardIcons.SetActive(true);
                rewardCounterText.gameObject.SetActive(false);
                StartCoroutine(Timer());
                break;
        }
    }

    private void AddCounter(int amount)
    {
        rewardCounterValue += amount;
        rewardCounterText.text = rewardCounterValue.ToString();
    }
}

public enum FortuneWheelState
{
    Active,
    Rotate,
    Cooldown
}
