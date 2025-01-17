using System;
using System.Collections.Generic;

namespace ArchiLib {

    public static class ArrayExtention {

        public static string ToArrayString<T>(this T[] arr) {
            string str = "";
            for (int i = 0; i < arr.Length; i += 1) {
                var value = arr[i];
                str += value.ToString() + ",";
            }
            str = str.TrimEnd(',');
            return str;
        }
        public static string ToListString<T>(this List<T> arr) {
            string str = "";
            for (int i = 0; i < arr.Count; i += 1) {
                var value = arr[i];
                str += value.ToString() + ",";
            }
            str = str.TrimEnd(',');
            return str;
        }

    }
}