using System;
using Unity.Collections;
using Unity.Jobs;

namespace Unity.Serialization.Json
{
    /// <summary>
    /// The intent for this validator is to test for the SJSON format. For the time being, it is a pass-through.
    /// Structural validation is still performed by the tokenizer.
    /// </summary>
    struct JsonSimpleValidator : IDisposable
    {
        public JsonSimpleValidator(Allocator label = SerializationConfiguration.DefaultAllocatorLabel)
        {
            
        }

        public void Reset()
        {
            
        }

        public JsonValidationResult GetResult()
        {
            return default;
        }

        public JobHandle ScheduleValidation(UnsafeBuffer<char> buffer, int start, int count, JobHandle dependsOn = default)
        {
            return dependsOn;
        }

        public void Dispose()
        {
            
        }
    }
}