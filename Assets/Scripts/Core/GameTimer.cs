using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Winston.Core
{
    public class GameTimer : MonoBehaviour
    {
        [SerializeField] private static int gameTime = 0;

        public static event Action<int> timerUpdated;
        public static event Action DayFinished;

        // Start is called before the first frame update
        void Start()
        {
            //After 1 second begin calling Update Timer every 1 second
            InvokeRepeating("UpdateTimer", 1f, 1f);
        }

        void UpdateTimer()
        {
            gameTime += 1;

            if (gameTime > 1440)
            {
                DayFinished?.Invoke();
                gameTime = 0;
            }

            timerUpdated?.Invoke(gameTime);
        }
    }
}








