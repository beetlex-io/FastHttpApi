using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using BeetleX.FastHttpApi.Admin;

namespace BeetleX.FastHttpApi
{
    class ActionHandler : IComparable
    {
        public ActionHandler(object controller, System.Reflection.MethodInfo method)
        {
            Remark = "";
            Parameters = new List<ParameterBinder>();
            mMethod = method;
            mMethodHandler = new MethodHandler(mMethod);
            Controller = controller;
            LoadParameter();
            Filters = new List<FilterAttribute>();
           
        }

        

        public string Remark { get; set; }

        public List<FilterAttribute> Filters { get; set; }

        private MethodHandler mMethodHandler;

        private System.Reflection.MethodInfo mMethod;

        public object Controller { get; set; }

        public string SourceUrl { get; set; }

        private void LoadParameter()
        {
            DescriptionAttribute da = mMethod.GetCustomAttribute<DescriptionAttribute>(false);
            if (da != null)
                this.Remark = da.Description;
            foreach (System.Reflection.ParameterInfo pi in mMethod.GetParameters())
            {
                ParameterBinder pb = new DefaultParameter();
                ParameterBinder[] customPB = (ParameterBinder[])pi.GetCustomAttributes(typeof(ParameterBinder), false);
                if (customPB != null && customPB.Length > 0)
                {
                    pb = customPB[0];
                }
                else if (pi.ParameterType == typeof(string))
                {
                    pb = new StringParameter();
                }
                else if (pi.ParameterType == typeof(DateTime))
                {
                    pb = new DateTimeParameter();
                }

                else if (pi.ParameterType == typeof(Decimal))
                {
                    pb = new DecimalParameter();
                }
                else if (pi.ParameterType == typeof(float))
                {
                    pb = new FloatParameter();
                }
                else if (pi.ParameterType == typeof(double))
                {
                    pb = new DoubleParameter();
                }
                else if (pi.ParameterType == typeof(short))
                {
                    pb = new ShortParameter();
                }
                else if (pi.ParameterType == typeof(int))
                {
                    pb = new IntParameter();
                }
                else if (pi.ParameterType == typeof(long))
                {
                    pb = new LongParameter();
                }
                else if (pi.ParameterType == typeof(ushort))
                {
                    pb = new UShortParameter();
                }
                else if (pi.ParameterType == typeof(uint))
                {
                    pb = new UIntParameter();
                }
                else if (pi.ParameterType == typeof(ulong))
                {
                    pb = new ULongParameter();
                }
                else if (pi.ParameterType == typeof(HttpRequest))
                {
                    pb = new RequestParameter();
                }
                else if (pi.ParameterType == typeof(IHttpContext))
                {
                    pb = new HttpContextParameter();
                }
                else if (pi.ParameterType == typeof(IDataContext))
                {
                    pb = new DataContextParameter();
                }
                else if (pi.ParameterType == typeof(HttpApiServer))
                {
                    pb = new HttpApiServerParameter();
                }
                else if (pi.ParameterType == typeof(HttpResponse))
                {
                    pb = new ResponseParameter();
                }
                else
                {


                    if (pi.ParameterType.GetInterface("BeetleX.HttpExtend.IBodyFlag") != null)
                    {
                        pb = new BodyParameter();
                    }
                    else
                    {
                        pb = new DefaultParameter();
                    }
                }
                pb.Name = pi.Name;
                pb.Type = pi.ParameterType;
                Parameters.Add(pb);
            }
        }

        public List<ParameterBinder> Parameters { get; private set; }

        private object[] GetValues(IHttpContext context)
        {

            object[] parameters = new object[Parameters.Count];
            for (int i = 0; i < parameters.Length; i++)
            {
                parameters[i] = Parameters[i].GetValue(context);

            }
            return parameters;
        }

        public object Invoke(IHttpContext context)
        {
            object[] parameters = GetValues(context);
            return mMethodHandler.Execute(Controller, parameters);
        }

        public int CompareTo(object obj)
        {
            return this.SourceUrl.CompareTo(((ActionHandler)obj).SourceUrl);
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public abstract class ParameterBinder : Attribute
    {
        public Type Type { get; internal set; }

        public string Name { get; internal set; }

        public virtual bool DataParameter => true;

        public abstract object GetValue(IHttpContext context);

        public abstract Admin.ParameterInfo GetInfo();
    }

    class IntParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            int result;
            context.Data.TryGetInt(this.Name, out result);
            return result;
        }

        public override Admin.ParameterInfo GetInfo()
        {
            return new Admin.ParameterInfo { Name = this.Name, Type = this.Type, Value = 0 };
        }
    }

    class ShortParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            short result;
            context.Data.TryGetShort(this.Name, out result);
            return result;
        }
        public override Admin.ParameterInfo GetInfo()
        {
            return new Admin.ParameterInfo { Name = this.Name, Type = this.Type, Value = 0 };
        }
    }

    class LongParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            long result;
            context.Data.TryGetLong(this.Name, out result);
            return result;
        }
        public override Admin.ParameterInfo GetInfo()
        {
            return new Admin.ParameterInfo { Name = this.Name, Type = this.Type, Value = 0 };
        }
    }

    class UIntParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            uint result;
            context.Data.TryGetUInt(this.Name, out result);
            return result;
        }
        public override Admin.ParameterInfo GetInfo()
        {
            return new Admin.ParameterInfo { Name = this.Name, Type = this.Type, Value = 0 };
        }
    }

    class UShortParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            ushort result;
            context.Data.TryGetUShort(this.Name, out result);
            return result;
        }
        public override Admin.ParameterInfo GetInfo()
        {
            return new Admin.ParameterInfo { Name = this.Name, Type = this.Type, Value = 0 };
        }
    }

    class ULongParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            ulong result;
            context.Data.TryGetULong(this.Name, out result);
            return result;
        }
        public override Admin.ParameterInfo GetInfo()
        {
            return new Admin.ParameterInfo { Name = this.Name, Type = this.Type, Value = 0 };
        }
    }

    class FloatParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            float result;
            context.Data.TryGetFloat(this.Name, out result);
            return result;
        }
        public override Admin.ParameterInfo GetInfo()
        {
            return new Admin.ParameterInfo { Name = this.Name, Type = this.Type, Value = 0 };
        }
    }

    class DoubleParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            double result;
            context.Data.TryGetDouble(this.Name, out result);
            return result;
        }
        public override Admin.ParameterInfo GetInfo()
        {
            return new Admin.ParameterInfo { Name = this.Name, Type = this.Type, Value = 0 };
        }
    }

    class StringParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            string result;
            context.Data.TryGetString(this.Name, out result);
            return result;
        }
        public override Admin.ParameterInfo GetInfo()
        {
            return new Admin.ParameterInfo { Name = this.Name, Type = this.Type, Value = "" };
        }
    }

    class DecimalParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            Decimal result;
            context.Data.TryGetDecimal(this.Name, out result);
            return result;
        }
        public override Admin.ParameterInfo GetInfo()
        {
            return new Admin.ParameterInfo { Name = this.Name, Type = this.Type, Value = 0 };
        }
    }

    class DateTimeParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            DateTime result;
            context.Data.TryGetDateTime(this.Name, out result);
            return result;
        }
        public override Admin.ParameterInfo GetInfo()
        {
            return new Admin.ParameterInfo { Name = this.Name, Type = this.Type, Value = DateTime.Now };
        }
    }

    public class BodyParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            return context.Data.GetBody(this.Type);
        }
        public override Admin.ParameterInfo GetInfo()
        {
            return new Admin.ParameterInfo { IsBody = true, Name = this.Name, Type = this.Type, Value = Activator.CreateInstance(Type) };
        }
    }

    class RequestParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            return context.Request;
        }

        public override bool DataParameter => false;
        public override Admin.ParameterInfo GetInfo()
        {
            return new Admin.ParameterInfo { Name = this.Name, Type = this.Type, Value = Activator.CreateInstance(Type) };
        }
    }


    class HttpApiServerParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            return context.Server;
        }

        public override bool DataParameter => false;
        public override Admin.ParameterInfo GetInfo()
        {
            return new Admin.ParameterInfo { Name = this.Name, Type = this.Type, Value = Activator.CreateInstance(Type) };
        }
    }


    class ResponseParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            return context.Response;
        }

        public override bool DataParameter => false;
        public override Admin.ParameterInfo GetInfo()
        {
            return new Admin.ParameterInfo { Name = this.Name, Type = this.Type, Value = Activator.CreateInstance(Type) };
        }
    }



    class HttpContextParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            return context;
        }

        public override bool DataParameter => false;
        public override Admin.ParameterInfo GetInfo()
        {
            return new Admin.ParameterInfo { Name = this.Name, Type = this.Type, Value = Activator.CreateInstance(Type) };
        }
    }

    public class DataContextParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            return context.Data;
        }

        public override bool DataParameter => false;

        public override Admin.ParameterInfo GetInfo()
        {
            return new Admin.ParameterInfo { Name = this.Name, Type = this.Type, Value = Activator.CreateInstance(Type) };
        }
    }

    class DefaultParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            return context.Data.GetObject(this.Name, this.Type);
        }
        public override Admin.ParameterInfo GetInfo()
        {
            return new Admin.ParameterInfo { Name = this.Name, Type = this.Type, Value = Activator.CreateInstance(Type) };
        }
    }


    #region emithandler
    class MethodHandler
    {
        public MethodHandler(MethodInfo method)
        {
            mExecute = ReflectionHandlerFactory.MethodHandler(method);
            mInfo = method;
        }
        private MethodInfo mInfo;
        public MethodInfo Info
        {
            get
            {
                return mInfo;
            }
        }
        private FastMethodHandler mExecute;
        public FastMethodHandler Execute
        {
            get
            {
                return mExecute;
            }
        }
    }
    public delegate object FastMethodHandler(object target, object[] paramters);
    class ReflectionHandlerFactory
    {
        #region Method Handler
        private static Dictionary<MethodInfo, FastMethodHandler> mMethodHandlers = new Dictionary<MethodInfo, FastMethodHandler>();
        public static FastMethodHandler MethodHandler(MethodInfo method)
        {
            FastMethodHandler handler = null;
            if (mMethodHandlers.ContainsKey(method))
            {
                handler = mMethodHandlers[method];
            }
            else
            {
                lock (typeof(ReflectionHandlerFactory))
                {
                    if (mMethodHandlers.ContainsKey(method))
                    {
                        handler = mMethodHandlers[method];
                    }
                    else
                    {
                        handler = CreateMethodHandler(method);
                        mMethodHandlers.Add(method, handler);
                    }
                }
            }
            return handler;
        }
        private static FastMethodHandler CreateMethodHandler(MethodInfo methodInfo)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, typeof(object), new Type[] { typeof(object), typeof(object[]) }, methodInfo.DeclaringType.Module);
            ILGenerator il = dynamicMethod.GetILGenerator();
            System.Reflection.ParameterInfo[] ps = methodInfo.GetParameters();
            Type[] paramTypes = new Type[ps.Length];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                    paramTypes[i] = ps[i].ParameterType.GetElementType();
                else
                    paramTypes[i] = ps[i].ParameterType;
            }
            LocalBuilder[] locals = new LocalBuilder[paramTypes.Length];

            for (int i = 0; i < paramTypes.Length; i++)
            {
                locals[i] = il.DeclareLocal(paramTypes[i], true);
            }
            for (int i = 0; i < paramTypes.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_1);
                EmitFastInt(il, i);
                il.Emit(OpCodes.Ldelem_Ref);
                EmitCastToReference(il, paramTypes[i]);
                il.Emit(OpCodes.Stloc, locals[i]);
            }
            if (!methodInfo.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
            }
            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                    il.Emit(OpCodes.Ldloca_S, locals[i]);
                else
                    il.Emit(OpCodes.Ldloc, locals[i]);
            }
            if (methodInfo.IsStatic)
                il.EmitCall(OpCodes.Call, methodInfo, null);
            else
                il.EmitCall(OpCodes.Callvirt, methodInfo, null);
            if (methodInfo.ReturnType == typeof(void))
                il.Emit(OpCodes.Ldnull);
            else
                EmitBoxIfNeeded(il, methodInfo.ReturnType);

            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                {
                    il.Emit(OpCodes.Ldarg_1);
                    EmitFastInt(il, i);
                    il.Emit(OpCodes.Ldloc, locals[i]);
                    if (locals[i].LocalType.IsValueType)
                        il.Emit(OpCodes.Box, locals[i].LocalType);
                    il.Emit(OpCodes.Stelem_Ref);
                }
            }

            il.Emit(OpCodes.Ret);
            FastMethodHandler invoder = (FastMethodHandler)dynamicMethod.CreateDelegate(typeof(FastMethodHandler));
            return invoder;
        }
        #endregion

        private static void EmitCastToReference(ILGenerator il, System.Type type)
        {
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, type);
            }
            else
            {
                il.Emit(OpCodes.Castclass, type);
            }
        }
        private static void EmitBoxIfNeeded(ILGenerator il, System.Type type)
        {
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Box, type);
            }
        }
        private static void EmitFastInt(ILGenerator il, int value)
        {
            switch (value)
            {
                case -1:
                    il.Emit(OpCodes.Ldc_I4_M1);
                    return;
                case 0:
                    il.Emit(OpCodes.Ldc_I4_0);
                    return;
                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    return;
                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    return;
                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    return;
                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    return;
                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    return;
                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    return;
                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    return;
                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    return;
            }

            if (value > -129 && value < 128)
            {
                il.Emit(OpCodes.Ldc_I4_S, (SByte)value);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, value);
            }
        }
    }
    #endregion
}
