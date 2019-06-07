using BeetleX.FastHttpApi.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


namespace BeetleX.FastHttpApi
{
    public class ActionHandler : IComparable
    {

        private static int mIdSeed = 0;

        public ActionHandler(object controller, System.Reflection.MethodInfo method, HttpApiServer httpApiServer)
        {

            ID = System.Threading.Interlocked.Increment(ref mIdSeed);
            Remark = "";
            Parameters = new List<ParameterBinder>();
            mMethod = method;
            mMethodHandler = new MethodHandler(mMethod);
            Controller = controller;
            HttpApiServer = httpApiServer;
            LoadParameter();
            Filters = new List<FilterAttribute>();
            Method = "GET";
            SingleInstance = true;
            ControllerType = Controller.GetType();
            NoConvert = false;
            var aname = controller.GetType().Assembly.GetName();
            this.AssmblyName = aname.Name;
            this.Version = aname.Version.ToString();
            Async = false;

        }

        public HttpApiServer HttpApiServer { get; private set; }

        public string Path { get; set; }

        public string Url { get; set; }

        public int ID { get; set; }

        public string Version { get; set; }

        public string AssmblyName { get; set; }

        public bool NoConvert { get; set; }

        public bool Async { get; set; }

        internal PropertyHandler PropertyHandler { get; set; }

        public Type ControllerType { get; set; }

        public bool SingleInstance { get; set; }

        public RouteTemplateAttribute Route { get; set; }

        public DataConvertAttribute DataConvert { get; set; }

        public OptionsAttribute OptionsAttribute { get; set; }

        public string Method { get; set; }

        public string Remark { get; set; }

        public List<FilterAttribute> Filters { get; set; }

        private MethodHandler mMethodHandler;

        private long mErrors;

        public long Errors => mErrors;

        public long LastErrors { get; set; }

        private long mRequests;

        public long LastRequests { get; set; }

        public long Requests => mRequests;

        public int MaxRPS { get; set; }

        private int mRPS;

        private long mLastTime;

        public bool ValidateRPS()
        {
            if (MaxRPS == 0)
                return true;
            long now = TimeWatch.GetElapsedMilliseconds();
            if (now - mLastTime >= 1000)
                return true;
            return mRPS < MaxRPS;
        }

        public void IncrementError()
        {
            System.Threading.Interlocked.Increment(ref mErrors);
        }

        public void IncrementRequest()
        {
            System.Threading.Interlocked.Increment(ref mRequests);
            if (MaxRPS > 0)
            {
                long now = TimeWatch.GetElapsedMilliseconds();
                if (now - mLastTime >= 1000)
                {
                    mLastTime = now;
                    System.Threading.Interlocked.Exchange(ref mRPS, 1);
                }
                else
                {
                    System.Threading.Interlocked.Increment(ref mRPS);
                }
            }
        }

        private MethodInfo mMethod;

        public MethodInfo MethodInfo => mMethod;

        public object Controller { get; set; }

        public string SourceUrl { get; set; }

        public bool HasValidation { get; private set; } = false;

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
                else if (pi.ParameterType == typeof(Boolean))
                {
                    pb = new BooleanParameter();
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
                else if (pi.ParameterType == typeof(PostFile))
                {
                    pb = new PostFileParameter();
                }
                else
                {
                    pb = HttpApiServer.ActionFactory.GetParameterBinder(pi.ParameterType);
                    if (pb == null)
                    {
                        if (HttpApiServer.ActionFactory.HasParameterBindEvent)
                        {
                            pb = new ParamterEventBinder(HttpApiServer.ActionFactory);

                        }
                        else
                        {
                            pb = new DefaultParameter();
                        }
                    }
                }
                pb.ActionHandler = this;
                pb.ParameterInfo = pi;
                pb.Name = pi.Name;
                pb.Type = pi.ParameterType;
                pb.Validations = pi.GetCustomAttributes<Validations.ValidationBase>(false).ToArray();
                if (!HasValidation)
                    HasValidation = pb.Validations != null && pb.Validations.Length > 0;
                pb.CacheKey = pi.GetCustomAttribute<CacheKeyParameter>(false);
                Parameters.Add(pb);
            }
        }

        public List<ParameterBinder> Parameters { get; private set; }

        public object[] GetParameters(IHttpContext context)
        {
            int count = this.Parameters.Count;
            object[] parameters = new object[count];
            for (int i = 0; i < count; i++)
            {
                try
                {
                    parameters[i] = Parameters[i].GetValue(context);
                }
                catch (Exception e_)
                {
                    throw new BXException($"{SourceUrl} bind {Parameters[i].Name} parameter error {e_.Message}", e_);
                }

            }
            return parameters;

        }

        public object Invoke(object controller, IHttpContext context, ActionHandlerFactory actionHandlerFactory, object[] parameters)
        {
            return mMethodHandler.Execute(controller, parameters);
        }

        public int CompareTo(object obj)
        {
            return this.SourceUrl.CompareTo(((ActionHandler)obj).SourceUrl);
        }

        public string GetCackeKey(object[] parameters)
        {
            StringBuilder key = new StringBuilder();
            key.Append(Url);
            for (int i = 0; i < Parameters.Count; i++)
            {
                var dp = Parameters[i];
                if (dp.DataParameter && dp.CacheKey != null)
                {
                    key.Append("_").Append(dp.CacheKey.GetValue(parameters[i]));
                }
            }
            return key.ToString();
        }

        public bool ValidateParamters(object[] parameters, out (Validations.ValidationBase, ParameterInfo) error)
        {
            error = (null, null);
            for (int i = 0; i < Parameters.Count; i++)
            {
                var dp = Parameters[i];
                Validations.ValidationBase[] vs = dp.Validations;
                if (vs != null && vs.Length > 0)
                {
                    for (int k = 0; k < vs.Length; k++)
                    {
                        if (!vs[k].Execute(parameters[i]))
                        {
                            error = (vs[k], dp.ParameterInfo);
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }


    public class ParamterEventBinder : ParameterBinder
    {
        public ParamterEventBinder(ActionHandlerFactory actionHandlerFactory)
        {
            ActionHandlerFactory = actionHandlerFactory;
        }

        public override bool DataParameter => false;

        public ActionHandlerFactory ActionHandlerFactory { get; internal set; }

        public override object GetValue(IHttpContext context)
        {
            EventParameterBinding e = new EventParameterBinding();
            e.ActionHandler = this.ActionHandler;
            e.Context = context;
            e.Type = Type;
            ActionHandlerFactory.OnParameterBinding(e);
            return e.Parameter;
        }
    }



    [AttributeUsage(AttributeTargets.Class)]
    public class PMapper : Attribute
    {
        public PMapper(Type type)
        {
            ParameterType = type;
        }
        public Type ParameterType { get; set; }
    }

    public interface IParameterBinder
    {
        string Name { get; }

        object GetValue(IHttpContext context);
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public abstract class ParameterBinder : Attribute, IParameterBinder
    {
        public Type Type { get; internal set; }

        public System.Reflection.ParameterInfo ParameterInfo { get; internal set; }

        public ActionHandler ActionHandler { get; internal set; }

        public string Name { get; internal set; }

        public virtual bool DataParameter => true;

        public Validations.ValidationBase[] Validations { get; set; }

        public abstract object GetValue(IHttpContext context);

        public CacheKeyParameter CacheKey { get; set; }

        public virtual object DefaultValue()
        {
            return null;
        }

    }

    class BooleanParameter : ParameterBinder
    {
        public override object DefaultValue()
        {
            return true;
        }

        public override object GetValue(IHttpContext context)
        {
            bool result;
            context.Data.TryGetBoolean(this.Name, out result);
            return result;
        }
    }

    class IntParameter : ParameterBinder
    {
        public override object DefaultValue()
        {
            return 0;
        }

        public override object GetValue(IHttpContext context)
        {
            int result;
            context.Data.TryGetInt(this.Name, out result);
            return result;
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
        public override object DefaultValue()
        {
            return 0;
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
        public override object DefaultValue()
        {
            return 0;
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
        public override object DefaultValue()
        {
            return 0;
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
        public override object DefaultValue()
        {
            return 0;
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
        public override object DefaultValue()
        {
            return 0;
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
        public override object DefaultValue()
        {
            return 0.0f;
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
        public override object DefaultValue()
        {
            return 0.0d;
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
        public override object DefaultValue()
        {
            return "";
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
        public override object DefaultValue()
        {
            return 0;
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
        public override object DefaultValue()
        {
            return DateTime.Now;
        }

    }


    class RequestParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            return context.Request;
        }

        public override bool DataParameter => false;
        public override object DefaultValue()
        {
            return new object();
        }

    }


    class HttpApiServerParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            return context.Server;
        }

        public override bool DataParameter => false;
        public override object DefaultValue()
        {
            return new object();
        }

    }


    class ResponseParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            return context.Response;
        }

        public override bool DataParameter => false;
        public override object DefaultValue()
        {
            return new object();
        }

    }



    class HttpContextParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            return context;
        }

        public override bool DataParameter => false;
        public override object DefaultValue()
        {
            return new object();
        }

    }

    class DataContextParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            return context.Data;
        }
        public override bool DataParameter => false;
        public override object DefaultValue()
        {
            return new object();
        }

    }

    class PostFileParameter : ParameterBinder
    {
        public override bool DataParameter => true;

        public override object GetValue(IHttpContext context)
        {
            return context.Data.GetObject(this.Name, this.Type);
        }
        public override object DefaultValue()
        {
            return new object();
        }
    }

    class DefaultParameter : ParameterBinder
    {
        public override object GetValue(IHttpContext context)
        {
            return context.Data.GetObject(this.Name, this.Type);
        }
        public override object DefaultValue()
        {
            return new object();
        }

    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class CacheKeyParameter : Attribute
    {
        public virtual string GetValue(object data)
        {
            return $"{data}";
        }
    }

    #region emithandler
    class FieldHandler
    {
        public FieldHandler(FieldInfo field)
        {
            mGetValue = ReflectionHandlerFactory.FieldGetHandler(field);
            mSetValue = ReflectionHandlerFactory.FieldSetHandler(field);
            Field = field;
        }
        private FieldInfo mField;
        public FieldInfo Field
        {
            get
            {
                return mField;
            }
            private set
            {
                mField = value;
            }
        }
        private GetValueHandler mGetValue;
        public GetValueHandler GetValue
        {
            get
            {
                return mGetValue;
            }

        }
        private SetValueHandler mSetValue;
        public SetValueHandler SetValue
        {
            get
            {
                return mSetValue;
            }

        }
    }
    class PropertyHandler
    {
        public PropertyHandler(PropertyInfo property)
        {
            if (property.CanWrite)
                mSetValue = ReflectionHandlerFactory.PropertySetHandler(property);
            if (property.CanRead)
                mGetValue = ReflectionHandlerFactory.PropertyGetHandler(property);
            mProperty = property;
            IndexProperty = mProperty.GetGetMethod().GetParameters().Length > 0;
        }
        private bool mIndexProperty;
        public bool IndexProperty
        {
            get
            {
                return mIndexProperty;
            }
            set
            {
                mIndexProperty = value;
            }
        }
        private PropertyInfo mProperty;
        public PropertyInfo Property
        {
            get
            {
                return mProperty;
            }
            set
            {
                mProperty = value;
            }
        }
        private GetValueHandler mGetValue;
        public GetValueHandler Get
        {
            get
            {
                return mGetValue;
            }

        }
        private SetValueHandler mSetValue;
        public SetValueHandler Set
        {
            get
            {
                return mSetValue;
            }

        }
    }
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
    class InstanceHandler
    {
        public InstanceHandler(Type type)
        {
            mInstance = ReflectionHandlerFactory.InstanceHandler(type);
        }
        private ObjectInstanceHandler mInstance;
        public ObjectInstanceHandler Instance
        {
            get
            {
                return mInstance;
            }
        }
    }
    delegate object GetValueHandler(object source);
    delegate object ObjectInstanceHandler();
    delegate void SetValueHandler(object source, object value);
    delegate object FastMethodHandler(object target, object[] paramters);
    class ReflectionHandlerFactory
    {


        #region field handler

        private static Dictionary<FieldInfo, GetValueHandler> mFieldGetHandlers = new Dictionary<FieldInfo, GetValueHandler>();
        private static Dictionary<FieldInfo, SetValueHandler> mFieldSetHandlers = new Dictionary<FieldInfo, SetValueHandler>();
        public static GetValueHandler FieldGetHandler(FieldInfo field)
        {
            GetValueHandler handler;
            if (mFieldGetHandlers.ContainsKey(field))
            {
                handler = mFieldGetHandlers[field];
            }
            else
            {
                lock (typeof(ReflectionHandlerFactory))
                {
                    if (mFieldGetHandlers.ContainsKey(field))
                    {
                        handler = mFieldGetHandlers[field];
                    }
                    else
                    {
                        handler = CreateFieldGetHandler(field);
                        mFieldGetHandlers.Add(field, handler);
                    }

                }
            }
            return handler;
        }
        private static GetValueHandler CreateFieldGetHandler(FieldInfo field)
        {
            DynamicMethod dm = new DynamicMethod("", typeof(object), new Type[] { typeof(object) }, field.DeclaringType);
            ILGenerator ilGenerator = dm.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, field);
            EmitBoxIfNeeded(ilGenerator, field.FieldType);
            ilGenerator.Emit(OpCodes.Ret);
            return (GetValueHandler)dm.CreateDelegate(typeof(GetValueHandler));
        }
        public static SetValueHandler FieldSetHandler(FieldInfo field)
        {
            SetValueHandler handler;
            if (mFieldSetHandlers.ContainsKey(field))
            {
                handler = mFieldSetHandlers[field];
            }
            else
            {
                lock (typeof(ReflectionHandlerFactory))
                {
                    if (mFieldSetHandlers.ContainsKey(field))
                    {
                        handler = mFieldSetHandlers[field];
                    }
                    else
                    {
                        handler = CreateFieldSetHandler(field);
                        mFieldSetHandlers.Add(field, handler);
                    }
                }
            }
            return handler;
        }
        private static SetValueHandler CreateFieldSetHandler(FieldInfo field)
        {
            DynamicMethod dm = new DynamicMethod("", null, new Type[] { typeof(object), typeof(object) }, field.DeclaringType);
            ILGenerator ilGenerator = dm.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            EmitCastToReference(ilGenerator, field.FieldType);
            ilGenerator.Emit(OpCodes.Stfld, field);
            ilGenerator.Emit(OpCodes.Ret);
            return (SetValueHandler)dm.CreateDelegate(typeof(SetValueHandler));
        }

        #endregion

        #region Property Handler

        private static Dictionary<PropertyInfo, GetValueHandler> mPropertyGetHandlers = new Dictionary<PropertyInfo, GetValueHandler>();
        private static Dictionary<PropertyInfo, SetValueHandler> mPropertySetHandlers = new Dictionary<PropertyInfo, SetValueHandler>();
        public static SetValueHandler PropertySetHandler(PropertyInfo property)
        {
            SetValueHandler handler;
            if (mPropertySetHandlers.ContainsKey(property))
            {
                handler = mPropertySetHandlers[property];
            }
            else
            {
                lock (typeof(ReflectionHandlerFactory))
                {
                    if (mPropertySetHandlers.ContainsKey(property))
                    {
                        handler = mPropertySetHandlers[property];
                    }
                    else
                    {
                        handler = CreatePropertySetHandler(property);
                        mPropertySetHandlers.Add(property, handler);
                    }
                }
            }
            return handler;
        }
        private static SetValueHandler CreatePropertySetHandler(PropertyInfo property)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, null, new Type[] { typeof(object), typeof(object) }, property.DeclaringType.Module);

            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();


            ilGenerator.Emit(OpCodes.Ldarg_0);


            ilGenerator.Emit(OpCodes.Ldarg_1);


            EmitCastToReference(ilGenerator, property.PropertyType);


            ilGenerator.EmitCall(OpCodes.Callvirt, property.GetSetMethod(), null);


            ilGenerator.Emit(OpCodes.Ret);


            SetValueHandler setter = (SetValueHandler)dynamicMethod.CreateDelegate(typeof(SetValueHandler));

            return setter;
        }
        public static GetValueHandler PropertyGetHandler(PropertyInfo property)
        {
            GetValueHandler handler;
            if (mPropertyGetHandlers.ContainsKey(property))
            {
                handler = mPropertyGetHandlers[property];
            }
            else
            {
                lock (typeof(ReflectionHandlerFactory))
                {
                    if (mPropertyGetHandlers.ContainsKey(property))
                    {
                        handler = mPropertyGetHandlers[property];
                    }
                    else
                    {
                        handler = CreatePropertyGetHandler(property);
                        mPropertyGetHandlers.Add(property, handler);
                    }
                }
            }
            return handler;
        }
        private static GetValueHandler CreatePropertyGetHandler(PropertyInfo property)
        {

            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, typeof(object), new Type[] { typeof(object) }, property.DeclaringType.Module);

            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();


            ilGenerator.Emit(OpCodes.Ldarg_0);


            ilGenerator.EmitCall(OpCodes.Callvirt, property.GetGetMethod(), null);


            EmitBoxIfNeeded(ilGenerator, property.PropertyType);


            ilGenerator.Emit(OpCodes.Ret);


            GetValueHandler getter = (GetValueHandler)dynamicMethod.CreateDelegate(typeof(GetValueHandler));

            return getter;
        }
        #endregion

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
            ParameterInfo[] ps = methodInfo.GetParameters();
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

        #region Instance Handler

        private static Dictionary<Type, ObjectInstanceHandler> mInstanceHandlers = new Dictionary<Type, ObjectInstanceHandler>();
        public static ObjectInstanceHandler InstanceHandler(Type type)
        {
            ObjectInstanceHandler handler;
            if (mInstanceHandlers.ContainsKey(type))
            {
                handler = mInstanceHandlers[type];
            }
            else
            {
                lock (typeof(ReflectionHandlerFactory))
                {
                    if (mInstanceHandlers.ContainsKey(type))
                    {
                        handler = mInstanceHandlers[type];
                    }
                    else
                    {
                        handler = CreateInstanceHandler(type);
                        mInstanceHandlers.Add(type, handler);
                    }
                }
            }
            return handler;
        }
        private static ObjectInstanceHandler CreateInstanceHandler(Type type)
        {
            DynamicMethod method = new DynamicMethod(string.Empty, type, null, type.Module);
            ILGenerator il = method.GetILGenerator();
            il.DeclareLocal(type, true);
            il.Emit(OpCodes.Newobj, type.GetConstructor(new Type[0]));
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);
            ObjectInstanceHandler creater = (ObjectInstanceHandler)method.CreateDelegate(typeof(ObjectInstanceHandler));
            return creater;

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
