using MyTypes;

namespace SourceGenerationMain.VerifyJson
{
	internal class ClassWithConstantJsonField
	{
		[IsJson]
		const string _myJson = "{ \"y\" : 1 }";
		[IsJson]
		const string _myInvalidJson = "{ y;:;; 1 }";
	}
}
