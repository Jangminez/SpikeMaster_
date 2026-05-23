using UnityEngine;

namespace JangLib
{
    public abstract class BaseObject : MonoBehaviour
    {
        public bool IsInitialized { get; set; }
        public abstract void Init();

        #region << =========== SETTERS =========== >>

        public virtual void SetPosition(Vector3 pos) => transform.position = pos;
        public virtual void SetLocalPosition(Vector3 pos) => transform.localPosition = pos;

        public virtual void SetRotation(Quaternion rot) => transform.rotation = rot;
        public virtual void SetRotation(Vector3 direction) => transform.rotation = Quaternion.LookRotation(direction);
        public virtual void SetEulerAngles(Vector3 euler) => transform.rotation = Quaternion.Euler(euler);

        public virtual void SetLocalRotation(Quaternion rot) => transform.localRotation = rot;
        public virtual void SetLocalRotation(Vector3 direction) => transform.localRotation = Quaternion.LookRotation(direction);
        public virtual void SetLocalEulerAngles(Vector3 euler) => transform.localRotation = Quaternion.Euler(euler);

        public virtual void SetScale(Vector3 scale) => transform.localScale = scale;
        public virtual void SetScale(float uniformScale) => transform.localScale = Vector3.one * uniformScale;

        public virtual void ChangeParent(Transform newParent, bool worldPositionStays = true) => transform.SetParent(newParent, worldPositionStays);

        public void SetAsFirstSibling() => transform.SetAsFirstSibling();
        public void SetAsLastSibling() => transform.SetAsLastSibling();

        #endregion

        #region << =========== GETTERS =========== >>

        public virtual Vector3 GetPosition() => transform.position;
        public virtual Vector3 GetLocalPosition() => transform.localPosition;

        public virtual Quaternion GetRotation() => transform.rotation;
        public virtual Vector3 GetEulerAngles() => transform.rotation.eulerAngles;
        public virtual Vector3 GetDirection() => transform.forward;

        public virtual Quaternion GetLocalRotation() => transform.localRotation;
        public virtual Vector3 GetLocalEulerAngles() => transform.localRotation.eulerAngles;
        public virtual Vector3 GetLocalDirection() => transform.localRotation * Vector3.forward;

        public virtual Vector3 GetScale() => transform.localScale;

        #endregion
    }
}