﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DevionGames.UIWidgets;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.ObjectModel;
using System;
using UnityEditor;
using DevionGames.StatSystem;

namespace DevionGames.InventorySystem
{
    /// <summary>
    /// Helper enum definition for multiple selection of PointerEventData.InputButton
    /// </summary>
    [System.Flags]
    public enum InputButton {
        Left = 1,
        Right = 2,
        Middle = 4
    }

    public class ItemContainer : UIWidget, IDropHandler
    {
        public override string[] Callbacks
        {
            get
            {
                List<string> callbacks = new List<string>(base.Callbacks);
                callbacks.Add("OnAddItem");
                callbacks.Add("OnFailedToAddItem");
                callbacks.Add("OnRemoveItem");
                callbacks.Add("OnRemoveItemReference");
                callbacks.Add("OnFailedToRemoveItem");
                callbacks.Add("OnUseItem");
                callbacks.Add("OnDropItem");
                return callbacks.ToArray();
            }
        }
        #region Delegates
        public delegate void AddItemDelegate(Item item, Slot slot);
        /// <summary>
        /// Called when an item is added to the container.
        /// </summary>
        public event AddItemDelegate OnAddItem;

        public delegate void FailedToAddItemDelegate(Item item);
        /// <summary>
        /// Called when an item could not be added to the container.
        /// </summary>
        public event FailedToAddItemDelegate OnFailedToAddItem;

        public delegate void RemoveItemDelegate(Item item, int amount, Slot slot);
        /// <summary>
        /// Called when an item was removed from the container
        /// </summary>
        public event RemoveItemDelegate OnRemoveItem;

        public delegate void FailedToRemoveItemDelegate(Item item, int amount);
        /// <summary>
        /// Called when an item could not be removed
        /// </summary>
        public event FailedToRemoveItemDelegate OnFailedToRemoveItem;

        public delegate void UseItemDelegate(Item item, Slot slot);
        /// <summary>
        /// Called when an item was used from this container.
        /// </summary>
        public event UseItemDelegate OnUseItem;

        public delegate void DropItemDelegate(Item item, GameObject droppedInstance);
        /// <summary>
        /// Called when an item was dropped from this container to world
        /// </summary>
        public event DropItemDelegate OnDropItem;

        #endregion

        /// <summary>
        /// The button to use item in slot
        /// </summary>
        [Header("Behaviour")]
        [EnumFlags]
        public InputButton useButton = InputButton.Right;
        /// <summary>
        /// Sets the container as dynamic. Slots are instantiated at runtime.
        /// </summary>
        [SerializeField]
        protected bool m_DynamicContainer = false;
        /// <summary>
        /// The parent transform of slots. 
        /// </summary>
        [SerializeField]
        protected Transform m_SlotParent;
        /// <summary>
        /// The slot prefab. This game object should contain the Slot component or a child class of Slot. 
        /// </summary>
        [SerializeField]
        protected GameObject m_SlotPrefab;

        [SerializeField]
        private bool m_UseReferences = false;
        /// <summary>
        /// If true this container will be used as reference.
        /// </summary>
        public bool UseReferences {
            get { return this.m_UseReferences; }
            protected set { this.m_UseReferences = value; }
        }

        [SerializeField]
        private bool m_CanDragIn = false;
        /// <summary>
        /// Can the items be dragged into this container
        /// </summary>
        public bool CanDragIn
        {
            get { return this.m_CanDragIn; }
            protected set { this.m_CanDragIn = value; }
        }

        [SerializeField]
        private bool m_CanDragOut = false;
        /// <summary>
        /// Can the items be dragged out from this container.
        /// </summary>
        public bool CanDragOut
        {
            get { return this.m_CanDragOut; }
            protected set { this.m_CanDragOut = value; }
        }

        [SerializeField]
        private bool m_CanDropItems = false;
        /// <summary>
        /// Can the items be dropped from this container to ground.
        /// </summary>
        public bool CanDropItems
        {
            get { return this.m_CanDropItems; }
            protected set { this.m_CanDropItems = value; }
        }

        [SerializeField]
        private bool m_CanReferenceItems = false;
        /// <summary>
        /// Can the items be referenced from this container
        /// </summary>
        public bool CanReferenceItems
        {
            get { return this.m_CanReferenceItems; }
            protected set { this.m_CanReferenceItems = value; }
        }

        [SerializeField]
        private bool m_CanSellItems = false;
        /// <summary>
        /// Can the items be sold from this container
        /// </summary>
        public bool CanSellItems
        {
            get { return this.m_CanSellItems; }
            protected set { this.m_CanSellItems = value; }
        }

        [SerializeField]
        private bool m_CanUseItems = false;
        /// <summary>
        /// Can items be used from this container
        /// </summary>
        public bool CanUseItems
        {
            get { return this.m_CanUseItems; }
            protected set { this.m_CanUseItems = value; }
        }

        [SerializeField]
        private bool m_UseContextMenu = false;
        /// <summary>
        /// Use context menu for item interaction
        /// </summary>
        public bool UseContextMenu
        {
            get { return this.m_UseContextMenu; }
            protected set { this.m_UseContextMenu = value; }
        }

        [SerializeField]
        private bool m_ShowTooltips = false;
        /// <summary>
        /// Show item tooltips?
        /// </summary>
        public bool ShowTooltips
        {
            get { return this.m_ShowTooltips; }
            protected set { this.m_ShowTooltips = value; }
        }


        [SerializeField]
        private bool m_MoveUsedItem = false;
        /// <summary>
        /// If true this container will be used as reference.
        /// </summary>
        public bool MoveUsedItem
        {
            get { return this.m_MoveUsedItem; }
            protected set { this.m_MoveUsedItem = value; }
        }

     

        /// <summary>
        /// Conditions for auto moving items when used
        /// </summary>
        public List<MoveItemCondition> moveItemConditions = new List<MoveItemCondition>();

        public List<Restriction> restrictions = new List<Restriction>();

        protected List<Slot> m_Slots = new List<Slot>();
        /// <summary>
        /// Collection of slots this container is holding
        /// </summary>
        public ReadOnlyCollection<Slot> Slots
        {
            get {
                return this.m_Slots.AsReadOnly();
            }
        }

        protected ItemCollection m_Collection;
        /// <summary>
        /// Set the collection for this container.
        /// </summary>
        public ItemCollection Collection {
            set {
                if (value == null) {
                    return;
                }
                RemoveItems(true);
                value.Initialize();
                this.m_Collection = value;
                CurrencySlot[] currencySlots = GetSlots<CurrencySlot>();
                for (int i = 0; i < currencySlots.Length; i++) {
                    Currency defaultCurrency = currencySlots[i].GetDefaultCurrency();
                    Currency currency = m_Collection.Where(x => typeof(Currency).IsAssignableFrom(x.GetType()) && x.Id == defaultCurrency.Id).FirstOrDefault() as Currency;
                    if (currency == null) {
                        ReplaceItem(currencySlots[i].Index, defaultCurrency);
                    } else {
                        currencySlots[i].ObservedItem = currency;
                    }
                }

                for(int i=0; i < this.m_Collection.Count; i++)
                {
                    Item item = this.m_Collection[i];
                    if (item.Slot != null) {
                        item.Slot.ObservedItem = item;
                        continue;
                    }

                    if (this.m_DynamicContainer) {
                        Slot slot = CreateSlot();
                        slot.ObservedItem = item;
                    } else {
                        Slot slot;
                        if (CanAddItem(item, out slot)) {
                            ReplaceItem(slot.Index, item);
                        }
                       
                    }

                }
            }
        }

        protected bool m_IsLocked = false;
        public bool IsLocked {
            get { return this.m_IsLocked; }
        }
        protected override void OnAwake()
        {
            if (this.m_SlotPrefab != null){
                this.m_SlotPrefab.SetActive(false);
            }
            RefreshSlots();
            RegisterCallbacks();
            this.Collection = GetComponent<ItemCollection>();
            
        }

        /// <summary>
        /// Stacks the item from s2 to s1. If stacking is not possible, swap the items.
        /// </summary>
        /// <param name="s1">Slot 1</param>
        /// <param name="s2">Slot 2</param>
        /// <returns>True if stacking or swaping possible.</returns>
        public bool StackOrSwap(Slot s1, Slot s2) {
            if (s1 == s2) {
                return false;
            }

            if (s1.Container.UseReferences && !s2.Container.CanReferenceItems) {
                return false;
            }

            if (StackOrAdd(s1, s2.ObservedItem)) {
                if (!s2.Container.UseReferences && !s1.Container.UseReferences) {
                    s2.Container.RemoveItem(s2.Index);
                }
                return true;
            }
            return SwapItems(s1, s2);

        }

        /// <summary>
        /// Swaps items in slots.
        /// </summary>
        /// <param name="s1">First slot</param>
        /// <param name="s2">Second slot</param>
        /// <returns>True if swapped.</returns>
        public bool SwapItems(Slot s1, Slot s2) {

            List<Slot> requiredSlotsObserved = s2.Container.GetRequiredSlots(s1.ObservedItem);
            if (requiredSlotsObserved.Count == 0)
            {
                requiredSlotsObserved.Add(s2);
            }
            Item[] itemsInRequiredSlotsObserved = requiredSlotsObserved.Where(x => x.ObservedItem != null).Select(y => y.ObservedItem).Distinct().ToArray();
            Slot[] willBeFreeSlotsObserved = s1.Container.GetSlots(s1.ObservedItem);
            Dictionary<Slot, Item> moveLocationsObserved = new Dictionary<Slot, Item>();

            List<Slot> requiredSlots = s1.Container.GetRequiredSlots(s2.ObservedItem);
            if (requiredSlots.Count == 0)
            {
                requiredSlots.Add(s1);
            }
            Item[] itemsInRequiredSlots = requiredSlots.Where(x => x.ObservedItem != null).Select(y => y.ObservedItem).Distinct().ToArray();
            Slot[] willBeFreeSlots = s2.Container.GetSlots(s2.ObservedItem);

            Dictionary<Slot, Item> moveLocations = new Dictionary<Slot, Item>();

            if (CanMoveItems(itemsInRequiredSlotsObserved, willBeFreeSlotsObserved, s1,s1.Container, ref moveLocationsObserved) && CanMoveItems(itemsInRequiredSlots, willBeFreeSlots,s2, s2.Container, ref moveLocations))
            {
                if (!s1.Container.UseReferences || !s2.Container.CanReferenceItems)
                {
                    for (int i = 0; i < itemsInRequiredSlots.Length; i++)
                    {
                        s1.Container.RemoveItem(itemsInRequiredSlots[i]);
                    }
                }
                if (!s1.Container.UseReferences){
                    s2.Container.RemoveItem(s2.Index);
                }

                if (!s1.Container.UseReferences || !s2.Container.CanReferenceItems)
                {
                    for (int i = 0; i < itemsInRequiredSlotsObserved.Length; i++)
                    {
                        s1.Container.RemoveItem(itemsInRequiredSlotsObserved[i]);
                    }
                }
                if (!s1.Container.UseReferences)
                {
                    foreach (KeyValuePair<Slot, Item> kvp in moveLocations)
                    {
                        s2.Container.ReplaceItem(kvp.Key.Index, kvp.Value);
                    }
                }
                if (!s2.Container.UseReferences)
                {
                    foreach (KeyValuePair<Slot, Item> kvp in moveLocationsObserved)
                    {
                        s1.Container.ReplaceItem(kvp.Key.Index, kvp.Value);
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the items in slots can be swapped.
        /// </summary>
        /// <param name="s1">Slot 1</param>
        /// <param name="s2">Slot 2</param>
        /// <returns>True if items can be swapped.</returns>
        public bool CanSwapItems(Slot s1, Slot s2) {
            List<Slot> requiredSlotsObserved = s2.Container.GetRequiredSlots(s1.ObservedItem);
            if (requiredSlotsObserved.Count == 0)
            {
                requiredSlotsObserved.Add(s2);
            }
            Item[] itemsInRequiredSlotsObserved = requiredSlotsObserved.Where(x => x.ObservedItem != null).Select(y => y.ObservedItem).Distinct().ToArray();
            Slot[] willBeFreeSlotsObserved = s1.Container.GetSlots(s1.ObservedItem);
            Dictionary<Slot, Item> moveLocationsObserved = new Dictionary<Slot, Item>();

            List<Slot> requiredSlots = s1.Container.GetRequiredSlots(s2.ObservedItem);
            if (requiredSlots.Count == 0)
            {
                requiredSlots.Add(s1);
            }
            Item[] itemsInRequiredSlots = requiredSlots.Where(x => x.ObservedItem != null).Select(y => y.ObservedItem).Distinct().ToArray();
            Slot[] willBeFreeSlots = s2.Container.GetSlots(s2.ObservedItem);

            Dictionary<Slot, Item> moveLocations = new Dictionary<Slot, Item>();

            return CanMoveItems(itemsInRequiredSlotsObserved, willBeFreeSlotsObserved, s1, s1.Container, ref moveLocationsObserved) && CanMoveItems(itemsInRequiredSlots, willBeFreeSlots, s2, s2.Container, ref moveLocations);
        }

        /// <summary>
        /// Try to stack the item to any item in container. If it fails add the item.
        /// </summary>
        /// <param name="item">Item to stack/add</param>
        /// <returns>True if item was stacked or added</returns>
        public virtual bool StackOrAdd(Item item)
        {
            if (!StackItem(item) && !AddItem(item)){
                return false;
            }
            return true;
        }
       
        /// <summary>
        /// Try to stack the item to the slots item. If Slot is empty add item to the slot.
        /// </summary>
        /// <param name="slot">Slot where to stack or add the item to.</param>
        /// <param name="item">Item to stack</param>
        /// <returns>Returns true if item was stacked or added to slot</returns>
        public virtual bool StackOrAdd(Slot slot, Item item)
        {
            //Check if the item can be stacked to slot;
            if (CanStack(slot, item) && !UseReferences){
                //Stack the item to slot and return true
                slot.ObservedItem.Stack += item.Stack;
                RemoveItemCompletely(item);
                OnAddItem(item, slot);
                return true;
            }
            //Slot is empty and the item can be added to slot.
            else if (CanAddItem(slot.Index,item)){
                if (!slot.Container.CanReferenceItems)
                {
                    RemoveItemReferences(item);
                }
                if (!slot.Container.UseReferences && item.Slot != null){
                    item.Container.RemoveItem(item.Slot.Index);
                }
               
                slot.Container.ReplaceItem(slot.Index, item);

                return true;
            }
            //Return false if the item can't be stacked to the slot
            return false;
        }

        /// <summary>
        /// Condition method if swapping is possible
        /// </summary>
        private bool CanMoveItems(Item[] items, Slot[] slotsWillBeFree, Slot preferredSlot, ItemContainer container, ref Dictionary<Slot, Item> moveLocations)
        {
            List<Slot> reservedSlots = new List<Slot>();
            List<Item> checkedItems = new List<Item>(items);

            for (int i = checkedItems.Count - 1; i >= 0; i--)
            {
                Item current = checkedItems[i];
                List<Slot> requiredSlots = container.GetRequiredSlots(current);
                for (int j = 0; j < requiredSlots.Count; j++)
                {
                    if ((requiredSlots[j].IsEmpty || slotsWillBeFree.Contains(requiredSlots[j])) && requiredSlots[j].CanAddItem(current) && !reservedSlots.Contains(requiredSlots[j]))
                    {
                        reservedSlots.Add(requiredSlots[j]);
                        checkedItems.RemoveAt(i);
                        moveLocations.Add(requiredSlots[j], current);
                        break;
                    }
                }
            }

            for (int i = checkedItems.Count - 1; i >= 0; i--)
            {
                Item current = checkedItems[i];
                if (preferredSlot.CanAddItem(current) && !reservedSlots.Contains(preferredSlot))
                {
                    reservedSlots.Add(preferredSlot);
                    checkedItems.RemoveAt(i);
                    moveLocations.Add(preferredSlot, current);
                    break;
                }
            }

            for (int i = checkedItems.Count - 1; i >= 0; i--)
            {
                Item current = checkedItems[i];
                for (int j = 0; j < container.Slots.Count; j++)
                {
                    if ((container.Slots[j].IsEmpty || slotsWillBeFree.Contains(container.Slots[j])) && container.Slots[j].CanAddItem(current) && !reservedSlots.Contains(container.Slots[j]))
                    {
                        reservedSlots.Add(container.Slots[j]);
                        checkedItems.RemoveAt(i);
                        moveLocations.Add(container.Slots[j], current);
                        break;
                    }
                }
            }
            return checkedItems.Count == 0;
        }

        /// <summary>
        /// Adds a new item to a free or dynamicly created slot in this container.
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <returns>True if item was added.</returns>
        public virtual bool AddItem(Item item)
        {
            Slot slot = null;
            if (CanAddItem(item, out slot, true))
            {
                ReplaceItem(slot.Index, item);
                return true;
            }
            OnFailedToAddItem(item);
            return false;
        }

        /// <summary>
        /// Checks if the item can be added at index.
        /// </summary>
        /// <param name="index">Slot index.</param>
        /// <param name="item">Item to add.</param>
        /// <returns></returns>
        public virtual bool CanAddItem(int index, Item item) {
            if (item == null) { return true; }
            List<Slot> requiredSlots = GetRequiredSlots(item);
            Slot slot = this.m_Slots[index];

            if (requiredSlots.Count > 0)
            {
                if (!requiredSlots.Contains(slot)) { return false; }

                for (int i = 0; i < requiredSlots.Count; i++)
                {
                    if (!(requiredSlots[i].IsEmpty && requiredSlots[i].CanAddItem(item)))
                    {
                        return false;
                    }
                }
                return true;
            }
            return slot.IsEmpty && slot.CanAddItem(item);
        }

        /// <summary>
        /// Checks if the item can be added to this container.
        /// </summary>
        /// <param name="item">Item to check.</param>
        /// <returns>Returns true if the item can be added.</returns>
        public virtual bool CanAddItem(Item item)
        {
            Slot slot = null;
            return CanAddItem(item, out slot);
        }

        /// <summary>
        /// Checks if the item can be added to this container.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <param name="slot">Required or next free slot</param>
        /// <param name="createSlot">Should a slot be created if the container is dynamic.</param>
        /// <returns>Returns true if the item can be added.</returns>
        public virtual bool CanAddItem(Item item, out Slot slot, bool createSlot = false)
        {
            slot = null;
            if (item == null) { return true; }
     
            List<Slot> requiredSlots = GetRequiredSlots(item);
            if (requiredSlots.Count > 0)
            {
                for (int i = 0; i < requiredSlots.Count; i++)
                {
                    if (!(requiredSlots[i].IsEmpty && requiredSlots[i].CanAddItem(item)))
                    {
                        return false;
                    }
                }
                slot = requiredSlots[0];
                return true;
            }
            else
            {
                for (int i = 0; i < this.m_Slots.Count; i++)
                {
                    if (this.m_Slots[i].IsEmpty && this.m_Slots[i].CanAddItem(item))
                    {
                        slot = this.m_Slots[i];
                        return true;
                    }
                }
            }

            if (this.m_DynamicContainer)
            {
                if (createSlot)
                {
                    slot = CreateSlot();
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Replaces the items at index and returns an array of items that were replaced.
        /// </summary>
        /// <param name="index">Index of slot to repalce.</param>
        /// <param name="item">Item to replace with.</param>
        /// <returns></returns>
        public virtual Item[] ReplaceItem(int index, Item item)
        {
            List<Item> list = new List<Item>();
            if (index < this.m_Slots.Count)
            {
                Slot slot = this.m_Slots[index];
                List<Slot> slotsForItem = GetRequiredSlots(item);

                if (!slotsForItem.Contains(slot) && slot.CanAddItem(item)) { slotsForItem.Add(slot); }
                for (int i = 0; i < slotsForItem.Count; i++)
                {
                    if (!slotsForItem[i].IsEmpty && !slotsForItem[i].CanAddItem(item))
                    {
                        OnFailedToAddItem(item);
                        return list.ToArray();
                    }
                }
                if (item != null)
                {
                    if (!UseReferences && !this.m_Collection.Contains(item))
                    {
                        this.m_Collection.Add(item);
                    }
                    for (int i = 0; i < slotsForItem.Count; i++)
                    {
                        Item current = slotsForItem[i].ObservedItem;
                        if (current != null && !list.Contains(current))
                        {
                            list.Add(current);
                            RemoveItem(slotsForItem[i].Index);
                        }
                        slotsForItem[i].ObservedItem = item;
                    }
                    OnAddItem(item, slot);
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// Removes the item at index. Sometimes an item requires more then one slot(two-handed weapon), this will remove the item with the extra slots.
        /// </summary>
        /// <param name="index">The slot index where to remove the item.</param>
        /// <returns>Returns true if the item was removed.</returns>
        public virtual bool RemoveItem(int index)
        {
            if (index < this.m_Slots.Count)
            {
                Slot slot = this.m_Slots[index];
                Item item = slot.ObservedItem;


                List<Slot> slotsForItem = GetRequiredSlots(item);
                if (!slotsForItem.Contains(slot)) { slotsForItem.Add(slot); }

                if (item != null)
                {
                    if (!UseReferences)
                    {
                        this.m_Collection.Remove(item);
                    }else {
                        item.ReferencedSlots.Remove(slot);
                    }

                    for (int i = 0; i < slotsForItem.Count; i++)
                    {
                        slotsForItem[i].ObservedItem = null;
                    }
                  
                    OnRemoveItem(item, item.Stack, slot);
                    if (this.m_DynamicContainer)
                    {
                        DestroyImmediate(this.m_Slots[index].gameObject);
                        RefreshSlots();
                    }

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes the amount/stack of items
        /// </summary>
        /// <param name="item">Item to remove</param>
        /// <param name="amount">Amount/Stack to remove</param>
        /// <returns>Returns true if the stack of items is removed.</returns>
        public virtual bool RemoveItem(Item item, int amount)
        {
            if (item == null) { return false; }
            if (typeof(Currency).IsAssignableFrom(item.GetType()))
            {
                Currency payCurrency = GetItems(item.Id).FirstOrDefault() as Currency;

                ConvertToSmallestCurrency();
                Currency current = null;
                Currency[] currencies = GetItems<Currency>();
                for (int i = 0; i < currencies.Length; i++)
                {
                    if (currencies[i].Stack > 0)
                    {
                        current = currencies[i];
                    }
                }

                bool result = TryRemove(current, payCurrency, amount);
                TryConvertCurrency(current);

                CurrencySlot slot = GetSlots<CurrencySlot>().Where(x => x.ObservedItem.Id == payCurrency.Id).FirstOrDefault();
                if (result)
                {
                    OnRemoveItem(payCurrency, amount, slot);
                }
                else
                {
                    OnFailedToRemoveItem(payCurrency, amount);
                }

                return result;
            }

            if (item.Stack == amount && RemoveItem(item))
            {
                return true;
            }

            if (!HasItem(item, amount))
            {
                OnFailedToRemoveItem(item, amount);
                return false;
            }

            Item[] checkedItems = GetItems(item.Id);
            int currentAmount = amount;
            for (int i = 0; i < checkedItems.Length; i++)
            {
                Item checkedItem = checkedItems[i];
                if (checkedItem != null)
                {
                    int mStack = checkedItem.Stack;

                    checkedItem.Stack -= currentAmount;
                    currentAmount -= mStack;
                    OnRemoveItem(checkedItem, mStack, checkedItem.Slot);
                    
                    if (checkedItem.Stack <= 0)
                    {
                        RemoveItemCompletely(checkedItem);
                    }
                    if (currentAmount <= 0)
                    {
                        break;
                    }
                }
            }

            return (currentAmount <= 0);
        }

        /// <summary>
        /// Removes the item from this container. If the container uses reference this will remove all references in this container.
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <returns>Returns true if item was removed</returns>
        public virtual bool RemoveItem(Item item)
        {
            if (item == null) { return false; }

            if (!UseReferences && this.m_Collection.Contains(item))
            {
                //Remove item from the collection
                this.m_Collection.Remove(item);

                //Remove item from all slots
                for (int i = 0; i < this.m_Slots.Count; i++)
                {
                    if (this.m_Slots[i].ObservedItem == item)
                    {
                        this.m_Slots[i].ObservedItem = null;
                        OnRemoveItem(item, item.Stack, this.m_Slots[i]);
                        if (this.m_DynamicContainer)
                        {
                            DestroyImmediate(this.m_Slots[i].gameObject);
                            Debug.Log("Destroy");
                        }
                    }
                }
                RefreshSlots();
                return true;
            }
            else if (UseReferences)
            {
                bool result = false;
                //Loop through all slots in this container and remove the item
                for (int i = 0; i < this.m_Slots.Count; i++)
                {
                    if (this.m_Slots[i].ObservedItem == item)
                    {
                        this.m_Slots[i].ObservedItem = null;
                        item.ReferencedSlots.Remove(this.m_Slots[i]);
                        OnRemoveItem(item, item.Stack, this.m_Slots[i]);
                        result = true;
                    }
                }
                return result;
            }
            return false;
        }

        /// <summary>
        /// Removes all items from this container. If the container uses reference this will remove all references in this container.
        /// </summary>
        /// <param name="keepInCollection">If set to true, items will be not removed from collection.</param>
        public virtual void RemoveItems(bool keepInCollection = false)
        {
            //Remove all visuals from this container
            if (this.m_DynamicContainer)
            {
                for (int i = 0; i < this.m_Slots.Count; i++)
                {
                    if (this.m_Slots[i].ObservedItem != null)
                    {
                        Item item = this.m_Slots[i].ObservedItem;
                        OnRemoveItem(item, item.Stack, this.m_Slots[i]);
                    }
                    DestroyImmediate(this.m_Slots[i].gameObject);
                }
                RefreshSlots();
            }
            else
            {
                for (int i = 0; i < this.m_Slots.Count; i++)
                {
                    if (this.m_Slots[i].ObservedItem != null)
                    {
                        Item item = this.m_Slots[i].ObservedItem;
                        OnRemoveItem(item, item.Stack, this.m_Slots[i]); 
                    }
                    this.m_Slots[i].ObservedItem = null;
                }
            }
            if (!UseReferences && !keepInCollection)
            {
                //Remove items from the collection
                this.m_Collection.Clear();
            }

        }

        /// <summary>
        /// Check if the container has the given amount of items
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <param name="amount">Amount/Stack of items</param>
        /// <returns></returns>
        public bool HasItem(Item item, int amount)
        {
            int existingAmount = 0;
            return HasItem(item,amount,out existingAmount);
        }

        /// <summary>
        /// Check if the container has the given amount of items
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <param name="amount">Amount/Stack of items</param>
        /// <returns></returns>
        public bool HasItem(Item item, int amount, out int existingAmount)
        {
            int stack = existingAmount =  0;
            for (int i = 0; i < this.m_Slots.Count; i++)
            {
                Slot slot = this.m_Slots[i];
                if (!slot.IsEmpty && slot.ObservedItem.Id == item.Id)
                {

                    stack += slot.ObservedItem.Stack;
                }
            }
            existingAmount = stack;
            return stack >= amount;
        }

        /// <summary>
        /// Get an array of items with given id in this container.
        /// </summary>
        /// <param name="id">Item id</param>
        /// <returns>Array of items with given id</returns>
        public Item[] GetItems(string id)
        {
            List<Item> items = new List<Item>();
            if (!this.m_UseReferences)
            {
                items.AddRange(this.m_Collection.Where(x => x.Id == id));
            }
            else
            {
                items.AddRange(this.m_Slots.Where(x => !x.IsEmpty && x.ObservedItem.Id == id).Select(y => y.ObservedItem));
            }
            return items.ToArray();
        }

        /// <summary>
        /// Returns an array of items of type this container is holding. 
        /// </summary>
        public T[] GetItems<T>(bool inherit = false) where T : Item
        {
            return GetItems(typeof(T), inherit).Cast<T>().ToArray();
        }

        /// <summary>
        /// Returns an array of items of type this container is holding. 
        /// </summary>
        public Item[] GetItems(Type type, bool inherit = false)
        {
            List<Item> items = new List<Item>();
            if (!this.m_UseReferences)
            {
                items.AddRange(this.m_Collection.Where(x => (!inherit && x.GetType() == type) || (inherit && type.IsAssignableFrom(x.GetType()))));
            }
            else
            {
                items.AddRange(this.m_Slots.Where(x => !x.IsEmpty && ((!inherit && x.ObservedItem.GetType() == type) || (inherit && type.IsAssignableFrom(x.ObservedItem.GetType())) )).Select(y => y.ObservedItem));
            }
            return items.ToArray();
        }

        /// <summary>
        /// Returns required slots for this item in this container. Some items like two-handed weapons require multiple slots.
        /// </summary>
        /// <param name="item">Item to get the required slots for.</param>
        /// <returns>Required slots for given item. If item does not require any special slots the returned list will be empty.</returns>
        public virtual List<Slot> GetRequiredSlots(Item item)
        {
            List<Slot> slots = new List<Slot>();
            if (item == null || !(item is EquipmentItem)) { return slots; }
            EquipmentItem equipmentItem = item as EquipmentItem;

            for (int i = 0; i < this.m_Slots.Count; i++)
            {
                Restrictions.EquipmentRegion restriction = this.m_Slots[i].GetComponent<Restrictions.EquipmentRegion>();
                if (restriction != null)
                {
                    for (int j = 0; j < equipmentItem.Region.Count; j++)
                    {
                        if (equipmentItem.Region[j].Name == restriction.region.Name)
                        {
                            slots.Add(this.m_Slots[i]);
                        }
                    }
                }
            }

            return slots;
        }

        /// <summary>
        /// Gets the slots in this container where the item is currently inside.
        /// </summary>
        /// <param name="item">The item in slots</param>
        /// <returns>Array of slots in this container, the item is currently located.</returns>
        public Slot[] GetSlots(Item item)
        {
            List<Slot> list = new List<Slot>();
            for (int i = 0; i < this.m_Slots.Count; i++)
            {
                if (this.m_Slots[i].ObservedItem == item)
                {
                    list.Add(this.m_Slots[i]);
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// Refreshs the slot list and reorganize indices. This method is slow!
        /// </summary>
        public void RefreshSlots()
        {
            if (this.m_DynamicContainer && this.m_SlotParent != null)
            {
                this.m_Slots = this.m_SlotParent.GetComponentsInChildren<Slot>(true).Where(x=>x.GetComponentsInParent<ItemContainer>(true).FirstOrDefault() == this).ToList();
                this.m_Slots.Remove(this.m_SlotPrefab.GetComponent<Slot>());
            }
            else
            {
                this.m_Slots = GetComponentsInChildren<Slot>(true).Where(x => x.GetComponentsInParent<ItemContainer>(true).FirstOrDefault() == this).ToList();
            }

            for (int i = 0; i < this.m_Slots.Count; i++)
            {
                Slot slot = this.m_Slots[i];
                slot.Index = i;
                slot.Container = this;
                slot.restrictions.AddRange(restrictions);
            }
        }

        /// <summary>
        /// Returns an array of slots of type this container is providing.
        /// </summary>
        public T[] GetSlots<T>() where T : Slot
        {
            return GetSlots(typeof(T)).Cast<T>().ToArray();
        }

        /// <summary>
        /// Returns an array of slots of type this container is providing.
        /// </summary>
        public Slot[] GetSlots(Type type)
        {
            return this.m_Slots.Where(x => x.GetType() == type).ToArray(); ;
        }

        /// <summary>
        /// Creates a new slot
        /// </summary>
        protected virtual Slot CreateSlot()
        {
            if (this.m_SlotPrefab != null && this.m_SlotParent != null)
            {
                GameObject go = (GameObject)Instantiate(this.m_SlotPrefab);
                go.SetActive(true);
                go.transform.SetParent(this.m_SlotParent, false);
                Slot slot = go.GetComponent<Slot>();
                this.m_Slots.Add(slot);
                slot.Index = Slots.Count - 1;
                slot.Container = this;
                slot.restrictions.AddRange(restrictions);
                return slot;
            }
            Debug.LogWarning("Please ensure that the slot prefab and slot parent is set in the inspector.");
            return null;
        }

        /// <summary>
        /// Destroy the slot and reorganize indices.
        /// </summary>
        /// <param name="slotID">Slot I.</param>
        protected virtual void DestroySlot(int index)
        {
            if (index < this.m_Slots.Count)
            {
                DestroyImmediate(this.m_Slots[index].gameObject);
                RefreshSlots();
            }
        }

        /// <summary>
        /// Checks if the item can be stacked to the item in slot.
        /// </summary>
        /// <param name="slot">Slot where to stack the item to.</param>
        /// <param name="item">Item to stack</param>
        /// <returns>Returns true if item can be stacked with the item in slot</returns>
        public bool CanStack(Slot slot, Item item)
        {
            Item slotItem = slot.ObservedItem;
            return (slotItem != null &&
                    item != null &&
                    slotItem.Id == item.Id &&
                    (slotItem.Stack + item.Stack) <= slotItem.MaxStack);
        }

        /// <summary>
        /// Checks if the item can be stacked to any item in this collection.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool CanStack(Item item) {
            //Check if item or collection is null
            if (item == null || this.m_Collection == null)
            {
                return false;
            }
            //Get all items in collection with same id as the item to stack
            Item[] items = this.m_Collection.Where(x => x != null && x.Id == item.Id).ToArray();
            //Loop through the items
            for (int i = 0; i < items.Length; i++)
            {
                Item current = items[i];
                //Check if max stack is reached
                if ((current.Stack + item.Stack) <= current.MaxStack)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Try to stack the item to any item in collection.
        /// </summary>
        public bool StackItem(Item item)
        {
            //Check if item or collection is null
            if (item == null || this.m_Collection == null)
            {
                return false;
            }
            //Get all items in collection with same id as the item to stack
            Item[] items = this.m_Collection.Where(x => x != null && x.Id == item.Id).ToArray();
            //Loop through the items
            for (int i = 0; i < items.Length; i++)
            {
                Item current = items[i];
                //Check if max stack is reached
                if ((current.Stack + item.Stack) <= current.MaxStack)
                {
                    current.Stack += item.Stack;
                    TryConvertCurrency(current as Currency);
                    OnAddItem(item, current.Slot);
                    return true;
                }
            }
            return false;
        }

        public void ShowByCategory(Dropdown dropdown) {
            int current = dropdown.value;
            string category = dropdown.options[current].text;
            if (current != 0)
            {
                for (int i = 0; i < this.m_Slots.Count; i++)
                {
                    if (this.m_Slots[i].ObservedItem != null)
                    {
                        this.m_Slots[i].gameObject.SetActive(this.m_Slots[i].ObservedItem.Category.Name == category);
                    }
                }
            }
            else
            {
                for (int i = 0; i < this.m_Slots.Count; i++)
                {
                    this.m_Slots[i].gameObject.SetActive(true);

                }
            }
        }


        /// <summary>
        /// Try to convert currency based on currency conversation set in the editor
        /// </summary>
        private void TryConvertCurrency(Currency currency)
        {
            if (currency == null) { return; }
            Currency[] currencies = GetItems<Currency>();
            for (int j = 0; j < currency.currencyConversions.Length; j++)
            {
                CurrencyConversion conversion = currency.currencyConversions[j];
                float amount = (currency.Stack * conversion.factor);
                if (amount >= 1f && amount < currency.Stack)
                {
                    float rest = amount % 1f;
                    Currency converted = currencies.Where(x => x.Name == conversion.currency.Name).FirstOrDefault();
                    converted.Stack += Mathf.RoundToInt(amount - rest);
                    currency.Stack = Mathf.RoundToInt(rest / conversion.factor);
                    TryConvertCurrency(converted);
                    break;
                }
            }
        }

        /// <summary>
        /// Try to remove pay currency from current currency
        /// </summary>
        /// <param name="current"></param>
        /// <param name="payCurrency"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        private bool TryRemove(Currency current, Currency payCurrency, int price)
        {
            if (current == null)
            {
                return false;
            }
            if (current.Id == payCurrency.Id)
            {
                if (current.Stack - price < 0)
                {
                    return false;
                }
                current.Stack -= price;
                return true;
            }
            else
            {
                for (int j = 0; j < current.currencyConversions.Length; j++)
                {
                    CurrencyConversion conversion = current.currencyConversions[j];
                    if (conversion.factor < 1f)
                    {
                        float amount = (current.Stack * conversion.factor);
                        float rest = amount % 1f;
                        Currency converted = GetItems(conversion.currency.Id).FirstOrDefault() as Currency;
                        converted.Stack += Mathf.RoundToInt(amount - rest);
                        current.Stack = Mathf.RoundToInt(rest / conversion.factor);
                        return TryRemove(converted, payCurrency, price);
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Convert all currencies to smallest currency value -> Sliver and Gold to Copper
        /// </summary>
        private void ConvertToSmallestCurrency()
        {
            Currency[] currencies = GetItems<Currency>();

            for (int i = 0; i < currencies.Length; i++)
            {
                Currency current = currencies[i];

                for (int j = 0; j < current.currencyConversions.Length; j++)
                {
                    CurrencyConversion conversion = current.currencyConversions[j];
                    if (conversion.factor > 1f && current.Stack > 0)
                    {
                        float amount = (current.Stack * conversion.factor);
                        GetItems(conversion.currency.Id).FirstOrDefault().Stack += Mathf.RoundToInt(amount);
                        current.Stack = 0;
                        ConvertToSmallestCurrency();
                    }
                }

            }
        }


        /// <summary>
        /// Register callbacks for inspector
        /// </summary>
        protected virtual void RegisterCallbacks()
        {
            OnAddItem += (Item item, Slot slot) => {
                ItemEventData eventData = new ItemEventData(item);
                eventData.slot = slot;
                Execute("OnAddItem", eventData);
            };
            OnFailedToAddItem += (Item item) => {
                ItemEventData eventData = new ItemEventData(item);
                Execute("OnFailedToAddItem", eventData);
            };
            OnRemoveItem += (Item item, int amount, Slot slot) => {
                ItemEventData eventData = new ItemEventData(item);
                eventData.slot = slot;
                Execute("OnRemoveItem", eventData);
            };
            OnFailedToRemoveItem += (Item item, int amount) => {
                ItemEventData eventData = new ItemEventData(item);
                Execute("OnFailedToRemoveItem", eventData);
            };
            OnUseItem += (Item item, Slot slot) => {
                ItemEventData eventData = new ItemEventData(item);
                eventData.slot = slot;
                Execute("OnUseItem", eventData);
            };
            OnDropItem += (Item item, GameObject droppedInstance) => {
                ItemEventData eventData = new ItemEventData(item);
                eventData.gameObject = droppedInstance;
                Execute("OnDropItem", eventData);
            };
           
        }

        /// <summary>
        /// Removes the amount/stack of items in all containers with windowName
        /// </summary>
        /// <param name="windowName">Name of item containers.</param>
        /// <param name="item">Item to remove.</param>
        /// <param name="amount">Amount/Stack to remove.</param>
        /// <returns>Returns true if the stack of items is removed.</returns>
        public static bool RemoveItem(string windowName, Item item, int amount)
        {
            if (!HasItem(windowName, item, amount)) {
                return false;
            }

            int restAmount = amount;
            ItemContainer[] windows = WidgetUtility.FindAll<ItemContainer>(windowName);
            for (int j = 0; j < windows.Length; j++)
            {
                ItemContainer current = windows[j];
                int currentAmount = 0;
                current.HasItem(item, restAmount, out currentAmount);
                int removeAmount = Mathf.Clamp(currentAmount, 0, restAmount);
                current.RemoveItem(item, removeAmount);
                restAmount -= removeAmount;
                if (restAmount <= 0) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes the item from collection it belongs to and all referenced slots
        /// </summary>
        /// <param name="item"></param>
        public static void RemoveItemCompletely(Item item)
        {
            if (item == null)
            {
                Debug.LogWarning("Can't remove item that is null.");
                return;
            }
            RemoveItemReferences(item);
            if (item.Container != null)
            {
                item.Container.RemoveItem(item);
            }
        }

        /// <summary>
        /// Removes the item in all references.
        /// </summary>
        /// <param name="item">Item to remove the references from</param>
        public static void RemoveItemReferences(Item item)
        {
            if (item == null) { return; }
            for (int i = 0; i < item.ReferencedSlots.Count; i++)
            {
                item.ReferencedSlots[i].Container.OnRemoveItem(item, item.Stack, item.ReferencedSlots[i]);
                item.ReferencedSlots[i].ObservedItem = null;
            }
            item.ReferencedSlots.Clear();
        }

        /// <summary>
        /// Checks in all containers named by windowName if the amount of items exists.
        /// </summary>
        /// <param name="windowName">The name of item containers</param>
        /// <param name="item">The item to check.</param>
        /// <param name="amount">Required amount/stack.</param>
        /// <returns>True if the containers have the amount of items.</returns>
        public static bool HasItem(string windowName, Item item, int amount)
        {
            ItemContainer[] windows = WidgetUtility.FindAll<ItemContainer>(windowName);
            int existingAmount = 0;
            for (int j = 0; j < windows.Length; j++)
            {
                ItemContainer current = windows[j];
                int currentAmount = 0;
                if (current.HasItem(item, amount, out currentAmount)) {
                    existingAmount += currentAmount;
                }

            }
            return existingAmount >= amount;
        }

        /// <summary>
        /// Adds items to item container. This will look up for all containers ordered by priority.
        /// </summary>
        /// <param name="windowName">Name of the item container.</param>
        /// <param name="items">Items to add.</param>
        /// <param name="allowStacking">Should the items be stacked, if possible?</param>
        /// <returns>True if items were stacked or added.</returns>
        public static bool AddItems(string windowName, Item[] items, bool allowStacking = true)
        {
            ItemContainer[] windows = WidgetUtility.FindAll<ItemContainer>(windowName);
            List<Item> checkedItems = new List<Item>(items);

            for (int i = checkedItems.Count - 1; i >= 0; i--)
            {
                Item item = checkedItems[i];
                if (windows.Length > 0)
                {
                    for (int j = 0; j < windows.Length; j++)
                    {
                        ItemContainer current = windows[j];

                        if ((allowStacking && current.StackOrAdd(checkedItems[i])) || (!allowStacking && current.AddItem(checkedItems[i])))
                        {
                            checkedItems.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            return checkedItems.Count == 0;
        }

        /// <summary>
        /// Adds an item to item container. This will look up for all containers ordered by priority.
        /// </summary>
        /// <param name="windowName">Name of the item container.</param>
        /// <param name="item">Item to add.</param>
        /// <param name="allowStacking">Should the item be stacked, if possible?</param>
        /// <returns>True if item was stacked or added.</returns>
        public static bool AddItem(string windowName, Item item, bool allowStacking = true) {
            ItemContainer[] windows = WidgetUtility.FindAll<ItemContainer>(windowName);
            for (int j = 0; j < windows.Length; j++)
            {
                ItemContainer current = windows[j];

                if ((allowStacking && current.StackOrAdd(item)) || (!allowStacking && current.AddItem(item)))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the items can be added to container.
        /// </summary>
        /// <param name="windowName">Name of item container.</param>
        /// <param name="item">Item to check</param>
        /// <param name="createSlot">Should a slot be created if the container is dynamic.</param>
        /// <returns>True if item can be added.</returns>
        public static bool CanAddItems(string windowName, Item[] items, bool createSlot = false)
        {
            ItemContainer[] windows = WidgetUtility.FindAll<ItemContainer>(windowName);
            List<Item> checkedItems = new List<Item>(items);

            for (int i = checkedItems.Count - 1; i >= 0; i--)
            {
                Item item = checkedItems[i];
                if (windows.Length > 0)
                {
                    for (int j = 0; j < windows.Length; j++)
                    {
                        ItemContainer current = windows[j];
                        Slot slot;
                        if (current.CanAddItem(item,out slot,createSlot))
                        {
                            checkedItems.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            return checkedItems.Count == 0;
        }

        /// <summary>
        /// Checks if the item can be added to container.
        /// </summary>
        /// <param name="windowName">Name of item container.</param>
        /// <param name="item">Item to check</param>
        /// <param name="createSlot">Should a slot be created if the container is dynamic.</param>
        /// <returns>True if item can be added.</returns>
        public static bool CanAddItem(string windowName, Item item, bool createSlot = false)
        {
            Slot slot;
            return CanAddItem(windowName,item,out slot,createSlot);
        }

        /// <summary>
        /// Checks if the item can be added to container.
        /// </summary>
        /// <param name="windowName">Name of item container.</param>
        /// <param name="item">Item to check</param>
        /// <param name="slot">Required or next free slot where to add.</param>
        /// <param name="createSlot">Should a slot be created if the container is dynamic.</param>
        /// <returns>True if item can be added.</returns>
        public static bool CanAddItem(string windowName, Item item, out Slot slot, bool createSlot = false)
        {
            slot = null;
            ItemContainer[] windows = WidgetUtility.FindAll<ItemContainer>(windowName);
            for (int j = 0; j < windows.Length; j++)
            {
                ItemContainer current = windows[j];
                if (current.CanAddItem(item, out slot, createSlot)) {
                    return true;
                }
            }
            return false;
        }

        public static void Cooldown(Item item, float globalCooldown)
        {
            ItemContainer[] containers = WidgetUtility.FindAll<ItemContainer>();
            for (int i = 0; i < containers.Length; i++)
            {
                ItemContainer mContainer = containers[i];
                mContainer.CooldownSlots(item, globalCooldown);

            }
        }

        private void CooldownSlots(Item item, float globalCooldown)
        {
            for (int i = 0; i < this.m_Slots.Count; i++)
            {
                ItemSlot slot = this.m_Slots[i] as ItemSlot;
                if (slot != null && !slot.IsEmpty)
                {
                    if (item is UsableItem && slot.ObservedItem == item)
                    {
                        slot.Cooldown((item as UsableItem).Cooldown);
                    }
                    else if (slot.ObservedItem.Category == item.Category)
                    {
                        slot.Cooldown(item.Category.Cooldown);
                    }
                    else {
                        slot.Cooldown(globalCooldown);
                    }
                }
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (CanDragIn && ItemSlot.dragObject != null) {
                for (int j = 0; j < this.m_Slots.Count; j++)
                {
                    if (SwapItems(this.m_Slots[j], ItemSlot.dragObject.slot))
                    {
                        ItemSlot.dragObject.slot.Repaint();
                        ItemSlot.dragObject = null;
                        return;
                    }
                }
            }
        }

        public void NotifyDropItem(Item item, GameObject instance) {
            OnDropItem(item, instance);
        }

        public void NotifyUseItem(Item item, Slot slot)
        {
            OnUseItem(item, slot);
        }

        public void Lock(bool state) {
            this.m_IsLocked = state;
        }

        public void MoveTo(string windowName)
        {
            Item[] items = GetItems<Item>(true);
            ItemContainer container = WidgetUtility.Find<ItemContainer>(windowName);
            for (int i = 0; i < items.Length; i++)
            {
                if (container.AddItem(items[i])) {
               
                    RemoveItem(items[i]);
                }
            }
        }

        [System.Serializable]
        public class MoveItemCondition {
            public string window;
            public bool requiresVisibility = true;
        }
	}
}