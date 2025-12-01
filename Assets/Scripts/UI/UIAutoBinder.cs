using UnityEngine;


namespace WheelGame.UI
{
    
    public static class UIAutoBinder
    {
        public static T FindComponentInChildren<T>(Component root, string childName) where T : Component
        {
            if (root == null)
            {
                return null;
            }

            var transforms = root.GetComponentsInChildren<Transform>(true);
            
            foreach (var t in transforms)
            {
                if(t.name == childName)
                {
                    return t.GetComponent<T>();
                }
            }
            return null;
        }
    }
}