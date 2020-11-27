using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class IYNet_IYMsg_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(IYNet.IYMsg);

            field = type.GetField("cmd", flag);
            app.RegisterCLRFieldGetter(field, get_cmd_0);
            app.RegisterCLRFieldSetter(field, set_cmd_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_cmd_0, AssignFromStack_cmd_0);


        }



        static object get_cmd_0(ref object o)
        {
            return ((IYNet.IYMsg)o).cmd;
        }

        static StackObject* CopyToStack_cmd_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((IYNet.IYMsg)o).cmd;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static void set_cmd_0(ref object o, object v)
        {
            ((IYNet.IYMsg)o).cmd = (System.Int32)v;
        }

        static StackObject* AssignFromStack_cmd_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Int32 @cmd = ptr_of_this_method->Value;
            ((IYNet.IYMsg)o).cmd = @cmd;
            return ptr_of_this_method;
        }



    }
}
