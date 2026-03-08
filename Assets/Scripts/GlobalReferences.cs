using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
   public class GlobalReferences : MonoBehaviour
   {
      [SerializeField] public GameObject bulletImpactPrefab;
      public static GlobalReferences Instance { get; private set; }
    
      private void Awake()
      {
         Instance = this;
      }
   }
}
