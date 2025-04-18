using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;

public class FortuneWheel : MonoBehaviour
{
    [SerializeField] private Transform wheel;
    [SerializeField] private GameObject[] rewards;
    [SerializeField] private TMP_Text[] sectorRewardTexts;

    private int _minRotationRounds = 2;
    private int _sectorsCount = 12;

    public void Init(int rewardType, List<int> sectorRewards)
    {
        UpdateRewardView(rewardType);
        UpdateSectorRewards(sectorRewards);
    }

    public void RotateWheel(int winSectorIndex, System.Action onComplete)
    {
        float angle = winSectorIndex * 360f / _sectorsCount  + (_minRotationRounds * 360f);
        wheel.DORotate(new Vector3(0f, 0f, angle), 5f, RotateMode.LocalAxisAdd)
            .SetEase(Ease.OutCirc)
            .OnComplete(() => { onComplete?.Invoke(); })
            .Play();
    }

    private void UpdateRewardView(int rewardType)
    {
        for (int i = 0; i < rewards.Length; i++)
        {
            rewards[i].SetActive(i == rewardType);
        }
    }

    private void UpdateSectorRewards(List<int> sectorRewards)
    {
        for (int i = 0; i < sectorRewardTexts.Length; i++)
        {            
            if(i >= sectorRewards.Count)
            {
                sectorRewardTexts[i].text = "";
                Debug.LogError($"Sector {i} do not have reward value");
            }
            else
            {
                sectorRewardTexts[i].text = sectorRewards[i].ToString();
            }
        }
    }
}
