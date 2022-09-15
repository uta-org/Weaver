using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Diagnostics;
using Weaver.Extensions;
using Debug = UnityEngine.Debug;

namespace Weaver
{
    public class MethodTimerComponent : WeaverComponent
    {
        public struct StopwatchDefinition
        {
            public MethodReference consturctor;
            public MethodReference start;
            public MethodReference stop;
            public MethodReference getElapsedMilliseconds;

            public StopwatchDefinition(TypeDefinition stopwatchTypeDef, ModuleDefinition module)
            {
                consturctor = module.ImportReference(stopwatchTypeDef.GetMethod(".ctor"));
                start = module.ImportReference(stopwatchTypeDef.GetMethod("Start"));
                stop = module.ImportReference(stopwatchTypeDef.GetMethod("Stop"));
                getElapsedMilliseconds = module.ImportReference(stopwatchTypeDef.GetProperty("ElapsedMilliseconds").GetMethod);
            }
        }

        private StopwatchDefinition m_StopWatchTypeDef;
        private MethodReference m_StringConcatMethodRef;
        private MethodReference m_DebugLogMethodRef;
        private TypeReference m_StopwatchTypeReference;

        public override string ComponentName => "Method Timer";

        public override DefinitionType EffectedDefintions => DefinitionType.Module | DefinitionType.Method;

        public override void VisitModule(ModuleDefinition moduleDefinition)
        {
            // Import our stopwatch type reference
            m_StopwatchTypeReference = moduleDefinition.ImportReference(typeof(Stopwatch));
            // Resolve it so we can get the type definition
            var stopwatchTypeDef = m_StopwatchTypeReference.Resolve();
            // Create our value holder
            m_StopWatchTypeDef = new StopwatchDefinition(stopwatchTypeDef, moduleDefinition);
            // String
            var stringTypeDef = typeSystem.String.Resolve();
            m_StringConcatMethodRef = moduleDefinition.ImportReference(stringTypeDef.GetMethod("Concat", 2));

            var debugTypeRef = moduleDefinition.ImportReference(typeof(Debug));
            var debugTypeDeff = debugTypeRef.Resolve();
            m_DebugLogMethodRef = moduleDefinition.ImportReference(debugTypeDeff.GetMethod("Log", 1));
        }

        public override void VisitMethod(MethodDefinition methodDefinition)
        {
            // Check if we have our attribute
            var customAttribute = methodDefinition.GetCustomAttribute<MethodTimerAttribute>();
            if (customAttribute == null)
            {
                return;
            }

            // Remove the attribute
            methodDefinition.CustomAttributes.Remove(customAttribute);

            var body = methodDefinition.Body;
            var bodyProcessor = body.GetILProcessor();

            var stopwatchVariable = new VariableDefinition(m_StopwatchTypeReference);
            var elapsedMilliseconds = new VariableDefinition(typeSystem.Int64);
            body.Variables.Add(stopwatchVariable);
            body.Variables.Add(elapsedMilliseconds);
            // Inject at the start of the function
            {
                var _00 = Instruction.Create(OpCodes.Newobj, m_StopWatchTypeDef.consturctor);
                var _01 = Instruction.Create(OpCodes.Stloc, stopwatchVariable);
                var _02 = Instruction.Create(OpCodes.Ldloc, stopwatchVariable);
                var _03 = Instruction.Create(OpCodes.Callvirt, m_StopWatchTypeDef.start);

                bodyProcessor.InsertBefore(body.Instructions[0], _00);
                bodyProcessor.InsertAfter(_00, _01);
                bodyProcessor.InsertAfter(_01, _02);
                bodyProcessor.InsertAfter(_02, _03);
            }

            // [Normal part of function]

            // Inject at the end
            {
                // Calls a late - bound method on an object, pushing the return value onto the evaluation stack
                var _00 = Instruction.Create(OpCodes.Ldloc, stopwatchVariable);
                // Calls a late - bound method on an object, pushing the return value onto the evaluation stack
                var _01 = Instruction.Create(OpCodes.Callvirt, m_StopWatchTypeDef.stop);
                // Pushes the integer value of 0 onto the evaluation stack as an int32.
                var _02 = Instruction.Create(OpCodes.Ldc_I4_0);
                // Converts the value on top of the evaluation stack to int64.
                var _03 = Instruction.Create(OpCodes.Conv_I8);
                // Pops the current value from the top of the evaluation stack and stores it in a the local variable list at index 1.
                var _04 = Instruction.Create(OpCodes.Stloc, elapsedMilliseconds);
                // Loads the local variable at index 0 onto the evaluation stack.
                var _05 = Instruction.Create(OpCodes.Ldloc, stopwatchVariable);
                // Calls a late - bound method on an object, pushing the return value onto the evaluation stack. Using the get method
                var _06 = Instruction.Create(OpCodes.Callvirt, m_StopWatchTypeDef.getElapsedMilliseconds);
                // Pops the current value from the top of the evaluation stack and stores it in a the local variable list at index 1.
                var _07 = Instruction.Create(OpCodes.Stloc, elapsedMilliseconds);
                // Pushes a new object reference to a string literal stored in the metadata.
                var _08 = Instruction.Create(OpCodes.Ldstr, methodDefinition.Name);
                // Loads the local variable at index 1 onto the evaluation stack.
                var _09 = Instruction.Create(OpCodes.Ldloc, elapsedMilliseconds);
                // Converts a value type to an object reference (type O).
                var _10 = Instruction.Create(OpCodes.Box, typeSystem.Int64);
                // Calls the method indicated by the passed method descriptor.
                var _11 = Instruction.Create(OpCodes.Call, m_StringConcatMethodRef);
                // Calls the method indicated by the passed method descriptor.
                var _12 = Instruction.Create(OpCodes.Call, m_DebugLogMethodRef);
                // Returns from the current method, pushing a return value (if present) from the callee's evaluation stack onto the caller's evaluation stack.
                var _13 = Instruction.Create(OpCodes.Ret);

                bodyProcessor.InsertBefore(body.Instructions[body.Instructions.Count - 1], _00);
                bodyProcessor.InsertAfter(_00, _01);
                bodyProcessor.InsertAfter(_01, _02);
                bodyProcessor.InsertAfter(_02, _03);
                bodyProcessor.InsertAfter(_03, _04);
                bodyProcessor.InsertAfter(_04, _05);
                bodyProcessor.InsertAfter(_05, _06);
                bodyProcessor.InsertAfter(_06, _07);
                bodyProcessor.InsertAfter(_07, _08);
                bodyProcessor.InsertAfter(_08, _09);
                bodyProcessor.InsertAfter(_09, _10);
                bodyProcessor.InsertAfter(_10, _11);
                bodyProcessor.InsertAfter(_11, _12);
                bodyProcessor.InsertAfter(_12, _13);
            }
        }
    }
}