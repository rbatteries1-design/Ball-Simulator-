using UnityEditor;
using UnityEngine;

namespace BounceBack
{
    internal static class BounceBackEffects
    {
        [MenuItem("Tools/Bounce Back/Effects/Fire")]
        private static void Fire() => Create("Fire", new Color(1f, .9f, .24f), new Color(1f, .15f, .025f),
            65f, .8f, 2f, .38f, 20f, -0.22f, ParticleSystemRenderMode.Billboard);

        [MenuItem("Tools/Bounce Back/Effects/Air")]
        private static void Air() => Create("Air", new Color(.75f, .95f, 1f), new Color(.35f, .68f, .85f),
            35f, .55f, 7f, .16f, 12f, 0f, ParticleSystemRenderMode.Stretch);

        [MenuItem("Tools/Bounce Back/Effects/Lightning")]
        private static void Lightning() => Create("Lightning", new Color(.86f, .97f, 1f), new Color(.28f, .39f, 1f),
            0f, .18f, 12f, .11f, 42f, 0f, ParticleSystemRenderMode.Stretch);

        private static void Create(string name, Color core, Color edge, float rate, float lifetime,
            float speed, float size, float angle, float gravity, ParticleSystemRenderMode renderMode)
        {
            var selected = Selection.activeTransform;
            Component owner = selected != null ? selected.GetComponentInParent<BallMotor>() : null;
            string effectField = "movementDust";
            if (owner == null && selected != null)
            {
                owner = selected.GetComponentInParent<BouncePad>();
                effectField = "impactEffect";
            }
            if (owner == null && selected != null)
            {
                owner = selected.GetComponentInParent<SpeedRing>();
                effectField = "passEffect";
            }
            bool oneShot = owner != null && effectField != "movementDust";

            var obj = new GameObject(name + " FX");
            Undo.RegisterCreatedObjectUndo(obj, "Create " + name + " FX");
            if (selected != null)
                Undo.SetTransformParent(obj.transform, selected, "Parent Effect");
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            var particles = Undo.AddComponent<ParticleSystem>(obj);
            var main = particles.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * .55f, lifetime);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed * .55f, speed);
            main.startSize = new ParticleSystem.MinMaxCurve(size * .25f, size);
            main.startColor = new ParticleSystem.MinMaxGradient(core, edge);
            main.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            main.gravityModifier = gravity;
            var noise = particles.noise;
            noise.enabled = true;
            noise.strength = gravity < 0f ? 0.45f : 0.2f;
            noise.frequency = 1.8f;
            main.maxParticles = 180;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = particles.emission;
            main.playOnAwake = !oneShot;
            main.loop = !oneShot;
            if (oneShot)
                main.duration = lifetime;
            emission.rateOverTime = oneShot ? 0f : rate;
            if (oneShot)
                emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)16, (short)26) });
            else if (rate == 0f)
                emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)20, (short)32, 8, .24f) });

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = angle;
            shape.radius = .16f;
            shape.rotation = gravity < 0f ? new Vector3(-90f, 0f, 0f) : Vector3.zero;

            var fade = new Gradient();
            fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(.95f, 0f), new GradientAlphaKey(.65f, .4f), new GradientAlphaKey(0f, 1f) });
            var color = particles.colorOverLifetime;
            color.enabled = true;
            color.color = fade;

            var scale = particles.sizeOverLifetime;
            scale.enabled = true;
            scale.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, .12f));

            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = renderMode;
            renderer.lengthScale = name == "Lightning" ? 4f : 2.2f;
            renderer.velocityScale = .12f;
            if (owner != null)
            {
                var settings = new SerializedObject(owner);
                settings.FindProperty(effectField).objectReferenceValue = particles;
                settings.ApplyModifiedProperties();
            }
            Selection.activeGameObject = obj;
        }
    }
}
