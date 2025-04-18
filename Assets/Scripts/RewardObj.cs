using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardObj : MonoBehaviour
{
    [SerializeField] private GameObject[] rewardImages;

    private int _rewardAmount;

    public int RewardAmount
    {
        get { return _rewardAmount; }
    }

    public void Init(int rewardType, int rewardAmount)
    {
        _rewardAmount = rewardAmount;
        SetImage(rewardType);
    }

    private void SetImage(int rewardType)
    {
        for (int i = 0; i < rewardImages.Length; i++)
        {
            rewardImages[i].SetActive(i == rewardType);
        }

        if (rewardType < 0 || (rewardType >=0 && rewardType >= rewardImages.Length))
        {
            Debug.LogError($"RewardObj SetImage error (rewardType={rewardType})");
        }
    }
}
