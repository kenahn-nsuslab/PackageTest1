using System.Collections;
using UnityEngine;
using DG.Tweening;
using Bebop.Model.EventParameters;
using TMPro;
using System;
using UnityEngine.UI;
using Bebop.Protocol;

namespace Bebop.UI
{
    public class StatisticsController : MonoBehaviour
    {

        private GameObject DataArea;

        //private GameObject LoadingImage;


        private StatisticsViewModel viewModel;

        private void Awake() {

            DataArea = transform.Find("MaskArea").gameObject;

            //LoadingImage = transform.Find("LoadingImage").gameObject;

            viewModel = GetComponent<StatisticsViewModel>();

            DataArea.SetActive(false);
            //LoadingImage.SetActive(true);
        }

        private void OnEnable() {

            //yield return new WaitWhile(()=> null == GameManager.Cl)
            GameManager.OnGameStatisticsResponse += OnReceiveData;
            
            DataArea.SetActive(false);
            //LoadingImage.SetActive(true);
            WaitIndicator.SetActive(true);

            GameManager.GetGameStatistics();

        }

        private void OnDisable() {

            GameManager.OnGameStatisticsResponse -= OnReceiveData;
            
            DataArea.SetActive(false);
            //LoadingImage.SetActive(true);
        }

        private void OnReceiveData(GameStatisticsResponse res)
        {
            viewModel.ApplyData(res.GameStatistics);

            if (DataArea.activeSelf ==false) DataArea.SetActive(true);

            WaitIndicator.SetActive(false);
            //LoadingImage.SetActive(false);
        }
    }
}