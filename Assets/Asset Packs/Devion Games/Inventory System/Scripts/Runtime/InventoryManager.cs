using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using DevionGames.UIWidgets;
using DevionGames.InventorySystem.Configuration;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace DevionGames.InventorySystem
{
	public class InventoryManager : MonoBehaviour
	{
		private static InventoryManager m_Current;

		/// <summary>
		/// The InventoryManager singleton object. This object is set inside Awake()
		/// </summary>
		public static InventoryManager current {
			get {
                Assert.IsNotNull(m_Current, "Requires an Inventory Manager.Create one from Tools > Devion Games > Inventory System > Create Inventory Manager!");
				return m_Current;
			}
		}


		[SerializeField]
		private ItemDatabase m_Database = null;

		/// <summary>
		/// Gets the item database. Configurate it inside the editor.
		/// </summary>
		/// <value>The database.</value>
		public static ItemDatabase Database {
			get {
				if (InventoryManager.current != null) {
                    Assert.IsNotNull(InventoryManager.current.m_Database, "Please assign ItemDatabase to the Inventory Manager!");
                    return InventoryManager.current.m_Database;
				}
				return null;
			}
		}

        private static Default m_DefaultSettings;
        public static Default DefaultSettings {
            get {
                if (m_DefaultSettings== null)
                {
                    m_DefaultSettings = GetSetting<Default>();
                }
                return m_DefaultSettings;
            }
        }

        private static UI m_UI;
        public static UI UI
        {
            get
            {
                if (m_UI == null)
                {
                    m_UI = GetSetting<UI>();
                }
                return m_UI;
            }
        }

        private static Notifications m_Notifications;
        public static Notifications Notifications
        {
            get
            {
                if (m_Notifications == null)
                {
                    m_Notifications= GetSetting<Notifications>();
                }
                return m_Notifications;
            }
        }

        private static SavingLoading m_SavingLoading;
        public static SavingLoading SavingLoading
        {
            get
            {
                if (m_SavingLoading == null)
                {
                    m_SavingLoading = GetSetting<SavingLoading>();
                }
                return m_SavingLoading;
            }
        }

        private static Configuration.Input m_Input;
        public static Configuration.Input Input
        {
            get
            {
                if (m_Input == null)
                {
                    m_Input = GetSetting<Configuration.Input>();
                }
                return m_Input;
            }
        }

        private static T GetSetting<T>() where T: Configuration.Settings{
            if (InventoryManager.Database != null)
            {
                return (T)InventoryManager.Database.settings.Where(x => x.GetType() == typeof(T)).FirstOrDefault();
            }
            return default(T);
        }


        protected static Dictionary<string, GameObject> m_PrefabCache;

        private PlayerInfo m_PlayerInfo;
        public PlayerInfo PlayerInfo {
            get { 
                if (this.m_PlayerInfo == null) { this.m_PlayerInfo = new PlayerInfo(InventoryManager.DefaultSettings.playerTag); }
                return this.m_PlayerInfo;
            }
        }

		/// <summary>
		/// Don't destroy this object instance when loading new scenes.
		/// </summary>
		public bool dontDestroyOnLoad = true;

        [HideInInspector]
        public UnityEvent onSceneLoaded;
        [HideInInspector]
        public UnityEvent onSceneSaved;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        private void Awake ()
		{
			if (InventoryManager.m_Current != null) {
                if(InventoryManager.DefaultSettings.debugMessages)
                    Debug.Log ("Multiple Inventory Manager in scene...this is not supported. Destroying instance!");

				Destroy (gameObject);
				return;
			} else {
				InventoryManager.m_Current = this;
                if (EventSystem.current == null) {
                    if (InventoryManager.DefaultSettings.debugMessages)
                        Debug.Log("Missing EventSystem in scene. Auto creating!");
                        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                }

                if (Camera.main != null && Camera.main.GetComponent<PhysicsRaycaster>() == null) {
                    if (InventoryManager.DefaultSettings.debugMessages)
                        Debug.Log("Missing PhysicsRaycaster on Main Camera. Auto adding!");
                    PhysicsRaycaster physicsRaycaster = Camera.main.gameObject.AddComponent<PhysicsRaycaster>();
                    physicsRaycaster.eventMask = Physics.DefaultRaycastLayers;
                }
           
                m_PrefabCache = new Dictionary<string, GameObject>();
                UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ChangedActiveScene;
                if (dontDestroyOnLoad) {
					DontDestroyOnLoad (gameObject);
				}
                if (InventoryManager.SavingLoading.autoSave) {
                    StartCoroutine(RepeatSaving(InventoryManager.SavingLoading.savingRate));
                }
                if (InventoryManager.DefaultSettings.debugMessages)
                    Debug.Log("Inventory Manager initialized.");
            }
		}

        private void Start()
        {
            if (InventoryManager.SavingLoading.autoSave){
                StartCoroutine(DelayedLoading(1f));
            }
        }

        private static void ChangedActiveScene(UnityEngine.SceneManagement.Scene current, UnityEngine.SceneManagement.Scene next)
        {
            if (InventoryManager.SavingLoading.autoSave)
            {
                InventoryManager.Load(false);
            }
        }
 
        //TODO move to utility
        [Obsolete("InventoryManager.GetBounds is obsolete Use UnityUtility.GetBounds")]
        public Bounds GetBounds(GameObject obj)
        {
            Bounds bounds = new Bounds();
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                foreach (Renderer renderer in renderers)
                {
                    if (renderer.enabled)
                    {
                        bounds = renderer.bounds;
                        break;
                    }
                }
                foreach (Renderer renderer in renderers)
                {
                    if (renderer.enabled){
                        bounds.Encapsulate(renderer.bounds);
                    }
                }
            }
            return bounds;
        }


        private IEnumerator DelayedLoading(float seconds) {
            yield return new WaitForSecondsRealtime(seconds);
            Load();
        }

        private IEnumerator RepeatSaving(float seconds) {
            while (true) {
                yield return new WaitForSeconds(seconds);
                Save();
            }
        }

        public static void Save() {
            Save(PlayerPrefs.GetString(InventoryManager.SavingLoading.savingKey, InventoryManager.SavingLoading.savingKey));
        }

        public static void Save(string key) {
            key += "InventorySystem";
            key +=" ["+ UnityEngine.SceneManagement.SceneManager.GetActiveScene().name+"]";
          
            string savedKeys = PlayerPrefs.GetString("SavedKeys");
            PlayerPrefs.SetString("SavedKeys",savedKeys+";"+key);

            List<MonoBehaviour> results = new List<MonoBehaviour>();
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().ToList().ForEach(g => results.AddRange(g.GetComponentsInChildren<MonoBehaviour>(true)));
            //DowntDestroyOnLoad GameObjects
            SingleInstance.GetInstanceObjects().ForEach(g => results.AddRange(g.GetComponentsInChildren<MonoBehaviour>(true)));
            
            IJsonSerializable[] serializables  = results.OfType<ItemCollection>().ToArray();
           
            string data = JsonSerializer.Serialize(serializables);
            PlayerPrefs.SetString(key, data);

            if (InventoryManager.current != null && InventoryManager.current.onSceneSaved != null)
            {
                InventoryManager.current.onSceneSaved.Invoke();
            }
            if (InventoryManager.DefaultSettings.debugMessages)
                Debug.Log("[Inventory System] Data saved: " + data);
        }

        public static void Load(bool includePersistent = true) {
            Load(PlayerPrefs.GetString(InventoryManager.SavingLoading.savingKey, InventoryManager.SavingLoading.savingKey),includePersistent);
        }

        public static void Load(string key, bool includePersistent = true) {
            key += "InventorySystem";
            key += " [" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "]";
            string data = PlayerPrefs.GetString(key);

            if (string.IsNullOrEmpty(data))
            {
                return;
            }

            ItemCollection[] itemCollections = FindObjectsOfType<ItemCollection>();
            for (int i= 0; i < itemCollections.Length; i++)
            {
                if (InventoryManager.GetPrefab(itemCollections[i].name.Replace("(Clone)", "")) == null)
                {
                    continue;
                }
                Destroy(itemCollections[i].gameObject);
            }

            List<object> list = MiniJSON.Deserialize(data) as List<object>;
            for (int i = 0; i < list.Count; i++)
            {
                Dictionary<string, object> mData = list[i] as Dictionary<string, object>;
                string prefab = (string)mData["Prefab"];
                List<object> positionData = mData["Position"] as List<object>;
                List<object> rotationData = mData["Rotation"] as List<object>;
                string type = (string)mData["Type"];

                Vector3 position = new Vector3(System.Convert.ToSingle(positionData[0]), System.Convert.ToSingle(positionData[1]), System.Convert.ToSingle(positionData[2]));
                Quaternion rotation = Quaternion.Euler(new Vector3(System.Convert.ToSingle(rotationData[0]), System.Convert.ToSingle(rotationData[1]), System.Convert.ToSingle(rotationData[2])));
                ItemCollection itemCollection = null;
                if (type == "UI")
                {
                    UIWidget container = WidgetUtility.Find<UIWidget>(prefab);
                    if (container != null && (includePersistent ||  container.gameObject.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene()))
                    {
                        itemCollection = container.GetComponent<ItemCollection>();
                    }
                }
                else
                {
                    GameObject collectionGameObject = CreateCollection(prefab, position, rotation);
                    if (collectionGameObject != null)
                    {
                        IGenerator[] generators = collectionGameObject.GetComponents<IGenerator>();
                        for (int j = 0; j < generators.Length; j++) {
                            generators[j].enabled = false;
                        }
                        itemCollection = collectionGameObject.GetComponent<ItemCollection>() ;
                    }
                }

                if (itemCollection != null)
                {
                    itemCollection.SetObjectData(mData);
                }
            }

            if (InventoryManager.current != null && InventoryManager.current.onSceneLoaded != null) {
                InventoryManager.current.onSceneLoaded.Invoke();
            }

            if (InventoryManager.DefaultSettings.debugMessages)
                Debug.Log("[Inventory System] Data loaded: " + data);
        }

        private static GameObject GetPrefab(string prefabName) {
#if Proxy
            return Proxy.GetPrefab(prefabName);
#else
            GameObject prefab = InventoryManager.Database.GetItemPrefab(prefabName);

            if (prefab == null && !InventoryManager.m_PrefabCache.TryGetValue(prefabName, out prefab))
            {
                prefab = Resources.Load<GameObject>(prefabName);
                InventoryManager.m_PrefabCache.Add(prefabName, prefab);
            }
            return prefab;
#endif
        }

        private static GameObject CreateCollection(string prefabName, Vector3 position, Quaternion rotation)
        {
            GameObject prefab = InventoryManager.GetPrefab(prefabName);
            if (prefab != null)
            {
                GameObject go = InventoryManager.Instantiate(prefab, position, rotation);
                go.name = go.name.Replace("(Clone)","");
                return go;

            }
            return null;
        }

        public static GameObject Instantiate(GameObject original,Vector3 position, Quaternion rotation) {
#if Proxy
            return Proxy.Instantiate(original, position, rotation);
#else
            return GameObject.Instantiate(original, position, rotation);
#endif
        }

        public static void Destroy(GameObject gameObject)
        {
#if Proxy
            Proxy.Destroy(gameObject);
#else
            GameObject.Destroy(gameObject);
#endif
        }

        public static Item[] CreateInstances(ItemGroup group)
        {
            if (group == null) {
        
                return CreateInstances(Database.items.ToArray(), Enumerable.Repeat(1, Database.items.Count).ToArray(), Enumerable.Repeat(0f, Database.items.Count).ToArray());
            }
            return CreateInstances(group.Items, group.Amounts, group.RandomProperty);
        }


        public static Item CreateInstance(Item item)
        {
            Item instance = Instantiate(item);
            if (item.IsCraftable)
            {
                for (int j = 0; j < item.ingredients.Count; j++)
                {
                    item.ingredients[j].item = Instantiate(item.ingredients[j].item);
                    item.ingredients[j].item.Stack = item.ingredients[j].amount;
                }
            }

            return instance;
        }

        public static Item[] CreateInstances(Item[] items)
        {
            Item[] instances = new Item[items.Length];

            for (int i = 0; i < items.Length; i++)
            {
                Item item = items[i];
                item = Instantiate(item);
                if (item.IsCraftable)
                {
                    for (int j = 0; j < item.ingredients.Count; j++)
                    {
                        item.ingredients[j].item = Instantiate(item.ingredients[j].item);
                        item.ingredients[j].item.Stack = item.ingredients[j].amount;
                    }
                }
                instances[i] = item;
            }
            return instances;
        }

        public static Item[] CreateInstances(Item[] items, int[] amounts, float[] randomProperty) {
            Item[] instances = new Item[items.Length];

            for (int i = 0; i < items.Length; i++)
            {
                Item item = items[i];
                item = Instantiate(item);
                item.Stack = amounts[i];
                item.PropertyPercentRange = randomProperty[i];
                
                item.RandomizeProperties(randomProperty[i]);
                if (item.IsCraftable)
                {
                    for (int j = 0; j < item.ingredients.Count; j++)
                    {
                        item.ingredients[j].item = Instantiate(item.ingredients[j].item);
                        item.ingredients[j].item.Stack = item.ingredients[j].amount;
                    }
                }
                instances[i] = item;
            }
            return instances;
        }
    }
}