using StarMap.API;

namespace StarMap.Core.ModRepository
{
    internal sealed class ModRegistry : IDisposable
    {
        private readonly Dictionary<Type, List<IStarMapInterface>> _map = new();

        public void Add(Type iface, IStarMapInterface instance)
        {
            // --- type-safety checks ---
            if (!typeof(IStarMapInterface).IsAssignableFrom(iface) || !iface.IsInterface)
                throw new ArgumentException($"{iface} is not an interface inheriting {typeof(IStarMapInterface).Name}");

            if (instance is null)
                throw new ArgumentNullException(nameof(instance));

            Type implType = instance.GetType();

            if (!iface.IsAssignableFrom(implType))
                throw new ArgumentException(
                    $"{implType.Name} does not implement {iface.Name}"
                );

            // --- add instance ---
            if (!_map.TryGetValue(iface, out var list))
            {
                list = [];
                _map[iface] = list;
            }

            list.Add(instance);
        }

        public IReadOnlyList<TInterface> Get<TInterface>()
            where TInterface : IStarMapInterface
        {
            if (_map.TryGetValue(typeof(TInterface), out var list))
            {
                return list.Cast<TInterface>().ToList();
            }

            return Array.Empty<TInterface>();
        }

        public IReadOnlyList<IStarMapInterface> Get(Type iface)
        {
            return _map.TryGetValue(iface, out var list)
                ? list
                : Array.Empty<IStarMapInterface>();
        }

        public void Dispose()
        {
            _map.Clear();
        }
    }
}
