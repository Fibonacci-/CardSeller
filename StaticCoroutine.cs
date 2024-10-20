using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CardSeller
{
    public class StaticCoroutine
    {
        private static StaticCoroutineRunner runner;

        public static Coroutine Start(IEnumerator coroutine)
        {
            EnsureRunner();
            return runner.StartCoroutine(coroutine);
        }

        public static void StopAll()
        {
            EnsureRunner();
            runner.StopAllCoroutines();
        }

        private static void EnsureRunner()
        {
            if (runner == null)
            {
                runner = new GameObject("[Static Coroutine Runner]").AddComponent<StaticCoroutineRunner>();
                UnityEngine.Object.DontDestroyOnLoad(runner.gameObject);
            }
        }



        private class StaticCoroutineRunner : MonoBehaviour { }
    }
}
