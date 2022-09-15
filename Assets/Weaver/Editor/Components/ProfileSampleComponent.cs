using System;
using Mono.Cecil;
using UnityEngine;
using Mono.Cecil.Cil;
using Weaver.Extensions;

#if UNITY_2017_1_OR_NEWER

using UnityEngine.Profiling;

#endif

namespace Weaver
{
    public class ProfileSampleComponent : WeaverComponent
    {
        private TypeReference m_ProfilerTypeReference;
        private MethodReference m_BeginSampleWithGameObjectMethodRef;
        private MethodReference m_BeginSampleMethodRef;
        private MethodReference m_EndSampleMethodRef;
        private MethodReference m_GetGameObjectMethodRef;

        public override string ComponentName
        {
            get
            {
                return "Profile Sample";
            }
        }

        public override DefinitionType EffectedDefintions
        {
            get
            {
                return DefinitionType.Module | DefinitionType.Method;
            }
        }

        public override void VisitModule(ModuleDefinition moduleDefinition)
        {
            // Get profiler type
            var profilerType = typeof(Profiler);
            // Import the profiler type
            m_ProfilerTypeReference = moduleDefinition.ImportReference(profilerType);
            // Get the type def by resolving
            var profilerTypeDef = m_ProfilerTypeReference.Resolve();
            // Get our start sample
            m_BeginSampleWithGameObjectMethodRef = profilerTypeDef.GetMethod("BeginSample", 2);
            m_BeginSampleMethodRef = profilerTypeDef.GetMethod("BeginSample", 1);
            // Get our end sample
            m_EndSampleMethodRef = profilerTypeDef.GetMethod("EndSample");
            // Get the type GameObject
            var componentType = typeof(Component);
            // Get Game Object Type R
            var componentTypeRef = moduleDefinition.ImportReference(componentType);
            // Get the type def
            var componentTypeDef = componentTypeRef.Resolve();
            // Get our get property
            var gameObjectPropertyDef = componentTypeDef.GetProperty("gameObject");
            m_GetGameObjectMethodRef = gameObjectPropertyDef.GetMethod;

            // Import everything
            moduleDefinition.ImportReference(typeof(GameObject));
            moduleDefinition.ImportReference(m_BeginSampleMethodRef);
            moduleDefinition.ImportReference(m_GetGameObjectMethodRef);
            moduleDefinition.ImportReference(m_BeginSampleWithGameObjectMethodRef);
        }

        public override void VisitMethod(MethodDefinition methodDefinition)
        {
            var profileSample = methodDefinition.GetCustomAttribute<ProfileSampleAttribute>();

            // Check if we have our attribute
            if (profileSample == null)
            {
                return;
            }

            var body = methodDefinition.Body;
            var bodyProcessor = body.GetILProcessor();

            // Start of method
            {
                var _00 = Instruction.Create(OpCodes.Ldstr, methodDefinition.DeclaringType.Name + ":" + methodDefinition.Name);
                var _01 = Instruction.Create(OpCodes.Ldarg_0);
                var _02 = Instruction.Create(OpCodes.Call, methodDefinition.Module.ImportReference(m_GetGameObjectMethodRef));
                var _03 = Instruction.Create(OpCodes.Call, methodDefinition.Module.ImportReference(m_BeginSampleWithGameObjectMethodRef));

                bodyProcessor.InsertBefore(body.Instructions[0], _00);
                bodyProcessor.InsertAfter(_00, _01);
                bodyProcessor.InsertAfter(_01, _02);
                bodyProcessor.InsertAfter(_02, _03);
            }
            // Loop over all types and insert end sample before return
            for (var i = 0; i < body.Instructions.Count; i++)
            {
                if (body.Instructions[i].OpCode == OpCodes.Ret)
                {
                    var _00 = Instruction.Create(OpCodes.Call, methodDefinition.Module.ImportReference(m_EndSampleMethodRef));
                    bodyProcessor.InsertBefore(body.Instructions[i], _00);
                    i++;
                }
            }

            methodDefinition.CustomAttributes.Remove(profileSample);
        }
    }
}