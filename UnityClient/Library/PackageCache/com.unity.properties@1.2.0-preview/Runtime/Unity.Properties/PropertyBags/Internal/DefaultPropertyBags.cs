#if !UNITY_DOTSPLAYER
using Unity.Properties.Internal;
using UnityEngine;

namespace Unity.Properties
{
    static class DefaultPropertyBagInitializer
    {
        [RuntimeInitializeOnLoadMethod]
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        static void Initialize()
        {
            PropertyBagStore.AddPropertyBag(new AnimationCurvePropertyBag());
            PropertyBagStore.AddPropertyBag(new KeyFramePropertyBag());
        }
    }
    
    class AnimationCurvePropertyBag : ContainerPropertyBag<AnimationCurve>
    {
        public AnimationCurvePropertyBag()
        {
            PropertyBag.RegisterList<AnimationCurve, Keyframe[], Keyframe>();
            
            AddProperty(new PreWrapModeProperty());
            AddProperty(new PostWrapModeProperty());
            AddProperty(new KeysProperty());
        }

        class PreWrapModeProperty : Property<AnimationCurve, WrapMode>
        {
            public override string Name => nameof(AnimationCurve.preWrapMode);
            public override bool IsReadOnly => false;
            public override WrapMode GetValue(ref AnimationCurve container) => container.preWrapMode;
            public override void SetValue(ref AnimationCurve container, WrapMode value) => container.preWrapMode = value;
        }

        class PostWrapModeProperty : Property<AnimationCurve, WrapMode>
        {
            public override string Name => nameof(AnimationCurve.postWrapMode);
            public override bool IsReadOnly => false;
            public override WrapMode GetValue(ref AnimationCurve container) => container.postWrapMode;
            public override void SetValue(ref AnimationCurve container, WrapMode value) => container.postWrapMode = value;
        }

        class KeysProperty : Property<AnimationCurve, Keyframe[]>
        {
            public override string Name => nameof(AnimationCurve.keys);
            public override bool IsReadOnly => false;
            public override Keyframe[] GetValue(ref AnimationCurve container) => container.keys;
            public override void SetValue(ref AnimationCurve container, Keyframe[] value) => container.keys = value;
        }
    }
    
    class KeyFramePropertyBag : ContainerPropertyBag<Keyframe>
    {
        public KeyFramePropertyBag()
        {
            AddProperty(new TimeProperty());
            AddProperty(new ValueProperty());
            AddProperty(new InTangentProperty());
            AddProperty(new OutTangentProperty());
            AddProperty(new InWeightProperty());
            AddProperty(new OutWeightProperty());
            AddProperty(new WeightedModeProperty());
        }

        class TimeProperty : Property<Keyframe, float>
        {
            public override string Name => nameof(Keyframe.time);
            public override bool IsReadOnly => false;
            public override float GetValue(ref Keyframe container) => container.time;
            public override void SetValue(ref Keyframe container, float value) => container.time = value;
        }

        class ValueProperty : Property<Keyframe, float>
        {
            public override string Name => nameof(Keyframe.value);
            public override bool IsReadOnly => false;
            public override float GetValue(ref Keyframe container) => container.value;
            public override void SetValue(ref Keyframe container, float value) => container.value = value;
        }

        class InTangentProperty : Property<Keyframe, float>
        {
            public override string Name => nameof(Keyframe.inTangent);
            public override bool IsReadOnly => false;
            public override float GetValue(ref Keyframe container) => container.inTangent;
            public override void SetValue(ref Keyframe container, float value) => container.inTangent = value;
        }

        class OutTangentProperty : Property<Keyframe, float>
        {
            public override string Name => nameof(Keyframe.outTangent);
            public override bool IsReadOnly => false;
            public override float GetValue(ref Keyframe container) => container.outTangent;
            public override void SetValue(ref Keyframe container, float value) => container.outTangent = value;
        }

        class InWeightProperty : Property<Keyframe, float>
        {
            public override string Name => nameof(Keyframe.inWeight);
            public override bool IsReadOnly => false;
            public override float GetValue(ref Keyframe container) => container.inWeight;
            public override void SetValue(ref Keyframe container, float value) => container.inWeight = value;
        }

        class OutWeightProperty : Property<Keyframe, float>
        {
            public override string Name => nameof(Keyframe.outWeight);
            public override bool IsReadOnly => false;
            public override float GetValue(ref Keyframe container) => container.outWeight;
            public override void SetValue(ref Keyframe container, float value) => container.outWeight = value;
        }

        class WeightedModeProperty : Property<Keyframe, WeightedMode>
        {
            public override string Name => nameof(Keyframe.weightedMode);
            public override bool IsReadOnly => false;
            public override WeightedMode GetValue(ref Keyframe container) => container.weightedMode;
            public override void SetValue(ref Keyframe container, WeightedMode value) => container.weightedMode = value;
        }
    }
}
#endif