using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InteractorBase;

namespace Utility
{
    public class UtilityMono : MonoBehaviour
    {
        public static Camera MainCamera;

        private void Awake()
        {
            MainCamera = Camera.main;
        }
    }

    public class UtilityFunctions
    {
        public Vector2 MultiplyVectors(Vector2 vec1, Vector2 vec2)
        {
            return new Vector2(vec1.x * vec2.x, vec1.y * vec2.y);
        }

        public Vector3 MultiplyVectors(Vector3 vec1, Vector3 vec2)
        {
            return new Vector3(vec1.x * vec2.x, vec1.y * vec2.y, vec1.z * vec2.z);
        }
    }
    public class CommonDelegates
    {
        public delegate void SimpleDelegate();
        public delegate void InteractableDelegate(InteractorBase i, Action action);
        public delegate void IntDelegate(int val);
    }

}