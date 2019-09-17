using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;
using Bebop.Protocol;
using Bebop.Model.EventParameters;

using UnityEngine.Networking;

using TMPro;

using Common.Scripts.Localization;

namespace Bebop.UI
{
    public class Records : MonoBehaviour
    {
        public enum E_Type
        {
            Playerlist,
            Leaderboard,
        }

        [System.Serializable]
        public struct RecordLayer
        {
            public E_Type type;
            public Toggle toggle;
            public TextMeshProUGUI toggleLabel;
            public IRecord records;
        }

        public List<RecordLayer> lstLayer = new List<RecordLayer>();

        public Button btnClose;

        [Header("[ Toggle Color Set ]")]
        public Color EnableToggleTextColor = Color.white;
        public Color DisableToggleTextColor = Color.white;

        public static Records Instance { get; private set; }

        private IRecordElement currentRecord;
        private Dictionary<E_Type, IRecordElement> dicRecords = new Dictionary<E_Type, IRecordElement>();

        private void Awake()
        {
            Instance = this;

            this.Hide();

            btnClose.onClick.AddListener(OnClose);

            transform.localPosition = Vector3.zero;

            foreach (var layer in lstLayer)
            {
                layer.toggleLabel.color = layer.toggle.isOn == true ? EnableToggleTextColor : DisableToggleTextColor;
                layer.toggle.onValueChanged.AddListener(isOn => OnClickToggle(isOn, layer.type));
                dicRecords.Add(layer.type, layer.records);
            }

            lstLayer.ForEach(d => d.records.gameObject.SetActive(false));

            //리더보드를 막는다.
            lstLayer.Find(d => d.type == E_Type.Leaderboard).toggle.enabled = false;
        }
        
        public void Open()
        {
            var findLayer = lstLayer.Find(d => d.type == E_Type.Playerlist);
            findLayer.toggle.isOn = true;
            currentRecord = findLayer.records;

            gameObject.SetActive(true);

            if (currentRecord != null)
                currentRecord.Show();
        }

        private void Hide()
        {
            if(currentRecord != null)
                currentRecord.Hide();

            gameObject.SetActive(false);

            lstLayer.ForEach(d => d.toggle.isOn = false );
        }
        
        private void OnClickToggle(bool isOn, E_Type type)
        {
            var findLayer = lstLayer.Find(d => d.type == type);
            var lable = findLayer.toggleLabel;

            var record = GetRecord(type);
            if (isOn == false)
            {
                lable.color = DisableToggleTextColor;
                record.Hide();
                return;
            }

            lable.color = EnableToggleTextColor;

            if (gameObject.activeSelf == false)
                return;

            if( currentRecord != record)
            {
                currentRecord = record;
                record.Show();
            }
        }

        private IRecordElement GetRecord(E_Type type)
        {
            IRecordElement element;
            if (false == dicRecords.TryGetValue(type, out element))
            {
                return null;
            }

            return element;
        }

        private T GetRecordScript<T>(E_Type type) where T : IRecord
        {
           IRecordElement element = GetRecord(type);
            if (element == null)
                return default(T);

            return element.GetRecord<T>();
        }

        private void OnClose()
        {
            this.Hide();
        }
    }
}