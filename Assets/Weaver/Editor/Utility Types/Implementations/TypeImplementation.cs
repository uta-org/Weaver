using Mono.Cecil;
using System;
using Weaver.Extensions;

namespace Weaver
{
    public struct TypeImplementation
    {
        public TypeReference reference;
        public TypeDefinition definition;
        private readonly ModuleDefinition m_Module;

        public TypeImplementation(ModuleDefinition module, Type type)
        {
            m_Module = module;
            reference = m_Module.ImportReference(type);
            definition = reference.Resolve();
        }

        public MethodImplementation GetConstructor()
        {
            return GetMethod(".ctor");
        }

        public MethodImplementation GetConstructor(params Type[] parameterTypes)
        {
            return GetMethod(".ctor", parameterTypes);
        }

        public MethodImplementation GetMethod(string methodName)
        {
            var methodDefinition = definition.GetMethod(methodName);
            var methodImplementation = new MethodImplementation(m_Module, methodDefinition);
            return methodImplementation;
        }

        public MethodImplementation GetMethod(string methodName, params Type[] parameterTypes)
        {
            var methodDefinition = definition.GetMethod(methodName, parameterTypes);
            var methodImplementation = new MethodImplementation(m_Module, methodDefinition);
            return methodImplementation;
        }

        public PropertyImplementation GetProperty(string methodName)
        {
            var propertyDefinition = definition.GetProperty(methodName);
            var methodImplementation = new PropertyImplementation(m_Module, propertyDefinition);
            return methodImplementation;
        }
    }
}