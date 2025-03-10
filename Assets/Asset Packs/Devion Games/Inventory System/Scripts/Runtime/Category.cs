﻿using UnityEngine;
using System.Collections;

namespace DevionGames.InventorySystem{
	[System.Serializable]
	public class Category : ScriptableObject,INameable {
		[SerializeField]
		private new string name="";
		public string Name{
			get{return this.name;}
			set{this.name = value;}
		}

        [SerializeField]
        private float m_Cooldown = 1f;
        public float Cooldown {
            get { return this.m_Cooldown; }
        }
	}
}