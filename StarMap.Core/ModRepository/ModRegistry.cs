using StarMap.API;
using System.Reflection;

namespace StarMap.Core.ModRepository
{
    internal sealed class ModRegistry : IDisposable
    {
        private readonly Dictionary<Type, List<(StarMapMethodAttribute attribute, object @object, MethodInfo method)>> _map = new();

        public void Add(StarMapMethodAttribute attribute, object @object, MethodInfo method)
        {
            var attributeType = attribute.GetType();

            // --- add instance ---
            if (!_map.TryGetValue(attributeType, out var list))
            {
                list = [];
                _map[attributeType] = list;
            }

            list.Add((attribute, @object, method));
        }

        public IReadOnlyList<(StarMapMethodAttribute attribute, object @object, MethodInfo method)> Get<TAttribute>()
            where TAttribute : Attribute
        {
            if (_map.TryGetValue(typeof(TAttribute), out var list))
            {
                return list.Cast<(StarMapMethodAttribute attribute, object @object, MethodInfo method)>().ToList();
            }

            return Array.Empty<(StarMapMethodAttribute attribute, object @object, MethodInfo method)>();
        }

        public IReadOnlyList<(StarMapMethodAttribute attribute, object @object, MethodInfo method)> Get(Type iface)
        {
            return _map.TryGetValue(iface, out var list)
                ? list
                : Array.Empty<(StarMapMethodAttribute attribute, object @object, MethodInfo method)>();
        }

        public void Dispose()
        {
            _map.Clear();
        }
    }
}
