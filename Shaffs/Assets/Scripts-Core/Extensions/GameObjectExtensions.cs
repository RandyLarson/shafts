using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Extensions
{
    public static class GameObjectExtensions
    {
        public static Vector3 GetPosition(this GameObject src)
        {
            if (src != null && src.transform != null)
                return src.transform.position;
            else
                return Vector3.zero;
        }

        public static bool GetComponent<S, T>(this S src, out T foundComponent)
            where S : Component
            where T : class
        {
            foundComponent = null;

            if (src == null)
                return false;

            foundComponent = src.GetComponent<T>();
            return foundComponent != null;
        }

        public static bool GetComponent<T>(this GameObject src, out T foundComponent) where T : class
        {
            foundComponent = null;

            if (src == null)
                return false;

            foundComponent = src.GetComponent<T>();
            return foundComponent != null;
        }

        public static bool GetComponentInChildren<T>(this GameObject src, out T foundComponent) where T : class
        {
            foundComponent = null;

            if (src == null)
                return false;

            foundComponent = src.GetComponentInChildren<T>();
            return foundComponent != null;
        }

        public static bool GetComponents<T>(this GameObject src, out IEnumerable<T> foundComponents) where T : class
        {
            foundComponents = null;

            if (src == null)
                return false;

            foundComponents = src.GetComponents<T>();
            return foundComponents != null;
        }


        public static bool GetInterface<T>(this GameObject src, out T wishedFace) where T : class
        {
            wishedFace = null;
            if (src == null)
                return false;

            wishedFace = src.GetComponent(typeof(T)) as T;
            return wishedFace != null;
        }


        public static bool GetInterfaceInChildren<T>(this GameObject src, out T wishedFace) where T : class
        {
            wishedFace = null;
            if (src == null)
                return false;

            wishedFace = src.GetComponentInChildren(typeof(T)) as T;
            return wishedFace != null;
        }

        public static T GetInterface<T>(this GameObject src) where T : class
        {
            if (src == null)
                return null;

            T wishedFace = src.GetComponent(typeof(T)) as T;
            return wishedFace;
        }

        public static IEnumerable<T> GetInterfaces<T>(this GameObject src) where T : class
        {
            if (src == null)
                return new T[0];

            var matchingComponents = src.GetComponents(typeof(T));
            var asFaces = matchingComponents
                .OfType<T>()
                .ToArray();

            return asFaces;
        }

        public static IEnumerable<T> GetInterfacesInChildren<T>(this GameObject src) where T : class
        {
            if (src == null)
                return new T[0];

            var matchingComponents = src.GetComponentsInChildren(typeof(T));
            var asFaces = matchingComponents
                .Where(cmp => cmp != src)
                .OfType<T>()
                .ToArray();

            return asFaces;
        }

        public static void SafeSetActive<T>(this T[] target, bool value) where T : MonoBehaviour
        {
            if (target == null)
                return;

            for (int i = 0; i < target.Length; i++)
            {
                target[i].gameObject.SetActive(value);
            }
        }

        public static void SafeSetActive(this GameObject[] target, bool value)
        {
            if (target == null)
                return;

            for (int i = 0; i < target.Length; i++)
            {
                target[i].SetActive(value);
            }
        }

        public static void SafeSetActive<T>(this T target, bool value) where T : MonoBehaviour
        {
            if (target == null || target.gameObject == null)
                return;

            target.gameObject.SetActive(value);
        }

        public static void SafeDestroy(this GameObject toDestroy)
        {
            if (toDestroy == null)
                return;

            GameObject.Destroy(toDestroy);
        }

        public static void SafeDestroy<T>(this T toDestroy) where T : MonoBehaviour
        {
            if (toDestroy == null || toDestroy.gameObject == null)
                return;

            GameObject.Destroy(toDestroy.gameObject);
        }

        public static void SafeSetActive(this GameObject target, bool value)
        {
            if (target == null)
                return;

            target.SetActive(value);
        }

        public static bool SafeIsActive<T>(this GameObject target)
        {
            return (target != null && target.gameObject.activeSelf);
        }

        public static bool IsValidGameobject(this MonoBehaviour target)
        {
            return target != null && target.gameObject != null;
        }

        public static bool SafeInstantiate<T>(this GameObject toInstantiate, Vector2 position, out T instantiated, float? autoDestructIn = null, Quaternion? rotation = null) where T : class
        {
            instantiated = null;

            if (toInstantiate == null)
                return false;

            var visual = GameObject.Instantiate(toInstantiate, position, rotation != null ? rotation.Value : toInstantiate.transform.rotation);
            if (autoDestructIn != null && autoDestructIn.Value > 0)
                GameObject.Destroy(visual, autoDestructIn.Value);

            if (typeof(T) == typeof(GameObject))
                instantiated = visual as T;
            else 
                instantiated = visual.GetComponent<T>();
        
            return true;
        }

        public static bool CallGameObject<T>(this T target, Action theCall) where T : MonoBehaviour
        {
            if (target == null || target.gameObject == null)
                return false;

            theCall();
            return true;
        }

        public static string CanonicalName<T>(this T target) where T : UnityEngine.Object
        {
            if (target == null)
                return null;

            int firstIndex = target.name.IndexOf("(clone)");
            if (firstIndex != -1)
                return target.name.Remove(firstIndex).TrimEnd();
            else
                return target.name;
        }

        public static GameObject GetTopmostParent(this GameObject srcObject)
        {
            if (srcObject == null)
                return null;

            GameObject topParent = srcObject;
            while (topParent.transform.parent != null)
            {
                topParent = topParent.transform.parent.gameObject;
            }

            return topParent;
        }
    }
}
