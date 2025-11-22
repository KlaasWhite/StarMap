using KSA;
using StarMap.API;
using System.Reflection;

namespace StarMap.Core.ModRepository
{
    internal sealed class ModRegistry : IDisposable
    {
        private readonly Dictionary<Type, List<(StarMapMethodAttribute attribute, object @object, MethodInfo method)>> _map = [];
        private readonly Dictionary<string, (object @object, MethodInfo method)> _beforeMainActions = [];
        private readonly Dictionary<string, (object @object, MethodInfo method)> _prepareSystemsActions = [];

        public void Add(string modId, StarMapMethodAttribute attribute, object @object, MethodInfo method)
        {
            var attributeType = attribute.GetType();

            if (!_map.TryGetValue(attributeType, out var list))
            {
                list = [];
                _map[attributeType] = list;
            }

            if (attribute.GetType() == typeof(StarMapBeforeMainAttribute))
                _beforeMainActions[modId] = (@object, method);

            if (attribute.GetType() == typeof(StarMapImmediateLoadAttribute))
                _prepareSystemsActions[modId] = (@object, method);

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

        public (object @object, MethodInfo method)? GetBeforeMainAction(string modId)
        {
            return _beforeMainActions.TryGetValue(modId, out var action) ? action : null;
        }

        public (object @object, MethodInfo method)? GetPrepareSystemsAction(string modId)
        {
            return _prepareSystemsActions.TryGetValue(modId, out var action) ? action : null;
        }

        public void Dispose()
        {
            _map.Clear();
        }
    }
}
