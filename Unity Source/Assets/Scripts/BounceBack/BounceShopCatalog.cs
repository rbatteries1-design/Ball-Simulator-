using UnityEngine;

namespace BounceBack
{
    [CreateAssetMenu(menuName = "Bounce Back/Shop Catalog")]
    public sealed class BounceShopCatalog : ScriptableObject
    {
        public Material[] skins = new Material[8];
        public ParticleSystem[] trails = new ParticleSystem[8];
    }
}
