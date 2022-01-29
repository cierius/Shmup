using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
   public Slider slider;

   public void Update()
   {
       slider.value += .01f;
   }
}
