using System;
using Unity.Entities;
using UnityEngine;

[AddComponentMenu("")]
[Obsolete("SceneSectionProxy has been deprecated. Please use the new GameObject-to-entity conversion workflows instead. (RemovedAfter 2020-07-03).")]
public class SceneSectionProxy : SharedComponentDataProxy<SceneSection>
{
}
