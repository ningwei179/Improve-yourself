using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace Improve
{   
    public class GComponentAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo mCreateDisplayObject_0 = new CrossBindingMethodInfo("CreateDisplayObject");
        static CrossBindingMethodInfo mDispose_1 = new CrossBindingMethodInfo("Dispose");
        static CrossBindingFunctionInfo<FairyGUI.GObject, System.Int32, FairyGUI.GObject> mAddChildAt_2 = new CrossBindingFunctionInfo<FairyGUI.GObject, System.Int32, FairyGUI.GObject>("AddChildAt");
        static CrossBindingFunctionInfo<System.Int32, System.Boolean, FairyGUI.GObject> mRemoveChildAt_3 = new CrossBindingFunctionInfo<System.Int32, System.Boolean, FairyGUI.GObject>("RemoveChildAt");
        static CrossBindingFunctionInfo<System.Int32> mGetFirstChildInView_4 = new CrossBindingFunctionInfo<System.Int32>("GetFirstChildInView");
        static CrossBindingMethodInfo mHandleSizeChanged_5 = new CrossBindingMethodInfo("HandleSizeChanged");
        static CrossBindingMethodInfo mHandleGrayedChanged_6 = new CrossBindingMethodInfo("HandleGrayedChanged");
        static CrossBindingMethodInfo<FairyGUI.Controller> mHandleControllerChanged_7 = new CrossBindingMethodInfo<FairyGUI.Controller>("HandleControllerChanged");
        static CrossBindingMethodInfo mUpdateBounds_8 = new CrossBindingMethodInfo("UpdateBounds");
        class GetSnappingPosition_9Info : CrossBindingMethodInfo
        {
            static Type[] pTypes = new Type[] {typeof(System.Single).MakeByRefType(), typeof(System.Single).MakeByRefType()};

            public GetSnappingPosition_9Info()
                : base("GetSnappingPosition")
            {

            }

            protected override Type ReturnType { get { return null; } }

            protected override Type[] Parameters { get { return pTypes; } }
            public void Invoke(ILTypeInstance instance, ref System.Single xValue, ref System.Single yValue)
            {
                EnsureMethod(instance);
                if (method != null)
                {
                    invoking = true;
                    try
                    {
                        using (var ctx = domain.BeginInvoke(method))
                        {
                            ctx.PushObject(xValue);
                            ctx.PushObject(yValue);
                            ctx.PushObject(instance);
                            ctx.PushReference(0);
                            ctx.PushReference(1);
                            ctx.Invoke();
                            xValue = ctx.ReadObject<System.Single>(0);
                            yValue = ctx.ReadObject<System.Single>(1);
                        }
                    }
                    finally
                    {
                        invoking = false;
                    }
                }
            }

            public override void Invoke(ILTypeInstance instance)
            {
                throw new NotSupportedException();
            }
        }
        static GetSnappingPosition_9Info mGetSnappingPosition_9 = new GetSnappingPosition_9Info();
        static CrossBindingMethodInfo mOnUpdate_10 = new CrossBindingMethodInfo("OnUpdate");
        static CrossBindingMethodInfo mConstructFromResource_11 = new CrossBindingMethodInfo("ConstructFromResource");
        static CrossBindingMethodInfo<FairyGUI.Utils.ByteBuffer> mConstructExtension_12 = new CrossBindingMethodInfo<FairyGUI.Utils.ByteBuffer>("ConstructExtension");
        static CrossBindingMethodInfo<FairyGUI.Utils.XML> mConstructFromXML_13 = new CrossBindingMethodInfo<FairyGUI.Utils.XML>("ConstructFromXML");
        static CrossBindingMethodInfo<FairyGUI.Utils.ByteBuffer, System.Int32> mSetup_AfterAdd_14 = new CrossBindingMethodInfo<FairyGUI.Utils.ByteBuffer, System.Int32>("Setup_AfterAdd");
        static CrossBindingFunctionInfo<FairyGUI.IFilter> mget_filter_15 = new CrossBindingFunctionInfo<FairyGUI.IFilter>("get_filter");
        static CrossBindingMethodInfo<FairyGUI.IFilter> mset_filter_16 = new CrossBindingMethodInfo<FairyGUI.IFilter>("set_filter");
        static CrossBindingFunctionInfo<FairyGUI.BlendMode> mget_blendMode_17 = new CrossBindingFunctionInfo<FairyGUI.BlendMode>("get_blendMode");
        static CrossBindingMethodInfo<FairyGUI.BlendMode> mset_blendMode_18 = new CrossBindingMethodInfo<FairyGUI.BlendMode>("set_blendMode");
        static CrossBindingFunctionInfo<System.String> mget_text_19 = new CrossBindingFunctionInfo<System.String>("get_text");
        static CrossBindingMethodInfo<System.String> mset_text_20 = new CrossBindingMethodInfo<System.String>("set_text");
        static CrossBindingFunctionInfo<System.String> mget_icon_21 = new CrossBindingFunctionInfo<System.String>("get_icon");
        static CrossBindingMethodInfo<System.String> mset_icon_22 = new CrossBindingMethodInfo<System.String>("set_icon");
        static CrossBindingMethodInfo mHandlePositionChanged_23 = new CrossBindingMethodInfo("HandlePositionChanged");
        static CrossBindingMethodInfo mHandleScaleChanged_24 = new CrossBindingMethodInfo("HandleScaleChanged");
        static CrossBindingMethodInfo mHandleAlphaChanged_25 = new CrossBindingMethodInfo("HandleAlphaChanged");
        static CrossBindingMethodInfo mHandleVisibleChanged_26 = new CrossBindingMethodInfo("HandleVisibleChanged");
        static CrossBindingMethodInfo<FairyGUI.Utils.ByteBuffer, System.Int32> mSetup_BeforeAdd_27 = new CrossBindingMethodInfo<FairyGUI.Utils.ByteBuffer, System.Int32>("Setup_BeforeAdd");
        static CrossBindingMethodInfo<System.String, FairyGUI.EventCallback1> mAddEventListener_28 = new CrossBindingMethodInfo<System.String, FairyGUI.EventCallback1>("AddEventListener");
        static CrossBindingMethodInfo<System.String, FairyGUI.EventCallback0> mAddEventListener_29 = new CrossBindingMethodInfo<System.String, FairyGUI.EventCallback0>("AddEventListener");
        static CrossBindingMethodInfo<System.String, FairyGUI.EventCallback1> mRemoveEventListener_30 = new CrossBindingMethodInfo<System.String, FairyGUI.EventCallback1>("RemoveEventListener");
        static CrossBindingMethodInfo<System.String, FairyGUI.EventCallback0> mRemoveEventListener_31 = new CrossBindingMethodInfo<System.String, FairyGUI.EventCallback0>("RemoveEventListener");
        static CrossBindingFunctionInfo<System.String, System.Boolean> mDispatchEvent_32 = new CrossBindingFunctionInfo<System.String, System.Boolean>("DispatchEvent");
        static CrossBindingFunctionInfo<System.String, System.Object, System.Boolean> mDispatchEvent_33 = new CrossBindingFunctionInfo<System.String, System.Object, System.Boolean>("DispatchEvent");
        static CrossBindingFunctionInfo<System.String, System.Object, System.Object, System.Boolean> mDispatchEvent_34 = new CrossBindingFunctionInfo<System.String, System.Object, System.Object, System.Boolean>("DispatchEvent");
        static CrossBindingFunctionInfo<FairyGUI.EventContext, System.Boolean> mDispatchEvent_35 = new CrossBindingFunctionInfo<FairyGUI.EventContext, System.Boolean>("DispatchEvent");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(FairyGUI.GComponent);
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(Adapter);
            }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adapter(appdomain, instance);
        }

        public class Adapter : FairyGUI.GComponent, CrossBindingAdaptorType
        {
            ILTypeInstance instance;
            ILRuntime.Runtime.Enviorment.AppDomain appdomain;

            public Adapter()
            {

            }

            public Adapter(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
            }

            public ILTypeInstance ILInstance { get { return instance; } }

            public override void Dispose()
            {
                if (mDispose_1.CheckShouldInvokeBase(this.instance))
                    base.Dispose();
                else
                    mDispose_1.Invoke(this.instance);
            }

            public override FairyGUI.GObject AddChildAt(FairyGUI.GObject child, System.Int32 index)
            {
                if (mAddChildAt_2.CheckShouldInvokeBase(this.instance))
                    return base.AddChildAt(child, index);
                else
                    return mAddChildAt_2.Invoke(this.instance, child, index);
            }

            public override FairyGUI.GObject RemoveChildAt(System.Int32 index, System.Boolean dispose)
            {
                if (mRemoveChildAt_3.CheckShouldInvokeBase(this.instance))
                    return base.RemoveChildAt(index, dispose);
                else
                    return mRemoveChildAt_3.Invoke(this.instance, index, dispose);
            }

            public override System.Int32 GetFirstChildInView()
            {
                if (mGetFirstChildInView_4.CheckShouldInvokeBase(this.instance))
                    return base.GetFirstChildInView();
                else
                    return mGetFirstChildInView_4.Invoke(this.instance);
            }

            protected override void HandleSizeChanged()
            {
                if (mHandleSizeChanged_5.CheckShouldInvokeBase(this.instance))
                    base.HandleSizeChanged();
                else
                    mHandleSizeChanged_5.Invoke(this.instance);
            }

            protected override void HandleGrayedChanged()
            {
                if (mHandleGrayedChanged_6.CheckShouldInvokeBase(this.instance))
                    base.HandleGrayedChanged();
                else
                    mHandleGrayedChanged_6.Invoke(this.instance);
            }

            public override void HandleControllerChanged(FairyGUI.Controller c)
            {
                if (mHandleControllerChanged_7.CheckShouldInvokeBase(this.instance))
                    base.HandleControllerChanged(c);
                else
                    mHandleControllerChanged_7.Invoke(this.instance, c);
            }

            protected override void UpdateBounds()
            {
                if (mUpdateBounds_8.CheckShouldInvokeBase(this.instance))
                    base.UpdateBounds();
                else
                    mUpdateBounds_8.Invoke(this.instance);
            }

            protected internal override void GetSnappingPosition(ref System.Single xValue, ref System.Single yValue)
            {
                if (mGetSnappingPosition_9.CheckShouldInvokeBase(this.instance))
                    base.GetSnappingPosition(ref xValue, ref yValue);
                else
                    mGetSnappingPosition_9.Invoke(this.instance, ref xValue, ref yValue);
            }

            protected override void OnUpdate()
            {
                if (mOnUpdate_10.CheckShouldInvokeBase(this.instance))
                    base.OnUpdate();
                else
                    mOnUpdate_10.Invoke(this.instance);
            }

            public override void ConstructFromResource()
            {
                if (mConstructFromResource_11.CheckShouldInvokeBase(this.instance))
                    base.ConstructFromResource();
                else
                    mConstructFromResource_11.Invoke(this.instance);
            }

            protected override void ConstructExtension(FairyGUI.Utils.ByteBuffer buffer)
            {
                if (mConstructExtension_12.CheckShouldInvokeBase(this.instance))
                    base.ConstructExtension(buffer);
                else
                    mConstructExtension_12.Invoke(this.instance, buffer);
            }

            public override void ConstructFromXML(FairyGUI.Utils.XML xml)
            {
                if (mConstructFromXML_13.CheckShouldInvokeBase(this.instance))
                    base.ConstructFromXML(xml);
                else
                    mConstructFromXML_13.Invoke(this.instance, xml);
            }

            public override void Setup_AfterAdd(FairyGUI.Utils.ByteBuffer buffer, System.Int32 beginPos)
            {
                if (mSetup_AfterAdd_14.CheckShouldInvokeBase(this.instance))
                    base.Setup_AfterAdd(buffer, beginPos);
                else
                    mSetup_AfterAdd_14.Invoke(this.instance, buffer, beginPos);
            }

            protected override void HandlePositionChanged()
            {
                if (mHandlePositionChanged_23.CheckShouldInvokeBase(this.instance))
                    base.HandlePositionChanged();
                else
                    mHandlePositionChanged_23.Invoke(this.instance);
            }

            protected override void HandleScaleChanged()
            {
                if (mHandleScaleChanged_24.CheckShouldInvokeBase(this.instance))
                    base.HandleScaleChanged();
                else
                    mHandleScaleChanged_24.Invoke(this.instance);
            }

            protected override void HandleAlphaChanged()
            {
                if (mHandleAlphaChanged_25.CheckShouldInvokeBase(this.instance))
                    base.HandleAlphaChanged();
                else
                    mHandleAlphaChanged_25.Invoke(this.instance);
            }

            public override void Setup_BeforeAdd(FairyGUI.Utils.ByteBuffer buffer, System.Int32 beginPos)
            {
                if (mSetup_BeforeAdd_27.CheckShouldInvokeBase(this.instance))
                    base.Setup_BeforeAdd(buffer, beginPos);
                else
                    mSetup_BeforeAdd_27.Invoke(this.instance, buffer, beginPos);
            }

            public override FairyGUI.IFilter filter
            {
            get
            {
                if (mget_filter_15.CheckShouldInvokeBase(this.instance))
                    return base.filter;
                else
                    return mget_filter_15.Invoke(this.instance);

            }
            set
            {
                if (mset_filter_16.CheckShouldInvokeBase(this.instance))
                    base.filter = value;
                else
                    mset_filter_16.Invoke(this.instance, value);

            }
            }

            public override FairyGUI.BlendMode blendMode
            {
            get
            {
                if (mget_blendMode_17.CheckShouldInvokeBase(this.instance))
                    return base.blendMode;
                else
                    return mget_blendMode_17.Invoke(this.instance);

            }
            set
            {
                if (mset_blendMode_18.CheckShouldInvokeBase(this.instance))
                    base.blendMode = value;
                else
                    mset_blendMode_18.Invoke(this.instance, value);

            }
            }

            public override System.String text
            {
            get
            {
                if (mget_text_19.CheckShouldInvokeBase(this.instance))
                    return base.text;
                else
                    return mget_text_19.Invoke(this.instance);

            }
            set
            {
                if (mset_text_20.CheckShouldInvokeBase(this.instance))
                    base.text = value;
                else
                    mset_text_20.Invoke(this.instance, value);

            }
            }

            public override System.String icon
            {
            get
            {
                if (mget_icon_21.CheckShouldInvokeBase(this.instance))
                    return base.icon;
                else
                    return mget_icon_21.Invoke(this.instance);

            }
            set
            {
                if (mset_icon_22.CheckShouldInvokeBase(this.instance))
                    base.icon = value;
                else
                    mset_icon_22.Invoke(this.instance, value);

            }
            }

            public override string ToString()
            {
                IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
                m = instance.Type.GetVirtualMethod(m);
                if (m == null || m is ILMethod)
                {
                    return instance.ToString();
                }
                else
                    return instance.Type.FullName;
            }
        }
    }
}

