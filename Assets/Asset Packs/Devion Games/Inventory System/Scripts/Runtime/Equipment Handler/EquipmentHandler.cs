﻿using System.Collections;
using System.Collections.Generic;
using DevionGames.UIWidgets;
using UnityEngine;

namespace DevionGames.InventorySystem
{
    public class EquipmentHandler : MonoBehaviour
    {
        [SerializeField]
        private string m_WindowName = "Equipment";

        [SerializeField]
        private List<EquipmentBone> m_Bones;
        public List<EquipmentBone> Bones
        {
            get { return this.m_Bones; }
            set { this.m_Bones = value; }
        }

        [SerializeField]
        private List<VisibleItem> m_VisibleItems= new List<VisibleItem>();
        public List<VisibleItem> VisibleItems {
            get { return this.m_VisibleItems; }
            set { this.m_VisibleItems = value; }
        }


        private ItemContainer m_EquipmentContainer;

        private void Start()
        {
            if (transform.root != transform)
            {
                EquipmentHandler handler = transform.root.gameObject.GetComponent<EquipmentHandler>();
                if (handler == null)
                {
                    handler = transform.root.gameObject.AddComponent<EquipmentHandler>();
                }
                handler.m_VisibleItems = m_VisibleItems;

                Destroy(this);
                return;
            }
           
            this.m_EquipmentContainer = WidgetUtility.Find<ItemContainer>(this.m_WindowName);
            if (this.m_EquipmentContainer != null)
            {
                for (int i = 0; i < this.m_VisibleItems.Count; i++)
                {
                    this.m_VisibleItems[i].enabled = false;
                }
                this.m_EquipmentContainer.OnAddItem += OnAddItem;
                this.m_EquipmentContainer.OnRemoveItem += OnRemoveItem;

                UpdateEquipment();
                if (InventoryManager.current != null) {
                    InventoryManager.current.onSceneLoaded.AddListener(UpdateEquipment);
                }
            }
        }

        private void OnAddItem(Item item, Slot slot)
        {
            if (item != null && item is EquipmentItem)
            {
                EquipItem(item as EquipmentItem);
            }
        }

        private void OnRemoveItem(Item item, int amount, Slot slot)
        {
            if (item != null && item is EquipmentItem)
            {
                UnEquipItem(item as EquipmentItem);
            }
        }

        public void EquipItem(EquipmentItem item)
        {
            foreach (ObjectProperty property in item.GetProperties())
            {
                if (property.SerializedType == typeof(int) || property.SerializedType == typeof(float))
                {
                    float value = System.Convert.ToSingle(property.GetValue());
                    SendMessage("AddModifier", new object[] { property.Name, value, (value <= 1f && value >= -1f) ? 1 : 0, item }, SendMessageOptions.DontRequireReceiver);
                }
            }
            for (int i = 0; i < this.m_VisibleItems.Count; i++) {
                VisibleItem visibleItem = this.m_VisibleItems[i];
                if (visibleItem.item.Id == item.Id) {
                    visibleItem.OnItemEquip(item);
                    break;
                }
            } 
        }

        public void UnEquipItem(EquipmentItem item)
        {
            foreach (ObjectProperty property in item.GetProperties())
            {
                if (property.SerializedType == typeof(int) || property.SerializedType == typeof(float))
                {
                    SendMessage("RemoveModifiersFromSource", new object[] { property.Name, item }, SendMessageOptions.DontRequireReceiver);
                }
            }
            for (int i = 0; i < this.m_VisibleItems.Count; i++)
            {
                VisibleItem visibleItem = this.m_VisibleItems[i];
                if (visibleItem.item.Id == item.Id)
                {
                    visibleItem.OnItemUnEquip(item);
                    break;
                }
            }
        }

        private void UpdateEquipment()
        {
            EquipmentItem[] containerItems = this.m_EquipmentContainer.GetItems<EquipmentItem>();
            foreach (EquipmentItem item in containerItems)
            {
                EquipItem(item);
            }

        }

        public Transform GetBone(EquipmentRegion region) {
            EquipmentBone bone = Bones.Find(x => x.region == region);
            if (bone == null || bone.bone == null) {
                Debug.LogWarning("Missing Bone Map configuration: "+gameObject.name);
                return null;
            }
            return bone.bone.transform;
        }

        [System.Serializable]
        public class EquipmentBone{
            [EquipmentPicker(true)]
            public EquipmentRegion region;
            public GameObject bone;
        }
      
    }
}