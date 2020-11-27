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
    unsafe class IYProtocal_ReqLogin_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(IYProtocal.ReqLogin);

            field = type.GetField("account", flag);
            app.RegisterCLRFieldGetter(field, get_account_0);
            app.RegisterCLRFieldSetter(field, set_account_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_account_0, AssignFromStack_account_0);
            field = type.GetField("pass", flag);
            app.RegisterCLRFieldGetter(field, get_pass_1);
            app.RegisterCLRFieldSetter(field, set_pass_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_pass_1, AssignFromStack_pass_1);

            args = new Type[]{};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }



        static object get_account_0(ref object o)
        {
            return ((IYProtocal.ReqLogin)o).account;
        }

        static StackObject* CopyToStack_account_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((IYProtocal.ReqLogin)o).account;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_account_0(ref object o, object v)
        {
            ((IYProtocal.ReqLogin)o).account = (System.String)v;
        }

        static StackObject* AssignFromStack_account_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @account = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((IYProtocal.ReqLogin)o).account = @account;
            return ptr_of_this_method;
        }

        static object get_pass_1(ref object o)
        {
            return ((IYProtocal.ReqLogin)o).pass;
        }

        static StackObject* CopyToStack_pass_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((IYProtocal.ReqLogin)o).pass;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_pass_1(ref object o, object v)
        {
            ((IYProtocal.ReqLogin)o).pass = (System.String)v;
        }

        static StackObject* AssignFromStack_pass_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @pass = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((IYProtocal.ReqLogin)o).pass = @pass;
            return ptr_of_this_method;
        }


        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = new IYProtocal.ReqLogin();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


    }
}
