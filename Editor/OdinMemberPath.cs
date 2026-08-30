#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;

namespace MartinCalander.OdinSequence.Editor
{
    internal static class OdinMemberPath
    {
        public static InspectorProperty Resolve(InspectorProperty element, string memberPath)
        {
            if (element == null || string.IsNullOrWhiteSpace(memberPath))
                return null;

            InspectorProperty current = element;
            string[] segments = memberPath.Split('.');
            for (int index = 0; index < segments.Length; index++)
            {
                string segment = segments[index].Trim();
                if (segment.Length == 0)
                    return null;

                current = current.Children[segment];
                if (current == null)
                    return null;
            }

            return current;
        }
    }
}
#endif
