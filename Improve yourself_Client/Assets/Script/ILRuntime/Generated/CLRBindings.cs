using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILRuntime.Runtime.Generated
{
    class CLRBindings
    {


        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            UnityEngine_Object_Binding.Register(app);
            Improve_Singleton_1_ResourceManager_Binding.Register(app);
            Improve_ResourceManager_Binding.Register(app);
            UnityEngine_UI_Image_Binding.Register(app);
            UnityEngine_UI_Graphic_Binding.Register(app);
            System_Object_Binding.Register(app);
            System_Collections_Generic_List_1_Button_Binding.Register(app);
            System_Collections_Generic_List_1_Button_Binding_Enumerator_Binding.Register(app);
            UnityEngine_UI_Button_Binding.Register(app);
            UnityEngine_Events_UnityEventBase_Binding.Register(app);
            System_IDisposable_Binding.Register(app);
            System_Collections_Generic_List_1_Toggle_Binding.Register(app);
            System_Collections_Generic_List_1_Toggle_Binding_Enumerator_Binding.Register(app);
            UnityEngine_UI_Toggle_Binding.Register(app);
            UnityEngine_Events_UnityEvent_Binding.Register(app);
            UnityEngine_Events_UnityEvent_1_Boolean_Binding.Register(app);
            UnityEngine_Debug_Binding.Register(app);
            UnityEngine_GameObject_Binding.Register(app);
            Improve_GameMapManager_Binding.Register(app);
            UnityEngine_UI_Slider_Binding.Register(app);
            System_String_Binding.Register(app);
            UnityEngine_UI_Text_Binding.Register(app);
            System_Array_Binding.Register(app);
            UnityEngine_Component_Binding.Register(app);
            UnityEngine_Transform_Binding.Register(app);
            Improve_Singleton_1_AddressableManager_Binding.Register(app);
            Improve_AddressableManager_Binding.Register(app);
            UnityEngine_Input_Binding.Register(app);
            IYProtocal_NetMsg_Binding.Register(app);
            IYNet_IYMsg_Binding.Register(app);
            IYProtocal_ReqLogin_Binding.Register(app);
            Improve_Singleton_1_NetWorkManager_Binding.Register(app);
            Improve_NetWorkManager_Binding.Register(app);
            Improve_Singleton_1_TimerController_Binding.Register(app);
            Improve_TimerController_Binding.Register(app);
            System_Action_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Type_Binding.Register(app);
            System_Type_Binding.Register(app);
            UnityEngine_Screen_Binding.Register(app);
            UnityEngine_Camera_Binding.Register(app);
            System_Collections_Generic_List_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Stack_1_ILTypeInstance_Binding.Register(app);
            System_Activator_Binding.Register(app);
            UnityEngine_Resources_Binding.Register(app);
            Improve_Singleton_1_ObjectManager_Binding.Register(app);
            Improve_ObjectManager_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Binding_ValueCollection_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Binding_ValueCollection_Binding_Enumerator_Binding.Register(app);
            UnityEngine_EventSystems_EventSystem_Binding.Register(app);
            UnityEngine_Time_Binding.Register(app);
            System_Single_Binding.Register(app);

            ILRuntime.CLR.TypeSystem.CLRType __clrType = null;
        }

        /// <summary>
        /// Release the CLR binding, please invoke this BEFORE ILRuntime Appdomain destroy
        /// </summary>
        public static void Shutdown(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
        }
    }
}
