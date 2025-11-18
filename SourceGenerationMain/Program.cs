using SourceGenerationMain.AddMethodToClass;
using SourceGenerationMain.Inheritance;

namespace SourceGenerationMain;

    public partial class Program
    {
        static void Main(string[] args)
        {
		WantToSayHello.Hello();

            var iWantFoo = new WantFoo();
            iWantFoo.Bar();


        }
    }
