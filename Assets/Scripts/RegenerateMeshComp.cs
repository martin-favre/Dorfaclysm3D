using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

 [ExecuteInEditMode]
 public class RegenerateMeshComp : MonoBehaviour
 {
 
     public bool buttonDisplayName; //"run" or "generate" for example
 
     void Update()
     {
         if (buttonDisplayName)
             ButtonFunction1 ();
         buttonDisplayName = false;
     }
 
     void ButtonFunction1 ()
     {
         GetComponent<GridMapComponent>().RegenerateMeshes();
     }
 }