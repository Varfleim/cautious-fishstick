
using Leopotam.EcsLite;

namespace SandOcean.UI.Events
{
    public enum ObjectNewCreatedType : byte
    {
        None,
        Organization,
        EcORAEO,
        Fleet,
        TaskForce
    }

    public struct EObjectNewCreated
    {
        public EObjectNewCreated(
            EcsPackedEntity objectPE, ObjectNewCreatedType objectNewCreatedType)
        {
            this.objectPE = objectPE;
            this.objectNewCreatedType = objectNewCreatedType;
        }

        public readonly EcsPackedEntity objectPE;
        public readonly ObjectNewCreatedType objectNewCreatedType;
    }
}