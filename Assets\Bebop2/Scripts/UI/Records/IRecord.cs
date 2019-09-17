
using UnityEngine;

namespace Bebop.UI
{
    public interface IRecordElement
    {
        T GetRecord<T>() where T : MonoBehaviour;

        void Show();
        void Hide();

        void AddNetworkEvent();
        void RemoveNetworkEvent();
    }

    public abstract class IRecord : MonoBehaviour, IRecordElement
    {
        protected IRecord() { }

        public T GetRecord<T>() where T : MonoBehaviour
        {
            return GetComponent<T>();
        }

        public void Show()
        {
            //포커스가 되기 이전에 뭔가 처리 해야할일(인디케이터 등)
            this.AddNetworkEvent();
            WaitIndicator.SetActive(true);
            gameObject.SetActive(true);
            this.FocusIn();
        }

        public void Hide()
        {
            this.RemoveNetworkEvent();
            gameObject.SetActive(false);
            this.FocusOut();
        }

        public void AddNetworkEvent()
        {
            AddEvent();
        }
        public void RemoveNetworkEvent()
        {
            RemoveEvent();
        }

        protected abstract void FocusIn();
        protected abstract void FocusOut();

        protected abstract void AddEvent();
        protected abstract void RemoveEvent();
    }
}