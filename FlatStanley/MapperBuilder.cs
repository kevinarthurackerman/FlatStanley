using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FlatStanley
{
    public class MapperBuilder
    {
        public IList<AbstractMap> Maps { get; set; } = new List<AbstractMap>();

        public MapperBuilder RegisterAssembly(Assembly assembly)
        {
            var maps = assembly.GetTypes()
                .Where(x => x.IsAssignableTo(typeof(AbstractMap)))
                .Select(x => (AbstractMap)Activator.CreateInstance(x))
                .ToArray();

            foreach (var map in maps) Maps.Add(map);

            return this;
        }

        public IMapper Build() => new Mapper(Maps);
    }
}
