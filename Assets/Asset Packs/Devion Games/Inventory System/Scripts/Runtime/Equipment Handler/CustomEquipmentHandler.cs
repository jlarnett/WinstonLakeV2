using DevionGames.UIWidgets;
using UnityEngine;

namespace DevionGames.InventorySystem
{
    public class CustomEquipmentHandler : MonoBehaviour
    {
        [SerializeField]
        private string m_WindowName = "Equipment";
        private ItemContainer m_EquipmentContainer;

        private void Start()
        {
            this.m_EquipmentContainer = WidgetUtility.Find<ItemContainer>(this.m_WindowName);
            if (this.m_EquipmentContainer != null)
            {
                this.m_EquipmentContainer.OnAddItem += OnAddItem;
                this.m_EquipmentContainer.OnRemoveItem += OnRemoveItem;
                UpdateEquipment();
            }
        }

        private void OnRemoveItem(Item item, int amount, Slot slot)
        {
            UnEquipItem(item);
        }

        private void OnAddItem(Item item, Slot slot)
        {
            EquipItem(item);
        }

        private void EquipItem(Item item)
        {
            foreach (ObjectProperty property in item.GetProperties())
            {
                if (property.SerializedType == typeof(int) || property.SerializedType == typeof(float))
                {
                    float value = System.Convert.ToSingle(property.GetValue());
                    SendMessage("AddModifier", new object[] { property.Name, value, (value <= 1f && value >= -1f) ? 1 : 0, item }, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        private void UnEquipItem(Item item)
        {
            foreach (ObjectProperty property in item.GetProperties())
            {
                if (property.SerializedType == typeof(int) || property.SerializedType == typeof(float))
                {
                    SendMessage("RemoveModifiersFromSource", new object[] { property.Name, item }, SendMessageOptions.DontRequireReceiver);
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
    }
}