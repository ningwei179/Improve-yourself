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
    unsafe class IYProtocal_NetMsg_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(IYProtocal.NetMsg);

            field = type.GetField("reqLogin", flag);
            app.RegisterCLRFieldGetter(field, get_reqLogin_0);
            app.RegisterCLRFieldSetter(field, set_reqLogin_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_reqLogin_0, AssignFromStack_reqLogin_0);

            args = new Type[]{};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }



        static object get_reqLogin_0(ref object o)
        {
            return ((IYProtocal.NetMsg)o).reqLogin;
        }

        static StackObject* CopyToStack_reqLogin_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((IYProtocal.NetMsg)o).reqLogin;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_reqLogin_0(ref object o, object v)
        {
            ((IYProtocal.NetMsg)o).reqLogin = (IYProtocal.ReqLogin)v;
        }

        static StackObject* AssignFromStack_reqLogin_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            IYProtocal.ReqLogin @reqLogin = (IYProtocal.ReqLogin)typeof(IYProtocal.ReqLogin).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((IYProtocal.NetMsg)o).reqLogin = @reqLogin;
            return ptr_of_this_method;
        }


        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = new IYProtocal.NetMsg();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


    }
}
