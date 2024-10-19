using System;
using UnityEngine;
using ArchiLib;
using System.Collections.Generic;

namespace Sample {

    public class App : MonoBehaviour {

        void Awake() {

            int[] arr = new int[3] {
                1,
                2,
                3,
            };
            string[] strArr=new string[3]{"first","second","third"};
            Debug.Log(arr.ToArrayString());
            Debug.Log(strArr.ToArrayString());


            Dictionary<string,int> dic = new Dictionary<string, int>(){
                {"first",1},
                {"second",2},
                {"third",3},
            };
            Debug.Log(dic.ToDicString());

        }

    }

}