using MyTypes;

namespace SourceGenerationMain.Inheritance;

    [Inherit(typeof(Foo)), Inherit(typeof(SecondFoo))]
    public partial class WantFoo
    {
    }
