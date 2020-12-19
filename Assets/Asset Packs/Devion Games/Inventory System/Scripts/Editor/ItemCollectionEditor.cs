using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

namespace DevionGames.InventorySystem
{
	[System.Serializable]
	public class ItemCollectionEditor : ScriptableObjectCollectionEditor<Item>
	{
		[SerializeField]
		protected List<string> searchFilters;
		[SerializeField]
		protected string searchFilter = "All";

        public override string ToolbarName
        {
            get
            {
                return "Items";
            }
        }

        public ItemCollectionEditor (UnityEngine.Object target, List<Item> items, List<string> searchFilters) : base (target, items)
		{
			this.target = target;
			this.items = items;
			this.searchFilters = searchFilters;
			this.searchFilters.Insert (0, "All");
            this.m_SearchString = "All";
        }

        protected override void DoSearchGUI ()
		{
			string[] searchResult = EditorTools.SearchField (m_SearchString, searchFilter, searchFilters);
			searchFilter = searchResult [0];
			m_SearchString = string.IsNullOrEmpty(searchResult [1])?searchFilter:searchResult[1] ;
		}

		protected override bool MatchesSearch (Item item, string search)
		{
			return (item.Name.ToLower ().Contains (search.ToLower ()) || m_SearchString == searchFilter || search.ToLower() == item.GetType().Name.ToLower()) && (searchFilter == "All" || item.Category.Name == searchFilter);
		}

		protected override bool HasConfigurationErrors(Item item)
		{
			return Items.Any(x => !x.Equals(item) && x.Name == item.Name) ||
				string.IsNullOrEmpty(item.Name);
		}

        protected override void Duplicate(Item item)
        {
            Item duplicate = (Item)ScriptableObject.Instantiate(item);
			duplicate.Id = System.Guid.NewGuid().ToString();
			duplicate.hideFlags = HideFlags.HideInHierarchy;
			AssetDatabase.AddObjectToAsset(duplicate, target);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			Items.Add(duplicate);
			EditorUtility.SetDirty(target);
			Select(duplicate);
		}

		protected override void AddContextItem(GenericMenu menu)
		{
			base.AddContextItem(menu);
			menu.AddItem(new GUIContent("Sort/Category"), false, delegate {
				Item selected = selectedItem;
				Items.Sort(delegate (Item a, Item b) {
					return a.Category.Name.CompareTo(b.Category.Name); 
				});
				Select(selected);
			});
		}
	}
}