using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotFixProject
{
    public class InstanceClass
    {
        // static method
        public static void StaticFunTest()
        {
            UnityEngine.Debug.Log("!!! InstanceClass.StaticFunTest()");
        }

        public static void StaticFunTest2(int num)
        {
            UnityEngine.Debug.Log("一个参数方法的调用"+num);
        }

        //public static void StaticFunTest2(int num1,int num2)
        //{
        //    UnityEngine.Debug.Log("2个参数方法的调用:param1=" + num1+"==param2="+num2);
        //}
    }
}
