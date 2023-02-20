﻿using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace tutinoco
{
    public class SimpleNetworkConverter : UdonSharpBehaviour
    {
        public static string Padding(string s, int len)
        {
            if (s.Length >= len) return s.Substring(s.Length - len);
            else return new String('!', len - s.Length) + s;
        }

        public static string ToBase94(int num, bool signed=false)
        {
            string result = "";
            int b = signed ? 93 : 94;
            bool neg = num < 0;
            if (neg) num = -num;
            do { result=(char)('!' + num % b) + result; num /= b; } while (num > 0);
            if (signed && neg) result = "~"+result;
            return result;
        }

        public static int FromBase94(string str, bool signed=false)
        {
            int result = 0;
            int b = signed ? 93 : 94;
            bool neg = signed && str.StartsWith("~");

            if (neg) str = str.Substring(1);

            for (int i = 0; i < str.Length; i++) {
                int digitValue = (int)str[i] - '!';
                result = result * b + digitValue;
            }

            return neg ? -1*result : result;
        }        
    }
}