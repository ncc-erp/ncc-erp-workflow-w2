using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;

namespace W2
{
    public class IdGenerator : Elsa.Services.IIdGenerator, ISingletonDependency
    {
        private readonly IGuidGenerator _guidGenerator;

        public IdGenerator(IGuidGenerator guidGenerator)
        {
            _guidGenerator = guidGenerator;
        }

        public string Generate()
        {
            return _guidGenerator.Create().ToString();
        }
    }
}
